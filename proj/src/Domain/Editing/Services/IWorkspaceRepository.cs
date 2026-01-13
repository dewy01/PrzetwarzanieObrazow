using MapEditor.Domain.Editing.Entities;

namespace MapEditor.Domain.Editing.Services;

/// <summary>
/// Interfejs repozytorium dla Workspace (Persistence abstraction)
/// </summary>
public interface IWorkspaceRepository
{
    Task<Workspace> CreateAsync(string name, ValueObjects.Size size);
    Task SaveAsync(Workspace workspace, string filePath);
    Task<Workspace> LoadAsync(string filePath);
    Task<bool> ExistsAsync(string filePath);
}
