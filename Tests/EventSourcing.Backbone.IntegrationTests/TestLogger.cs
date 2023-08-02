using Bnaya.Extensions.Common.Disposables;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;



namespace EventSourcing.Backbone;

public class TestLogger : ILogger
{
    private readonly ITestOutputHelper _outputHelper;

    public static ILogger Create(ITestOutputHelper outputHelper) => new TestLogger(outputHelper);

    public TestLogger(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    IDisposable ILogger.BeginScope<TState>(TState state) => Disposable.Empty;

    bool ILogger.IsEnabled(LogLevel logLevel) => true;

#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
#pragma warning disable HAA0102 // Non-overridden virtual method call on value type
    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _outputHelper.WriteLine($"{logLevel.ToString().ToUpper()}: {formatter(state, exception)}");
    }
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
#pragma warning restore HAA0102 
}
