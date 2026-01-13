using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Preset - Predefiniowana konfiguracja wielu Square (Domain Entity)
/// Reprezentuje wielokrotnie używalny szablon terenu
/// </summary>
public class Preset
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Point OriginPoint { get; private set; }
    public Size Size { get; private set; }
    public List<SquareDefinition> Squares { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Preset(string name, Size size, List<SquareDefinition> squares, Point? originPoint = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Preset name cannot be empty", nameof(name));

        if (size.Width <= 0 || size.Height <= 0)
            throw new ArgumentException("Preset size must be positive", nameof(size));

        Id = Guid.NewGuid();
        Name = name;
        Size = size;
        Squares = squares ?? new List<SquareDefinition>();
        OriginPoint = originPoint ?? new Point(0, 0);
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Preset name cannot be empty", nameof(newName));

        Name = newName;
    }

    public void SetOrigin(Point newOrigin)
    {
        OriginPoint = newOrigin ?? throw new ArgumentNullException(nameof(newOrigin));
    }

    public override string ToString() => $"Preset: {Name} ({Size.Width}x{Size.Height}, {Squares.Count} squares)";
}

/// <summary>
/// Definicja pojedynczego Square w Preset
/// </summary>
public class SquareDefinition
{
    public Point RelativePosition { get; private set; }
    public SquareType Type { get; private set; }
    public int Rotation { get; private set; }

    public SquareDefinition(Point relativePosition, SquareType type, int rotation = 0)
    {
        RelativePosition = relativePosition ?? throw new ArgumentNullException(nameof(relativePosition));
        Type = type;
        Rotation = rotation % 360;
    }

    public Point GetAbsolutePosition(Point presetOrigin)
    {
        return new Point(
            presetOrigin.X + RelativePosition.X,
            presetOrigin.Y + RelativePosition.Y
        );
    }

    public override string ToString() => $"{Type} at {RelativePosition} (rot: {Rotation}°)";
}
