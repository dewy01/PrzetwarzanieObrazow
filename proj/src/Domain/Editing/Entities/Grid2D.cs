using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Grid2D - Dwuwymiarowa siatka komórek (Domain Entity)
/// </summary>
public class Grid2D
{
    private readonly Cell[,] _cells;

    public Size Size { get; private set; }

    public Grid2D(Size size)
    {
        if (!size.IsValid())
            throw new ArgumentException("Invalid grid size", nameof(size));

        Size = size;
        _cells = new Cell[size.Height, size.Width];

        // Inicjalizacja wszystkich komórek
        for (int y = 0; y < size.Height; y++)
        {
            for (int x = 0; x < size.Width; x++)
            {
                _cells[y, x] = new Cell(new Point(x, y));
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        if (!IsValidPosition(x, y))
            throw new ArgumentOutOfRangeException($"Position ({x}, {y}) is out of bounds");

        return _cells[y, x];
    }

    public Cell GetCell(Point position) => GetCell(position.X, position.Y);

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < Size.Width && y >= 0 && y < Size.Height;
    }

    public bool IsValidPosition(Point position) => IsValidPosition(position.X, position.Y);

    public IEnumerable<Cell> GetAllCells()
    {
        for (int y = 0; y < Size.Height; y++)
        {
            for (int x = 0; x < Size.Width; x++)
            {
                yield return _cells[y, x];
            }
        }
    }

    public IEnumerable<Square> GetAllSquares()
    {
        return GetAllCells()
            .Where(c => !c.IsEmpty)
            .Select(c => c.Square!);
    }
}
