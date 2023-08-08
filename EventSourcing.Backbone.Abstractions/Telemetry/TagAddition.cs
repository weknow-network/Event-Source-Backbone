using System.Diagnostics;

namespace EventSourcing.Backbone;

/// <summary>
/// Tag addition
/// </summary>
/// <seealso cref="EventSourcing.Backbone.ITagAddition" />
public class TagAddition : ITagAddition
{
    private readonly ActivityTagsCollection _tags;

    public TagAddition(ActivityTagsCollection tags)
    {
        _tags = tags;
    }
    public ITagAddition Add<T>(string key, T value)
    {
        _tags.Add(key, value);
        return this;
    }
}
