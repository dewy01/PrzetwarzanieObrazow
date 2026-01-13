using MapEditor.Domain.Biometric.Services;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Implementation of branch detection algorithm for skeletonized images
/// Uses crossing number method (CN) to classify skeleton points
/// </summary>
public class BranchDetectionService : IBranchDetectionService
{
    public Dictionary<string, object> DetectBranches(int[,] skeletonMatrix)
    {
        if (skeletonMatrix == null)
            throw new ArgumentNullException(nameof(skeletonMatrix));

        int height = skeletonMatrix.GetLength(0);
        int width = skeletonMatrix.GetLength(1);

        var endpoints = new List<(int x, int y)>();
        var bifurcations = new List<(int x, int y)>();
        var crossings = new List<(int x, int y)>();
        var regularPoints = new List<(int x, int y)>();

        // Scan all skeleton pixels
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (skeletonMatrix[y, x] == 1)
                {
                    string classification = ClassifyPoint(skeletonMatrix, x, y);

                    switch (classification)
                    {
                        case "Endpoint":
                            endpoints.Add((x, y));
                            break;
                        case "Bifurcation":
                            bifurcations.Add((x, y));
                            break;
                        case "Crossing":
                            crossings.Add((x, y));
                            break;
                        case "Regular":
                            regularPoints.Add((x, y));
                            break;
                    }
                }
            }
        }

        return new Dictionary<string, object>
        {
            { "EndpointCount", endpoints.Count },
            { "BifurcationCount", bifurcations.Count },
            { "CrossingCount", crossings.Count },
            { "RegularCount", regularPoints.Count },
            { "TotalBranchPoints", endpoints.Count + bifurcations.Count + crossings.Count },
            { "Endpoints", endpoints },
            { "Bifurcations", bifurcations },
            { "Crossings", crossings }
        };
    }

    public string ClassifyPoint(int[,] skeletonMatrix, int x, int y)
    {
        if (skeletonMatrix == null)
            throw new ArgumentNullException(nameof(skeletonMatrix));

        int height = skeletonMatrix.GetLength(0);
        int width = skeletonMatrix.GetLength(1);

        if (x < 0 || x >= width || y < 0 || y >= height)
            throw new ArgumentOutOfRangeException("Coordinates out of bounds");

        if (skeletonMatrix[y, x] != 1)
            return "Background";

        // Count neighbors using 8-connectivity
        int neighborCount = CountNeighbors(skeletonMatrix, x, y);

        // Crossing Number (CN) method classification:
        // CN = 0: Isolated point
        // CN = 1: Endpoint
        // CN = 2: Regular point (continuation)
        // CN = 3: Bifurcation (Y-junction)
        // CN = 4: Crossing (X-junction)
        // CN >= 5: Complex junction

        if (neighborCount == 0)
            return "Isolated";
        else if (neighborCount == 1)
            return "Endpoint";
        else if (neighborCount == 2)
            return "Regular";
        else if (neighborCount == 3)
            return "Bifurcation";
        else if (neighborCount == 4)
            return "Crossing";
        else
            return "ComplexJunction";
    }

    public Dictionary<string, object> AnalyzeBranchStructure(int[,] skeletonMatrix)
    {
        if (skeletonMatrix == null)
            throw new ArgumentNullException(nameof(skeletonMatrix));

        var basicDetection = DetectBranches(skeletonMatrix);
        var endpoints = (List<(int x, int y)>)basicDetection["Endpoints"];
        var bifurcations = (List<(int x, int y)>)basicDetection["Bifurcations"];
        var crossings = (List<(int x, int y)>)basicDetection["Crossings"];

        // Calculate average distances between branch points
        double avgEndpointDistance = CalculateAverageDistance(endpoints);
        double avgBifurcationDistance = CalculateAverageDistance(bifurcations);

        // Estimate branch complexity
        int totalBranchPoints = endpoints.Count + bifurcations.Count + crossings.Count;
        double complexity = totalBranchPoints > 0
            ? (double)bifurcations.Count / totalBranchPoints
            : 0.0;

        // Calculate branching density (branch points per 100 pixels)
        int totalPixels = CountTotalSkeletonPixels(skeletonMatrix);
        double branchDensity = totalPixels > 0
            ? (totalBranchPoints * 100.0) / totalPixels
            : 0.0;

        return new Dictionary<string, object>
        {
            { "EndpointCount", endpoints.Count },
            { "BifurcationCount", bifurcations.Count },
            { "CrossingCount", crossings.Count },
            { "TotalBranchPoints", totalBranchPoints },
            { "AverageEndpointDistance", avgEndpointDistance },
            { "AverageBifurcationDistance", avgBifurcationDistance },
            { "BranchComplexity", complexity },
            { "BranchDensity", branchDensity },
            { "TotalSkeletonPixels", totalPixels },
            { "Endpoints", endpoints },
            { "Bifurcations", bifurcations },
            { "Crossings", crossings }
        };
    }

    private int CountNeighbors(int[,] matrix, int x, int y)
    {
        int height = matrix.GetLength(0);
        int width = matrix.GetLength(1);
        int count = 0;

        // Check all 8 neighbors
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                if (matrix[ny, nx] == 1)
                    count++;
            }
        }

        return count;
    }

    private double CalculateAverageDistance(List<(int x, int y)> points)
    {
        if (points.Count < 2)
            return 0.0;

        double totalDistance = 0.0;
        int pairCount = 0;

        // Calculate pairwise Euclidean distances
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                double dx = points[i].x - points[j].x;
                double dy = points[i].y - points[j].y;
                totalDistance += Math.Sqrt(dx * dx + dy * dy);
                pairCount++;
            }
        }

        return pairCount > 0 ? totalDistance / pairCount : 0.0;
    }

    private int CountTotalSkeletonPixels(int[,] matrix)
    {
        int height = matrix.GetLength(0);
        int width = matrix.GetLength(1);
        int count = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (matrix[y, x] == 1)
                    count++;
            }
        }

        return count;
    }
}
