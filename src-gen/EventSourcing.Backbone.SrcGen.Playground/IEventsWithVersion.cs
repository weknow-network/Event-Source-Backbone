#pragma warning disable S1133 // Obsolete

namespace EventSourcing.Backbone.WebEventTest;

using EventSourcing.Backbone;

using Generated.EventsWithVersion;

using Microsoft.Extensions.Logging;

[EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
[EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
[Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer")]
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
        ILogger logger = ctx.Logger;
        ConsumerContext consumerContext = ctx.Context;
        Metadata meta = consumerContext.Metadata;
        //switch (meta.Signature.ToString())
        //{
        //    case Generated.EventsWithVersion.CONSTANTS.ACTIVE.ExecuteAsync.V1.P_:
        //        break;
        //}
        var (succeed1, data1) = await ctx.TryGetExecuteAsync_V0_String_Int32_DeprecatedAsync();
        if (succeed1)
        {
            await target.Execute3Async(consumerContext, data1!.value.ToString());
            await ctx.AckAsync();
            return true;
        }
        var (succeed2, data2) = await ctx.TryGetExecuteAsync_V2_Boolean_DeprecatedAsync();
        if (succeed2)
        {
            await target.Execute3Async(consumerContext, data2!.value.ToString());
            await ctx.AckAsync();
            return true;
        }
        var (succeed3, data3) = await ctx.TryGetNotIncludesAsync_V2_String_DeprecatedAsync();
        if (succeed3)
        {
            await target.Execute3Async(consumerContext, data3!.value);
            await ctx.AckAsync();
            return true;
        }
        logger.LogWarning("Fallback didn't handle: {uri}, {signature}", meta.Uri, meta.Signature);
        //await ctx.CancelAsync();
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

    /// <summary>
    /// Executes the asynchronous.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="i">The i.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Executes the asynchronous.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    /// <remarks>Some remarks</remarks>
    [EventSourceVersion(2)]
    [EventSourceDeprecateVersionAttribute(EventsContractType.Consumer, Date = "2023-07-28", Remark = "sample of deprecation")]
    ValueTask ExecuteAsync(bool value);

    [EventSourceVersion(3)]
    ValueTask ExecuteAsync(string value);

    [EventSourceVersion(2)]
    [EventSourceDeprecateVersionAttribute(EventsContractType.Producer, Date = "2023-07-27", Remark = "sample of deprecation")]
    [EventSourceDeprecateVersionAttribute(EventsContractType.Consumer, Date = "2023-07-28", Remark = "sample of deprecation")]
    ValueTask NotIncludesAsync(string value);
}
