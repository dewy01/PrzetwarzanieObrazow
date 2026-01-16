using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Domain.Editing.ValueObjects;

/// <summary>
/// Selection - Directly selected grid area.
/// Per Specyfikacja.md: Selection represents a rectangular area of the grid
/// that the user has selected for bulk operations.
/// </summary>
public class Selection
{
    /// <summary>
    /// Top-left corner of the selection rectangle
    /// </summary>
    public Point StartPoint { get; private set; }

    /// <summary>
    /// Bottom-right corner of the selection rectangle
    /// </summary>
    public Point EndPoint { get; private set; }

    /// <summary>
    /// All selected points in the rectangle (inclusive)
    /// </summary>
    public IReadOnlyList<Point> SelectedPoints { get; private set; }

    /// <summary>
    /// True if selection is active and visible
    /// </summary>
    public bool IsActive { get; private set; }

    private Selection(Point startPoint, Point endPoint)
    {
        StartPoint = NormalizeStart(startPoint, endPoint);
        EndPoint = NormalizeEnd(startPoint, endPoint);
        SelectedPoints = GeneratePointsInRectangle(StartPoint, EndPoint);
        IsActive = true;
    }

    /// <summary>
    /// Create a selection rectangle from two points
    /// </summary>
    public static Selection CreateFromPoints(Point point1, Point point2)
    {
        return new Selection(point1, point2);
    }

    /// <summary>
    /// Create an empty inactive selection
    /// </summary>
    public static Selection Empty()
    {
        var empty = new Selection(new Point(0, 0), new Point(0, 0));
        empty.IsActive = false;
        return empty;
    }

    /// <summary>
    /// Check if a point is within the selection
    /// </summary>
    public bool Contains(Point point)
    {
        if (!IsActive)
            return false;

        return point.X >= Math.Min(StartPoint.X, EndPoint.X) &&
               point.X <= Math.Max(StartPoint.X, EndPoint.X) &&
               point.Y >= Math.Min(StartPoint.Y, EndPoint.Y) &&
               point.Y <= Math.Max(StartPoint.Y, EndPoint.Y);
    }

    /// <summary>
    /// Get the width of the selection rectangle
    /// </summary>
    public int Width => Math.Abs(EndPoint.X - StartPoint.X) + 1;

    /// <summary>
    /// Get the height of the selection rectangle
    /// </summary>
    public int Height => Math.Abs(EndPoint.Y - StartPoint.Y) + 1;

    /// <summary>
    /// Get the area (total number of cells) in the selection
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// Deselect (make inactive)
    /// </summary>
    public void Clear()
    {
        IsActive = false;
    }

    private static Point NormalizeStart(Point p1, Point p2)
    {
        return new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
    }

    private static Point NormalizeEnd(Point p1, Point p2)
    {
        return new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
    }

    private static IReadOnlyList<Point> GeneratePointsInRectangle(Point start, Point end)
    {
        var points = new List<Point>();
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                points.Add(new Point(x, y));
            }
        }
        return points;
    }

    public override string ToString() =>
        IsActive ? $"Selection [{Width}x{Height}] from {StartPoint} to {EndPoint}" : "Selection [Empty]";
}
