using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Square - Podstawowa jednostka terenu w Workspace (Domain Entity)
/// </summary>
public class Square
{
    public Guid Id { get; private set; }
    public Point Position { get; private set; }
    public SquareType Type { get; private set; }
    public int Rotation { get; private set; } // 0, 90, 180, 270

    public Square(Point position, SquareType type, int rotation = 0)
    {
        Id = Guid.NewGuid();
        Position = position ?? throw new ArgumentNullException(nameof(position));
        Type = type;
        Rotation = rotation % 360;
    }

    public void ChangeType(SquareType newType)
    {
        Type = newType;
    }

    public void Rotate(int degrees)
    {
        Rotation = (Rotation + degrees) % 360;
    }

    public override string ToString() => $"Square[{Type}] at {Position}";
}
