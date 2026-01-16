using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Service for detecting connected regions (canvases) in a grid.
/// A Canvas is a connected component of filled or empty cells.
/// </summary>
public class CanvasDetectionService
{
    /// <summary>
    /// Detects all canvases (connected components) in the workspace.
    /// </summary>
    /// <param name="workspace">The workspace to analyze</param>
    /// <param name="detectFilled">If true, detects filled regions; if false, detects empty regions</param>
    /// <returns>List of canvases with their cells</returns>
    public List<Canvas> DetectCanvases(Workspace workspace, bool detectFilled = true)
    {
        var canvases = new List<Canvas>();
        var visited = new HashSet<(int x, int y)>();
        var grid = workspace.Grid;

        for (int y = 0; y < grid.Size.Height; y++)
        {
            for (int x = 0; x < grid.Size.Width; x++)
            {
                if (visited.Contains((x, y)))
                    continue;

                var cell = grid.GetCell(new Point(x, y));
                bool isFilled = !cell.IsEmpty || workspace.ActiveGroup.HasSquare(new Point(x, y));

                if (isFilled == detectFilled)
                {
                    // Found unvisited cell of target type - start flood fill
                    var canvasCells = FloodFill(workspace, new Point(x, y), visited, detectFilled);
                    if (canvasCells.Count > 0)
                    {
                        canvases.Add(new Canvas(
                            id: canvases.Count + 1,
                            cells: canvasCells,
                            isFilled: detectFilled
                        ));
                    }
                }
            }
        }

        return canvases;
    }

    /// <summary>
    /// Flood fill algorithm to find all connected cells of the same type.
    /// Uses 4-connectivity (up, down, left, right).
    /// </summary>
    private List<Point> FloodFill(Workspace workspace, Point start, HashSet<(int, int)> visited, bool targetFilled)
    {
        var cells = new List<Point>();
        var queue = new Queue<Point>();
        queue.Enqueue(start);
        visited.Add((start.X, start.Y));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            cells.Add(current);

            // Check 4 neighbors
            var neighbors = new[]
            {
                new Point(current.X - 1, current.Y), // Left
                new Point(current.X + 1, current.Y), // Right
                new Point(current.X, current.Y - 1), // Up
                new Point(current.X, current.Y + 1)  // Down
            };

            foreach (var neighbor in neighbors)
            {
                if (!workspace.Grid.IsValidPosition(neighbor))
                    continue;

                if (visited.Contains((neighbor.X, neighbor.Y)))
                    continue;

                var cell = workspace.Grid.GetCell(neighbor);
                bool isFilled = !cell.IsEmpty || workspace.ActiveGroup.HasSquare(neighbor);

                if (isFilled == targetFilled)
                {
                    visited.Add((neighbor.X, neighbor.Y));
                    queue.Enqueue(neighbor);
                }
            }
        }

        return cells;
    }

    /// <summary>
    /// Gets statistics for detected canvases.
    /// </summary>
    public string GetCanvasStatistics(List<Canvas> canvases)
    {
        if (canvases.Count == 0)
            return "No canvases detected";

        var filled = canvases.Where(c => c.IsFilled).ToList();
        var empty = canvases.Where(c => !c.IsFilled).ToList();

        var stats = $"Total Canvases: {canvases.Count}\n";
        stats += $"  Filled Regions: {filled.Count} ({filled.Sum(c => c.Size)} cells)\n";
        stats += $"  Empty Regions: {empty.Count} ({empty.Sum(c => c.Size)} cells)\n";

        if (canvases.Count > 0)
        {
            stats += $"\nLargest: Canvas #{canvases.OrderByDescending(c => c.Size).First().Id} ({canvases.Max(c => c.Size)} cells)\n";
            stats += $"Smallest: Canvas #{canvases.OrderBy(c => c.Size).First().Id} ({canvases.Min(c => c.Size)} cells)";
        }

        return stats;
    }
}

/// <summary>
/// Represents a connected region (canvas) in the grid.
/// </summary>
public class Canvas
{
    public int Id { get; }
    public List<Point> Cells { get; }
    public bool IsFilled { get; }
    public int Size => Cells.Count;

    public Canvas(int id, List<Point> cells, bool isFilled)
    {
        Id = id;
        Cells = cells ?? new List<Point>();
        IsFilled = isFilled;
    }

    public (int minX, int maxX, int minY, int maxY) GetBoundingBox()
    {
        if (Cells.Count == 0)
            return (0, 0, 0, 0);

        return (
            Cells.Min(p => p.X),
            Cells.Max(p => p.X),
            Cells.Min(p => p.Y),
            Cells.Max(p => p.Y)
        );
    }

    public override string ToString() => $"Canvas #{Id}: {Size} cells ({(IsFilled ? "Filled" : "Empty")})";
}
