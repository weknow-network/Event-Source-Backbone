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

        private ShardMetadata(
            string partition,
            string shard,
            IAsyncDisposable disposeShard)
        {
            Partition = partition;
            Shard = shard;
            DisposeShard = disposeShard;
        }

        #endregion // Ctor

        public string Partition { get; }
        public string Shard { get; }
        public IAsyncDisposable DisposeShard { get; }
    }
}
