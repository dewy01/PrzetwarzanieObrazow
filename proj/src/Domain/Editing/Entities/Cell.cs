using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Cell - Pojedyncza komórka w 2D grid (może zawierać Square lub Entity)
/// </summary>
public class Cell
{
    public Point Position { get; private set; }
    public Square? Square { get; private set; }

    public Cell(Point position)
    {
        Position = position ?? throw new ArgumentNullException(nameof(position));
    }

    public bool IsEmpty => Square == null;

    public void PlaceSquare(Square square)
    {
        if (square.Position != Position)
            throw new InvalidOperationException("Square position must match cell position");

        Square = square;
    }

    public Square? RemoveSquare()
    {
        var removed = Square;
        Square = null;
        return removed;
    }

    public override string ToString() => $"Cell at {Position} [{(IsEmpty ? "Empty" : Square!.Type.ToString())}]";
}
