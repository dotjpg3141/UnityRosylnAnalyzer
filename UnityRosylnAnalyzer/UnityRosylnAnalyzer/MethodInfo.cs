using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityRosylnAnalyzer
{
	public struct MethodInfo
	{
		public string TypeName { get; }
		public string MethodName { get; }

		public MethodInfo(string typeName, string methodName) : this()
		{
			TypeName = typeName;
			MethodName = methodName;
		}

		public static MethodInfo FromFullName(string fullName)
		{
			var parts = fullName.Split('.');
			if (parts.Length < 2)
			{
				throw new ArgumentException("Invalid Method name", nameof(fullName));
			}

			var tpName = string.Join(".", parts.Take(parts.Length - 1));
			var methodName = parts.Last();
			return new MethodInfo(tpName, methodName);
		}

		public static MethodInfo FromSymbol(IMethodSymbol symbol)
		{
			return new MethodInfo(symbol.ContainingType?.ToDisplayString(), symbol.Name);
		}

		#region Equals

		public override bool Equals(object obj)
		{
			if (!(obj is MethodInfo))
			{
				return false;
			}

			var info = (MethodInfo)obj;
			return TypeName == info.TypeName &&
				   MethodName == info.MethodName;
		}

		public override int GetHashCode()
		{
			var hashCode = -1966479503;
			hashCode = hashCode * -1521134295 + base.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MethodName);
			return hashCode;
		}

		public static bool operator ==(MethodInfo info1, MethodInfo info2)
		{
			return info1.Equals(info2);
		}

		public static bool operator !=(MethodInfo info1, MethodInfo info2)
		{
			return !(info1 == info2);
		}

		#endregion
	}
}