namespace SpotifyNowPlaying.Services;

public interface ITeamsService
{
    bool IsConnected { get; }
    string? ConnectedUser { get; }
    Task<bool> TryRestoreConnectionAsync();
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    Task SetStatusMessageAsync(string message);
    Task ClearStatusMessageAsync();
}
