using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent metadata of message (command / event) metadata of
    /// a communication channel (Pub/Sub, Event Source, REST, GraphQL).
    /// It represent the operation's intent or represent event.
    /// </summary>
    [DebuggerDisplay("{Intent} [{MessageId}]: DataType= {DataType}")]
    public sealed class ShardMetadata 
    {
        #region Ctor

        public ShardMetadata(
            Metadata metadata,
            IAsyncDisposable disposeShard)
        {
            Metadata = metadata;
            DisposeShard = disposeShard;
        }

        #endregion // Ctor

        public Metadata Metadata { get; }
        public IAsyncDisposable DisposeShard { get; }
    }
}
