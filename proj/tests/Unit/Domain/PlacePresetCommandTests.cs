using MapEditor.Domain.Editing.Commands;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using Xunit;

namespace MapEditor.Tests.Unit.Domain;

/// <summary>
/// Tests for PlacePresetCommand with undo/redo support
/// </summary>
public class PlacePresetCommandTests
{
    [Fact]
    public void Execute_PlacesPresetAtPosition()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(2, 2), SquareType.Grass);
        workspace.PlaceSquare(new Point(3, 2), SquareType.Water);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        var command = new PlacePresetCommand(workspace, new Point(2, 2), preset);

        // Act
        command.Execute();

        // Assert
        // Check that preset squares are placed
        var cell1 = workspace.Grid.GetCell(new Point(2, 2));
        Assert.NotNull(cell1.Square);
        Assert.Equal(SquareType.Stone, cell1.Square.Type);

        var cell2 = workspace.Grid.GetCell(new Point(3, 2));
        Assert.NotNull(cell2.Square);
        Assert.Equal(SquareType.Grass, cell2.Square.Type);
    }

    [Fact]
    public void Undo_RestoresPreviousState()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(2, 2), SquareType.Grass);
        workspace.PlaceSquare(new Point(3, 2), SquareType.Water);

        var originalGrass = workspace.Grid.GetCell(new Point(2, 2)).Square;
        var originalWater = workspace.Grid.GetCell(new Point(3, 2)).Square;

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Sand, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        var command = new PlacePresetCommand(workspace, new Point(2, 2), preset);

        // Act
        command.Execute();
        command.Undo();

        // Assert
        var cell1 = workspace.Grid.GetCell(new Point(2, 2));
        Assert.NotNull(cell1.Square);
        Assert.Equal(SquareType.Grass, cell1.Square.Type);

        var cell2 = workspace.Grid.GetCell(new Point(3, 2));
        Assert.NotNull(cell2.Square);
        Assert.Equal(SquareType.Water, cell2.Square.Type);
    }

    [Fact]
    public void Undo_RemovesPlacedPressetSquares()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));

        // Empty cells where preset will be placed
        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        var command = new PlacePresetCommand(workspace, new Point(5, 5), preset);

        // Act
        command.Execute();

        // Verify preset was placed
        var cell1Before = workspace.Grid.GetCell(new Point(5, 5));
        Assert.NotNull(cell1Before.Square);

        command.Undo();

        // Assert
        var cell1 = workspace.Grid.GetCell(new Point(5, 5));
        var cell2 = workspace.Grid.GetCell(new Point(6, 5));
        Assert.Null(cell1.Square);
        Assert.Null(cell2.Square);
    }

    [Fact]
    public void Constructor_ThrowsOnNullWorkspace()
    {
        // Arrange
        var squares = new List<SquareDefinition> { new SquareDefinition(new Point(0, 0), SquareType.Grass, 0) };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PlacePresetCommand(null!, new Point(0, 0), preset)
        );
    }

    [Fact]
    public void Constructor_ThrowsOnNullPreset()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PlacePresetCommand(workspace, new Point(0, 0), null!)
        );
    }

    [Fact]
    public void Description_ShowsPresetNameAndPosition()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        var squares = new List<SquareDefinition> { new SquareDefinition(new Point(0, 0), SquareType.Grass, 0) };
        var preset = new Preset("MyPreset", new Size(1, 1), squares);

        var command = new PlacePresetCommand(workspace, new Point(3, 5), preset);

        // Act
        var description = command.Description;

        // Assert
        Assert.Contains("MyPreset", description);
        Assert.Contains("3", description);
        Assert.Contains("5", description);
    }

    [Fact]
    public void Execute_OverwritesExistingSquares()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(5, 5), SquareType.Water);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        var command = new PlacePresetCommand(workspace, new Point(5, 5), preset);

        // Act
        command.Execute();

        // Assert
        var cell = workspace.Grid.GetCell(new Point(5, 5));
        Assert.NotNull(cell.Square);
        Assert.Equal(SquareType.Stone, cell.Square.Type);
    }

    [Fact]
    public void Execute_MultipleUndoRedo_MaintainsCorrectState()
    {
        // Arrange
        var workspace = new Workspace("TestMap", new Size(10, 10));
        workspace.PlaceSquare(new Point(2, 2), SquareType.Grass);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        var command = new PlacePresetCommand(workspace, new Point(2, 2), preset);

        // Act
        command.Execute();
        var afterExecute = workspace.Grid.GetCell(new Point(2, 2)).Square?.Type;

        command.Undo();
        var afterUndo = workspace.Grid.GetCell(new Point(2, 2)).Square?.Type;

        command.Execute();
        var afterRedo = workspace.Grid.GetCell(new Point(2, 2)).Square?.Type;

        // Assert
        Assert.Equal(SquareType.Stone, afterExecute);
        Assert.Equal(SquareType.Grass, afterUndo);
        Assert.Equal(SquareType.Stone, afterRedo);
    }
}
