using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Domain;

public class WorkspaceTests
{
    [Fact]
    public void CreateWorkspace_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var name = "Test Map";
        var size = new Size(10, 10);

        // Act
        var workspace = new Workspace(name, size);

        // Assert
        Assert.NotNull(workspace);
        Assert.Equal(name, workspace.Name);
        Assert.Equal(10, workspace.Grid.Size.Width);
        Assert.Equal(10, workspace.Grid.Size.Height);
        Assert.Single(workspace.Groups); // Default group
    }

    [Fact]
    public void PlaceSquare_AtValidPosition_ShouldSucceed()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        var position = new Point(2, 2);

        // Act
        workspace.PlaceSquare(position, SquareType.Grass);

        // Assert
        var cell = workspace.Grid.GetCell(position);
        Assert.False(cell.IsEmpty);
        Assert.Equal(SquareType.Grass, cell.Square!.Type);
    }

    [Fact]
    public void PlaceSquare_OutsideBounds_ShouldThrow()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        var position = new Point(10, 10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            workspace.PlaceSquare(position, SquareType.Grass));
    }

    [Fact]
    public void RemoveSquare_AtOccupiedPosition_ShouldSucceed()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        var position = new Point(2, 2);
        workspace.PlaceSquare(position, SquareType.Stone);

        // Act
        workspace.RemoveSquare(position);

        // Assert
        var cell = workspace.Grid.GetCell(position);
        Assert.True(cell.IsEmpty);
    }

    [Fact]
    public void AddGroup_WithUniqueName_ShouldSucceed()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        var group = new Group("Layer 1");

        // Act
        workspace.AddGroup(group);

        // Assert
        Assert.Equal(2, workspace.Groups.Count); // Default + new
        Assert.Contains(group, workspace.Groups);
    }

    [Fact]
    public void SetActiveGroup_WithValidGroup_ShouldSucceed()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        var group = new Group("Layer 1");
        workspace.AddGroup(group);

        // Act
        workspace.SetActiveGroup(group);

        // Assert
        Assert.Equal(group, workspace.ActiveGroup);
        Assert.True(group.IsActive);
    }
}
