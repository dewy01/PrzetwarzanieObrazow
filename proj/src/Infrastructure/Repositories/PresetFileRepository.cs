using System.Text.Json;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Infrastructure.Repositories;

/// <summary>
/// Implementacja repozytorium używająca JSON do serializacji Preset
/// </summary>
public class PresetFileRepository : IPresetRepository
{
    public async Task<Preset> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Preset file not found: {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var dto = JsonSerializer.Deserialize<PresetDto>(json)
            ?? throw new InvalidOperationException("Failed to deserialize preset");

        return ConvertFromDto(dto);
    }

    public async Task SaveAsync(Preset preset, string filePath)
    {
        var dto = ConvertToDto(preset);
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    public Task<bool> ExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<List<Preset>> GetAllAsync(string directory)
    {
        if (!Directory.Exists(directory))
            return new List<Preset>();

        var files = Directory.GetFiles(directory, "*.preset.json");
        var presets = new List<Preset>();

        foreach (var file in files)
        {
            try
            {
                presets.Add(await LoadAsync(file));
            }
            catch
            {
                // Skip invalid preset files
            }
        }

        return presets;
    }

    // DTOs dla serializacji
    private PresetDto ConvertToDto(Preset preset)
    {
        return new PresetDto
        {
            Id = preset.Id,
            Name = preset.Name,
            Width = preset.Size.Width,
            Height = preset.Size.Height,
            OriginX = preset.OriginPoint.X,
            OriginY = preset.OriginPoint.Y,
            CreatedAt = preset.CreatedAt,
            Squares = preset.Squares.Select(s => new SquareDefinitionDto
            {
                X = s.RelativePosition.X,
                Y = s.RelativePosition.Y,
                Type = s.Type,
                Rotation = s.Rotation
            }).ToList(),
            Entities = preset.Entities.Select(e => new EntityDefinitionDto
            {
                X = e.RelativePosition.X,
                Y = e.RelativePosition.Y,
                Type = e.Type,
                Name = e.Name
            }).ToList()
        };
    }

    private Preset ConvertFromDto(PresetDto dto)
    {
        var size = new Size(dto.Width, dto.Height);
        var origin = new Point(dto.OriginX, dto.OriginY);
        var squares = dto.Squares.Select(s => new SquareDefinition(
            new Point(s.X, s.Y),
            s.Type,
            s.Rotation
        )).ToList();
        var entities = (dto.Entities ?? new List<EntityDefinitionDto>()).Select(e => new EntityDefinition(
            new Point(e.X, e.Y),
            e.Type,
            e.Name
        )).ToList();

        return new Preset(dto.Name, size, squares, entities, origin);
    }

    // Data Transfer Objects
    private class PresetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SquareDefinitionDto> Squares { get; set; } = new();
        public List<EntityDefinitionDto>? Entities { get; set; } = new();
    }

    private class SquareDefinitionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public SquareType Type { get; set; }
        public int Rotation { get; set; }
    }

    private class EntityDefinitionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public EntityType Type { get; set; }
        public string? Name { get; set; }
    }
}
