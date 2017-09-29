using Microsoft.CodeAnalysis;
using System.Linq;

namespace UnityRosylnAnalyzer
{
	internal static class SemanticExtension
	{
		public static string GenerateValidName(this SemanticModel semanticModel, int position, string input, string fallback = "temp", Case firstChar = Case.Default)
		{
			// make valid identifier
			var arr = input.ToCharArray()
					.SkipWhile(c => !char.IsLetter(c))
					.Select((c, i) => i == 0 && firstChar == Case.LowerCase ? char.ToLowerInvariant(c) :
									  i == 0 && firstChar == Case.UpperCase ? char.ToUpperInvariant(c) :
									  c)
					.TakeWhile(char.IsLetterOrDigit)
					.ToArray();

			var baseName = arr.Length != 0 ? new string(arr) : fallback;
			var validName = baseName;
			int index = 1;

			// retry until valid name is found
			while (semanticModel.LookupSymbols(position, name: validName).Any())
			{
				index++;
				validName = baseName + index;
			}

			return validName;
		}
	}


	internal enum Case
	{
		Default, LowerCase, UpperCase
	}
}
