using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Group - Logiczna warstwa grupująca elementy (Domain Entity)
/// Each group is a separate layer that stores its own squares
/// </summary>
public class Group
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public bool IsVisible { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// Squares stored in this group, keyed by position for fast lookup
    /// </summary>
    public Dictionary<Point, Square> Elements { get; private set; }

    /// <summary>
    /// Entities stored in this group, keyed by position to avoid duplicates
    /// </summary>
    public Dictionary<Point, Entity> Entities { get; private set; }

    public Group(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsVisible = true;
        IsActive = false;
        Elements = new Dictionary<Point, Square>();
        Entities = new Dictionary<Point, Entity>();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    public void SetVisible(bool visible)
    {
        IsVisible = visible;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Group name cannot be empty", nameof(newName));

        Name = newName;
    }

    /// <summary>
    /// Place a square in this group at the specified position
    /// </summary>
    public void PlaceSquare(Point position, SquareType squareType)
    {
        var square = new Square(position, squareType);
        Elements[position] = square;
    }

    /// <summary>
    /// Place a square with rotation in this group at the specified position
    /// </summary>
    public void PlaceSquare(Point position, SquareType squareType, int rotation)
    {
        var square = new Square(position, squareType, rotation);
        Elements[position] = square;
    }

    /// <summary>
    /// Remove a square from this group at the specified position
    /// </summary>
    public void RemoveSquare(Point position)
    {
        Elements.Remove(position);
    }

    /// <summary>
    /// Place an entity in this group at the specified position (replaces existing)
    /// </summary>
    public void PlaceEntity(Point position, EntityType entityType, string? name = null)
    {
        Entities[position] = new Entity(position, entityType, name);
    }

    /// <summary>
    /// Remove an entity from this group at the specified position
    /// </summary>
    public void RemoveEntity(Point position)
    {
        Entities.Remove(position);
    }

    /// <summary>
    /// Get an entity from this group at the specified position, or null if not found
    /// </summary>
    public Entity? GetEntity(Point position)
    {
        return Entities.TryGetValue(position, out var entity) ? entity : null;
    }

    /// <summary>
    /// Check if this group has an entity at the specified position
    /// </summary>
    public bool HasEntity(Point position)
    {
        return Entities.ContainsKey(position);
    }

    /// <summary>
    /// Get a square from this group at the specified position, or null if not found
    /// </summary>
    public Square? GetSquare(Point position)
    {
        return Elements.TryGetValue(position, out var square) ? square : null;
    }

    /// <summary>
    /// Check if this group has a square at the specified position
    /// </summary>
    public bool HasSquare(Point position)
    {
        return Elements.ContainsKey(position);
    }

    public override string ToString() => $"Group: {Name} (Active: {IsActive}, Squares: {Elements.Count}, Entities: {Entities.Count})";
}
