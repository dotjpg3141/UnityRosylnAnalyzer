using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Resources;

namespace UnityRosylnAnalyzer.Analyzers
{
	public abstract class UnityDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(SupportedDiagnostic);

		public string DiagnosticId
			=> GetDiagnosticId(GetType());

		protected abstract DiagnosticDescriptor SupportedDiagnostic { get; }

		protected DiagnosticDescriptor CreateDescriptorFromResource<TResource>(ResourceManager resourceManager, Category category, DiagnosticSeverity severity = DiagnosticSeverity.Info)
		{
			var title = new LocalizableResourceString("AnalyzerTitle", resourceManager, typeof(TResource));
			var messageFormat = new LocalizableResourceString("AnalyzerMessageFormat", resourceManager, typeof(TResource));
			var description = new LocalizableResourceString("AnalyzerDescription", resourceManager, typeof(TResource));
			var categoryString = Enum.GetName(typeof(Category), category);
			return new DiagnosticDescriptor(DiagnosticId, title, messageFormat, categoryString, severity, true, description: description);
		}

		protected Diagnostic CreateDiagnostic(Location[] location, object[] messageArgs, IDictionary<string, string> properties = null)
		{
			var propertyDict = properties?.ToImmutableDictionary();
			return Diagnostic.Create(SupportedDiagnostic, location[0], location.Skip(1), propertyDict, messageArgs);
		}

		public static string GetDiagnosticId(Type diagnosticType)
		{
			var id = diagnosticType.Name;
			return TrimStringEnd(id, "Analyzer");

			string TrimStringEnd(string input, string trimString)
			{
				if (input.EndsWith(trimString))
				{
					return input.Substring(0, input.Length - trimString.Length);
				}
				else
				{
					return input;
				}
			}
		}
	}
}
