namespace EventSource.Backbone.SrcGen.Playground
{
    /// <summary>
    /// Some doc
    /// </summary>
    [GenerateEventSource(EventSourceGenType.Producer)]
    [GenerateEventSource(EventSourceGenType.Consumer)]
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
