using System;


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Bucket storage type
    /// </summary>
    [Flags]
    public enum EventBucketCategories 
    {
        None = 0,
        Segments = 1,
        Interceptions = 2,
        All = Segments | Interceptions
    }
}
