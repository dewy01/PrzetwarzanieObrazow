using MapEditor.Domain.Editing.Entities;

namespace MapEditor.Domain.Editing.Services;

/// <summary>
/// Service do zarządzania Preset (Domain Service Interface)
/// </summary>
public interface IPresetRepository
{
    Task<Preset> LoadAsync(string filePath);
    Task SaveAsync(Preset preset, string filePath);
    Task<bool> ExistsAsync(string filePath);
    Task<List<Preset>> GetAllAsync(string directory);
}
