using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using Xunit;

namespace MapEditor.Tests.Unit.Infrastructure;

/// <summary>
/// Tests to verify grid cell counting works correctly for Grid Features analysis.
/// </summary>
public class GridCellCountingTests
{
    [Fact]
    public void GetAllCells_EmptyGrid_ReturnsAllCells()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 3));

        // Act
        var allCells = workspace.Grid.GetAllCells().ToList();

        // Assert
        Assert.Equal(15, allCells.Count); // 5 x 3 = 15
        Assert.All(allCells, cell => Assert.True(cell.IsEmpty));
    }

    [Fact]
    public void GetAllCells_WithSquares_CountsCorrectly()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(4, 4));
        workspace.PlaceSquare(new Point(0, 0), SquareType.Grass);
        workspace.PlaceSquare(new Point(1, 1), SquareType.Water);
        workspace.PlaceSquare(new Point(2, 2), SquareType.Stone);

        // Act
        var allCells = workspace.Grid.GetAllCells().ToList();
        var filledCells = allCells.Where(c => !c.IsEmpty).ToList();
        var emptyCells = allCells.Count - filledCells.Count;

        // Assert
        Assert.Equal(16, allCells.Count);
        Assert.Equal(3, filledCells.Count);
        Assert.Equal(13, emptyCells);
    }

    [Fact]
    public void GetAllCells_FullGrid_AllCellsFilled()
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
        var allCells = workspace.Grid.GetAllCells().ToList();
        var filledCells = allCells.Where(c => !c.IsEmpty).ToList();

        // Assert
        Assert.Equal(9, allCells.Count);
        Assert.Equal(9, filledCells.Count);
        Assert.All(allCells, cell => Assert.False(cell.IsEmpty));
    }

    [Fact]
    public void Cell_IsEmpty_WorksCorrectly()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(3, 3));
        var centerCell = workspace.Grid.GetCell(1, 1);

        // Act & Assert - Initially empty
        Assert.True(centerCell.IsEmpty);
        Assert.Null(centerCell.Square);

        // Place square
        workspace.PlaceSquare(new Point(1, 1), SquareType.Grass);

        // Act & Assert - Now filled
        Assert.False(centerCell.IsEmpty);
        Assert.NotNull(centerCell.Square);
        Assert.Equal(SquareType.Grass, centerCell.Square.Type);
    }

    [Fact]
    public void GetAllCells_LargeGrid_CountsCorrectly()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(20, 15));

        // Act
        var allCells = workspace.Grid.GetAllCells().ToList();

        // Assert
        Assert.Equal(300, allCells.Count); // 20 x 15 = 300
    }

    [Theory]
    [InlineData(10, 10, 100)]
    [InlineData(5, 8, 40)]
    [InlineData(1, 1, 1)]
    [InlineData(100, 50, 5000)]
    public void GetAllCells_VariousSizes_ReturnsCorrectCount(int width, int height, int expected)
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(width, height));

        // Act
        var allCells = workspace.Grid.GetAllCells().ToList();

        // Assert
        Assert.Equal(expected, allCells.Count);
    }
}
