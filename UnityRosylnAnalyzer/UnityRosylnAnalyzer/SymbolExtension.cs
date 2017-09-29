using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace UnityRosylnAnalyzer
{
	public static class SymbolExtension
	{

		public static IEnumerable<ITypeSymbol> GetAncestors(this ITypeSymbol typeSymbol)
		{
			typeSymbol = typeSymbol.BaseType;
			while (typeSymbol != null)
			{
				yield return typeSymbol;
				typeSymbol = typeSymbol.BaseType;
			}
		}

	}
}
