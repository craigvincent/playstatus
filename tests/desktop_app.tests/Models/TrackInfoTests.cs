using PlayStatus.Models;
using Xunit;

namespace PlayStatus.Tests.Models;

public class TrackInfoTests
{
    [Fact]
    public void Format_ReplacesArtistAndTitle()
    {
        var track = new TrackInfo("Radiohead", "Creep");
        var result = track.Format("{artist} - {title}");
        Assert.Equal("Radiohead - Creep", result);
    }

    [Fact]
    public void Format_WithEmoji()
    {
        var track = new TrackInfo("Daft Punk", "Around the World");
        var result = track.Format("\U0001f3b5 {artist} - {title}");
        Assert.Equal("\U0001f3b5 Daft Punk - Around the World", result);
    }

    [Fact]
    public void Format_OnlyArtist()
    {
        var track = new TrackInfo("Nirvana", "Smells Like Teen Spirit");
        var result = track.Format("{artist}");
        Assert.Equal("Nirvana", result);
    }

    [Fact]
    public void Format_OnlyTitle()
    {
        var track = new TrackInfo("Nirvana", "Smells Like Teen Spirit");
        var result = track.Format("{title}");
        Assert.Equal("Smells Like Teen Spirit", result);
    }

    [Fact]
    public void Format_NoPlaceholders_ReturnsTemplateUnchanged()
    {
        var track = new TrackInfo("Artist", "Title");
        var result = track.Format("Now playing");
        Assert.Equal("Now playing", result);
    }

    [Fact]
    public void Format_EmptyArtistAndTitle()
    {
        var track = new TrackInfo("", "");
        var result = track.Format("{artist} - {title}");
        Assert.Equal(" - ", result);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new TrackInfo("Artist", "Title");
        var b = new TrackInfo("Artist", "Title");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = new TrackInfo("Artist A", "Title");
        var b = new TrackInfo("Artist B", "Title");
        Assert.NotEqual(a, b);
    }
}
