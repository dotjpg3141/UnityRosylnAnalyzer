using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace UnityRosylnAnalyzer.Analyzers.StringMethodUsage
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringMethodUsageMethodCallCodeFix)), Shared]
	public class StringMethodUsageMethodCallCodeFix : StringMethodUsageCodeFix
	{
		public override StringMethodUsageAnalyzer.CodeFixSuggestion CodeFixSuggestion
			=> StringMethodUsageAnalyzer.CodeFixSuggestion.ReplaceWithMethodCall;

		public override string Title
			=> "Replace string argument with method call";

		protected override async Task<Document> FixStringMethodUsageAsync(Document document, InvocationExpressionSyntax invocation, int constStringArgumentIndex, string constStringValue, CancellationToken cancel)
		{
			var oldArguments = invocation.ArgumentList.Arguments;
			var newArguments = oldArguments
				.RemoveAt(constStringArgumentIndex)
				.Insert(constStringArgumentIndex,
					SyntaxFactory.Argument(
						SyntaxFactory.InvocationExpression(
							SyntaxFactory.IdentifierName(constStringValue))));

			var newArgumentList = invocation.ArgumentList
				.WithArguments(newArguments);

			var oldRoot = await document.GetSyntaxRootAsync(cancel);
			var newRoot = oldRoot.ReplaceNode(invocation.ArgumentList, newArgumentList);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}