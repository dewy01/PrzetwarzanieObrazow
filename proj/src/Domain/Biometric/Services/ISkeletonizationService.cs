namespace MapEditor.Domain.Biometric.Services;

/// <summary>
/// Service for skeletonization of binary images.
/// Reduces binary objects to their skeleton (medial axis) while preserving topology.
/// </summary>
public interface ISkeletonizationService
{
    /// <summary>
    /// Applies Zhang-Suen thinning algorithm to create a skeleton.
    /// The algorithm iteratively removes pixels from the boundary while preserving connectivity.
    /// </summary>
    /// <param name="matrix">Binary matrix (1 = foreground, 0 = background)</param>
    /// <returns>Skeletonized binary matrix</returns>
    int[,] ZhangSuenThinning(int[,] matrix);

    /// <summary>
    /// Counts the number of iterations needed to complete skeletonization.
    /// Useful for understanding algorithm convergence.
    /// </summary>
    /// <param name="matrix">Binary matrix</param>
    /// <returns>Tuple with skeletonized matrix and iteration count</returns>
    (int[,] skeleton, int iterations) ZhangSuenWithIterations(int[,] matrix);

    /// <summary>
    /// Calculates skeleton metrics: total skeleton pixels, branches, endpoints.
    /// </summary>
    /// <param name="skeleton">Skeletonized binary matrix</param>
    /// <returns>Dictionary with metrics</returns>
    Dictionary<string, int> CalculateSkeletonMetrics(int[,] skeleton);

    /// <summary>
    /// Applies K3M (Keh-Chin Morphological) thinning algorithm.
    /// A fast morphological thinning algorithm that uses a lookup table approach.
    /// </summary>
    /// <param name="matrix">Binary matrix (1 = foreground, 0 = background)</param>
    /// <returns>Skeletonized binary matrix</returns>
    int[,] K3MThinning(int[,] matrix);

    /// <summary>
    /// K3M thinning with iteration count.
    /// </summary>
    /// <param name="matrix">Binary matrix</param>
    /// <returns>Tuple with skeletonized matrix and iteration count</returns>
    (int[,] skeleton, int iterations) K3MWithIterations(int[,] matrix);
}
