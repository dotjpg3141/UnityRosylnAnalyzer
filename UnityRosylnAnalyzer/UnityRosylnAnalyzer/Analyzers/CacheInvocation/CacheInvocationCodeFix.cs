using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityRosylnAnalyzer.Analyzers.CacheInvocation
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CacheInvocationCodeFix)), Shared]
	public class CacheInvocationCodeFix : UnityCodeFix
	{
		public override IEnumerable<Type> FixableDiagnostics
			=> new[] { typeof(CacheInvocationAnalyzer) };

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var invocation = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan);

			RegisterCodeFix(context, "Cache invocation (initialize in Awake)",
				(cancel) => CacheInvocationAsync(context.Document, invocation, "Awake", cancel));

			RegisterCodeFix(context, "Cache invocation (initialize in Start)",
				(cancel) => CacheInvocationAsync(context.Document, invocation, "Start", cancel));
		}

		private async Task<Document> CacheInvocationAsync(Document document, InvocationExpressionSyntax invocation, string initMethodName, CancellationToken cancel)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancel);
			var method = (IMethodSymbol)semanticModel.GetSymbolInfo(invocation).Symbol;

			var oldClass = invocation.Ancestors().OfType<ClassDeclarationSyntax>().First();
			MethodDeclarationSyntax oldInitMethod = FindInitMethod(initMethodName, oldClass);

			string variableName = SuggestVariableName(semanticModel, invocation);

			// declaration of cached expression
			var fieldDeclaration = SyntaxExtension.DeclareField(
				new[] { SyntaxKind.PrivateKeyword },
				method.ReturnType.ToMinimalDisplayString(semanticModel, invocation.GetLocation().SourceSpan.Start),
				variableName)
				.WithAdditionalAnnotations(Formatter.Annotation);

			// initialization of cached expression
			var newInitMethod = oldInitMethod ?? MethodDeclaration(IdentifierName("void"), initMethodName).WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
			newInitMethod = newInitMethod.AddBodyStatements(
				ExpressionStatement(AssignmentExpression(
					SyntaxKind.SimpleAssignmentExpression,
					IdentifierName(variableName),
					invocation)));

			// update class
			var newClass = oldClass
				.ReplaceNode(invocation, SyntaxFactory.IdentifierName(variableName))
				.InsertField(fieldDeclaration);
			if (oldInitMethod == null)
			{
				newClass = newClass.InsertMethod(newInitMethod);
			}
			else
			{
				var initMethod = FindInitMethod(initMethodName, newClass);
				newClass = newClass.ReplaceNode(initMethod, newInitMethod);
			}

			var oldRoot = await document.GetSyntaxRootAsync(cancel);
			var newRoot = oldRoot.ReplaceNode(oldClass, newClass);
			return document.WithSyntaxRoot(newRoot);
		}

		private static MethodDeclarationSyntax FindInitMethod(string initMethodName, ClassDeclarationSyntax oldClass)
		{
			return oldClass.Members.OfType<MethodDeclarationSyntax>()
				.Where(m => m.Identifier.Text == initMethodName && m.ParameterList?.Parameters.Count == 0)
				.FirstOrDefault();
		}

		private string SuggestVariableName(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
		{
			string name = null;

			// extract name from first generic argument
			if (invocation.GetMethodName() is GenericNameSyntax genericName
				&& genericName.TypeArgumentList.Arguments.FirstOrDefault() is TypeSyntax firstType)
			{
				var type = semanticModel.GetTypeInfo(firstType).Type ?? semanticModel.GetDeclaredSymbol(firstType) as ITypeSymbol;
				name = type?.Name;
			}

			// extract name from fist argument (if string constant)
			if (string.IsNullOrEmpty(name) &&
				invocation.ArgumentList?.Arguments.FirstOrDefault()?.Expression is ExpressionSyntax firstExpr)
			{
				var constValue = semanticModel.GetConstantValue(firstExpr);
				if (constValue.HasValue && constValue.Value is string strConst)
				{
					name = strConst;
				}
			}

			return semanticModel.GenerateValidName(invocation.GetLocation().SourceSpan.Start, name ?? "", fallback: "cached", firstChar: Case.LowerCase);
		}
	}
}