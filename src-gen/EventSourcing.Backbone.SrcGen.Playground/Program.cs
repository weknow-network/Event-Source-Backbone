using EventSourcing.Backbone.WebEventTest;

using FakeItEasy;

namespace EventSourcing.Backbone.SrcGen.Playground
{
    internal class Program
    {
        private static IEventsWithVersionConsumer _subscriber = A.Fake<IEventsWithVersionConsumer>();
        private static IConsumerChannelProvider _channel = A.Fake<IConsumerChannelProvider>();

        private static void Main(string[] args)
        {
            IConsumerLifetime subscription = ConsumerBuilder.Empty
                                    .UseChannel(l => _channel)
                                     .Uri("sample")
                                     //.Fallback<IEventsWithVersionConsumer>(m =>
                                     //{
                                     //    switch (m.Metadata.Metadata)
                                     //    {
                                     //        case
                                     //        {
                                     //            Operation: nameof(IEventsWithVersionConsumer.ExecuteAsync),
                                     //            Version: 0,
                                     //            ParamsSignature: "String_Int32" // TODO: have generated constants
                                     //        }:
                                     //            break;
                                     //    }
                                     //    return Task.CompletedTask;
                                     //})
                                     .SubscribeEventsWithVersionConsumer(_subscriber);
            IConsumerLifetime subscription1 = ConsumerBuilder.Empty
                                    .UseChannel(l => _channel)
                                     .Uri("sample")
                                     .SubscribeEventsWithVersionConsumer(_subscriber);
            Console.WriteLine("Hello World!");
        }
    }
}
