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
            if(member is IMethodSymbol ms && !ms.IsStatic) 
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
}
