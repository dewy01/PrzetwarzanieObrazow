using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

public class BranchDetectionServiceTests
{
    private readonly BranchDetectionService _service;

    public BranchDetectionServiceTests()
    {
        _service = new BranchDetectionService();
    }

    [Fact]
    public void DetectBranches_WithNullMatrix_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.DetectBranches(null!));
    }

    [Fact]
    public void DetectBranches_WithEmptyMatrix_ReturnsZeroCounts()
    {
        var matrix = new int[0, 0];

        var result = _service.DetectBranches(matrix);

        Assert.Equal(0, result["EndpointCount"]);
        Assert.Equal(0, result["BifurcationCount"]);
        Assert.Equal(0, result["CrossingCount"]);
        Assert.Equal(0, result["TotalBranchPoints"]);
    }

    [Fact]
    public void ClassifyPoint_WithIsolatedPixel_ReturnsIsolated()
    {
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        var result = _service.ClassifyPoint(matrix, 1, 1);

        Assert.Equal("Isolated", result);
    }

    [Fact]
    public void ClassifyPoint_WithEndpoint_ReturnsEndpoint()
    {
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 1, 1, 0 },
            { 0, 0, 0 }
        };

        var result = _service.ClassifyPoint(matrix, 0, 1);

        Assert.Equal("Endpoint", result);
    }

    [Fact]
    public void ClassifyPoint_WithRegularPoint_ReturnsRegular()
    {
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 }
        };

        var result = _service.ClassifyPoint(matrix, 1, 1);

        Assert.Equal("Regular", result);
    }

    [Fact]
    public void ClassifyPoint_WithBifurcation_ReturnsBifurcation()
    {
        // Y-shape: 3 neighbors
        var matrix = new int[,]
        {
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        var result = _service.ClassifyPoint(matrix, 1, 1);

        Assert.Equal("Bifurcation", result);
    }

    [Fact]
    public void ClassifyPoint_WithCrossing_ReturnsCrossing()
    {
        // X-shape: 4 neighbors
        var matrix = new int[,]
        {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        };

        var result = _service.ClassifyPoint(matrix, 1, 1);

        Assert.Equal("Crossing", result);
    }

    [Fact]
    public void ClassifyPoint_WithBackgroundPixel_ReturnsBackground()
    {
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        var result = _service.ClassifyPoint(matrix, 1, 1);

        Assert.Equal("Background", result);
    }

    [Fact]
    public void DetectBranches_WithStraightLine_DetectsTwoEndpoints()
    {
        var matrix = new int[,]
        {
            { 1, 1, 1, 1, 1 }
        };

        var result = _service.DetectBranches(matrix);

        Assert.Equal(2, result["EndpointCount"]);
        Assert.Equal(0, result["BifurcationCount"]);
        Assert.Equal(0, result["CrossingCount"]);
        Assert.Equal(3, result["RegularCount"]);
    }

    [Fact]
    public void DetectBranches_WithYShape_DetectsThreeEndpointsOneBifurcation()
    {
        // Y-shape structure
        var matrix = new int[,]
        {
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        var result = _service.DetectBranches(matrix);

        Assert.Equal(3, result["EndpointCount"]); // 3 ends of Y
        Assert.Equal(1, result["BifurcationCount"]); // 1 junction
    }

    [Fact]
    public void DetectBranches_WithCrossShape_DetectsFourEndpointsMultipleCrossings()
    {
        // + shape structure (extended arms so we have endpoints)
        var matrix = new int[,]
        {
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 1, 1, 1, 1, 1 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 }
        };

        var result = _service.DetectBranches(matrix);

        Assert.Equal(4, result["EndpointCount"]); // 4 ends
        Assert.True((int)result["CrossingCount"] >= 1); // At least 1 crossing in the center
    }

    [Fact]
    public void AnalyzeBranchStructure_WithStraightLine_CalculatesCorrectMetrics()
    {
        var matrix = new int[,]
        {
            { 1, 1, 1, 1, 1 }
        };

        var result = _service.AnalyzeBranchStructure(matrix);

        Assert.Equal(2, result["EndpointCount"]);
        Assert.Equal(0, result["BifurcationCount"]);
        Assert.Equal(2, result["TotalBranchPoints"]);
        Assert.Equal(5, result["TotalSkeletonPixels"]);
        Assert.True((double)result["AverageEndpointDistance"] > 0);
    }

    [Fact]
    public void AnalyzeBranchStructure_WithComplexShape_CalculatesDensity()
    {
        // Y-shape with longer branches
        var matrix = new int[,]
        {
            { 1, 0, 0, 0, 1 },
            { 0, 1, 0, 1, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 }
        };

        var result = _service.AnalyzeBranchStructure(matrix);

        Assert.True((int)result["EndpointCount"] > 0);
        Assert.True((double)result["BranchDensity"] > 0);
        Assert.True((double)result["BranchComplexity"] >= 0 && (double)result["BranchComplexity"] <= 1);
    }

    [Fact]
    public void ClassifyPoint_WithOutOfBoundsCoordinates_ThrowsArgumentOutOfRangeException()
    {
        var matrix = new int[3, 3];

        Assert.Throws<ArgumentOutOfRangeException>(() => _service.ClassifyPoint(matrix, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.ClassifyPoint(matrix, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.ClassifyPoint(matrix, 3, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.ClassifyPoint(matrix, 0, 3));
    }

    [Fact]
    public void DetectBranches_ReturnsCorrectListTypes()
    {
        var matrix = new int[,]
        {
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        var result = _service.DetectBranches(matrix);

        Assert.IsType<List<(int x, int y)>>(result["Endpoints"]);
        Assert.IsType<List<(int x, int y)>>(result["Bifurcations"]);
        Assert.IsType<List<(int x, int y)>>(result["Crossings"]);
    }
}
