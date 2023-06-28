using EventsAbstractions;

using EventSourcing.Backbone;

Console.WriteLine("Consuming Events");

IConsumerLifetime subscription = RedisConsumerBuilder.Create()
                                            .Uri(URIs.Default)
                                            //.Group("sample.hello-world")
                                            .SubscribeHelloEventsConsumer(Subscription.Instance);
await subscription.Completion;
Console.ReadKey(false);
