using MapEditor.Domain.Editing.Commands;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Domain;

public class EntityCommandsTests
{
    [Fact]
    public void PlaceEntityCommand_Execute_ShouldAddEntity()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        var command = new PlaceEntityCommand(workspace, position, EntityType.Player, null);

        // Act
        command.Execute();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.NotNull(entity);
        Assert.Equal(EntityType.Player, entity.Type);
    }

    [Fact]
    public void PlaceEntityCommand_Undo_ShouldRemoveEntity()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        var command = new PlaceEntityCommand(workspace, position, EntityType.Enemy, null);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.Null(entity);
    }

    [Fact]
    public void PlaceEntityCommand_UndoWithPreviousEntity_ShouldRestorePrevious()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        workspace.PlaceEntity(position, EntityType.Player, null);
        var command = new PlaceEntityCommand(workspace, position, EntityType.Enemy, null);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.NotNull(entity);
        Assert.Equal(EntityType.Player, entity.Type);
    }

    [Fact]
    public void RemoveEntityCommand_Execute_ShouldRemoveEntity()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        workspace.PlaceEntity(position, EntityType.Checkpoint, null);
        var command = new RemoveEntityCommand(workspace, position);

        // Act
        command.Execute();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.Null(entity);
    }

    [Fact]
    public void RemoveEntityCommand_Undo_ShouldRestoreEntity()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        workspace.PlaceEntity(position, EntityType.Collectible, "Coin");
        var command = new RemoveEntityCommand(workspace, position);
        command.Execute();

        // Act
        command.Undo();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.NotNull(entity);
        Assert.Equal(EntityType.Collectible, entity.Type);
        Assert.Equal("Coin", entity.Name);
    }

    [Fact]
    public void RemoveEntityCommand_WithNoEntity_ShouldDoNothing()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        var command = new RemoveEntityCommand(workspace, position);

        // Act
        command.Execute();

        // Assert - should not throw
        Assert.Null(workspace.GetEntityAt(position));
    }

    [Fact]
    public void PlaceEntityCommand_WithCustomName_ShouldSetName()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(3, 7);
        var command = new PlaceEntityCommand(workspace, position, EntityType.Enemy, "Dragon");

        // Act
        command.Execute();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.NotNull(entity);
        Assert.Equal("Dragon", entity.Name);
    }

    [Fact]
    public void Commands_ExecuteUndoExecute_ShouldWorkCorrectly()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        var position = new Point(5, 5);
        var command = new PlaceEntityCommand(workspace, position, EntityType.StartPoint, null);

        // Act
        command.Execute();
        command.Undo();
        command.Execute();

        // Assert
        var entity = workspace.GetEntityAt(position);
        Assert.NotNull(entity);
        Assert.Equal(EntityType.StartPoint, entity.Type);
    }
}
