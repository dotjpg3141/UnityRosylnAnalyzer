using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace UnityRosylnAnalyzer.Analyzers.CoroutineMustReturnIEnumerator
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CoroutineMustReturnIEnumeratorCodeFix)), Shared]
	public class CoroutineMustReturnIEnumeratorCodeFix : UnityCodeFix
	{
		public override IEnumerable<Type> FixableDiagnostics
			=> new[] { typeof(CoroutineMustReturnIEnumeratorAnalyzer) };

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.AdditionalLocations[0];

			RegisterCodeFix(context, "Replace generic Coroutine with non generic", (cancel) => FixCoroutineReturnTypeAsync(context.Document, diagnosticSpan));
		}

		private async Task<Solution> FixCoroutineReturnTypeAsync(Document document, Location diagnosticSpan)
		{
			var editDocument = document.Project.GetDocument(diagnosticSpan.SourceTree);
			var oldRoot = await editDocument.GetSyntaxRootAsync();
			var oldMethod = oldRoot.DescendantNodes(diagnosticSpan.SourceSpan)
				.OfType<MethodDeclarationSyntax>()
				.Where(methodNode => diagnosticSpan.SourceSpan.Contains(methodNode.Span))
				.FirstOrDefault();

			var semanticModel = await document.GetSemanticModelAsync();

			var newMethod = oldMethod.WithReturnType(
				SyntaxFactory.IdentifierName("IEnumerator"));

			var newRoot = ((CompilationUnitSyntax)oldRoot)
				.ReplaceNode(oldMethod, newMethod);

			if (newRoot.Usings.All(usingNode => usingNode.Name.GetText().ToString() != "System.Collections"))
			{
				newRoot = newRoot.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections")));
			}

			return document.Project.Solution.WithDocumentSyntaxRoot(
				editDocument.Id, newRoot);
		}
	}
}