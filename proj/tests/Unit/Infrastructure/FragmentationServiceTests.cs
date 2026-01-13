using MapEditor.Domain.Biometric.Services;
using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

public class FragmentationServiceTests
{
    private readonly IFragmentationService _service;

    public FragmentationServiceTests()
    {
        _service = new FragmentationService();
    }

    [Fact]
    public void DetectFragments_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.DetectFragments(null!));
    }

    [Fact]
    public void DetectFragments_WithEmptyMatrix_ReturnsEmptyList()
    {
        // Arrange - All zeros
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert
        Assert.Empty(fragments);
    }

    [Fact]
    public void DetectFragments_WithSinglePixel_ReturnsSingleFragment()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert
        Assert.Single(fragments);
        Assert.Equal(1, fragments[0].PixelCount);
        Assert.Equal(1, fragments[0].Id);
    }

    [Fact]
    public void DetectFragments_WithConnectedComponent_ReturnsOneFragment()
    {
        // Arrange - L-shaped component
        var matrix = new int[,]
        {
            { 1, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 1 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert
        Assert.Single(fragments);
        Assert.Equal(5, fragments[0].PixelCount);
    }

    [Fact]
    public void DetectFragments_WithTwoSeparateComponents_ReturnsTwoFragments()
    {
        // Arrange - Two separate components
        var matrix = new int[,]
        {
            { 1, 1, 0, 0, 0 },
            { 1, 1, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 1 },
            { 0, 0, 0, 1, 1 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert
        Assert.Equal(2, fragments.Count);
        Assert.Equal(4, fragments[0].PixelCount);
        Assert.Equal(4, fragments[1].PixelCount);
    }

    [Fact]
    public void DetectFragments_WithDiagonalConnection_DetectsAsConnected()
    {
        // Arrange - Diagonal connection (8-connectivity)
        var matrix = new int[,]
        {
            { 1, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 1 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert - Should be one fragment with 8-connectivity
        Assert.Single(fragments);
        Assert.Equal(3, fragments[0].PixelCount);
    }

    [Fact]
    public void DetectFragments_CalculatesBoundingBoxCorrectly()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 0, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert
        Assert.Single(fragments);
        var fragment = fragments[0];
        Assert.Equal(1, fragment.MinX);
        Assert.Equal(3, fragment.MaxX);
        Assert.Equal(1, fragment.MinY);
        Assert.Equal(3, fragment.MaxY);
        Assert.Equal(3, fragment.Width);
        Assert.Equal(3, fragment.Height);
    }

    [Fact]
    public void CreateLabeledMatrix_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.CreateLabeledMatrix(null!));
    }

    [Fact]
    public void CreateLabeledMatrix_WithTwoFragments_LabelsCorrectly()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 1, 1, 0, 0, 0 },
            { 1, 1, 0, 1, 1 },
            { 0, 0, 0, 1, 1 }
        };

        // Act
        var labeled = _service.CreateLabeledMatrix(matrix);

        // Assert
        Assert.Equal(1, labeled[0, 0]);
        Assert.Equal(1, labeled[0, 1]);
        Assert.Equal(1, labeled[1, 0]);
        Assert.Equal(1, labeled[1, 1]);

        Assert.Equal(2, labeled[1, 3]);
        Assert.Equal(2, labeled[1, 4]);
        Assert.Equal(2, labeled[2, 3]);
        Assert.Equal(2, labeled[2, 4]);

        Assert.Equal(0, labeled[0, 2]); // Background remains 0
    }

    [Fact]
    public void CalculateStatistics_WithNullFragments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.CalculateStatistics(null!));
    }

    [Fact]
    public void CalculateStatistics_WithEmptyList_ReturnsZeroStats()
    {
        // Arrange
        var fragments = new List<MapEditor.Domain.Biometric.ValueObjects.Fragment>();

        // Act
        var stats = _service.CalculateStatistics(fragments);

        // Assert
        Assert.Equal(0, stats["FragmentCount"]);
        Assert.Equal(0, stats["AverageSize"]);
        Assert.Equal(0, stats["LargestSize"]);
        Assert.Equal(0, stats["SmallestSize"]);
    }

    [Fact]
    public void CalculateStatistics_WithMultipleFragments_CalculatesCorrectly()
    {
        // Arrange
        var fragments = new List<MapEditor.Domain.Biometric.ValueObjects.Fragment>
        {
            new() { Id = 1, PixelCount = 10, Pixels = new(), MinX = 0, MaxX = 0, MinY = 0, MaxY = 0 },
            new() { Id = 2, PixelCount = 20, Pixels = new(), MinX = 0, MaxX = 0, MinY = 0, MaxY = 0 },
            new() { Id = 3, PixelCount = 30, Pixels = new(), MinX = 0, MaxX = 0, MinY = 0, MaxY = 0 }
        };

        // Act
        var stats = _service.CalculateStatistics(fragments);

        // Assert
        Assert.Equal(3, stats["FragmentCount"]);
        Assert.Equal(20, stats["AverageSize"]);
        Assert.Equal(30, stats["LargestSize"]);
        Assert.Equal(10, stats["SmallestSize"]);
        Assert.Equal(60, stats["TotalPixels"]);
    }

    [Fact]
    public void DetectFragments_WithComplexPattern_DetectsAllComponents()
    {
        // Arrange - Multiple small fragments
        var matrix = new int[,]
        {
            { 1, 0, 1, 0, 1 },
            { 0, 0, 0, 0, 0 },
            { 1, 0, 1, 0, 1 },
            { 0, 0, 0, 0, 0 },
            { 1, 0, 1, 0, 1 }
        };

        // Act
        var fragments = _service.DetectFragments(matrix);

        // Assert - Should find 9 separate single-pixel fragments
        Assert.Equal(9, fragments.Count);
        Assert.All(fragments, f => Assert.Equal(1, f.PixelCount));
    }
}
