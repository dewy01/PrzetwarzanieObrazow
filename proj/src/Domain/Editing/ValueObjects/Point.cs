namespace MapEditor.Domain.Editing.ValueObjects;

/// <summary>
/// Point - Value Object reprezentujący współrzędne w 2D grid
/// </summary>
public record Point(int X, int Y)
{
    public static Point Zero => new(0, 0);

    public bool IsValid(int maxWidth, int maxHeight)
    {
        return X >= 0 && X < maxWidth && Y >= 0 && Y < maxHeight;
    }

    public override string ToString() => $"({X}, {Y})";
}
