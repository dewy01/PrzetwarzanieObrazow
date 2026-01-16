using MapEditor.Domain.Editing.Commands;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Application;

public class GroupRemovalTests
{
    [Fact]
    public void RemoveGroup_WithValidGroup_ShouldRemoveGroup()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var group1 = new Group("Group1");
        workspace.AddGroup(group1);
        var group2 = new Group("Group2");
        workspace.AddGroup(group2);

        var initialCount = workspace.Groups.Count;

        // Act
        workspace.RemoveGroup(group1);

        // Assert
        Assert.Equal(initialCount - 1, workspace.Groups.Count);
        Assert.DoesNotContain(group1, workspace.Groups);
        Assert.Contains(group2, workspace.Groups);
    }

    [Fact]
    public void RemoveGroup_WhenGroupIsActive_ShouldSwitchToAnotherGroup()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var defaultGroup = workspace.ActiveGroup;
        var group1 = new Group("Group1");
        workspace.AddGroup(group1);

        workspace.SetActiveGroup(group1);
        Assert.Equal(group1, workspace.ActiveGroup);

        // Act
        workspace.RemoveGroup(group1);

        // Assert
        Assert.NotEqual(group1, workspace.ActiveGroup);
        Assert.Contains(workspace.ActiveGroup, workspace.Groups);
    }

    [Fact]
    public void RemoveGroup_WithLastGroup_ShouldThrowException()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var defaultGroup = workspace.ActiveGroup;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => workspace.RemoveGroup(defaultGroup));
    }

    [Fact]
    public void RemoveGroup_WithNonExistentGroup_ShouldThrowException()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var externalGroup = new Group("ExternalGroup");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => workspace.RemoveGroup(externalGroup));
    }

    [Fact]
    public void RemoveGroup_ShouldClearGroupContents()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var group = new Group("TestGroup");
        workspace.AddGroup(group);

        // Add squares to group
        group.PlaceSquare(new Point(1, 1), SquareType.Grass);
        group.PlaceSquare(new Point(2, 2), SquareType.Stone);
        group.PlaceEntity(new Point(3, 3), EntityType.StartPoint);

        Assert.NotEmpty(group.Elements);
        Assert.NotEmpty(group.Entities);

        // Act
        workspace.RemoveGroup(group);

        // Assert
        Assert.Empty(group.Elements);
        Assert.Empty(group.Entities);
    }

    [Fact]
    public void RemoveGroupCommand_ShouldExecuteAndUndo()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var group = new Group("TestGroup");
        workspace.AddGroup(group);

        group.PlaceSquare(new Point(1, 1), SquareType.Grass);
        group.PlaceEntity(new Point(2, 2), EntityType.StartPoint);

        var initialCount = workspace.Groups.Count;
        var command = new RemoveGroupCommand(workspace, group);

        // Act - Execute
        command.Execute();

        // Assert - After execute
        Assert.Equal(initialCount - 1, workspace.Groups.Count);
        Assert.DoesNotContain(group, workspace.Groups);

        // Act - Undo
        command.Undo();

        // Assert - After undo
        Assert.Equal(initialCount, workspace.Groups.Count);
        Assert.Contains(group, workspace.Groups);
        Assert.NotEmpty(group.Elements);
        Assert.NotEmpty(group.Entities);
    }

    [Fact]
    public void RemoveGroupCommand_ShouldRestoreActiveGroupOnUndo()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var group1 = new Group("Group1");
        var group2 = new Group("Group2");
        workspace.AddGroup(group1);
        workspace.AddGroup(group2);

        workspace.SetActiveGroup(group2);
        var command = new RemoveGroupCommand(workspace, group1);

        // Act - Execute
        command.Execute();
        var activeAfterRemove = workspace.ActiveGroup;

        // Act - Undo
        command.Undo();

        // Assert
        Assert.Equal(group2, workspace.ActiveGroup);
    }

    [Fact]
    public void RemoveGroupCommand_Description_ShouldReturnMeaningfulMessage()
    {
        // Arrange
        var workspace = new Workspace("TestWorkspace", new Size(10, 10));
        var group = new Group("MyTestGroup");
        workspace.AddGroup(group);

        var command = new RemoveGroupCommand(workspace, group);

        // Assert
        Assert.Contains("MyTestGroup", command.Description);
    }

    [Fact]
    public void GroupClear_ShouldRemoveAllElementsAndEntities()
    {
        // Arrange
        var group = new Group("TestGroup");
        group.PlaceSquare(new Point(1, 1), SquareType.Grass);
        group.PlaceSquare(new Point(2, 2), SquareType.Stone);
        group.PlaceEntity(new Point(3, 3), EntityType.StartPoint);
        group.PlaceEntity(new Point(4, 4), EntityType.EndPoint);

        Assert.NotEmpty(group.Elements);
        Assert.NotEmpty(group.Entities);

        // Act
        group.Clear();

        // Assert
        Assert.Empty(group.Elements);
        Assert.Empty(group.Entities);
    }
}
