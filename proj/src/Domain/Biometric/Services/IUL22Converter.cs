using MapEditor.Domain.Editing.Entities;

namespace MapEditor.Domain.Biometric.Services;

/// <summary>
/// Service for converting workspace to UL22 binary matrix representation.
/// UL22 format: 1 = square present, 0 = empty cell
/// </summary>
public interface IUL22Converter
{
    /// <summary>
    /// Converts a workspace to a binary matrix where 1 represents a square and 0 represents an empty cell.
    /// </summary>
    /// <param name="workspace">The workspace to convert</param>
    /// <returns>A binary matrix (int[row, col]) representing the workspace</returns>
    int[,] ConvertToUL22(Workspace workspace);

    /// <summary>
    /// Gets the dimensions of the resulting UL22 matrix for a given workspace.
    /// </summary>
    /// <param name="workspace">The workspace to check</param>
    /// <returns>A tuple containing (rows, columns)</returns>
    (int rows, int columns) GetMatrixDimensions(Workspace workspace);
}
