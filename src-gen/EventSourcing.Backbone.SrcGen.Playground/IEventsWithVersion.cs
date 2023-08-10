using Microsoft.Extensions.Logging;

namespace EventSourcing.Backbone.WebEventTest;

using System.Data;

using Generated.EventsWithVersion;
using static Generated.EventsWithVersionSignatures;



[EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
[EventsContract(EventsContractType.Consumer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
#pragma warning disable S1133 // Deprecated code should be removed
[Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer")]
#pragma warning restore S1133 
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

        if (ctx.IsMatchExecuteAsync_V0_String_Int32_Deprecated())
        {
            var data = await ctx.GetExecuteAsync_V0_String_Int32_DeprecatedAsync();
            await target.Execute3Async(consumerContext, $"{data.key}-{data.value}");
            await ctx.AckAsync();
            return true;
        }
        if (ctx.IsMatchExecuteAsync_V2_Boolean_Deprecated())
        {
            var data = await ctx.GetExecuteAsync_V2_Boolean_DeprecatedAsync();
            await target.Execute3Async(consumerContext, data.value.ToString());
            await ctx.AckAsync();
            return true;
        }
        if (ctx.IsMatchNotIncludesAsync_V2_String_Deprecated())
        {
            var data = await ctx.GetNotIncludesAsync_V2_String_DeprecatedAsync();
            await target.Execute3Async(consumerContext, data.value);
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
    [EventSourceDeprecateAttribute(EventsContractType.Consumer, Date = "2023-07-28", Remark = "sample of deprecation")]
    ValueTask ExecuteAsync(bool value);

    [EventSourceVersion(3)]
    ValueTask ExecuteAsync(string value);

    [EventSourceVersion(2)]
    [EventSourceDeprecateAttribute(EventsContractType.Producer, Date = "2023-07-27", Remark = "sample of deprecation")]
    [EventSourceDeprecateAttribute(EventsContractType.Consumer, Date = "2023-07-28", Remark = "sample of deprecation")]
    ValueTask NotIncludesAsync(string value);
}
