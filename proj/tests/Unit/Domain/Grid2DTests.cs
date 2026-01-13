using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;

namespace MapEditor.Tests.Domain;

public class Grid2DTests
{
    [Fact]
    public void CreateGrid_WithValidSize_ShouldInitializeAllCells()
    {
        // Arrange
        var size = new Size(5, 5);

        // Act
        var grid = new Grid2D(size);

        // Assert
        Assert.Equal(5, grid.Size.Width);
        Assert.Equal(5, grid.Size.Height);
        Assert.Equal(25, grid.GetAllCells().Count());
        Assert.All(grid.GetAllCells(), cell => Assert.True(cell.IsEmpty));
    }

    [Fact]
    public void GetCell_WithValidPosition_ShouldReturnCorrectCell()
    {
        // Arrange
        var grid = new Grid2D(new Size(5, 5));

        // Act
        var cell = grid.GetCell(2, 3);

        // Assert
        Assert.NotNull(cell);
        Assert.Equal(2, cell.Position.X);
        Assert.Equal(3, cell.Position.Y);
    }

    [Fact]
    public void GetCell_WithInvalidPosition_ShouldThrow()
    {
        // Arrange
        var grid = new Grid2D(new Size(5, 5));

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.GetCell(10, 10));
    }

    [Fact]
    public void IsValidPosition_WithValidCoordinates_ShouldReturnTrue()
    {
        // Arrange
        var grid = new Grid2D(new Size(5, 5));

        // Act
        var result = grid.IsValidPosition(2, 3);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidPosition_WithInvalidCoordinates_ShouldReturnFalse()
    {
        // Arrange
        var grid = new Grid2D(new Size(5, 5));

        // Act & Assert
        Assert.False(grid.IsValidPosition(-1, 0));
        Assert.False(grid.IsValidPosition(0, -1));
        Assert.False(grid.IsValidPosition(5, 0));
        Assert.False(grid.IsValidPosition(0, 5));
    }
}
