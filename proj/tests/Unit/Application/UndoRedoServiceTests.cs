using MapEditor.Application.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Commands;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Application;

public class UndoRedoServiceTests
{
    private readonly UndoRedoService _service;
    private readonly Workspace _workspace;

    public UndoRedoServiceTests()
    {
        _service = new UndoRedoService();
        _workspace = new Workspace("Test", new Size(10, 10));
    }

    [Fact]
    public void ExecuteCommand_ShouldExecuteCommand()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);

        // Act
        _service.ExecuteCommand(command);

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.False(cell.IsEmpty);
        Assert.Equal(SquareType.Water, cell.Square!.Type);
    }

    [Fact]
    public void ExecuteCommand_ShouldAddToUndoStack()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);

        // Act
        _service.ExecuteCommand(command);

        // Assert
        Assert.True(_service.CanUndo);
        Assert.False(_service.CanRedo);
    }

    [Fact]
    public void ExecuteCommand_ShouldClearRedoStack()
    {
        // Arrange
        var command1 = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        var command2 = new PlaceSquareCommand(_workspace, new Point(6, 6), SquareType.Grass);

        // Act
        _service.ExecuteCommand(command1);
        _service.Undo();
        _service.ExecuteCommand(command2); // Should clear redo stack

        // Assert
        Assert.True(_service.CanUndo);
        Assert.False(_service.CanRedo);
    }

    [Fact]
    public void Undo_ShouldRevertCommand()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        _service.ExecuteCommand(command);

        // Act
        _service.Undo();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.True(cell.IsEmpty);
        Assert.False(_service.CanUndo);
        Assert.True(_service.CanRedo);
    }

    [Fact]
    public void Undo_WhenEmpty_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.Undo());
    }

    [Fact]
    public void Redo_ShouldReExecuteCommand()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        _service.ExecuteCommand(command);
        _service.Undo();

        // Act
        _service.Redo();

        // Assert
        var cell = _workspace.Grid.GetCell(new Point(5, 5));
        Assert.False(cell.IsEmpty);
        Assert.Equal(SquareType.Water, cell.Square!.Type);
        Assert.True(_service.CanUndo);
        Assert.False(_service.CanRedo);
    }

    [Fact]
    public void Redo_WhenEmpty_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _service.Redo());
    }

    [Fact]
    public void MultipleCommands_ShouldMaintainCorrectState()
    {
        // Arrange & Act
        _service.ExecuteCommand(new PlaceSquareCommand(_workspace, new Point(1, 1), SquareType.Water));
        _service.ExecuteCommand(new PlaceSquareCommand(_workspace, new Point(2, 2), SquareType.Grass));
        _service.ExecuteCommand(new PlaceSquareCommand(_workspace, new Point(3, 3), SquareType.Stone));

        // Assert
        var cell11 = _workspace.Grid.GetCell(new Point(1, 1));
        var cell22 = _workspace.Grid.GetCell(new Point(2, 2));
        var cell33 = _workspace.Grid.GetCell(new Point(3, 3));

        Assert.False(cell11.IsEmpty);
        Assert.False(cell22.IsEmpty);
        Assert.False(cell33.IsEmpty);

        // Undo twice
        _service.Undo();
        _service.Undo();

        cell11 = _workspace.Grid.GetCell(new Point(1, 1));
        cell22 = _workspace.Grid.GetCell(new Point(2, 2));
        cell33 = _workspace.Grid.GetCell(new Point(3, 3));

        Assert.False(cell11.IsEmpty);
        Assert.True(cell22.IsEmpty);
        Assert.True(cell33.IsEmpty);

        // Redo once
        _service.Redo();

        cell11 = _workspace.Grid.GetCell(new Point(1, 1));
        cell22 = _workspace.Grid.GetCell(new Point(2, 2));
        cell33 = _workspace.Grid.GetCell(new Point(3, 3));

        Assert.False(cell11.IsEmpty);
        Assert.False(cell22.IsEmpty);
        Assert.True(cell33.IsEmpty);
    }

    [Fact]
    public void Clear_ShouldEmptyBothStacks()
    {
        // Arrange
        _service.ExecuteCommand(new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water));
        _service.Undo();

        // Act
        _service.Clear();

        // Assert
        Assert.False(_service.CanUndo);
        Assert.False(_service.CanRedo);
    }

    [Fact]
    public void GetUndoDescription_ShouldReturnCommandDescription()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        _service.ExecuteCommand(command);

        // Act
        var description = _service.GetUndoDescription();

        // Assert
        Assert.Contains("Place", description);
    }

    [Fact]
    public void GetRedoDescription_ShouldReturnCommandDescription()
    {
        // Arrange
        var command = new PlaceSquareCommand(_workspace, new Point(5, 5), SquareType.Water);
        _service.ExecuteCommand(command);
        _service.Undo();

        // Act
        var description = _service.GetRedoDescription();

        // Assert
        Assert.Contains("Place", description);
    }

    [Fact]
    public void HistoryLimit_ShouldNotExceedMaxSize()
    {
        // Arrange
        var service = new UndoRedoService(5); // Max 5 commands

        // Act - Add 10 commands
        for (int i = 0; i < 10; i++)
        {
            service.ExecuteCommand(new PlaceSquareCommand(_workspace, new Point(i, i), SquareType.Water));
        }

        // Assert - Can only undo 5 times
        int undoCount = 0;
        while (service.CanUndo)
        {
            service.Undo();
            undoCount++;
        }

        Assert.Equal(5, undoCount);
    }
}
