using EventsAbstractions;

using EventSourcing.Backbone;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

Console.WriteLine("Producing events");

IHelloEventsProducer producer = RedisProducerBuilder.Create()
                                .Uri(URIs.Default)
                                .BuildHelloEventsProducer();



Console.Write("What is your name? ");
string name = Console.ReadLine();
await producer.NameAsync(name ?? "Unknown");

var rnd = new Random(Guid.NewGuid().GetHashCode());
Console.WriteLine("Press Esc to exit");
Console.Write("Press Number for delay: ");
Console.WriteLine("1 is ms, 2 is 10 ms, 3 is 100 ms, 4 is 1s, 5 is 10s, 6 is 30s, 7 is 1m");


var colors = Enum.GetValues<ConsoleColor>() ?? Array.Empty<ConsoleColor>();
while (!Console.KeyAvailable || Console.ReadKey(true).Key == ConsoleKey.Escape)
{
    int index = Environment.TickCount % colors.Length;
    var color = colors[index];
    await producer.ColorAsync(color);
    await producer.StarAsync();

    ConsoleKey press = Console.KeyAvailable ? Console.ReadKey(true).Key : ConsoleKey.Clear;
    int delay = press switch
    {
        ConsoleKey.D1 => 1,
        ConsoleKey.D2 => 10,
        ConsoleKey.D3 => 100,
        ConsoleKey.D4 => 1_000,
        ConsoleKey.D5 => 10_000,
        ConsoleKey.D6 => 30_000,
        ConsoleKey.D7 => 60_000,
        ConsoleKey.Escape => -1,
        _ => 0
    };
    if (delay == -1)
        break;
    if (delay != 0)
        await Task.Delay(delay);
    Console.ForegroundColor = color;
    Console.Write("☆");
}

Console.WriteLine(" Done");