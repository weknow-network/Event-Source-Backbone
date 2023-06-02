﻿using StackExchange.Redis;
/// <summary>
/// Redis reconnect retry policy
/// </summary>
/// <seealso cref="StackExchange.Redis.IReconnectRetryPolicy" />
public class RedisReconnectRetryPolicy : IReconnectRetryPolicy
{
    /// <summary>
    /// Shoulds the retry.
    /// </summary>
    /// <param name="currentRetryCount">The current retry count.</param>
    /// <param name="timeElapsedMillisecondsSinceLastRetry">The time elapsed milliseconds since last retry.</param>
    /// <returns></returns>
    bool IReconnectRetryPolicy.ShouldRetry(
        long currentRetryCount,
        int timeElapsedMillisecondsSinceLastRetry)
    {
        return timeElapsedMillisecondsSinceLastRetry > 1000;
    }
}