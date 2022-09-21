namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IConsumerOriginFilterBuilder<T>
    {
        #region OriginFilter

        /// <summary>
        /// Filter listening by origin.
        /// </summary>
        /// <param name="originFilter">The origin filter.</param>
        /// <returns></returns>
        T OriginFilter(MessageOrigin originFilter = MessageOrigin.Original);

        #endregion // OriginFilter

    }
}
