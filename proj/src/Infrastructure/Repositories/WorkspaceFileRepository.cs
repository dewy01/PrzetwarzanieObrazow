using System.Linq;
using System.Text.Json;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Infrastructure.Repositories;

/// <summary>
/// Implementacja repozytorium używająca JSON do serializacji Workspace
/// </summary>
public class WorkspaceFileRepository : IWorkspaceRepository
{
    public Task<Workspace> CreateAsync(string name, Size size)
    {
        var workspace = new Workspace(name, size);
        return Task.FromResult(workspace);
    }

    public async Task SaveAsync(Workspace workspace, string filePath)
    {
        try
        {
            var dto = ConvertToDto(workspace);
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to save workspace to {filePath}", ex);
        }
    }

    public async Task<Workspace> LoadAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Workspace file not found: {filePath}");

            var json = await File.ReadAllTextAsync(filePath);
            var dto = JsonSerializer.Deserialize<WorkspaceDto>(json);

            if (dto == null)
                throw new InvalidDataException("Failed to deserialize workspace");

            return ConvertFromDto(dto);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new IOException($"Failed to load workspace from {filePath}", ex);
        }
    }

    public Task<bool> ExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    // DTOs dla serializacji
    private WorkspaceDto ConvertToDto(Workspace workspace)
    {
        var groupDtos = workspace.Groups.Select(g => new GroupDto
        {
            Id = g.Id,
            Name = g.Name,
            IsVisible = g.IsVisible,
            IsActive = g.IsActive,
            Entities = g.Entities.Select(kvp => new EntityDto
            {
                X = kvp.Key.X,
                Y = kvp.Key.Y,
                Type = kvp.Value.Type,
                Name = kvp.Value.Name
            }).ToList(),
            Squares = g.Elements.Select(kvp => new SquareDto
            {
                X = kvp.Key.X,
                Y = kvp.Key.Y,
                Type = kvp.Value.Type,
                Rotation = kvp.Value.Rotation
            }).ToList()
        }).ToList();

        return new WorkspaceDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Width = workspace.Grid.Size.Width,
            Height = workspace.Grid.Size.Height,
            CreatedAt = workspace.CreatedAt,
            ModifiedAt = workspace.ModifiedAt,
            Groups = groupDtos,
            Squares = new() // Keep for backward compatibility but now use groups
        };
    }

    private Workspace ConvertFromDto(WorkspaceDto dto)
    {
        var workspace = new Workspace(dto.Name, new Size(dto.Width, dto.Height));

        // Clear the default group to avoid duplicates
        var defaultGroup = workspace.Groups[0];
        defaultGroup.Elements.Clear();
        defaultGroup.Entities.Clear();

        // Restore groups with their squares
        foreach (var groupDto in dto.Groups)
        {
            Group groupToRestore;

            if (groupDto.Name == "Default")
            {
                // Restore to the existing default group
                groupToRestore = defaultGroup;
            }
            else
            {
                // Create new group
                groupToRestore = new Group(groupDto.Name);
                workspace.AddGroup(groupToRestore);
            }

            // Restore visibility
            groupToRestore.SetVisible(groupDto.IsVisible);

            // Restore squares for this group
            foreach (var squareDto in groupDto.Squares)
            {
                var position = new Point(squareDto.X, squareDto.Y);
                groupToRestore.PlaceSquare(position, squareDto.Type);
            }

            // Restore entities for this group
            if (groupDto.Entities != null)
            {
                foreach (var entityDto in groupDto.Entities)
                {
                    var position = new Point(entityDto.X, entityDto.Y);
                    groupToRestore.PlaceEntity(position, entityDto.Type, entityDto.Name);
                }
            }

            // Set active group
            if (groupDto.IsActive)
                workspace.SetActiveGroup(groupToRestore);
        }

        // For backward compatibility: if no groups but old squares format
        if (dto.Groups.Count == 0 && dto.Squares.Count > 0)
        {
            foreach (var squareDto in dto.Squares)
            {
                workspace.PlaceSquare(
                    new Point(squareDto.X, squareDto.Y),
                    squareDto.Type
                );
            }
        }

        return workspace;
    }

    // Data Transfer Objects
    private class WorkspaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public List<GroupDto> Groups { get; set; } = new();
        public List<SquareDto> Squares { get; set; } = new();
    }

    private class GroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool IsActive { get; set; }
        public List<EntityDto> Entities { get; set; } = new();
        public List<SquareDto> Squares { get; set; } = new();
    }

    private class SquareDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public SquareType Type { get; set; }
        public int Rotation { get; set; }
    }

    private class EntityDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public EntityType Type { get; set; }
        public string? Name { get; set; }
    }
}
