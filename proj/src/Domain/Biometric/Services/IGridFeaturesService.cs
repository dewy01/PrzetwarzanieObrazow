using MapEditor.Domain.Biometric.ValueObjects;

namespace MapEditor.Domain.Biometric.Services;

/// <summary>
/// Service for calculating grid features and workspace metrics.
/// </summary>
public interface IGridFeaturesService
{
    /// <summary>
    /// Calculate grid features from workspace and biometric analysis results.
    /// </summary>
    GridFeatures CalculateGridFeatures(
        int width,
        int height,
        int squareCount,
        int entityCount,
        List<(int x, int y)>? endpoints,
        List<(int x, int y)>? bifurcations,
        List<(int x, int y)>? crossings,
        int[,]? skeletonMatrix);
}
