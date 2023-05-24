using EventsAbstractions;

using EventSourcing.Backbone;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Drawing;

Console.WriteLine("Consuming Events");

IConsumerLifetime subscription = RedisConsumerBuilder.Create()
                                            .Uri(URIs.Default)
                                            .Group("sample.hello-world")
                                            .SubscribeHelloEventsConsumer(Subscription.Instance);
Console.ReadKey(false);

class Subscription : IHelloEventsConsumer
{
    public static readonly Subscription Instance = new Subscription();
    public ValueTask NameAsync(string name)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.Write($"Hello {name}: ");
        return ValueTask.CompletedTask;
    }

    public ValueTask ColorAcync(ConsoleColor color)
    {
        Console.ForegroundColor = color;
        return ValueTask.CompletedTask;
    }

    public ValueTask StarAsync()
    {
        Console.Write("✱");
        return ValueTask.CompletedTask;
    }
}
