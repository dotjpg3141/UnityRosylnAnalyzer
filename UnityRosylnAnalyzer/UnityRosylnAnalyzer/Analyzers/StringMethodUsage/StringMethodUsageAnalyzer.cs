using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace UnityRosylnAnalyzer.Analyzers.StringMethodUsage
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class StringMethodUsageAnalyzer : UnityDiagnosticAnalyzer
	{

		internal const string CodeFixSuggestionProperty = "CodeFixSuggestion";

		private static readonly ImmutableDictionary<MethodInfo, string> StringMethods = new Dictionary<string, CodeFixSuggestion>
		{
			["UnityEngine.MonoBehaviour.CancelInvoke"] = CodeFixSuggestion.None,
			["UnityEngine.MonoBehaviour.Invoke"] = CodeFixSuggestion.None,
			["UnityEngine.MonoBehaviour.InvokeRepeating"] = CodeFixSuggestion.None,
			["UnityEngine.MonoBehaviour.IsInvoking"] = CodeFixSuggestion.None,
			["UnityEngine.MonoBehaviour.StartCoroutine"] = CodeFixSuggestion.ReplaceWithMethodCall,
			["UnityEngine.MonoBehaviour.StopCoroutine"] = CodeFixSuggestion.ReplaceWithMethodCall,
			["UnityEngine.Component.BroadcastMessage"] = CodeFixSuggestion.None,
			["UnityEngine.Component.GetComponent"] = CodeFixSuggestion.ReplaceWithGenericParameter,
			["UnityEngine.Component.SendMessage"] = CodeFixSuggestion.None,
			["UnityEngine.Component.SendMessageUpwards"] = CodeFixSuggestion.None,

		}.ToImmutableDictionary(
			keySelector: kvp => MethodInfo.FromFullName(kvp.Key),
			elementSelector: kvp => Enum.GetName(typeof(CodeFixSuggestion), kvp.Value));

		protected override DiagnosticDescriptor SupportedDiagnostic
			=> CreateDescriptorFromResource<StringMethodUsageResources>(StringMethodUsageResources.ResourceManager, Category.Performance, DiagnosticSeverity.Warning);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
		}

		private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			var invocation = (InvocationExpressionSyntax)context.Node;
			if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
				&& StringMethods.TryGetValue(MethodInfo.FromSymbol(methodSymbol), out var codeFixSuggestion)
				&& methodSymbol.Parameters.Any(param => param.Type.ToDisplayString() == "string"))
			{

				var properties = new Dictionary<string, string>()
				{
					[CodeFixSuggestionProperty] = codeFixSuggestion
				};

				var diagnostic = CreateDiagnostic(new[] { invocation.GetLocation() }, new object[] { methodSymbol }, properties);
				context.ReportDiagnostic(diagnostic);
			}
		}

		public enum CodeFixSuggestion
		{
			None,
			ReplaceWithGenericParameter,
			ReplaceWithMethodCall,
		}
	}
}