using FakeItEasy;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceConsumerApiDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventSourceConsumerBuilder _builder = A.Fake<IEventSourceConsumerBuilder>();

        public EventSourceConsumerApiDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void Build_Raw_Consumerr_Test()
        {
            //ISourceBlock<Ackable<AnnouncementRaw>> consumer =
            //    _builder.Main
            //        .Version("V1-test")
            //        .BuildRaw();
        }

        [Fact]
        public void Build_Personal_Consumer_Test()
        {
            //ISourceBlock<Ackable<Announcement<User>>> consumer =
            //    _builder.Main
            //        .Version("V1-test")
            //        .BuildPersonal<User>();
        }

        [Fact]
        public void Build_Anonymous_Consumer_Test()
        {
            //ISourceBlock<Ackable<AnonymousAnnouncement<Interaction>>> consumer =
            //    _builder.Main
            //        .Version("V1-test")
            //        // TODO: .Filter(m => m.ActionType == "RegisterUser")
            //        .BuildAnonymous<Interaction>();
        }

        [Fact]
        public void Build_Consumer_Test()
        {
            //ISourceBlock<Ackable<Announcement<User, Interaction>>> consumer =
            //    _builder.Main
            //        .Version("V1-test")
            //        .Build<User, Interaction>();
        }
    }
}
