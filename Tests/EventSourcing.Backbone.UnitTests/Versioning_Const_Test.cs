using Xunit;
using Xunit.Abstractions;


namespace EventSourcing.Backbone.UnitTests.Entities;

using Generated;

public class VersionAware_Const_Test
{
    private readonly ITestOutputHelper _outputHelper;

    public VersionAware_Const_Test(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }


    [Fact]
    public void Consumer_Const_Test()
    {
        _outputHelper.WriteLine(VersionAwareMixConstants.DEPRECATED.ExecuteAsync.V0.P_String_Int32.SignatureString);
        Assert.Equal("String,Int32", VersionAwareMixConstants.DEPRECATED.ExecuteAsync.V0.P_String_Int32.SignatureString);
        _outputHelper.WriteLine(VersionAwareMixConstants.ACTIVE.ExecuteAsync.V2.P_DateTime.SignatureString);
        Assert.Equal("DateTime", VersionAwareMixConstants.ACTIVE.ExecuteAsync.V2.P_DateTime.SignatureString);
    }

}
