using FakeItEasy;

using System;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
{
    public class EventKeyTests
    {
        // private const string JSON = "{\"id\":2}";
        // private const string STR = "ABC";
        // private const string STR_AS_JSON = "{\"value\":\"ABC\"}";

        #region  // EventKey_Json_Test

        //[Fact]
        //public void EventKey_Json_Test()
        //{
        //    var doc = JsonDocument.Parse(JSON);
        //    var id = new EventKey(doc.RootElement);

        //    string toString = id.ToString();
        //    string castString = id;
        //    JsonElement json = id;

        //    Assert.Equal(JSON, toString);
        //    Assert.Equal(JSON, castString);
        //    Assert.Equal(2, json.GetProperty("id").GetInt32());
        //}

        #endregion // EventKey_Json_Test

        #region // EventKey_Json_Test

        //[Fact]
        //public void EventKey_string_Test()
        //{
        //    var id = new EventKey(STR);

        //    string toString = id.ToString();
        //    string castString = id;
        //    JsonElement json = id;

        //    Assert.Equal(STR, toString);
        //    Assert.Equal(STR, castString);
        //    var e1 = new EventKey(json);
        //    var candidate = json.ToString();
        //    Assert.Equal(STR_AS_JSON, candidate);
        //}

        #endregion // EventKey_Json_Test
    }
}
