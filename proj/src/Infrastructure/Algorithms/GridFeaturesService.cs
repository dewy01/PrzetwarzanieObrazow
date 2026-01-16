using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Biometric.ValueObjects;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Implementation of grid features calculation service.
/// </summary>
public class GridFeaturesService : IGridFeaturesService
{
    /// <inheritdoc/>
    public GridFeatures CalculateGridFeatures(
        int width,
        int height,
        int squareCount,
        int entityCount,
        List<(int x, int y)>? endpoints,
        List<(int x, int y)>? bifurcations,
        List<(int x, int y)>? crossings,
        int[,]? skeletonMatrix)
    {
        var features = new GridFeatures
        {
            GridWidth = width,
            GridHeight = height,
            TotalSquareCount = squareCount,
            TotalEntityCount = entityCount,
            EndpointCount = endpoints?.Count ?? 0,
            BifurcationCount = bifurcations?.Count ?? 0,
            CrossingCount = crossings?.Count ?? 0,
            TotalBranchPoints = (endpoints?.Count ?? 0) + (bifurcations?.Count ?? 0) + (crossings?.Count ?? 0),
            TotalSkeletonPixels = CountSkeletonPixels(skeletonMatrix),
        };

        // Calculate derived metrics
        CalculateDensityMetrics(features, skeletonMatrix);
        CalculateDistanceMetrics(features, endpoints, bifurcations);
        CalculateComplexityScore(features);

        return features;
    }

    private int CountSkeletonPixels(int[,]? skeleton)
    {
        if (skeleton == null)
            return 0;

        int count = 0;
        int rows = skeleton.GetLength(0);
        int cols = skeleton.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (skeleton[y, x] == 1)
                    count++;
            }
        }

        return count;
    }

    private void CalculateDensityMetrics(GridFeatures features, int[,]? skeleton)
    {
        if (skeleton == null || features.TotalSkeletonPixels == 0)
        {
            features.BranchDensity = 0;
            features.BranchComplexity = 0;
            return;
        }

        // Branch density: branch points per 100 skeleton pixels
        features.BranchDensity = (features.TotalBranchPoints * 100.0) / features.TotalSkeletonPixels;

        // Branch complexity: combination of branch diversity
        // Bifurcations are more complex than endpoints, crossings most complex
        double complexityFactors = (features.EndpointCount * 1.0) +
                                   (features.BifurcationCount * 2.0) +
                                   (features.CrossingCount * 3.0);

        features.BranchComplexity = features.TotalBranchPoints > 0
            ? complexityFactors / features.TotalBranchPoints
            : 0;
    }

    private void CalculateDistanceMetrics(
        GridFeatures features,
        List<(int x, int y)>? endpoints,
        List<(int x, int y)>? bifurcations)
    {
        // Calculate average distance from branch points to grid center
        double centerX = features.GridWidth / 2.0;
        double centerY = features.GridHeight / 2.0;

        if (endpoints != null && endpoints.Count > 0)
        {
            double sumDistance = 0;
            foreach (var ep in endpoints)
            {
                double dx = ep.x - centerX;
                double dy = ep.y - centerY;
                sumDistance += Math.Sqrt(dx * dx + dy * dy);
            }
            features.AverageEndpointDistance = sumDistance / endpoints.Count;
        }

        if (bifurcations != null && bifurcations.Count > 0)
        {
            double sumDistance = 0;
            foreach (var bf in bifurcations)
            {
                double dx = bf.x - centerX;
                double dy = bf.y - centerY;
                sumDistance += Math.Sqrt(dx * dx + dy * dy);
            }
            features.AverageBifurcationDistance = sumDistance / bifurcations.Count;
        }
    }

    private void CalculateComplexityScore(GridFeatures features)
    {
        // Complexity score combines multiple factors
        // Formula: (BranchPoints * 10 + TotalSquares * 2 + Density * 5) / (Width * Height)

        if (features.GridWidth <= 0 || features.GridHeight <= 0)
        {
            features.ComplexityScore = 0;
            return;
        }

        double branchContribution = features.TotalBranchPoints * 10.0;
        double squareContribution = features.TotalSquareCount * 2.0;
        double densityContribution = features.BranchDensity * 5.0;
        double totalArea = features.GridWidth * features.GridHeight;

        features.ComplexityScore = (branchContribution + squareContribution + densityContribution) / totalArea;
    }
}
