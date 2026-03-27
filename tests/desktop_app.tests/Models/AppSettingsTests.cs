using SpotifyNowPlaying.Models;
using Xunit;

namespace SpotifyNowPlaying.Tests.Models;

public class AppSettingsTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var settings = new AppSettings();

        Assert.Equal(string.Empty, settings.Spotify.ClientId);
        Assert.Equal(string.Empty, settings.Spotify.RefreshToken);
        Assert.Equal(string.Empty, settings.Teams.ClientId);
        Assert.Equal("common", settings.Teams.TenantId);
        Assert.Equal("\U0001f3b5 {artist} - {title}", settings.Format);
        Assert.Equal(10, settings.PollIntervalSeconds);
        Assert.True(settings.Enabled);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var settings = new AppSettings
        {
            Format = "{title} by {artist}",
            PollIntervalSeconds = 30,
            Enabled = false
        };

        settings.Spotify.ClientId = "spotify-id";
        settings.Teams.ClientId = "teams-id";
        settings.Teams.TenantId = "my-tenant";

        Assert.Equal("{title} by {artist}", settings.Format);
        Assert.Equal(30, settings.PollIntervalSeconds);
        Assert.False(settings.Enabled);
        Assert.Equal("spotify-id", settings.Spotify.ClientId);
        Assert.Equal("teams-id", settings.Teams.ClientId);
        Assert.Equal("my-tenant", settings.Teams.TenantId);
    }
}
