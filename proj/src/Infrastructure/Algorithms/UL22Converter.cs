using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Implementation of UL22 converter that transforms a workspace into a binary matrix.
/// </summary>
public class UL22Converter : IUL22Converter
{
    /// <inheritdoc/>
    public int[,] ConvertToUL22(Workspace workspace)
    {
        if (workspace == null)
            throw new ArgumentNullException(nameof(workspace));

        var (rows, columns) = GetMatrixDimensions(workspace);
        var matrix = new int[rows, columns];

        var grid = workspace.Grid;

        // Iterate through all cells in the grid
        // According to ALGORITHMS.md specification:
        // - 0 = tło (Background) - empty cells
        // - 1 = obiekt (Map/Square) - cells with squares (to be skeletonized)
        for (int y = 0; y < grid.Size.Height; y++)
        {
            for (int x = 0; x < grid.Size.Width; x++)
            {
                // Check if ANY group has a square at this position
                var position = new MapEditor.Domain.Editing.ValueObjects.Point(x, y);
                bool hasSquare = workspace.Groups.Any(g => g.HasSquare(position));

                if (hasSquare)
                {
                    // Object (square) - marked as 1 (will be skeletonized)
                    matrix[y, x] = 1;
                }
                else
                {
                    // Background (empty cells) - marked as 0
                    matrix[y, x] = 0;
                }
            }
        }

        return matrix;
    }

    /// <inheritdoc/>
    public (int rows, int columns) GetMatrixDimensions(Workspace workspace)
    {
        if (workspace == null)
            throw new ArgumentNullException(nameof(workspace));

        var grid = workspace.Grid;
        return (grid.Size.Height, grid.Size.Width);
    }
}
