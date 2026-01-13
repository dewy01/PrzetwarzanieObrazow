namespace MapEditor.Domain.Biometric.ValueObjects;

/// <summary>
/// Represents a single connected component (fragment) in a binary matrix.
/// </summary>
public record Fragment
{
    /// <summary>
    /// Unique identifier for this fragment.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Number of pixels in this fragment.
    /// </summary>
    public int PixelCount { get; init; }

    /// <summary>
    /// List of pixel coordinates (x, y) that belong to this fragment.
    /// </summary>
    public List<(int x, int y)> Pixels { get; init; } = new();

    /// <summary>
    /// Bounding box: minimum X coordinate.
    /// </summary>
    public int MinX { get; init; }

    /// <summary>
    /// Bounding box: maximum X coordinate.
    /// </summary>
    public int MaxX { get; init; }

    /// <summary>
    /// Bounding box: minimum Y coordinate.
    /// </summary>
    public int MinY { get; init; }

    /// <summary>
    /// Bounding box: maximum Y coordinate.
    /// </summary>
    public int MaxY { get; init; }

    /// <summary>
    /// Width of the bounding box.
    /// </summary>
    public int Width => MaxX - MinX + 1;

    /// <summary>
    /// Height of the bounding box.
    /// </summary>
    public int Height => MaxY - MinY + 1;

    public override string ToString() =>
        $"Fragment {Id}: {PixelCount} pixels, BBox: ({MinX},{MinY})-({MaxX},{MaxY})";
}
