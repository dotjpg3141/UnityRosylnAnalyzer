using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnityRosylnAnalyzer
{
	public static class SyntaxExtension
	{
		public static ClassDeclarationSyntax InsertField(this ClassDeclarationSyntax classSyntax, FieldDeclarationSyntax fieldSyntax)
		{
			// insert after last field
			var insertIndex = classSyntax.Members
				.TakeWhile(member => member is FieldDeclarationSyntax)
				.Count();

			var newMembers = classSyntax.Members.Insert(insertIndex, fieldSyntax);
			return classSyntax.WithMembers(List(newMembers));
		}

		public static ClassDeclarationSyntax InsertMethod(this ClassDeclarationSyntax classSyntax, MethodDeclarationSyntax methodSyntax)
		{
			//insert before first method
			var insertIndex = classSyntax.Members
				.TakeWhile(member => !(member is MethodDeclarationSyntax))
				.Count();

			var newMembers = classSyntax.Members.Insert(insertIndex, methodSyntax);
			return classSyntax.WithMembers(List(newMembers));
		}

		public static InvocationExpressionSyntax RemoveArgument(this InvocationExpressionSyntax invocation, int argumentIndex)
		{
			var arguments = invocation.ArgumentList.Arguments.RemoveAt(argumentIndex);
			return invocation.WithArgumentList(SyntaxFactory.ArgumentList(arguments));
		}

		public static NameSyntax GetMethodName(this InvocationExpressionSyntax invocation)
		{
			var expr = invocation.Expression;
			while (true)
			{
				switch (expr)
				{
					case NameSyntax name:
						return name;

					case MemberAccessExpressionSyntax memberAccess:
						expr = memberAccess.Name;
						break;

					default:
						return null;
				}
			}
		}

		public static bool IsInvokedOnThis(this InvocationExpressionSyntax invocation)
		{
			return invocation.Expression is IdentifierNameSyntax
				|| invocation.Expression is GenericNameSyntax
				|| (invocation.Expression is MemberAccessExpressionSyntax memberAcess && memberAcess.Expression is ThisExpressionSyntax);
		}

		public static FieldDeclarationSyntax DeclareField(SyntaxKind[] modifiers, string type, string identifier, ExpressionSyntax initializer = null)
		{
			return FieldDeclaration(
				List<AttributeListSyntax>(),
				TokenList(modifiers.Select(SyntaxFactory.Token)),
				VariableDeclaration(
					IdentifierName(type),
					SingletonSeparatedList(
						VariableDeclarator(
							identifier: Identifier(identifier),
							argumentList: null,
							initializer: initializer == null ? null : EqualsValueClause(initializer)))));
		}
	}
}
