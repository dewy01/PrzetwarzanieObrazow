using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using Xunit;

namespace MapEditor.Tests.Unit.Domain;

/// <summary>
/// Tests for Workspace preset placement functionality
/// </summary>
public class WorkspacePresetPlacementTests
{
    [Fact]
    public void PlacePreset_PlacesSquaresAtCorrectPositions()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(0, 1), SquareType.Water, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 2), squares);

        // Act
        workspace.PlacePreset(new Point(3, 3), preset);

        // Assert
        var cell1 = workspace.Grid.GetCell(new Point(3, 3)); // relative (0, 0)
        var cell2 = workspace.Grid.GetCell(new Point(4, 3)); // relative (1, 0)
        var cell3 = workspace.Grid.GetCell(new Point(3, 4)); // relative (0, 1)

        Assert.NotNull(cell1.Square);
        Assert.Equal(SquareType.Grass, cell1.Square.Type);

        Assert.NotNull(cell2.Square);
        Assert.Equal(SquareType.Stone, cell2.Square.Type);

        Assert.NotNull(cell3.Square);
        Assert.Equal(SquareType.Water, cell3.Square.Type);
    }

    [Fact]
    public void PlacePreset_UpdatesWorkspaceModificationTime()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        var originalTime = workspace.ModifiedAt;

        System.Threading.Thread.Sleep(10); // Ensure time difference

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        // Act
        workspace.PlacePreset(new Point(5, 5), preset);

        // Assert
        Assert.True(workspace.ModifiedAt > originalTime);
    }

    [Fact]
    public void RemovePresetSquares_ReturnsRemovedSquares()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(3, 3), SquareType.Grass);
        workspace.PlaceSquare(new Point(4, 3), SquareType.Water);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Sand, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        // Act
        var removed = workspace.RemovePresetSquares(new Point(3, 3), preset);

        // Assert
        Assert.Equal(2, removed.Count);
        Assert.Equal(SquareType.Grass, removed[0].Type);
        Assert.Equal(SquareType.Water, removed[1].Type);
    }

    [Fact]
    public void RemovePresetSquares_ClearsSquaresFromGrid()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(3, 3), SquareType.Grass);
        workspace.PlaceSquare(new Point(4, 3), SquareType.Water);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Sand, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        // Act
        workspace.RemovePresetSquares(new Point(3, 3), preset);

        // Assert
        var cell1 = workspace.Grid.GetCell(new Point(3, 3));
        var cell2 = workspace.Grid.GetCell(new Point(4, 3));

        Assert.Null(cell1.Square);
        Assert.Null(cell2.Square);
    }

    [Fact]
    public void RemovePresetSquares_IgnoresEmptyPositions()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(3, 3), SquareType.Grass);
        // Position (4, 3) is empty

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Sand, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        // Act
        var removed = workspace.RemovePresetSquares(new Point(3, 3), preset);

        // Assert
        Assert.Single(removed);
        Assert.Equal(SquareType.Grass, removed[0].Type);
    }

    [Fact]
    public void PlacePreset_PartiallyOutOfBounds_OnlyPlacesValidSquares()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(5, 5));

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0),
            new SquareDefinition(new Point(4, 4), SquareType.Stone, 0)
        };
        var preset = new Preset("TestPreset", new Size(5, 5), squares);

        // Act - try to place at (1, 1), (5, 5) would be out of bounds in 5x5 grid
        workspace.PlacePreset(new Point(1, 1), preset);

        // Assert - only in-bounds squares should be placed
        var cell1 = workspace.Grid.GetCell(new Point(1, 1));
        Assert.NotNull(cell1.Square);
        Assert.Equal(SquareType.Grass, cell1.Square.Type);
    }

    [Fact]
    public void PlacePreset_EmptyPreset_DoesNothing()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        var preset = new Preset("EmptyPreset", new Size(1, 1), new List<SquareDefinition>());

        // Act
        workspace.PlacePreset(new Point(5, 5), preset);

        // Assert - should not crash and grid should remain empty
        var cell = workspace.Grid.GetCell(new Point(5, 5));
        Assert.Null(cell.Square);
    }

    [Fact]
    public void RemovePresetSquares_WithNoSquaresPlaced_ReturnsEmptyList()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        // Act
        var removed = workspace.RemovePresetSquares(new Point(5, 5), preset);

        // Assert
        Assert.Empty(removed);
    }
}
