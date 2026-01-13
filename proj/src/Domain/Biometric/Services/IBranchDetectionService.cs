namespace MapEditor.Domain.Biometric.Services;

/// <summary>
/// Service for detecting branches, bifurcations, and junctions in skeletonized images
/// </summary>
public interface IBranchDetectionService
{
    /// <summary>
    /// Detects branch points in a binary skeleton matrix
    /// </summary>
    /// <param name="skeletonMatrix">Binary skeleton matrix (1 = foreground, 0 = background)</param>
    /// <returns>Dictionary with branch type counts and list of branch point coordinates</returns>
    Dictionary<string, object> DetectBranches(int[,] skeletonMatrix);

    /// <summary>
    /// Classifies a specific point in the skeleton
    /// </summary>
    /// <param name="skeletonMatrix">Binary skeleton matrix</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Classification: "Endpoint", "Regular", "Bifurcation", "Crossing", etc.</returns>
    string ClassifyPoint(int[,] skeletonMatrix, int x, int y);

    /// <summary>
    /// Extracts detailed branch information including branch lengths and angles
    /// </summary>
    /// <param name="skeletonMatrix">Binary skeleton matrix</param>
    /// <returns>Detailed branch statistics</returns>
    Dictionary<string, object> AnalyzeBranchStructure(int[,] skeletonMatrix);
}
