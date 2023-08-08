using EventsAbstractions;

using EventSourcing.Backbone;

class Subscription : IHelloEventsConsumer
{
    public static readonly Subscription Instance = new Subscription();
    public ValueTask NameAsync(ConsumerContext meta, string name)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.Write($"Hello {name}: ");
        return ValueTask.CompletedTask;
    }

    public ValueTask ColorAsync(ConsumerContext meta, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        return ValueTask.CompletedTask;
    }

    public ValueTask StarAsync(ConsumerContext meta)
    {
        Console.Write("✱");
        return ValueTask.CompletedTask;
    }
}
