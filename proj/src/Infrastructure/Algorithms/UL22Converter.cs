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

        // Iterate through all cells in the grid, but only use active group's squares for conversion
        for (int y = 0; y < grid.Size.Height; y++)
        {
            for (int x = 0; x < grid.Size.Width; x++)
            {
                // Check if the active group has a square at this position
                var position = new MapEditor.Domain.Editing.ValueObjects.Point(x, y);
                if (workspace.ActiveGroup.HasSquare(position))
                {
                    matrix[y, x] = 1;
                }
                // Otherwise it remains 0 (default value)
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
