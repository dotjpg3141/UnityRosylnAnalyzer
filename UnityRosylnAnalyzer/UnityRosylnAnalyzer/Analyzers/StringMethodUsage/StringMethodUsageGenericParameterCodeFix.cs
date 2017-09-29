using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace UnityRosylnAnalyzer.Analyzers.StringMethodUsage
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringMethodUsageGenericParameterCodeFix)), Shared]
	public class StringMethodUsageGenericParameterCodeFix : StringMethodUsageCodeFix
	{
		public override StringMethodUsageAnalyzer.CodeFixSuggestion CodeFixSuggestion
			=> StringMethodUsageAnalyzer.CodeFixSuggestion.ReplaceWithGenericParameter;

		public override string Title
			=> "Replace string argument with generic argument";

		protected override async Task<Document> FixStringMethodUsageAsync(Document document, InvocationExpressionSyntax invocation, int constStringArgumentIndex, string constStringValue, CancellationToken cancel)
		{
			var oldMethodName = invocation.GetMethodName() as SimpleNameSyntax;

			var newMethodName = SyntaxFactory.GenericName(
				oldMethodName.Identifier,
				SyntaxFactory.TypeArgumentList(
					SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
						SyntaxFactory.IdentifierName(constStringValue))));

			var newInvocation = invocation
				.ReplaceNode(oldMethodName, newMethodName)
				.RemoveArgument(constStringArgumentIndex);

			var oldRoot = await document.GetSyntaxRootAsync(cancel);
			var newRoot = oldRoot.ReplaceNode(invocation, newInvocation);
			return document.WithSyntaxRoot(newRoot);
		}
	}
}
