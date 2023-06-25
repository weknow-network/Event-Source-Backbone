namespace EventSourcing.Backbone;

public interface ITagAddition
{
    ITagAddition Add<T>(string key, T value);
}
