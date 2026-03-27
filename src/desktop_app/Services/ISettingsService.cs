using SpotifyNowPlaying.Models;

namespace SpotifyNowPlaying.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }
    void Load();
    void Save();
}
