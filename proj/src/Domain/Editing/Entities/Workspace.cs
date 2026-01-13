using System.Linq;
using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Workspace - Główny agregat reprezentujący całą przestrzeń roboczą (Aggregate Root)
/// </summary>
public class Workspace
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Grid2D Grid { get; private set; }
    public List<Group> Groups { get; private set; }
    public IEnumerable<Entity> Entities => Groups.SelectMany(g => g.Entities.Values);
    public DateTime CreatedAt { get; private set; }
    public DateTime ModifiedAt { get; private set; }

    private Group? _activeGroup;

    public Workspace(string name, Size size)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workspace name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Grid = new Grid2D(size);
        Groups = new List<Group>();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;

        // Tworzenie domyślnej grupy
        var defaultGroup = new Group("Default");
        Groups.Add(defaultGroup);
        SetActiveGroup(defaultGroup);
    }

    public Group ActiveGroup => _activeGroup ?? throw new InvalidOperationException("No active group");

    public void SetActiveGroup(Group group)
    {
        if (!Groups.Contains(group))
            throw new ArgumentException("Group does not belong to this workspace", nameof(group));

        if (_activeGroup != null)
            _activeGroup.SetActive(false);

        _activeGroup = group;
        _activeGroup.SetActive(true);
        UpdateModifiedTime();
    }

    public void AddGroup(Group group)
    {
        if (Groups.Any(g => g.Name == group.Name))
            throw new InvalidOperationException($"Group with name '{group.Name}' already exists");

        Groups.Add(group);
        UpdateModifiedTime();
    }

    public void PlaceSquare(Point position, Shared.Enums.SquareType squareType)
    {
        if (!Grid.IsValidPosition(position))
            throw new ArgumentException("Position is outside grid bounds", nameof(position));

        // Store in both the grid (for compatibility) and the active group
        var cell = Grid.GetCell(position);
        var square = new Square(position, squareType);
        cell.PlaceSquare(square);

        // Also store in active group for layer isolation
        ActiveGroup.PlaceSquare(position, squareType);
        UpdateModifiedTime();
    }

    public void RemoveSquare(Point position)
    {
        if (!Grid.IsValidPosition(position))
            throw new ArgumentException("Position is outside grid bounds", nameof(position));

        // Remove from both grid and active group
        var cell = Grid.GetCell(position);
        cell.RemoveSquare();
        ActiveGroup.RemoveSquare(position);
        UpdateModifiedTime();
    }

    public void PlaceEntity(Point position, Shared.Enums.EntityType entityType, string? name = null)
    {
        if (!Grid.IsValidPosition(position))
            throw new ArgumentException("Position is outside grid bounds", nameof(position));

        // Replace entity in active group at this position
        ActiveGroup.PlaceEntity(position, entityType, name);
        UpdateModifiedTime();
    }

    public void RemoveEntity(Point position)
    {
        if (ActiveGroup.HasEntity(position))
        {
            ActiveGroup.RemoveEntity(position);
            UpdateModifiedTime();
        }
    }

    public Entity? GetEntityAt(Point position)
    {
        return ActiveGroup.GetEntity(position);
    }

    public void PlacePreset(Point position, Preset preset)
    {
        if (preset == null)
            throw new ArgumentNullException(nameof(preset));

        // Place all squares from preset at specified position
        foreach (var squareDef in preset.Squares)
        {
            var absolutePosition = squareDef.GetAbsolutePosition(position);

            if (Grid.IsValidPosition(absolutePosition))
            {
                // Place in grid (for compatibility)
                var cell = Grid.GetCell(absolutePosition);
                var square = new Square(absolutePosition, squareDef.Type, squareDef.Rotation);
                cell.PlaceSquare(square);

                // Also place in active group (for rendering and layer isolation)
                ActiveGroup.PlaceSquare(absolutePosition, squareDef.Type, squareDef.Rotation);
            }
        }

        UpdateModifiedTime();
    }

    public List<Square> RemovePresetSquares(Point position, Preset preset)
    {
        if (preset == null)
            throw new ArgumentNullException(nameof(preset));

        var removedSquares = new List<Square>();

        // Remove all squares that would be placed by preset
        foreach (var squareDef in preset.Squares)
        {
            var absolutePosition = squareDef.GetAbsolutePosition(position);

            if (Grid.IsValidPosition(absolutePosition))
            {
                var cell = Grid.GetCell(absolutePosition);
                if (!cell.IsEmpty && cell.Square != null)
                {
                    removedSquares.Add(cell.Square);
                    cell.RemoveSquare();
                }

                // Also remove from active group
                ActiveGroup.RemoveSquare(absolutePosition);
            }
        }

        UpdateModifiedTime();
        return removedSquares;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Workspace name cannot be empty", nameof(newName));

        Name = newName;
        UpdateModifiedTime();
    }

    private void UpdateModifiedTime()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"Workspace: {Name} ({Grid.Size})";
}
