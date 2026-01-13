using MapEditor.Domain.Biometric.ValueObjects;

namespace MapEditor.Domain.Biometric.Services;

/// <summary>
/// Service for detecting and analyzing connected components (fragments) in binary matrices.
/// </summary>
public interface IFragmentationService
{
    /// <summary>
    /// Detects all connected components in a binary matrix using 8-connectivity.
    /// </summary>
    /// <param name="matrix">The binary matrix (1 = foreground, 0 = background)</param>
    /// <returns>List of detected fragments</returns>
    List<Fragment> DetectFragments(int[,] matrix);

    /// <summary>
    /// Creates a labeled matrix where each pixel is assigned its fragment ID.
    /// Background pixels (0) remain 0, foreground pixels get their fragment ID.
    /// </summary>
    /// <param name="matrix">The binary matrix</param>
    /// <returns>Matrix with fragment labels</returns>
    int[,] CreateLabeledMatrix(int[,] matrix);

    /// <summary>
    /// Calculates statistics about the fragmentation.
    /// </summary>
    /// <param name="fragments">List of fragments</param>
    /// <returns>Dictionary with statistics (FragmentCount, AverageSize, LargestSize, SmallestSize)</returns>
    Dictionary<string, double> CalculateStatistics(List<Fragment> fragments);
}
