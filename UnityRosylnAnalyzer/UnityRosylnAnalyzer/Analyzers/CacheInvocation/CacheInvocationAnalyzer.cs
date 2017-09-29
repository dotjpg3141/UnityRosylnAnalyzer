using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace UnityRosylnAnalyzer.Analyzers.CacheInvocation
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CacheInvocationAnalyzer : UnityDiagnosticAnalyzer
	{

		private static readonly ImmutableHashSet<string> UpdateMethods = new[] {
			"FixedUpdate",
			"LateUpdate",
			"OnDrawGizmos",
			"OnDrawGizmosSelected",
			"OnGUI",
			"OnMouseOver",
			"OnPostRender",
			"OnPreCull",
			"OnPreCull",
			"OnValidate",
			"Update"
		}.ToImmutableHashSet();

		private static readonly ImmutableHashSet<MethodInfo> CachedMethods = new[] {
			"UnityEngine.GameObject.Find",
			"UnityEngine.GameObject.FindGameObjectWithTag",
			"UnityEngine.GameObject.FindGameObjectsWithTag",
			"UnityEngine.GameObject.FindWithTag",
			"UnityEngine.Object.FindObjectOfType",
			"UnityEngine.Object.FindObjectsOfType",
			"UnityEngine.Component.GetComponent",
			"UnityEngine.Component.GetComponentInChildren",
			"UnityEngine.Component.GetComponentInParent",
			"UnityEngine.Component.GetComponents",
			"UnityEngine.Component.GetComponentsInChildren",
			"UnityEngine.Component.GetComponentsInParent",
		}.Select(MethodInfo.FromFullName).ToImmutableHashSet();

		protected override DiagnosticDescriptor SupportedDiagnostic
			=> CreateDescriptorFromResource<CacheInvocationResources>(CacheInvocationResources.ResourceManager, Category.Performance);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
		}

		private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			var invocation = (InvocationExpressionSyntax)context.Node;

			if (IsValidInvocationContext(context.SemanticModel, invocation)
				&& context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
				&& CachedMethods.Contains(MethodInfo.FromSymbol(methodSymbol))
				&& (invocation.IsInvokedOnThis() || methodSymbol.IsStatic))
			{
				var diagnostic = CreateDiagnostic(new[] { invocation.GetLocation() }, new[] { methodSymbol });
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static bool IsValidInvocationContext(SemanticModel semanticModel, InvocationExpressionSyntax invocation)
		{
			// inside update method
			var methodContext = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
			if (methodContext == null
				|| !UpdateMethods.Contains(methodContext.Identifier.Text))
			{
				return false;
			}

			// inside component class
			var classContext = methodContext.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
			if (classContext == null
				|| !(semanticModel.GetDeclaredSymbol(classContext) is ITypeSymbol typeSymbol)
				|| typeSymbol.GetAncestors().Any(baseType => baseType.Name == "UnityEngine.Component"))
			{
				return false;
			}

			return true;
		}
	}
}