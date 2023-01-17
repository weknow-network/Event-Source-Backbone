namespace Microsoft.CodeAnalysis;

internal static class RoslynHelper
{
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
}
