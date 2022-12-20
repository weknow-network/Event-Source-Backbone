using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
{
    public class InheritGenerationTest
    {
        private readonly ITestOutputHelper _outputHelper;

        public InheritGenerationTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        [Fact(Skip = "not implemented")]
        public void Inherit_ShouldMatch_Test()
        {
            var origin = typeof(ISimpleEventConsumer).GetMethods().Select(m => m.Name).ToArray();
            var inherit = typeof(ISimpleEventTagConsumer).GetMethods().Select(m => m.Name).ToArray();

            Assert.True(origin.SequenceEqual(inherit));
        }

    }
}
