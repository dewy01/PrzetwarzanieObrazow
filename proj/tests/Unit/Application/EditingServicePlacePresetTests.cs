using MapEditor.Application.Services;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using MapEditor.Infrastructure.Algorithms;
using MapEditor.Infrastructure.Repositories;
using Xunit;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Biometric.Services;

namespace MapEditor.Tests.Unit.Application;

/// <summary>
/// Tests for EditingService.PlacePreset integration
/// </summary>
public class EditingServicePlacePresetTests
{
    private EditingService CreateEditingService()
    {
        var repository = new WorkspaceFileRepository();
        var ul22Converter = new UL22Converter();
        var fragmentationService = new FragmentationService();
        var undoRedoService = new UndoRedoService();

        return new EditingService(repository, fragmentationService, ul22Converter, undoRedoService);
    }

    [Fact]
    public async Task PlacePreset_PlacesPresetInWorkspace()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Stone, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        // Act
        editingService.PlacePreset(5, 5, preset);

        // Assert
        var cell1 = workspace.Grid.GetCell(new Point(5, 5));
        var cell2 = workspace.Grid.GetCell(new Point(6, 5));

        Assert.NotNull(cell1.Square);
        Assert.Equal(SquareType.Grass, cell1.Square.Type);

        Assert.NotNull(cell2.Square);
        Assert.Equal(SquareType.Stone, cell2.Square.Type);
    }

    [Fact]
    public async Task PlacePreset_EnablesUndo()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);
        workspace.PlaceSquare(new Point(5, 5), SquareType.Water);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        // Act
        editingService.PlacePreset(5, 5, preset);

        Assert.True(editingService.CanUndo);

        editingService.Undo();

        // Assert
        var cell = workspace.Grid.GetCell(new Point(5, 5));
        Assert.NotNull(cell.Square);
        Assert.Equal(SquareType.Water, cell.Square.Type);
    }

    [Fact]
    public async Task PlacePreset_RaisesWorkspaceChangedEvent()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        bool eventRaised = false;
        editingService.WorkspaceChanged += (sender, e) => { eventRaised = true; };

        // Act
        editingService.PlacePreset(5, 5, preset);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public async Task PlacePreset_ThrowsWhenNoWorkspaceLoaded()
    {
        // Arrange
        var editingService = CreateEditingService();

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            editingService.PlacePreset(5, 5, preset)
        );
    }

    [Fact]
    public async Task PlacePreset_ThrowsOnNullPreset()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            editingService.PlacePreset(5, 5, null!)
        );
    }

    [Fact]
    public async Task PlacePreset_OverwritesExistingSquares()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);
        workspace.PlaceSquare(new Point(5, 5), SquareType.Water);
        workspace.PlaceSquare(new Point(6, 5), SquareType.Sand);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0),
            new SquareDefinition(new Point(1, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(2, 1), squares);

        // Act
        editingService.PlacePreset(5, 5, preset);

        // Assert
        var cell1 = workspace.Grid.GetCell(new Point(5, 5));
        var cell2 = workspace.Grid.GetCell(new Point(6, 5));

        Assert.NotNull(cell1.Square);
        Assert.Equal(SquareType.Stone, cell1.Square.Type);

        Assert.NotNull(cell2.Square);
        Assert.Equal(SquareType.Grass, cell2.Square.Type);
    }

    [Fact]
    public async Task PlacePreset_AndUndo_RestoresOverwrittenSquares()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);
        workspace.PlaceSquare(new Point(5, 5), SquareType.Water);

        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        };
        var preset = new Preset("TestPreset", new Size(1, 1), squares);

        // Act
        editingService.PlacePreset(5, 5, preset);
        var afterPlace = workspace.Grid.GetCell(new Point(5, 5)).Square?.Type;

        editingService.Undo();
        var afterUndo = workspace.Grid.GetCell(new Point(5, 5)).Square?.Type;

        // Assert
        Assert.Equal(SquareType.Grass, afterPlace);
        Assert.Equal(SquareType.Water, afterUndo);
    }

    [Fact]
    public async Task PlacePreset_MultiplePresets_WithUndoRedo()
    {
        // Arrange
        var editingService = CreateEditingService();
        var workspace = await editingService.CreateWorkspaceAsync("TestMap", 10, 10);

        var preset1 = new Preset("Preset1", new Size(1, 1), new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass, 0)
        });

        var preset2 = new Preset("Preset2", new Size(1, 1), new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Stone, 0)
        });

        // Act
        editingService.PlacePreset(5, 5, preset1);
        editingService.PlacePreset(6, 5, preset2);

        var afterPlace = new[]
        {
            workspace.Grid.GetCell(new Point(5, 5)).Square?.Type,
            workspace.Grid.GetCell(new Point(6, 5)).Square?.Type
        };

        editingService.Undo();
        editingService.Undo();

        var afterUndoAll = new[]
        {
            workspace.Grid.GetCell(new Point(5, 5)).Square,
            workspace.Grid.GetCell(new Point(6, 5)).Square
        };

        // Assert
        Assert.Equal(SquareType.Grass, afterPlace[0]);
        Assert.Equal(SquareType.Stone, afterPlace[1]);

        Assert.Null(afterUndoAll[0]);
        Assert.Null(afterUndoAll[1]);
    }
}
