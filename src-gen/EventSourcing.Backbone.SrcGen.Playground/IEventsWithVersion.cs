namespace EventSourcing.Backbone.WebEventTest
{
    [EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
    [EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
    [Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
    public interface IEventsWithVersion
    {
        /// <summary>
        /// Consumers the fallback.
        /// Excellent for Migration scenario
        /// </summary>
        /// <param name="ctx">The context.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static async Task<bool> Fallback(IConsumerInterceptionContext ctx, IEventsWithVersionConsumer target)
        { 
            Metadata meta = ctx.Context;
            switch (meta)
            {
                case { Operation: "ExecuteAsync", Version: 1,  ParamsSignature: "TimeSpan" }:
                    int? val1 = await ctx.GetParameterAsync<int>("value");
                    if (val1 == null)
                        throw new NullReferenceException();
                    await target.Execute3Async(ctx.Context, val1.ToString()!);
                    return true;
                case { Operation: "ExecuteAsync", Version: 2 }:
                    DateTime? val2 = await ctx.GetParameterAsync<DateTime>("value");
                    if (val2 == null)
                        throw new NullReferenceException();
                    await target.Execute3Async(ctx.Context, val2.ToString()!);
                    return true;
                //default:
                //    await ctx.Context.AckAsync(AckBehavior.OnFallback);
                //    return true;
            }
            return false;
        }

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

        //[EventSourceVersion(1, Date = "2023-06-01", Remark = "sample of deprecation")]
        //[EventSourceDeprecateVersionAttribute(EventsContractType.Producer, 3, Date = "2023-07-21", Remark = "sample of deprecation")]
        //[EventSourceDeprecateVersionAttribute(EventsContractType.Consumer, 3, Date = "2023-07-21", Remark = "sample of deprecation")]
        //ValueTask ExecuteAsync(int value);

        [EventSourceVersion(1, Date = "2023-06-03", Remark = "sample of deprecation")]
        ValueTask ExecuteAsync(TimeSpan value, int i);

        [EventSourceVersion(2)]
        ValueTask ExecuteAsync(DateTime value);

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>Some remarks</remarks>
        [EventSourceVersion(2)]
        ValueTask ExecuteAsync(Version value);

        [EventSourceVersion(3)]
        ValueTask ExecuteAsync(string value);

        [EventSourceVersion(2)]
        [EventSourceDeprecateVersionAttribute(EventsContractType.Producer, Date = "2023-07-27", Remark = "sample of deprecation")]
        [EventSourceDeprecateVersionAttribute(EventsContractType.Consumer, Date = "2023-07-28", Remark = "sample of deprecation")]
        ValueTask NotIncludesAsync(string value);
    }
}
