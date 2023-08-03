using Newtonsoft.Json;


namespace EventSourcing.Backbone.UnitTests;

using static IVersionAwareFallbackConsumer.CONSTANTS;
using Entities.Generated;
using static Generated.Entities.VersionAwareFallback;

/// <summary>
/// Test contract
/// </summary>
[EventsContract(EventsContractType.Producer, MinVersion = 1, VersionNaming = VersionNaming.Append)]
[EventsContract(EventsContractType.Consumer, MinVersion = 2, VersionNaming = VersionNaming.AppendUnderscore)]
[Obsolete("This interface is base for code generation, please use ISimpleEventProducer or ISimpleEventConsumer", true)]
public interface IVersionAwareFallback// : IVersionAwareBase
{
    /// <summary>
    /// Consumers the fallback.
    /// Excellent for Migration scenario
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    public static async Task<bool> Fallback(IConsumerInterceptionContext ctx, IVersionAwareFallbackConsumer target)
    {
        Metadata meta = ctx.Context;
        switch (meta)
        {
            case { Operation: nameof(ExecuteAsync), Version: 0, ParamsSignature: DEPRECATED.ExecuteAsync.V0.P_String_Int32 }:
                int? key = await ctx.GetParameterAsync<int>("key");
                int? val = await ctx.GetParameterAsync<int>("value");
                if (val == null)
                    throw new NullReferenceException();
                var entity = new ExecuteAsync_3_String($"{key}:{val}");
                await target.Execute_3Async(ctx.Context, val.ToString()!);
                return true;
            case { Operation: nameof(ExecuteAsync), Version: 1, ParamsSignature: DEPRECATED.ExecuteAsync.V1.P_Int32 }:
                int? val1 = await ctx.GetParameterAsync<int>("value");
                if (val1 == null)
                    throw new NullReferenceException();
                await target.Execute_3Async(ctx.Context, val1.ToString()!);
                return true;
            //case { Operation: nameof(ExecuteAsync), Version: 4, ParamsSignature: DEPRECATED.ExecuteAsync.V3.P_ }:
            //    TimeSpan? val2 = await ctx.GetParameterAsync<TimeSpan>("value");
            //    if (val2 == null)
            //        throw new NullReferenceException();
            //    await target.Execute_3Async(ctx.Context, val2.ToString()!);
            //    return true;
                //default:
                //    await ctx.Context.AckAsync(AckBehavior.OnFallback);
                //    return true;
        }
        return false;
    }

    ValueTask ExecuteAsync(string key, int value);
    [EventSourceVersion(1)]
    ValueTask ExecuteAsync(int value);
    [EventSourceVersion(2)]
    ValueTask ExecuteAsync(DateTime value);
    [EventSourceVersion(3)]
    ValueTask ExecuteAsync(string value);
    [EventSourceVersion(4)]
    [EventSourceDeprecateVersion(EventsContractType.Consumer, Date = "2023-08-02", Remark = "For testing")]
    ValueTask ExecuteAsync(TimeSpan value);
    [EventSourceVersion(3)]
    ValueTask NotIncludesAsync(string value);
}
