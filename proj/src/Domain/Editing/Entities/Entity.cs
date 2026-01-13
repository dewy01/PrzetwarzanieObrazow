using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Entity - obiekt nie-terenowy umieszczony na Grid (np. gracz, wróg, checkpoint)
/// </summary>
public class Entity
{
    public Guid Id { get; private set; }
    public Point Position { get; private set; }
    public EntityType Type { get; private set; }
    public string? Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Entity(Point position, EntityType type, string? name = null)
    {
        if (type == EntityType.None)
            throw new ArgumentException("Entity type cannot be None", nameof(type));

        Id = Guid.NewGuid();
        Position = position;
        Type = type;
        Name = name ?? type.ToString();
        CreatedAt = DateTime.UtcNow;
    }

    public void MoveTo(Point newPosition)
    {
        Position = newPosition;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty", nameof(newName));

        Name = newName;
    }

    public override string ToString() => $"{Name} at ({Position.X}, {Position.Y})";
}
