namespace Weknow.EventSource.Backbone.SrcGen.Playground
{
    /// <summary>
    /// Some doc
    /// </summary>
    [GenerateEventSource(EventSourceGenType.Consumer)]
    [GenerateEventSource(EventSourceGenType.Producer)]
    public interface ISample
    {
        /// <summary>
        /// Execute.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        ValueTask ExecAsync(int i, string s);
    }
}
