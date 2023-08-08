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

    IDisposable ILogger.BeginScope<TState>(TState state) => null;

    bool ILogger.IsEnabled(LogLevel logLevel) => true;

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _outputHelper.WriteLine($"{logLevel.ToString().ToUpper()}: {formatter(state, exception)}");
    }
}
