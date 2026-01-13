using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using MapEditor.Infrastructure.Algorithms;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

public class UL22ConverterTests
{
    private readonly IUL22Converter _converter;

    public UL22ConverterTests()
    {
        _converter = new UL22Converter();
    }

    [Fact]
    public void ConvertToUL22_WithEmptyWorkspace_ReturnsAllZeros()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 8));

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Assert
        Assert.Equal(8, matrix.GetLength(0)); // rows
        Assert.Equal(10, matrix.GetLength(1)); // columns

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                Assert.Equal(0, matrix[y, x]);
            }
        }
    }

    [Fact]
    public void ConvertToUL22_WithSingleSquare_ReturnsBinaryMatrix()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        workspace.PlaceSquare(new Point(2, 2), SquareType.Grass);

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Assert
        Assert.Equal(5, matrix.GetLength(0));
        Assert.Equal(5, matrix.GetLength(1));
        Assert.Equal(1, matrix[2, 2]);

        // Check that all other cells are 0
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (x == 2 && y == 2)
                    continue;
                Assert.Equal(0, matrix[y, x]);
            }
        }
    }

    [Fact]
    public void ConvertToUL22_WithMultipleSquares_ReturnsBinaryMatrix()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(4, 4));
        workspace.PlaceSquare(new Point(0, 0), SquareType.Water);
        workspace.PlaceSquare(new Point(1, 1), SquareType.Sand);
        workspace.PlaceSquare(new Point(2, 2), SquareType.Grass);
        workspace.PlaceSquare(new Point(3, 3), SquareType.Wood);

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Assert
        Assert.Equal(4, matrix.GetLength(0));
        Assert.Equal(4, matrix.GetLength(1));

        // Check diagonal has 1s
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(1, matrix[1, 1]);
        Assert.Equal(1, matrix[2, 2]);
        Assert.Equal(1, matrix[3, 3]);

        // Check some empty cells are 0
        Assert.Equal(0, matrix[0, 1]);
        Assert.Equal(0, matrix[1, 0]);
        Assert.Equal(0, matrix[3, 0]);
    }

    [Fact]
    public void ConvertToUL22_WithFullGrid_ReturnsAllOnes()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(3, 3));
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                workspace.PlaceSquare(new Point(x, y), SquareType.Stone);
            }
        }

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Assert
        Assert.Equal(3, matrix.GetLength(0));
        Assert.Equal(3, matrix.GetLength(1));

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Assert.Equal(1, matrix[y, x]);
            }
        }
    }

    [Fact]
    public void ConvertToUL22_WithNullWorkspace_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _converter.ConvertToUL22(null!));
    }

    [Fact]
    public void GetMatrixDimensions_ReturnsCorrectDimensions()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(15, 10));

        // Act
        var (rows, columns) = _converter.GetMatrixDimensions(workspace);

        // Assert
        Assert.Equal(10, rows);
        Assert.Equal(15, columns);
    }

    [Fact]
    public void ConvertToUL22_SquareTypeDoesNotMatter_AllSquaresConvertedToOne()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(7, 1));
        workspace.PlaceSquare(new Point(0, 0), SquareType.Water);
        workspace.PlaceSquare(new Point(1, 0), SquareType.Sand);
        workspace.PlaceSquare(new Point(2, 0), SquareType.Grass);
        workspace.PlaceSquare(new Point(3, 0), SquareType.Wood);
        workspace.PlaceSquare(new Point(4, 0), SquareType.Metal);
        workspace.PlaceSquare(new Point(5, 0), SquareType.Stone);
        workspace.PlaceSquare(new Point(6, 0), SquareType.Lava);

        // Act
        var matrix = _converter.ConvertToUL22(workspace);

        // Assert
        for (int x = 0; x < 7; x++)
        {
            Assert.Equal(1, matrix[0, x]);
        }
    }
}
