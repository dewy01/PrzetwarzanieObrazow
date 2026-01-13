using System.Linq;
using MapEditor.Application.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Application;

public class EntityPlacementTests
{
    private readonly EditingService _editingService;
    private readonly InMemoryWorkspaceRepository _repository;
    private readonly UndoRedoService _undoRedoService;

    public EntityPlacementTests()
    {
        _repository = new InMemoryWorkspaceRepository();
        var fragmentationService = new MapEditor.Infrastructure.Algorithms.FragmentationService();
        var ul22Converter = new MapEditor.Infrastructure.Algorithms.UL22Converter();
        _undoRedoService = new UndoRedoService();
        _editingService = new EditingService(_repository, fragmentationService, ul22Converter, _undoRedoService);
    }

    [Fact]
    public async Task PlaceEntity_ShouldAddEntityToWorkspace()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Act
        _editingService.PlaceEntity(5, 5, EntityType.Player);

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        var entity = _editingService.CurrentWorkspace.GetEntityAt(new Point(5, 5));
        Assert.NotNull(entity);
        Assert.Equal(EntityType.Player, entity.Type);
    }

    [Fact]
    public async Task PlaceEntity_ShouldReplaceExistingEntity()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceEntity(5, 5, EntityType.Player);

        // Act
        _editingService.PlaceEntity(5, 5, EntityType.Enemy);

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        var entity = _editingService.CurrentWorkspace.GetEntityAt(new Point(5, 5));
        Assert.NotNull(entity);
        Assert.Equal(EntityType.Enemy, entity.Type);
        Assert.Single(_editingService.CurrentWorkspace.Entities);
    }

    [Fact]
    public async Task RemoveEntity_ShouldRemoveEntityFromWorkspace()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceEntity(5, 5, EntityType.Player);

        // Act
        _editingService.RemoveEntity(5, 5);

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        var entity = _editingService.CurrentWorkspace.GetEntityAt(new Point(5, 5));
        Assert.Null(entity);
        Assert.Empty(_editingService.CurrentWorkspace.Entities);
    }

    [Fact]
    public async Task PlaceEntity_ShouldSupportUndo()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceEntity(5, 5, EntityType.Player);

        // Act
        _editingService.Undo();

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        var entity = _editingService.CurrentWorkspace.GetEntityAt(new Point(5, 5));
        Assert.Null(entity);
    }

    [Fact]
    public async Task PlaceEntity_ShouldSupportRedo()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceEntity(5, 5, EntityType.Player);
        _editingService.Undo();

        // Act
        _editingService.Redo();

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        var entity = _editingService.CurrentWorkspace.GetEntityAt(new Point(5, 5));
        Assert.NotNull(entity);
        Assert.Equal(EntityType.Player, entity.Type);
    }

    [Fact]
    public async Task PlaceEntity_WithName_ShouldSetCustomName()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Act
        _editingService.PlaceEntity(5, 5, EntityType.Enemy, "Boss");

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        var entity = _editingService.CurrentWorkspace.GetEntityAt(new Point(5, 5));
        Assert.NotNull(entity);
        Assert.Equal("Boss", entity.Name);
    }

    [Fact]
    public async Task PlaceEntity_OutsideBounds_ShouldThrow()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _editingService.PlaceEntity(15, 15, EntityType.Player));
    }

    [Fact]
    public async Task MultipleEntities_ShouldCoexist()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Act
        _editingService.PlaceEntity(1, 1, EntityType.StartPoint);
        _editingService.PlaceEntity(5, 5, EntityType.Player);
        _editingService.PlaceEntity(9, 9, EntityType.EndPoint);

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        Assert.Equal(3, _editingService.CurrentWorkspace.Entities.Count());
    }
}
