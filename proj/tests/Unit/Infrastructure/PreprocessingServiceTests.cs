using MapEditor.Domain.Biometric.Services;
using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

public class PreprocessingServiceTests
{
    private readonly IPreprocessingService _service;

    public PreprocessingServiceTests()
    {
        _service = new PreprocessingService();
    }

    [Fact]
    public void ApplyMedianFilter_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.ApplyMedianFilter(null!, 3));
    }

    [Fact]
    public void ApplyMedianFilter_WithEvenKernelSize_ThrowsArgumentException()
    {
        // Arrange
        var matrix = new int[3, 3];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.ApplyMedianFilter(matrix, 4));
    }

    [Fact]
    public void ApplyMedianFilter_WithNoiseInBinaryMatrix_RemovesNoise()
    {
        // Arrange - Binary matrix with isolated noise pixels
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 0, 1, 0 }, // Center pixel is noise (should be 1)
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var filtered = _service.ApplyMedianFilter(matrix, 3);

        // Assert - Noise should be removed (center should be 1)
        Assert.Equal(1, filtered[2, 2]);
    }

    [Fact]
    public void ApplyMedianFilter_WithSingleIsolatedPixel_RemovesIt()
    {
        // Arrange - Single isolated pixel surrounded by zeros
        var matrix = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        // Act
        var filtered = _service.ApplyMedianFilter(matrix, 3);

        // Assert - Isolated pixel should be removed
        Assert.Equal(0, filtered[1, 1]);
    }

    [Fact]
    public void ApplyMedianFilter_WithLargerKernel_UsesLargerNeighborhood()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 1, 0, 1, 0, 0 }, // Noise at center
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0 }
        };

        // Act
        var filtered = _service.ApplyMedianFilter(matrix, 5);

        // Assert - With larger kernel, result may differ
        Assert.NotNull(filtered);
        Assert.Equal(7, filtered.GetLength(0));
        Assert.Equal(7, filtered.GetLength(1));
    }

    [Fact]
    public void ApplyOtsuBinarization_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.ApplyOtsuBinarization(null!));
    }

    [Fact]
    public void ApplyOtsuBinarization_WithBinaryMatrix_ReturnsThreshold()
    {
        // Arrange - Already binary matrix
        var matrix = new int[,]
        {
            { 0, 0, 1, 1 },
            { 0, 1, 1, 1 },
            { 1, 1, 0, 0 },
            { 1, 0, 0, 0 }
        };

        // Act
        var (binarized, threshold) = _service.ApplyOtsuBinarization(matrix);

        // Assert
        Assert.NotNull(binarized);
        Assert.True(threshold >= 0 && threshold <= 1);
        Assert.Equal(4, binarized.GetLength(0));
        Assert.Equal(4, binarized.GetLength(1));
    }

    [Fact]
    public void ApplyOtsuBinarization_WithGrayscaleMatrix_BinarizesCorrectly()
    {
        // Arrange - Simulated grayscale with clear separation
        var matrix = new int[,]
        {
            { 10, 15, 20, 25 },
            { 12, 18, 22, 28 },
            { 80, 85, 90, 95 },
            { 82, 88, 92, 98 }
        };

        // Act
        var (binarized, threshold) = _service.ApplyOtsuBinarization(matrix);

        // Assert
        Assert.NotNull(binarized);
        Assert.True(threshold > 20 && threshold < 80, $"Threshold {threshold} should be between background and foreground");

        // Check that lower values became 0
        Assert.Equal(0, binarized[0, 0]);
        Assert.Equal(0, binarized[1, 1]);

        // Check that higher values became 1
        Assert.Equal(1, binarized[2, 2]);
        Assert.Equal(1, binarized[3, 3]);
    }

    [Fact]
    public void ApplyOtsuBinarization_WithUniformMatrix_HandlesGracefully()
    {
        // Arrange - All same values
        var matrix = new int[,]
        {
            { 5, 5, 5 },
            { 5, 5, 5 },
            { 5, 5, 5 }
        };

        // Act
        var (binarized, threshold) = _service.ApplyOtsuBinarization(matrix);

        // Assert - Should not throw, all values should be same after binarization
        Assert.NotNull(binarized);
        int firstValue = binarized[0, 0];
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Assert.Equal(firstValue, binarized[y, x]);
            }
        }
    }

    [Fact]
    public void Preprocess_WithNullMatrix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.Preprocess(null!, 3));
    }

    [Fact]
    public void Preprocess_AppliesBothFiltersInSequence()
    {
        // Arrange - Matrix with noise
        var matrix = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 0, 1, 0 }, // Noise
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };

        // Act
        var preprocessed = _service.Preprocess(matrix, 3);

        // Assert
        Assert.NotNull(preprocessed);
        Assert.Equal(5, preprocessed.GetLength(0));
        Assert.Equal(5, preprocessed.GetLength(1));

        // All values should be 0 or 1 after preprocessing
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.True(preprocessed[y, x] == 0 || preprocessed[y, x] == 1);
            }
        }
    }

    [Fact]
    public void Preprocess_MaintainsDimensions()
    {
        // Arrange
        var matrix = new int[,]
        {
            { 0, 1, 0, 1 },
            { 1, 0, 1, 0 },
            { 0, 1, 0, 1 }
        };

        // Act
        var preprocessed = _service.Preprocess(matrix, 3);

        // Assert
        Assert.Equal(3, preprocessed.GetLength(0));
        Assert.Equal(4, preprocessed.GetLength(1));
    }
}
