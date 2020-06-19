using FakeItEasy;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone
{
    public class EventSourceApiProducerDesignTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IEventSourceProducerBuilder _builder = A.Fake<IEventSourceProducerBuilder>();

        public EventSourceApiProducerDesignTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Build_Personal_Producer_Test()
        {
            //IEventSourceProducer<User> producer =
            //    _builder.Main
            //        // .setting
            //        .Version("V1-test")
            //        // TODO .Action("RegisterUser", "V1-test")
            //        .MapPersonalSegment<User>()
            //        .Build();

            //await producer.SendAsync(new User());
        }

        [Fact]
        public async Task Build_Custom_Personal_Producer_Test()
        {
            //IEventSourceProducer<User> producer =
            //_builder.CustomSource("custom")
            //    .Version("V1-test")
            //    .MapPersonalSegment<User>()
            //    .Build();

            //await producer.SendAsync(new User());
        }

        [Fact]
        public async Task Build_Custom_Anonymous_Producer_Test()
        {
            //IEventSourceProducer<Interaction> producer =
            //_builder.Main   
            //    .Version("V1-test")
            //    .MapAnonymousSegment<Interaction>()
            //    .Build();

            //// TODO: SendAnonymousAsync
            //await producer.SendAsync(new Interaction());
        }

        [Fact]
        public async Task Build_Personal_Anonymous_Producer_Test()
        {
            //IEventSourceProducer<User, Interaction> producer =
            //    _builder.Main
            //        .Version("V1-test")
            //        .MapPersonalSegment<User>()
            //        .MapAnonymousSegment<Interaction>()
            //        .Build();

            //await producer.SendAsync(new User(), new Interaction());
        }

        [Fact]
        public async Task Build_Anonymous_Personal_Producer_Test()
        {
            //IEventSourceProducer<User, Interaction> producer =
            //    _builder.Main
            //        .Version("V1-test")
            //        .MapAnonymousSegment<Interaction>()
            //        .MapPersonalSegment<User>()
            //        .Build();

            //await producer.SendAsync(new User(), new Interaction());
        }
    }
}
