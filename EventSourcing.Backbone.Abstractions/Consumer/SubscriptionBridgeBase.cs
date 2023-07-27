using EventSourcing.Backbone;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using EventSourcing.Backbone.Building;

namespace EventSourcing.Backbone.Private;

/// <summary>
/// Subscription bridge to a specialized consumer target
/// </summary>
[GeneratedCode("EventSourcing.Backbone.SrcGen", "1.2.151.0")]
public abstract class SubscriptionBridgeBase<T> : ISubscriptionBridge // <T>
                        // where T : class
{
    protected readonly IEnumerable<T> _targets;
    private readonly Func<IConsumerFallback, Task>? _fallback;


    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="targets">The target.</param>
    public SubscriptionBridgeBase(IEnumerable<T> targets) //, Func<IConsumerFallback<T>, Task>? fallback)
    {
        _targets = targets;
        //_fallback = fallback;
    }

    /// <summary>
    /// Handle end target call.
    /// </summary>
    /// <param name="announcement">The announcement.</param>
    /// <param name="consumerBridge">The consumer bridge.</param>
    /// <returns></returns>
    protected abstract Task<bool> OnBridgeAsync(Announcement announcement, IConsumerBridge consumerBridge);

    async Task<bool> ISubscriptionBridge.BridgeAsync(Announcement announcement, IConsumerBridge consumerBridge)
    {
        bool handled = await OnBridgeAsync(announcement, consumerBridge);
        return handled; 
        //if (handled) return true;

        //await _fallback()
        //// toto: fallback
        //return false;
    }

}
