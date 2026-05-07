using PlayStatus.Models;

namespace PlayStatus.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }
    void Load();
    void Save();
}
