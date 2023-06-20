using FakeItEasy;

namespace ConsoleTest;

internal static class Constants
{
    public const int MAX = 3_000;
    public const string END_POINT_KEY = "REDIS_EVENT_SOURCE_ENDPOINT";
    public const string ENV = $"console-test";
    public static readonly IFooConsumer Subscriber = A.Fake<IFooConsumer>();

}
