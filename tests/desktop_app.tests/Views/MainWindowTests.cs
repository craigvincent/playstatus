using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using NSubstitute;
using PlayStatus.Models;
using PlayStatus.Services;
using PlayStatus.ViewModels;
using PlayStatus.Views;
using Xunit;

namespace PlayStatus.Tests.Views;

public class MainWindowTests
{
    private static (MainWindow window, MainWindowViewModel vm) CreateWindow()
    {
        var settings = Substitute.For<ISettingsService>();
        settings.Settings.Returns(new AppSettings());
        settings.Settings.Spotify.ClientId = "test-client";
        settings.Settings.Teams.ClientId = "teams-client";
        settings.Settings.Teams.TenantId = "common";

        var spotify = Substitute.For<ISpotifyService>();
        var teams = Substitute.For<ITeamsService>();
        var nowPlaying = Substitute.For<INowPlayingService>();

        var vm = new MainWindowViewModel(settings, spotify, teams, nowPlaying);
        var window = new MainWindow { DataContext = vm };
        window.Show();

        return (window, vm);
    }

    [AvaloniaFact]
    public void Window_ShowsCurrentTrackDisplay()
    {
        var (window, vm) = CreateWindow();

        vm.CurrentTrackDisplay = "Radiohead - Creep";

        var textBlocks = window.GetVisualDescendants().OfType<TextBlock>().ToList();
        var trackBlock = textBlocks.FirstOrDefault(tb => tb.Text == "Radiohead - Creep");

        Assert.NotNull(trackBlock);
    }

    [AvaloniaFact]
    public void Window_ShowsVersionDisplay()
    {
        var (window, vm) = CreateWindow();

        var textBlocks = window.GetVisualDescendants().OfType<TextBlock>().ToList();
        var versionBlock = textBlocks.FirstOrDefault(tb =>
            tb.Text?.StartsWith("Version:", StringComparison.Ordinal) == true);

        Assert.NotNull(versionBlock);
    }

    [AvaloniaFact]
    public void SpotifyClientId_TextBox_BindsTwoWay()
    {
        var (window, vm) = CreateWindow();

        // VM -> View
        var textBoxes = window.GetVisualDescendants().OfType<TextBox>().ToList();
        var spotifyIdBox = textBoxes.FirstOrDefault(tb => tb.Text == "test-client");

        Assert.NotNull(spotifyIdBox);

        // View -> VM
        spotifyIdBox.Text = "new-spotify-id";
        Assert.Equal("new-spotify-id", vm.SpotifyClientId);
    }

    [AvaloniaFact]
    public void TeamsClientId_TextBox_BindsTwoWay()
    {
        var (window, vm) = CreateWindow();

        var textBoxes = window.GetVisualDescendants().OfType<TextBox>().ToList();
        var teamsIdBox = textBoxes.FirstOrDefault(tb => tb.Text == "teams-client");

        Assert.NotNull(teamsIdBox);

        teamsIdBox.Text = "new-teams-id";
        Assert.Equal("new-teams-id", vm.TeamsClientId);
    }

    [AvaloniaFact]
    public void StatusFormat_TextBox_BindsTwoWay()
    {
        var (window, vm) = CreateWindow();

        var textBoxes = window.GetVisualDescendants().OfType<TextBox>().ToList();
        var formatBox = textBoxes.FirstOrDefault(tb => tb.Text == vm.StatusFormat);

        Assert.NotNull(formatBox);

        formatBox.Text = "{title} by {artist}";
        Assert.Equal("{title} by {artist}", vm.StatusFormat);
    }

    [AvaloniaFact]
    public void EnableCheckbox_BindsTwoWay()
    {
        var (window, vm) = CreateWindow();

        var checkBoxes = window.GetVisualDescendants().OfType<CheckBox>().ToList();
        var enableBox = checkBoxes.FirstOrDefault(cb =>
            cb.Content?.ToString() == "Enable automatic updates");

        Assert.NotNull(enableBox);
        Assert.True(enableBox.IsChecked);

        enableBox.IsChecked = false;
        Assert.False(vm.IsEnabled);
    }

    [AvaloniaFact]
    public void ConnectSpotifyButton_IsVisible_WhenNotConnected()
    {
        var (window, vm) = CreateWindow();

        var buttons = window.GetVisualDescendants().OfType<Button>().ToList();
        var connectBtn = buttons.FirstOrDefault(b => b.Content?.ToString() == "Connect"
            && b.IsVisible);

        Assert.NotNull(connectBtn);
    }

    [AvaloniaFact]
    public void DisconnectSpotifyButton_IsVisible_WhenConnected()
    {
        var (window, vm) = CreateWindow();

        vm.IsSpotifyConnected = true;

        // Force layout update
        window.UpdateLayout();

        var buttons = window.GetVisualDescendants().OfType<Button>().ToList();
        var disconnectBtns = buttons.Where(b => b.Content?.ToString() == "Disconnect"
            && b.IsVisible).ToList();

        Assert.NotEmpty(disconnectBtns);
    }

    [AvaloniaFact]
    public void SpotifyStatus_DisplaysInWindow()
    {
        var (window, vm) = CreateWindow();

        vm.SpotifyStatus = "Connected as TestUser";

        var textBlocks = window.GetVisualDescendants().OfType<TextBlock>().ToList();
        var statusBlock = textBlocks.FirstOrDefault(tb => tb.Text == "Connected as TestUser");

        Assert.NotNull(statusBlock);
    }

    [AvaloniaFact]
    public void PollInterval_Slider_Exists()
    {
        var (window, _) = CreateWindow();

        var sliders = window.GetVisualDescendants().OfType<Slider>().ToList();

        Assert.NotEmpty(sliders);
        var slider = sliders.First();
        Assert.Equal(5, slider.Minimum);
        Assert.Equal(60, slider.Maximum);
    }
}
