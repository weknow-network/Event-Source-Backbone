using System.Text.Json;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.UnitTests.Entities;

[assembly: GenerateEventSourceBridge(EventSourceGenType.Producer, typeof(ISequenceOperationsProducer), AutoSuffix = true)]
