using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityRosylnAnalyzer.Analyzers.StringMethodUsage
{
	public abstract class StringMethodUsageCodeFix : UnityCodeFix
	{
		public override IEnumerable<Type> FixableDiagnostics
			=> new[] { typeof(StringMethodUsageAnalyzer) };

		public abstract StringMethodUsageAnalyzer.CodeFixSuggestion CodeFixSuggestion { get; }
		public abstract string Title { get; }

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var requestedCodefix = diagnostic.Properties[StringMethodUsageAnalyzer.CodeFixSuggestionProperty];
			var actualCodefix = Enum.GetName(typeof(StringMethodUsageAnalyzer.CodeFixSuggestion), CodeFixSuggestion);
			if (actualCodefix != requestedCodefix)
			{
				return;
			}

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan);

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
			int index = 0;
			string constString = null;

			foreach (var argument in invocation.ArgumentList.Arguments)
			{
				var constValue = semanticModel.GetConstantValue(argument.Expression);
				if (constValue.HasValue && constValue.Value is string s)
				{
					constString = s;
					break;
				}
				index++;
			}

			if (constString != null)
			{
				RegisterCodeFix(context, Title, (cancel) => FixStringMethodUsageAsync(context.Document, invocation, index, constString, cancel));
			}
		}

		protected abstract Task<Document> FixStringMethodUsageAsync(Document document, InvocationExpressionSyntax invocation, int constStringArgumentIndex, string constStringValue, CancellationToken cancel);
	}
}