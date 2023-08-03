namespace EventSourcing.Backbone.Tests.Entities
{

    /// <summary>
    /// Test contract
    /// </summary>
    [EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Default)]
    [EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.Default)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    public interface IVersionAwareNone //: IVersionAwareBase
    {
        //// [ConsumerFallback]        
        ///// <summary>
        ///// Consumers the fallback.
        ///// Excellent for Migration scenario
        ///// </summary>
        ///// <param name="ctx">The context.</param>
        ///// <returns></returns>
        //public static async Task ConsumerFallback(IConsumerFallback<IVersionAwareNone> ctx)
        //{ // wouldn't be called if handled (Ack) by the builder fallback 
        //    Metadata meta = ctx.Metadata;
        //    switch (meta.Operation)
        //    {
        //        case "ExecuteAsync" when meta.Version == 1:
        //            int? val1 = await ctx.GetParameterAsync<int>("value");
        //            if (val1 == null)
        //                throw new NullReferenceException();
        //            await ctx.Consumer.ExecuteAsync(val1.ToString()!);
        //            break;
        //        case "ExecuteAsync" when meta.Version == 2:
        //            DateTime? val2 = await ctx.GetParameterAsync<DateTime>("value");
        //            if (val2 == null)
        //                throw new NullReferenceException();
        //            var consumer = ctx.Consumer;
        //            await consumer.ExecuteAsync(val2.ToString()!);
        //            break;
        //        default:
        //            await ctx.AckAsync(AckBehavior.OnFallback);
        //            break;
        //    }
        //}

        //[ProducerFallback]
        //public static bool ProducerFallback(IProduceFallback handler)
        //{
        //    Metadata meta = ctx.Metadata;
        //    switch (meta.Operation)
        //    {
        //        case "ExecuteAsync" when meta.Version = 1:
        //            var val = ctx.SetParameterAsync<int>("value");
        //            handler.Producer.ExecuteAsync(val.ToString());
        //            // call ther overload (convertor should get the consumer interface)
        //    }
        //}

        ValueTask ExecuteAsync(string key, int value);
        [EventSourceVersion(1, Date = "2023-06-01", Remark = "sample of deprecation")]
        [EventSourceDeprecateVersionAttribute(EventsContractType.Producer,  Date = "2023-07-21", Remark = "sample of deprecation")]
        [EventSourceDeprecateVersionAttribute(EventsContractType.Consumer, Date = "2023-07-21", Remark = "sample of deprecation")]
        ValueTask ExecuteAsync(int value);

        [EventSourceVersion(2)]
        ValueTask ExecuteAsync(DateTime value);

        [EventSourceVersion(2)]
        ValueTask XAsync(DateTime value);

        [EventSourceVersion(3)]
        ValueTask ExecuteAsync(string value);
        [EventSourceVersion(4)]
        ValueTask ExecuteAsync(TimeSpan value);
        [EventSourceVersion(3)]
        ValueTask NotIncludesAsync(string value);
    }
}



