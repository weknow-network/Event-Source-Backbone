using System.Collections.Immutable;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis;

internal static class RoslynHelper
{
    #region GetBaseTypesAndThis

    /// <summary>
    /// Gets the base types and this.
    /// credits: https://stackoverflow.com/questions/30443616/is-there-any-way-to-get-members-of-a-type-and-all-subsequent-base-types
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        yield return type;
        foreach (var current in type.GetBaseTypes())
        {
            yield return current;
        }
    }

    #endregion // GetBaseTypesAndThis

    #region GetBaseTypes

    /// <summary>
    /// Gets the base types and this.
    /// credits: https://stackoverflow.com/questions/30443616/is-there-any-way-to-get-members-of-a-type-and-all-subsequent-base-types
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static IEnumerable<ITypeSymbol> GetBaseTypes(this ITypeSymbol type)
    {
        var current = type.BaseType;
        while (current != null && current.Name != "Object")
        {
            yield return current;
            current = current.BaseType;
        }
    }

    #endregion // GetBaseTypes

    #region GetAllMethods

    /// <summary>
    /// Gets methods including those of the base types (excludes object type).
    /// credits: https://stackoverflow.com/questions/30443616/is-there-any-way-to-get-members-of-a-type-and-all-subsequent-base-types
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static IEnumerable<IMethodSymbol> GetAllMethods(this ITypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (member is IMethodSymbol ms && !ms.IsStatic)
                yield return ms;
        }
        foreach (var child in type.Interfaces)
        {
            foreach (var childMember in GetAllMethods(child))
            {
                yield return childMember;
            }
        }
    }

    #endregion // GetAllMethods

    #region GetUsing

    /// <summary>
    /// Gets the using statements.
    /// </summary>
    /// <param name="syntaxNode">The syntax node.</param>
    /// <returns></returns>
    public static IEnumerable<string> GetUsing(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is CompilationUnitSyntax m)
        {
            foreach (var u in m.Usings)
            {
                var match = u.ToString();
                yield return match;
            }
        }

        if (syntaxNode.Parent == null)
            yield break;

        foreach (var u in GetUsing(syntaxNode.Parent))
        {
            yield return u;
        }
    }

    #endregion // GetUsing

    #region ToNameConvention

    public static string ToNameConvention(this MethodDeclarationSyntax method)
    {
        string name = method.Identifier.ValueText;
        if (name.EndsWith("Async"))
            return name;
        return $"{name}Async";
    }

    public static string ToNameConvention(this IMethodSymbol method)
    {
        string name = method.Name;
        if (name.EndsWith("Async"))
            return name;
        return $"{name}Async";
    }

    #endregion // ToNameConvention
}
