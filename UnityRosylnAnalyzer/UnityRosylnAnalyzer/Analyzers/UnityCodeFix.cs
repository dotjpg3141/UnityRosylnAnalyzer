using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnityRosylnAnalyzer.Analyzers
{
	public abstract class UnityCodeFix : CodeFixProvider
	{
		private const string equivalenceKeyPrefix = "UnityRosylnAnalyzer";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
			=> FixableDiagnostics.Select(UnityDiagnosticAnalyzer.GetDiagnosticId).ToImmutableArray();

		public abstract IEnumerable<Type> FixableDiagnostics { get; }

		public override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public void RegisterCodeFix(CodeFixContext context, string title, Func<CancellationToken, Task<Document>> createChangedDocument)
		{
			context.RegisterCodeFix(
				CodeAction.Create(title, createChangedDocument, equivalenceKeyPrefix + title),
				context.Diagnostics.First());
		}

		public void RegisterCodeFix(CodeFixContext context, string title, Func<CancellationToken, Task<Solution>> createChangedSolution)
		{
			context.RegisterCodeFix(
				CodeAction.Create(title, createChangedSolution, equivalenceKeyPrefix + title),
				context.Diagnostics.First());
		}

		
	}
}
