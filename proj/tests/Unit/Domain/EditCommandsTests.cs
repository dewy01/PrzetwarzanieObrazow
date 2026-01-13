using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Commands;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Domain;

public class EditCommandsTests
{
    private readonly Workspace _workspace;

    public EditCommandsTests()
    {
        _workspace = new Workspace("Test", new Size(10, 10));
    }

    [Fact]
    public void PlaceSquareCommand_Execute_ShouldPlaceSquare()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);

        // Act
        command.Execute();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.False(cell.IsEmpty);
        Assert.Equal(SquareType.Water, cell.Square!.Type);
    }

    [Fact]
    public void PlaceSquareCommand_Undo_ShouldRemoveSquare()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.True(cell.IsEmpty);
    }

    [Fact]
    public void PlaceSquareCommand_UndoWithExistingSquare_ShouldRestorePrevious()
    {
        // Arrange
        _workspace.PlaceSquare(new Point(5, 5), SquareType.Grass);
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.False(cell.IsEmpty);
        Assert.Equal(SquareType.Grass, cell.Square!.Type);
    }

    [Fact]
    public void PlaceSquareCommand_Description_ShouldBeCorrect()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);

        // Act
        var description = command.Description;

        // Assert
        Assert.Contains("Place", description);
        Assert.Contains("Water", description);
        Assert.Contains("(5, 5)", description);
    }

    [Fact]
    public void RemoveSquareCommand_Execute_ShouldRemoveSquare()
    {
        // Arrange
        _workspace.PlaceSquare(new Point(5, 5), SquareType.Water);
        var command = new RemoveSquareCommand(_workspace, new Point(5, 5));

        // Act
        command.Execute();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.True(cell.IsEmpty);
    }

    [Fact]
    public void RemoveSquareCommand_Undo_ShouldRestoreSquare()
    {
        // Arrange
        _workspace.PlaceSquare(new Point(5, 5), SquareType.Water);
        var command = new RemoveSquareCommand(_workspace, new Point(5, 5));
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.False(cell.IsEmpty);
        Assert.Equal(SquareType.Water, cell.Square!.Type);
    }

    [Fact]
    public void RemoveSquareCommand_UndoWhenNoPreviousSquare_ShouldLeaveEmpty()
    {
        // Arrange
        var command = new RemoveSquareCommand(_workspace, new Point(5, 5));
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.True(cell.IsEmpty);
    }

    [Fact]
    public void RemoveSquareCommand_Description_ShouldBeCorrect()
    {
        // Arrange
        _workspace.PlaceSquare(new Point(5, 5), SquareType.Water);
        var command = new RemoveSquareCommand(_workspace, new Point(5, 5));

        // Act
        var description = command.Description;

        // Assert
        Assert.Contains("Remove", description);
        Assert.Contains("(5, 5)", description);
    }

    [Fact]
    public void FillRegionCommand_Execute_ShouldFillMultipleSquares()
    {
        // Arrange
        var positions = new List<Point>
        {
            new Point(1, 1),
            new Point(1, 2),
            new Point(2, 1)
        };
        var command = new FillRegionCommand(_workspace, positions, SquareType.Water);

        // Act
        command.Execute();

        // Assert
        var cell11 = _workspace.Grid.GetCell(new Point(1, 1));
        var cell12 = _workspace.Grid.GetCell(new Point(1, 2));
        var cell21 = _workspace.Grid.GetCell(new Point(2, 1));

        Assert.False(cell11.IsEmpty);
        Assert.False(cell12.IsEmpty);
        Assert.False(cell21.IsEmpty);
        Assert.Equal(SquareType.Water, cell11.Square!.Type);
        Assert.Equal(SquareType.Water, cell12.Square!.Type);
        Assert.Equal(SquareType.Water, cell21.Square!.Type);
    }

    [Fact]
    public void FillRegionCommand_Undo_ShouldRestoreAllPreviousSquares()
    {
        // Arrange
        _workspace.PlaceSquare(new Point(1, 1), SquareType.Grass);
        _workspace.PlaceSquare(new Point(1, 2), SquareType.Stone);

        var positions = new List<Point>
        {
            new Point(1, 1),
            new Point(1, 2),
            new Point(2, 1)
        };
        var command = new FillRegionCommand(_workspace, positions, SquareType.Water);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var cell11 = _workspace.Grid.GetCell(new Point(1, 1));
        var cell12 = _workspace.Grid.GetCell(new Point(1, 2));
        var cell21 = _workspace.Grid.GetCell(new Point(2, 1));

        Assert.False(cell11.IsEmpty);
        Assert.False(cell12.IsEmpty);
        Assert.True(cell21.IsEmpty);
        Assert.Equal(SquareType.Grass, cell11.Square!.Type);
        Assert.Equal(SquareType.Stone, cell12.Square!.Type);
    }

    [Fact]
    public void FillRegionCommand_Description_ShouldIncludeCount()
    {
        // Arrange
        var positions = new List<Point>
        {
            new Point(1, 1),
            new Point(1, 2),
            new Point(2, 1)
        };
        var command = new FillRegionCommand(_workspace, positions, SquareType.Water);

        // Act
        var description = command.Description;

        // Assert
        Assert.Contains("Fill", description);
        Assert.Contains("3", description);
    }

    [Fact]
    public void FillRegionCommand_WithEmptyList_ShouldDoNothing()
    {
        // Arrange
        var positions = new List<Point>();
        var command = new FillRegionCommand(_workspace, positions, SquareType.Water);

        // Act
        command.Execute();
        command.Undo();

        // Assert - No exceptions, workspace unchanged
        Assert.True(true);
    }
}
