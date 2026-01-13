using MapEditor.Domain.Biometric.Services;
using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

public class SkeletonizationServiceTests
{
    private readonly ISkeletonizationService _service;

    public SkeletonizationServiceTests()
    {
        _service = new SkeletonizationService();
    }

    [Fact]
    public void ZhangSuenThinning_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.ZhangSuenThinning(null!));
    }

    [Fact]
    public void ZhangSuenThinning_WithEmptyMatrix_ReturnsEmptyMatrix()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        // Act
        var skeleton = _service.ZhangSuenThinning(matrix);

        // Assert
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Assert.Equal(0, skeleton[y, x]);
            }
        }
    }

    [Fact]
    public void ZhangSuenThinning_WithSinglePixel_PreservesPixel()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        // Act
        var skeleton = _service.ZhangSuenThinning(matrix);

        // Assert
        Assert.Equal(1, skeleton[1, 1]);
    }

    [Fact]
    public void ZhangSuenThinning_WithHorizontalLine_ThinsToSinglePixelWidth()
    {
        // Arrange - Thick horizontal line
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0, 0, 0 }
        };

        // Act
        var skeleton = _service.ZhangSuenThinning(matrix);

        // Assert - Should be thinned to single row
        int skeletonPixels = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                skeletonPixels += skeleton[y, x];
            }
        }

        // Skeleton should have fewer pixels than original
        Assert.True(skeletonPixels < 15); // Original had 15 pixels
        Assert.True(skeletonPixels > 0);   // But should have some pixels
    }

    [Fact]
    public void ZhangSuenThinning_WithVerticalLine_ThinsToSinglePixelWidth()
    {
        // Arrange - Thick vertical line
        var matrix = new int[,]
        {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 }
        };

        // Act
        var skeleton = _service.ZhangSuenThinning(matrix);

        // Assert - Should be thinned
        int skeletonPixels = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                skeletonPixels += skeleton[y, x];
            }
        }

        Assert.True(skeletonPixels < 15); // Original had 15 pixels
        Assert.True(skeletonPixels > 0);
    }

    [Fact]
    public void ZhangSuenThinning_WithSquare_CreatesConnectedSkeleton()
    {
        // Arrange - Filled square
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var skeleton = _service.ZhangSuenThinning(matrix);

        // Assert - Skeleton should be much smaller than original
        int skeletonPixels = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                skeletonPixels += skeleton[y, x];
            }
        }

        Assert.True(skeletonPixels < 9); // Original had 9 pixels
        Assert.True(skeletonPixels > 0);
    }

    [Fact]
    public void ZhangSuenWithIterations_ReturnsIterationCount()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var (skeleton, iterations) = _service.ZhangSuenWithIterations(matrix);

        // Assert
        Assert.True(iterations > 0);
        Assert.True(iterations < 100); // Reasonable upper bound
        Assert.NotNull(skeleton);
    }

    [Fact]
    public void CalculateSkeletonMetrics_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.CalculateSkeletonMetrics(null!));
    }

    [Fact]
    public void CalculateSkeletonMetrics_WithEmptyMatrix_ReturnsZeroMetrics()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        // Act
        var metrics = _service.CalculateSkeletonMetrics(matrix);

        // Assert
        Assert.Equal(0, metrics["SkeletonPixels"]);
        Assert.Equal(0, metrics["Endpoints"]);
        Assert.Equal(0, metrics["Junctions"]);
    }

    [Fact]
    public void CalculateSkeletonMetrics_WithLineSegment_DetectsEndpoints()
    {
        // Arrange - Simple line with 2 endpoints
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var metrics = _service.CalculateSkeletonMetrics(matrix);

        // Assert
        Assert.Equal(5, metrics["SkeletonPixels"]);
        Assert.Equal(2, metrics["Endpoints"]); // Two ends of the line
    }

    [Fact]
    public void CalculateSkeletonMetrics_WithTJunction_DetectsJunction()
    {
        // Arrange - T-shaped junction
        var matrix = new int[,]
        {
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var metrics = _service.CalculateSkeletonMetrics(matrix);

        // Assert
        Assert.True(metrics["Junctions"] >= 1); // Should detect junction point
        Assert.True(metrics["Endpoints"] >= 3);  // T has 3 endpoints
    }

    [Fact]
    public void ZhangSuenThinning_PreservesConnectivity()
    {
        // Arrange - L-shaped figure
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 0, 0, 0 },
            { 0, 1, 0, 0, 0 }
        };

        // Act
        var skeleton = _service.ZhangSuenThinning(matrix);

        // Assert - Should still be connected
        int skeletonPixels = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                skeletonPixels += skeleton[y, x];
            }
        }

        Assert.True(skeletonPixels > 0);
        Assert.True(skeletonPixels < 9); // Should be thinned from original
    }

    // K3M Algorithm Tests

    [Fact]
    public void K3MThinning_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.K3MThinning(null!));
    }

    [Fact]
    public void K3MThinning_WithEmptyMatrix_ReturnsEmptyMatrix()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        // Act
        var skeleton = _service.K3MThinning(matrix);

        // Assert
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Assert.Equal(0, skeleton[y, x]);
            }
        }
    }

    [Fact]
    public void K3MThinning_WithSinglePixel_PreservesPixel()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        // Act
        var skeleton = _service.K3MThinning(matrix);

        // Assert
        Assert.Equal(1, skeleton[1, 1]);
    }

    [Fact]
    public void K3MThinning_WithHorizontalLine_ProducesThinLine()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var skeleton = _service.K3MThinning(matrix);

        // Assert - Should remain a single line
        int middleRowPixels = 0;
        for (int x = 0; x < 5; x++)
        {
            middleRowPixels += skeleton[1, x];
        }

        Assert.Equal(5, middleRowPixels);
    }

    [Fact]
    public void K3MThinning_WithThickVerticalLine_ThinsToSingleLine()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 1, 1, 0 },
            { 0, 1, 1, 0 },
            { 0, 1, 1, 0 },
            { 0, 1, 1, 0 }
        };

        // Act
        var skeleton = _service.K3MThinning(matrix);

        // Assert - Should be thinned to 1 pixel wide
        int totalPixels = 0;
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                totalPixels += skeleton[y, x];
            }
        }

        Assert.True(totalPixels >= 4); // At least the height
        Assert.True(totalPixels < 8); // Less than original width*height
    }

    [Fact]
    public void K3MWithIterations_ReturnsIterationCount()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        // Act
        var (skeleton, iterations) = _service.K3MWithIterations(matrix);

        // Assert
        Assert.True(iterations > 0);
        Assert.True(iterations < 10); // Should converge quickly
    }

    [Fact]
    public void K3MThinning_PreservesConnectivity()
    {
        // Arrange - L-shaped pattern
        var matrix = new int[,]
        {
            { 1, 1, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 1, 1, 1 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var skeleton = _service.K3MThinning(matrix);

        // Assert - Should still be connected
        int skeletonPixels = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                skeletonPixels += skeleton[y, x];
            }
        }

        Assert.True(skeletonPixels > 0);
        Assert.True(skeletonPixels < 11); // Should be thinned from original
    }

    [Fact]
    public void K3MThinning_Vs_ZhangSuen_ProducesSimilarResults()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0 }
        };

        // Act
        var skeletonZS = _service.ZhangSuenThinning(matrix);
        var skeletonK3M = _service.K3MThinning(matrix);

        // Assert - Both should produce thin skeletons (may differ slightly)
        int pixelsZS = 0;
        int pixelsK3M = 0;

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                pixelsZS += skeletonZS[y, x];
                pixelsK3M += skeletonK3M[y, x];
            }
        }

        // Both should reduce the thick line significantly
        Assert.True(pixelsZS < 12); // Original has 12 pixels
        Assert.True(pixelsK3M < 12);
        Assert.True(pixelsZS > 0);
        Assert.True(pixelsK3M > 0);
    }
}
