using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using Xunit;

namespace MapEditor.Tests.Unit.Domain;

/// <summary>
/// Tests for Selection functionality per Specyfikacja.md:
/// "Selection = Bezpośrednio zaznaczony obszar siatki" (directly selected grid area)
/// </summary>
public class SelectionTests
{
    [Fact]
    public void Selection_CreateFromPoints_NormalizesCoordinates()
    {
        // Arrange
        var p1 = new Point(5, 5);
        var p2 = new Point(1, 1);

        // Act
        var selection = Selection.CreateFromPoints(p1, p2);

        // Assert
        Assert.True(selection.IsActive);
        Assert.Equal(new Point(1, 1), selection.StartPoint);
        Assert.Equal(new Point(5, 5), selection.EndPoint);
    }

    [Fact]
    public void Selection_CalculatesDimensions()
    {
        // Arrange - 3x4 rectangle from (1,1) to (3,4)
        var selection = Selection.CreateFromPoints(new Point(1, 1), new Point(3, 4));

        // Act & Assert
        Assert.Equal(3, selection.Width);  // 1,2,3
        Assert.Equal(4, selection.Height); // 1,2,3,4
        Assert.Equal(12, selection.Area);
    }

    [Fact]
    public void Selection_Contains_PointInsideRectangle()
    {
        // Arrange
        var selection = Selection.CreateFromPoints(new Point(0, 0), new Point(2, 2));

        // Act & Assert
        Assert.True(selection.Contains(new Point(1, 1))); // Inside
        Assert.True(selection.Contains(new Point(0, 0))); // Corner
        Assert.True(selection.Contains(new Point(2, 2))); // Corner
        Assert.False(selection.Contains(new Point(3, 3))); // Outside
    }

    [Fact]
    public void Selection_Empty_IsInactive()
    {
        // Act
        var empty = Selection.Empty();

        // Assert
        Assert.False(empty.IsActive);
        Assert.Equal(1, empty.Area); // Still has 1 cell internally
    }

    [Fact]
    public void Selection_Clear_MakesInactive()
    {
        // Arrange
        var selection = Selection.CreateFromPoints(new Point(0, 0), new Point(5, 5));
        Assert.True(selection.IsActive);

        // Act
        selection.Clear();

        // Assert
        Assert.False(selection.IsActive);
    }

    [Fact]
    public void Selection_GeneratesAllPoints()
    {
        // Arrange - 2x2 rectangle
        var selection = Selection.CreateFromPoints(new Point(0, 0), new Point(1, 1));

        // Act
        var points = selection.SelectedPoints;

        // Assert
        Assert.Equal(4, points.Count);
        Assert.Contains(new Point(0, 0), points);
        Assert.Contains(new Point(0, 1), points);
        Assert.Contains(new Point(1, 0), points);
        Assert.Contains(new Point(1, 1), points);
    }

    [Fact]
    public void Selection_SingleCell()
    {
        // Arrange - Select single cell
        var point = new Point(3, 3);

        // Act
        var selection = Selection.CreateFromPoints(point, point);

        // Assert
        Assert.Equal(1, selection.Width);
        Assert.Equal(1, selection.Height);
        Assert.Equal(1, selection.Area);
        Assert.True(selection.Contains(point));
    }
}

/// <summary>
/// Integration tests for Selection with Workspace
/// </summary>
public class WorkspaceSelectionIntegrationTests
{
    [Fact]
    public void Workspace_HasEmptySelectionByDefault()
    {
        // Arrange & Act
        var workspace = new Workspace("Test", new Size(10, 10));

        // Assert
        Assert.NotNull(workspace.Selection);
        Assert.False(workspace.Selection.IsActive);
    }

    [Fact]
    public void Workspace_CanSetSelection()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));

        // Act
        workspace.SetSelection(new Point(1, 1), new Point(3, 3));

        // Assert
        Assert.True(workspace.Selection.IsActive);
        Assert.Equal(3, workspace.Selection.Width);
        Assert.Equal(3, workspace.Selection.Height);
    }

    [Fact]
    public void Workspace_CanClearSelection()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(10, 10));
        workspace.SetSelection(new Point(1, 1), new Point(3, 3));
        Assert.True(workspace.Selection.IsActive);

        // Act
        workspace.ClearSelection();

        // Assert
        Assert.False(workspace.Selection.IsActive);
    }

    [Fact]
    public void Workspace_SelectAll_CoversEntireGrid()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 7));

        // Act
        workspace.SelectAll();

        // Assert
        Assert.True(workspace.Selection.IsActive);
        Assert.Equal(5, workspace.Selection.Width);
        Assert.Equal(7, workspace.Selection.Height);
        Assert.Equal(35, workspace.Selection.Area);
    }

    [Fact]
    public void Workspace_DeleteSelectedSquares_RemovesCells()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));

        // Place 4 squares
        workspace.ActiveGroup.PlaceSquare(new Point(1, 1), SquareType.Stone);
        workspace.ActiveGroup.PlaceSquare(new Point(2, 1), SquareType.Stone);
        workspace.ActiveGroup.PlaceSquare(new Point(1, 2), SquareType.Stone);
        workspace.ActiveGroup.PlaceSquare(new Point(2, 2), SquareType.Stone);

        Assert.Equal(4, workspace.ActiveGroup.Elements.Count);

        // Select 2x2 area
        workspace.SetSelection(new Point(1, 1), new Point(2, 2));

        // Act
        var deleted = workspace.DeleteSelectedSquares();

        // Assert
        Assert.Equal(4, deleted);
        Assert.Equal(0, workspace.ActiveGroup.Elements.Count);
    }

    [Fact]
    public void Workspace_DeleteSelectedSquares_OnlyDeletesFromActiveGroup()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));

        // Add second group
        var group2 = new Group("Group2");
        workspace.AddGroup(group2);

        // Place squares in both groups
        workspace.ActiveGroup.PlaceSquare(new Point(1, 1), SquareType.Stone);
        group2.PlaceSquare(new Point(2, 2), SquareType.Grass);

        workspace.SetSelection(new Point(0, 0), new Point(4, 4));

        // Act - Delete from active group (group 1)
        var deleted = workspace.DeleteSelectedSquares();

        // Assert
        Assert.Equal(1, deleted); // Only 1 square from active group
        Assert.Equal(0, workspace.ActiveGroup.Elements.Count);
        Assert.Equal(1, group2.Elements.Count); // Group2 unaffected
    }

    [Fact]
    public void Workspace_DeleteSelectedSquares_NoSelectionReturnsZero()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        workspace.ActiveGroup.PlaceSquare(new Point(1, 1), SquareType.Stone);

        // Act
        var deleted = workspace.DeleteSelectedSquares();

        // Assert
        Assert.Equal(0, deleted);
        Assert.Equal(1, workspace.ActiveGroup.Elements.Count); // Square still there
    }

    [Fact]
    public void Workspace_Selection_UpdatesModifiedTime()
    {
        // Arrange
        var workspace = new Workspace("Test", new Size(5, 5));
        var originalTime = workspace.ModifiedAt;
        System.Threading.Thread.Sleep(10); // Ensure time passes

        // Act
        workspace.SetSelection(new Point(0, 0), new Point(2, 2));

        // Assert
        Assert.True(workspace.ModifiedAt > originalTime);
    }
}
