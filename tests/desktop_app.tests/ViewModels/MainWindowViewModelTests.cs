using NSubstitute;
using PlayStatus.Models;
using PlayStatus.Services;
using PlayStatus.ViewModels;
using Xunit;

namespace PlayStatus.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly ISettingsService _settings;
    private readonly ISpotifyService _spotify;
    private readonly ITeamsService _teams;
    private readonly INowPlayingService _nowPlaying;
    private readonly MainWindowViewModel _vm;

    public MainWindowViewModelTests()
    {
        _settings = Substitute.For<ISettingsService>();
        _settings.Settings.Returns(new AppSettings
        {
            Format = "\U0001f3b5 {artist} - {title}",
            PollIntervalSeconds = 10,
            Enabled = true
        });
        _settings.Settings.Spotify.ClientId = "test-spotify-id";
        _settings.Settings.Teams.ClientId = "test-teams-id";
        _settings.Settings.Teams.TenantId = "test-tenant";

        _spotify = Substitute.For<ISpotifyService>();
        _teams = Substitute.For<ITeamsService>();
        _nowPlaying = Substitute.For<INowPlayingService>();

        _vm = new MainWindowViewModel(_settings, _spotify, _teams, _nowPlaying);
    }

    [Fact]
    public void Constructor_LoadsSettingsIntoProperties()
    {
        Assert.Equal("test-spotify-id", _vm.SpotifyClientId);
        Assert.Equal("test-teams-id", _vm.TeamsClientId);
        Assert.Equal("test-tenant", _vm.TeamsTenantId);
        Assert.Equal("\U0001f3b5 {artist} - {title}", _vm.StatusFormat);
        Assert.Equal(10, _vm.PollIntervalSeconds);
        Assert.True(_vm.IsEnabled);
    }

    [Fact]
    public void Constructor_DefaultState_NotConnected()
    {
        Assert.False(_vm.IsSpotifyConnected);
        Assert.Equal("Not connected", _vm.SpotifyStatus);
        Assert.False(_vm.IsTeamsConnected);
        Assert.Equal("Not connected", _vm.TeamsStatus);
        Assert.Equal("Nothing playing", _vm.CurrentTrackDisplay);
    }

    [Fact]
    public async Task ConnectSpotify_Success_UpdatesStatus()
    {
        _spotify.ConnectAsync().Returns(true);
        _spotify.ConnectedUser.Returns("TestUser");

        await _vm.ConnectSpotifyCommand.ExecuteAsync(null);

        Assert.True(_vm.IsSpotifyConnected);
        Assert.Equal("Connected as TestUser", _vm.SpotifyStatus);
        _settings.Received().Save();
    }

    [Fact]
    public async Task ConnectSpotify_Failure_ShowsFailedStatus()
    {
        _spotify.ConnectAsync().Returns(false);

        await _vm.ConnectSpotifyCommand.ExecuteAsync(null);

        Assert.False(_vm.IsSpotifyConnected);
        Assert.Equal("Connection failed", _vm.SpotifyStatus);
    }

    [Fact]
    public void DisconnectSpotify_ClearsState()
    {
        _vm.DisconnectSpotifyCommand.Execute(null);

        _spotify.Received().Disconnect();
        Assert.False(_vm.IsSpotifyConnected);
        Assert.Equal("Not connected", _vm.SpotifyStatus);
    }

    [Fact]
    public async Task ConnectTeams_Success_UpdatesStatus()
    {
        _teams.ConnectAsync().Returns(true);
        _teams.ConnectedUser.Returns("TeamUser");

        await _vm.ConnectTeamsCommand.ExecuteAsync(null);

        Assert.True(_vm.IsTeamsConnected);
        Assert.Equal("Connected as TeamUser", _vm.TeamsStatus);
        _settings.Received().Save();
    }

    [Fact]
    public async Task ConnectTeams_Failure_ShowsFailedStatus()
    {
        _teams.ConnectAsync().Returns(false);

        await _vm.ConnectTeamsCommand.ExecuteAsync(null);

        Assert.False(_vm.IsTeamsConnected);
        Assert.Equal("Connection failed", _vm.TeamsStatus);
    }

    [Fact]
    public async Task DisconnectTeams_ClearsState()
    {
        await _vm.DisconnectTeamsCommand.ExecuteAsync(null);

        await _teams.Received().DisconnectAsync();
        Assert.False(_vm.IsTeamsConnected);
        Assert.Equal("Not connected", _vm.TeamsStatus);
    }

    [Fact]
    public void StatusFormatChanged_SavesSettings()
    {
        _settings.ClearReceivedCalls();

        _vm.StatusFormat = "{title} by {artist}";

        _settings.Received().Save();
    }

    [Fact]
    public void PollIntervalChanged_SavesAndRestartsIfRunning()
    {
        _nowPlaying.IsRunning.Returns(true);
        _settings.ClearReceivedCalls();

        _vm.PollIntervalSeconds = 30;

        _settings.Received().Save();
        _nowPlaying.Received().Restart();
    }

    [Fact]
    public void PollIntervalChanged_DoesNotRestartIfNotRunning()
    {
        _nowPlaying.IsRunning.Returns(false);
        _settings.ClearReceivedCalls();

        _vm.PollIntervalSeconds = 30;

        _settings.Received().Save();
        _nowPlaying.DidNotReceive().Restart();
    }

    [Fact]
    public void IsEnabledChanged_ToFalse_StopsService()
    {
        _settings.ClearReceivedCalls();

        _vm.IsEnabled = false;

        _settings.Received().Save();
        _nowPlaying.Received().Stop();
    }

    [Fact]
    public void IsEnabledChanged_ToTrue_StartsServiceWhenBothConnected()
    {
        // Make both connected first
        _vm.IsEnabled = false; // set to false first so change to true triggers
        _settings.ClearReceivedCalls();
        _nowPlaying.ClearReceivedCalls();

        // Simulate connected state
        typeof(MainWindowViewModel)
            .GetProperty(nameof(MainWindowViewModel.IsSpotifyConnected))!
            .SetValue(_vm, true);
        typeof(MainWindowViewModel)
            .GetProperty(nameof(MainWindowViewModel.IsTeamsConnected))!
            .SetValue(_vm, true);

        _vm.IsEnabled = true;

        _nowPlaying.Received().Start();
    }

    [Fact]
    public async Task TryRestoreConnections_BothSucceed_StartsService()
    {
        _spotify.TryRestoreConnectionAsync().Returns(true);
        _spotify.ConnectedUser.Returns("SpotifyUser");
        _teams.TryRestoreConnectionAsync().Returns(true);
        _teams.ConnectedUser.Returns("TeamsUser");

        await _vm.TryRestoreConnectionsAsync();

        Assert.True(_vm.IsSpotifyConnected);
        Assert.Equal("Connected as SpotifyUser", _vm.SpotifyStatus);
        Assert.True(_vm.IsTeamsConnected);
        Assert.Equal("Connected as TeamsUser", _vm.TeamsStatus);
        _nowPlaying.Received().Start();
    }

    [Fact]
    public async Task TryRestoreConnections_SpotifyFails_DoesNotStartService()
    {
        _spotify.TryRestoreConnectionAsync().Returns(false);
        _teams.TryRestoreConnectionAsync().Returns(true);
        _teams.ConnectedUser.Returns("TeamsUser");

        await _vm.TryRestoreConnectionsAsync();

        Assert.False(_vm.IsSpotifyConnected);
        Assert.True(_vm.IsTeamsConnected);
        _nowPlaying.DidNotReceive().Start();
    }

    [Fact]
    public async Task TryRestoreConnections_BothFail_DoesNotStartService()
    {
        _spotify.TryRestoreConnectionAsync().Returns(false);
        _teams.TryRestoreConnectionAsync().Returns(false);

        await _vm.TryRestoreConnectionsAsync();

        Assert.False(_vm.IsSpotifyConnected);
        Assert.False(_vm.IsTeamsConnected);
        _nowPlaying.DidNotReceive().Start();
    }

    [Fact]
    public void TrackChanged_UpdatesCurrentTrackDisplay()
    {
        // Capture the event handler that the VM subscribes on construction
        _nowPlaying.TrackChanged += Raise.Event<Action<TrackInfo?>>(
            new TrackInfo("Artist", "Song"));

        Assert.Equal("Artist - Song", _vm.CurrentTrackDisplay);
    }

    [Fact]
    public void TrackChanged_Null_ShowsNothingPlaying()
    {
        _nowPlaying.TrackChanged += Raise.Event<Action<TrackInfo?>>(
            (TrackInfo?)null);

        Assert.Equal("Nothing playing", _vm.CurrentTrackDisplay);
    }

    [Fact]
    public void StatusUpdated_UpdatesLastUpdateStatus()
    {
        _nowPlaying.StatusUpdated += Raise.Event<Action<string>>("Updated: \U0001f3b5 Artist - Song");

        Assert.Equal("Updated: \U0001f3b5 Artist - Song", _vm.LastUpdateStatus);
    }
}
