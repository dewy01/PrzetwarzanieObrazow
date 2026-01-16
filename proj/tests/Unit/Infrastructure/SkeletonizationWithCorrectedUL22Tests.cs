using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

/// <summary>
/// Practical verification tests for UL22 converter + skeletonization pipeline.
/// These tests verify that after the binary convention fix, skeletonization
/// produces correct results on realistic test cases.
/// </summary>
public class SkeletonizationWithCorrectedUL22Tests
{
    private readonly UL22Converter _converter;
    private readonly SkeletonizationService _skeletonService;

    public SkeletonizationWithCorrectedUL22Tests()
    {
        _converter = new UL22Converter();
        _skeletonService = new SkeletonizationService();
    }

    [Fact]
    public void Pipeline_SimpleBox_SkeletonizedCorrectly()
    {
        // Arrange - Create a 5x5 workspace with a box in the center
        // Box from (1,1) to (3,3) = 9 squares (objects = 1, will be skeletonized)
        // Background = 16 cells (= 0, unchanged)
        var workspace = new Workspace("Test", new Size(5, 5));

        // Fill center 3x3 box with squares
        for (int y = 1; y <= 3; y++)
        {
            for (int x = 1; x <= 3; x++)
            {
                workspace.PlaceSquare(new Point(x, y), SquareType.Stone);
            }
        }

        // Act
        var matrix = _converter.ConvertToUL22(workspace);
        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert
        // The center box (Squares) should be marked as 1 in matrix
        for (int y = 1; y <= 3; y++)
        {
            for (int x = 1; x <= 3; x++)
            {
                Assert.Equal(1, matrix[y, x]);
            }
        }

        // The background around it should be 0s (unchanged by skeletonization)
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (y < 1 || y > 3 || x < 1 || x > 3)
                {
                    Assert.Equal(0, skeleton[y, x]);
                }
            }
        }

        // The box should be thinned to a skeleton (some 1s remaining)
        int skeletonPixelsInBox = 0;
        for (int y = 1; y <= 3; y++)
        {
            for (int x = 1; x <= 3; x++)
            {
                if (skeleton[y, x] == 1)
                    skeletonPixelsInBox++;
            }
        }

        // After thinning, box should have fewer pixels than original 9
        Assert.True(skeletonPixelsInBox > 0 && skeletonPixelsInBox < 9,
            $"Box should be thinned (found {skeletonPixelsInBox} pixels, expected between 1 and 8)");
    }

    [Fact]
    public void Pipeline_HorizontalLine_SkeletonPreserved()
    {
        // Arrange - Horizontal line of squares in middle
        var workspace = new Workspace("Test", new Size(7, 3));

        // Place horizontal line at y=1 from x=1 to x=5
        for (int x = 1; x <= 5; x++)
        {
            workspace.PlaceSquare(new Point(x, 1), SquareType.Stone);
        }

        // Act
        var matrix = _converter.ConvertToUL22(workspace);
        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert
        // The line (Squares) should be marked as 1s in matrix
        for (int x = 1; x <= 5; x++)
        {
            Assert.Equal(1, matrix[1, x]);
        }

        // Background above and below should remain 0
        Assert.Equal(0, skeleton[0, 1]);
        Assert.Equal(0, skeleton[2, 1]);

        // Line should be thinned (likely all preserved as 1-pixel wide already)
        for (int x = 1; x <= 5; x++)
        {
            Assert.Equal(1, skeleton[1, x]);
        }
    }

    [Fact]
    public void Pipeline_DiagonalPattern_DiagonalSkeletonized()
    {
        // Arrange - Diagonal pattern of squares
        var workspace = new Workspace("Test", new Size(5, 5));

        // Diagonal from (0,0) to (4,4)
        for (int i = 0; i < 5; i++)
        {
            workspace.PlaceSquare(new Point(i, i), SquareType.Stone);
        }

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Verify binary conversion first
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(1, matrix[i, i]); // Diagonal should be 1
        }

        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert
        // Diagonal (Squares) should remain as skeleton (1s)
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(1, skeleton[i, i]);
        }

        // Background (non-diagonal) should remain 0
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (x != y)
                {
                    Assert.Equal(0, skeleton[y, x]);
                }
            }
        }
    }

    [Fact]
    public void Pipeline_CrossPattern_CrossPreserved()
    {
        // Arrange - Cross pattern (plus sign)
        var workspace = new Workspace("Test", new Size(7, 7));

        // Horizontal line at y=3
        for (int x = 0; x < 7; x++)
        {
            workspace.PlaceSquare(new Point(x, 3), SquareType.Stone);
        }

        // Vertical line at x=3
        for (int y = 0; y < 7; y++)
        {
            workspace.PlaceSquare(new Point(3, y), SquareType.Stone);
        }

        // Act
        var matrix = _converter.ConvertToUL22(workspace);
        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert
        // Cross should be preserved as 1s (skeleton)
        for (int i = 0; i < 7; i++)
        {
            Assert.Equal(1, skeleton[3, i]); // Horizontal
            Assert.Equal(1, skeleton[i, 3]); // Vertical
        }

        // Background should remain 0
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                if (x != 3 && y != 3)
                {
                    Assert.Equal(0, skeleton[y, x]);
                }
            }
        }
    }

    [Fact]
    public void BinaryConvention_AllGroupsChecked()
    {
        // Arrange - Create workspace with multiple groups
        var workspace = new Workspace("Test", new Size(5, 5));

        // Group 1: Square at (1,1)
        var group1 = new Group("Group1");
        workspace.AddGroup(group1);
        group1.PlaceSquare(new Point(1, 1), SquareType.Stone);

        // Group 2: Square at (3,3)
        var group2 = new Group("Group2");
        workspace.AddGroup(group2);
        group2.PlaceSquare(new Point(3, 3), SquareType.Stone);

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Assert - Both squares from different groups should be detected as 1
        Assert.Equal(1, matrix[1, 1]); // From group1
        Assert.Equal(1, matrix[3, 3]); // From group2

        // All other cells should be background (0)
        int backgroundCount = 0;
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (matrix[y, x] == 0)
                    backgroundCount++;
            }
        }

        Assert.Equal(23, backgroundCount); // 25 - 2 squares
    }

    [Fact]
    public void Skeletonization_RingPattern_FrameThinned()
    {
        // Arrange - Ring pattern (outer square, inner empty)
        var workspace = new Workspace("Test", new Size(7, 7));

        // Outer 7x7 frame
        for (int x = 0; x < 7; x++)
        {
            workspace.PlaceSquare(new Point(x, 0), SquareType.Stone);
            workspace.PlaceSquare(new Point(x, 6), SquareType.Stone);
        }
        for (int y = 1; y < 6; y++)
        {
            workspace.PlaceSquare(new Point(0, y), SquareType.Stone);
            workspace.PlaceSquare(new Point(6, y), SquareType.Stone);
        }

        // Act
        var matrix = _converter.ConvertToUL22(workspace);
        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert
        // Frame (Squares) should be marked as 1 in matrix
        for (int x = 0; x < 7; x++)
        {
            Assert.Equal(1, matrix[0, x]); // Top
            Assert.Equal(1, matrix[6, x]); // Bottom
        }
        for (int y = 1; y < 6; y++)
        {
            Assert.Equal(1, matrix[y, 0]); // Left
            Assert.Equal(1, matrix[y, 6]); // Right
        }

        // After skeletonization, frame should be thinned to 1-pixel outline
        // Interior (background) should remain 0
        for (int y = 1; y < 6; y++)
        {
            for (int x = 1; x < 6; x++)
            {
                Assert.Equal(0, skeleton[y, x]);
            }
        }
    }

    [Fact]
    public void MatrixDimensions_PreservedThroughPipeline()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 8));

        // Act
        var matrix = _converter.ConvertToUL22(workspace);
        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert - Dimensions should match
        Assert.Equal(8, matrix.GetLength(0));
        Assert.Equal(10, matrix.GetLength(1));
        Assert.Equal(8, skeleton.GetLength(0));
        Assert.Equal(10, skeleton.GetLength(1));
    }

    [Fact]
    public void NoSquares_AllBackground_NoSkeletonization()
    {
        // Arrange - Empty workspace = all background (0s)
        var workspace = new Workspace("Test", new Size(5, 5));

        // Act
        var matrix = _converter.ConvertToUL22(workspace);
        var skeleton = _skeletonService.ZhangSuenThinning(matrix);

        // Assert
        // All 0s should remain 0s (no objects to skeletonize)
        Assert.NotNull(skeleton);
        Assert.Equal(5, skeleton.GetLength(0));
        Assert.Equal(5, skeleton.GetLength(1));

        // All values should be 0 (no objects)
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.Equal(0, skeleton[y, x]);
            }
        }
    }
}
