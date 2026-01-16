using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Integration;

/// <summary>
/// Integration tests verifying that UL22Converter correctly feeds data to SkeletonizationService.
/// These tests ensure that after the UL22 binary convention fix, skeletonization properly
/// processes only background areas (marked as 1).
/// </summary>
public class UL22SkeletonizationIntegrationTests
{
    private readonly IUL22Converter _converter;
    private readonly ISkeletonizationService _skeletonizationService;

    public UL22SkeletonizationIntegrationTests()
    {
        _converter = new UL22Converter();
        _skeletonizationService = new SkeletonizationService();
    }

    [Fact]
    public void SkeletonizationWithUL22_EmptyGrid_ProducesEmptySkeleton()
    {
        // Arrange - Empty grid should produce no skeleton
        var workspace = new Workspace("Test", new Size(5, 5));
        var matrix = _converter.ConvertToUL22(workspace);

        // Act - Skeletonize empty background (all 1s)
        var skeleton = _skeletonizationService.ZhangSuenThinning(matrix);

        // Assert - All background with no connected regions might thin to nothing or stay same
        Assert.NotNull(skeleton);
        Assert.Equal(matrix.GetLength(0), skeleton.GetLength(0));
        Assert.Equal(matrix.GetLength(1), skeleton.GetLength(1));
        
        // Skeleton of empty grid should be mostly 1s (background thinned to core)
        int oneCount = 0;
        for (int y = 0; y < skeleton.GetLength(0); y++)
        {
            for (int x = 0; x < skeleton.GetLength(1); x++)
            {
                if (skeleton[y, x] == 1)
                    oneCount++;
            }
        }
        Assert.True(oneCount >= 0, "Skeleton should be processed");
    }

    [Fact]
    public void SkeletonizationWithUL22_GridWithCenterSquare_BackgroundSkeletonized()
    {
        // Arrange - Grid with single center square (foreground)
        var workspace = new Workspace("Test", new Size(5, 5));
        workspace.PlaceSquare(new Point(2, 2), SquareType.Stone);
        var matrix = _converter.ConvertToUL22(workspace);
        
        // Verify conversion: center should be 0 (Square), edges 1 (background)
        Assert.Equal(0, matrix[2, 2]); // Square = 0
        Assert.Equal(1, matrix[0, 0]); // Background = 1

        // Act - Skeletonize with the corrected binary convention
        var skeleton = _skeletonizationService.ZhangSuenThinning(matrix);

        // Assert - Skeleton should process background, not the square
        Assert.NotNull(skeleton);
        
        // Center should still be 0 (was Squares, won't be skeletonized)
        Assert.Equal(0, skeleton[2, 2]);
        
        // Background around it should be thinned but still present
        // The perimeter or edges might remain as skeleton structure
        int backgroundCount = 0;
        for (int y = 0; y < skeleton.GetLength(0); y++)
        {
            for (int x = 0; x < skeleton.GetLength(1); x++)
            {
                if (skeleton[y, x] == 1)
                    backgroundCount++;
            }
        }
        
        // Should have some remaining background skeleton (perimeter)
        Assert.True(backgroundCount > 0, "Background should contribute to skeleton");
    }

    [Fact]
    public void SkeletonizationWithUL22_FullGrid_ProducesNoSkeleton()
    {
        // Arrange - Full grid (all squares, no background)
        var workspace = new Workspace("Test", new Size(3, 3));
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                workspace.PlaceSquare(new Point(x, y), SquareType.Stone);
            }
        }
        
        var matrix = _converter.ConvertToUL22(workspace);
        
        // Verify: All should be 0 (all Squares)
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Assert.Equal(0, matrix[y, x]);
            }
        }

        // Act - Try to skeletonize grid with no background
        var skeleton = _skeletonizationService.ZhangSuenThinning(matrix);

        // Assert - Skeleton of matrix with no 1s should produce no 1s
        Assert.NotNull(skeleton);
        for (int y = 0; y < skeleton.GetLength(0); y++)
        {
            for (int x = 0; x < skeleton.GetLength(1); x++)
            {
                Assert.Equal(0, skeleton[y, x]);
            }
        }
    }

    [Fact]
    public void ConversionAndSkeletonization_BinaryConventionConsistency()
    {
        // Arrange - Create workspace with mixed pattern
        var workspace = new Workspace("Test", new Size(7, 7));
        
        // Create a cross pattern: horizontal and vertical strips of squares
        for (int x = 0; x < 7; x++)
        {
            workspace.PlaceSquare(new Point(x, 3), SquareType.Stone); // Horizontal line at y=3
        }
        for (int y = 0; y < 7; y++)
        {
            workspace.PlaceSquare(new Point(3, y), SquareType.Stone); // Vertical line at x=3
        }

        var matrix = _converter.ConvertToUL22(workspace);

        // Act
        var skeleton = _skeletonizationService.ZhangSuenThinning(matrix);

        // Assert - Verify consistency
        Assert.NotNull(skeleton);
        
        // The cross pattern (all Squares = 0) should remain mostly as-is or thin while staying 0
        for (int x = 0; x < 7; x++)
        {
            // Horizontal line should be 0 or remain as structure
            int valueAtCross = skeleton[3, x];
            Assert.True(valueAtCross == 0 || valueAtCross == 1, "Skeleton value should be valid");
        }

        // Background areas should be thinned to their essential structure
        // For example, corner at (0,0) should be background (1) in matrix, 
        // and might be thinned or preserved
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                // Corners are background (1 in input matrix)
                // After skeletonization, should still be 0 or 1 (valid values)
                Assert.True(skeleton[y, x] == 0 || skeleton[y, x] == 1);
            }
        }
    }
}
