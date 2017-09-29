using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace UnityRosylnAnalyzer.Analyzers.CoroutineMustReturnIEnumerator
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CoroutineMustReturnIEnumeratorAnalyzer : UnityDiagnosticAnalyzer
	{
		protected override DiagnosticDescriptor SupportedDiagnostic
			=> CreateDescriptorFromResource<CoroutineMustReturnIEnumeratorResource>(CoroutineMustReturnIEnumeratorResource.ResourceManager, Category.Performance, DiagnosticSeverity.Warning);

		private static readonly ImmutableHashSet<MethodInfo> CoroutineMethods = new[] {
			"UnityEngine.MonoBehaviour.StartCoroutine",
			"UnityEngine.MonoBehaviour.StopCoroutine",
		}.Select(MethodInfo.FromFullName).ToImmutableHashSet();

		private static readonly ImmutableHashSet<string> InvalidCoroutineReturnTypes = new[] {
			"System.Collections.Generic.IEnumerator<T>",
		}.ToImmutableHashSet();

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
		}

		private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			var invocation = (InvocationExpressionSyntax)context.Node;
			if (invocation.ArgumentList?.Arguments.Count == 1
				&& invocation.ArgumentList.Arguments[0].Expression is ExpressionSyntax argument
				&& context.SemanticModel.GetTypeInfo(argument).Type is INamedTypeSymbol typeSymbol
				&& InvalidCoroutineReturnTypes.Contains((typeSymbol.ConstructedFrom ?? typeSymbol).ToDisplayString())
				&& context.SemanticModel.GetSymbolInfo(argument).Symbol is IMethodSymbol methodSymbol
				&& methodSymbol.DeclaringSyntaxReferences.Length == 1)
			{
				var decl = methodSymbol.DeclaringSyntaxReferences.First();
				var declLocation = Location.Create(decl.SyntaxTree, decl.Span);

				var diagnostic = CreateDiagnostic(new[] { invocation.GetLocation(), declLocation }, new[] { methodSymbol });
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}