using BelgianStructuredCommunication;
using Xunit;

namespace Tests;

public class CommunicationTests
{
    [Theory]
    [InlineData("+++ 085 / 1927 / 54115 +++")]
    [InlineData("+++085/1927/54115+++")]
    [InlineData("085192754115")]
    public void GivenWellFormattedCommunication_WhenParse_ThenSuccess(string communicationStr)
    {
        var communication = Communication.Parse(communicationStr);
        
        Assert.Equal((uint)851927541, communication.Digits);
        Assert.Equal(15, communication.CheckSum);
        Assert.True(communication.ValidChecksum);
    }
    
    [Theory]
    [InlineData("+++XXX/5331/03705+++")]
    [InlineData("choucroute")]
    public void GivenFormattingError_WhenParse_ThenThrowException(string badCommunication)
    {
        var ex = Assert.Throws<FormatException>(() => Communication.Parse(badCommunication));
        Assert.Equal("Invalid communication format", ex.Message);
    }

    [Fact]
    public void GivenNull_WhenParse_ThenThrowException()
    {
        Assert.Throws<ArgumentNullException>(() => Communication.Parse(null!));
    }
    
    [Fact]
    public void GivenNull_WhenTryParse_ThenThrowException()
    {
        Assert.Throws<ArgumentNullException>(() => Communication.TryParse(null, out _));
    }

    [Theory]
    [InlineData("+++XXX/5331/03705+++")]
    [InlineData("choucroute")]
    public void GivenFormattingError_WhenTryParse_ThenFail(string badCommunication)
    {
        var parseResult = Communication.TryParse(badCommunication, out var communication);
        
        Assert.False(parseResult);
        Assert.Null(communication);
    }

    [Fact]
    public void GivenBadChecksum_WhenParse_ThenThrowException()
    {
        const string badChecksum = "+++333/5331/03799+++";
        
        var ex = Assert.Throws<InvalidChecksumException>(() => Communication.Parse(badChecksum));
        Assert.Equal("Expected 05 but got 99 for 3335331037", ex.Message);
    }

    [Fact]
    public void GivenBadChecksum_WhenTryParse_ThenSetInvalidChecksumTrue()
    {
        const string badChecksum = "+++333/5331/03799+++";
        
        var parseResult = Communication.TryParse(badChecksum, out var communication);
        
        Assert.True(parseResult);
        Assert.False(communication!.ValidChecksum);
    }

    [Fact]
    public void GivenInvalidDigits_WhenNew_ThenThrowException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Communication("12345678901"));
        Assert.Equal("Digits must be 10 characters (Parameter 'digits')", ex.Message);
    }

    [Fact]
    public void GivenValidDigits_WhenNew_ThenSuccess()
    {
        const uint digits = 0123456789;
        const byte checksum = 39;

        var communication = new Communication(digits);
        
        Assert.Equal(digits, communication.Digits);
        Assert.Equal(checksum, communication.CheckSum);
        Assert.True(communication.ValidChecksum);
        Assert.Equal("+++012/3456/78939+++", communication.ToString());
    }
}