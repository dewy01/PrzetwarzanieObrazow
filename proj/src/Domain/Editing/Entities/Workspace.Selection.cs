using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Domain.Editing.Entities;

/// <summary>
/// Partial Workspace: Selection management
/// </summary>
public partial class Workspace
{
    /// <summary>
    /// Create a new selection rectangle
    /// Hold Shift and drag in GridCanvas to set selection
    /// </summary>
    public void SetSelection(Point startPoint, Point endPoint)
    {
        Selection = Selection.CreateFromPoints(startPoint, endPoint);
        UpdateModifiedTime();
    }

    /// <summary>
    /// Clear the current selection
    /// </summary>
    public void ClearSelection()
    {
        Selection = Selection.Empty();
        UpdateModifiedTime();
    }

    /// <summary>
    /// Delete all squares in the selection from the active group
    /// </summary>
    public int DeleteSelectedSquares()
    {
        if (!Selection.IsActive)
            return 0;

        int deletedCount = 0;
        foreach (var point in Selection.SelectedPoints)
        {
            if (ActiveGroup.Elements.ContainsKey(point))
            {
                ActiveGroup.Elements.Remove(point);
                Grid.GetCell(point).RemoveSquare();
                deletedCount++;
            }
        }

        if (deletedCount > 0)
            UpdateModifiedTime();

        return deletedCount;
    }

    /// <summary>
    /// Fill all cells in the selection with the given square type and rotation in the active group.
    /// Ensures both group elements and grid cells are updated for consistency.
    /// </summary>
    public int FillSelectedSquares(MapEditor.Domain.Shared.Enums.SquareType type, int rotation = 0)
    {
        if (!Selection.IsActive)
            return 0;

        int placed = 0;
        foreach (var point in Selection.SelectedPoints)
        {
            if (!Grid.IsValidPosition(point))
                continue;

            var squareForGroup = new Square(point, type, rotation);
            ActiveGroup.Elements[point] = squareForGroup;

            var cell = Grid.GetCell(point);
            var squareForGrid = new Square(point, type, rotation);
            cell.PlaceSquare(squareForGrid);
            placed++;
        }

        if (placed > 0)
            UpdateModifiedTime();

        return placed;
    }

    /// <summary>
    /// Move all squares within the current selection by a given offset.
    /// Only affects the active group. Returns the number of moved squares.
    /// </summary>
    public int MoveSelectedSquares(int dx, int dy)
    {
        if (!Selection.IsActive)
            return 0;

        // Collect squares to move
        var selectedPositions = new HashSet<MapEditor.Domain.Editing.ValueObjects.Point>(Selection.SelectedPoints);
        var toMove = ActiveGroup.Elements
            .Where(kvp => Selection.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        if (!toMove.Any())
            return 0;

        // Validate new positions and collisions (do not overwrite non-selected squares)
        var moveMap = new List<(MapEditor.Domain.Editing.ValueObjects.Point oldPos, MapEditor.Domain.Editing.ValueObjects.Point newPos, Square square)>();
        foreach (var square in toMove)
        {
            var oldPos = square.Position;
            var newPos = new MapEditor.Domain.Editing.ValueObjects.Point(oldPos.X + dx, oldPos.Y + dy);
            if (!Grid.IsValidPosition(newPos))
                return 0; // Abort move if any would go out of bounds

            // Collision: destination occupied by a square not in selection
            if (ActiveGroup.Elements.ContainsKey(newPos) && !selectedPositions.Contains(newPos))
                return 0; // Abort move on collision

            moveMap.Add((oldPos, newPos, square));
        }

        // Perform move in two phases to avoid transient conflicts
        int moved = 0;
        // Phase 1: remove all old squares from active group and grid
        foreach (var (oldPos, _, _) in moveMap)
        {
            ActiveGroup.Elements.Remove(oldPos);
            Grid.GetCell(oldPos).RemoveSquare();
        }
        // Phase 2: place new squares
        foreach (var (_, newPos, square) in moveMap)
        {
            var newSquare = new Square(newPos, square.Type, square.Rotation);
            ActiveGroup.Elements[newPos] = newSquare;
            Grid.GetCell(newPos).PlaceSquare(new Square(newPos, square.Type, square.Rotation));
            moved++;
        }

        // Offset selection rectangle as well
        OffsetSelection(dx, dy);
        UpdateModifiedTime();
        return moved;
    }

    /// <summary>
    /// Offset the current selection rectangle by dx, dy.
    /// </summary>
    public void OffsetSelection(int dx, int dy)
    {
        if (!Selection.IsActive)
            return;

        var start = new MapEditor.Domain.Editing.ValueObjects.Point(Selection.StartPoint.X + dx, Selection.StartPoint.Y + dy);
        var end = new MapEditor.Domain.Editing.ValueObjects.Point(Selection.EndPoint.X + dx, Selection.EndPoint.Y + dy);
        Selection = Selection.CreateFromPoints(start, end);
    }

    /// <summary>
    /// Select all cells in the grid
    /// </summary>
    public void SelectAll()
    {
        var topLeft = new Point(0, 0);
        var bottomRight = new Point(Grid.Size.Width - 1, Grid.Size.Height - 1);
        SetSelection(topLeft, bottomRight);
    }
}
