using System.Diagnostics.CodeAnalysis;
using SpotifyNowPlaying.Models;

namespace SpotifyNowPlaying.Services;

public interface INowPlayingService
{
    TrackInfo? CurrentTrack { get; }
    bool IsRunning { get; }
    event Action<TrackInfo?>? TrackChanged;
    event Action<string>? StatusUpdated;
    void Start();
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches existing API")]
    void Stop();
    void Restart();
}
