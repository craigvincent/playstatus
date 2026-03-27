using SpotifyNowPlaying.Models;

namespace SpotifyNowPlaying.Services;

public interface ISpotifyService
{
    bool IsConnected { get; }
    string? ConnectedUser { get; }
    Task<bool> TryRestoreConnectionAsync();
    Task<bool> ConnectAsync();
    void Disconnect();
    Task<TrackInfo?> GetCurrentlyPlayingAsync();
}
