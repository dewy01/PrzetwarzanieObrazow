using MapEditor.Application.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Editing.Services;

namespace MapEditor.Tests.Unit.Application;

public class GroupSwitchingTests
{
    private readonly EditingService _editingService;
    private readonly InMemoryWorkspaceRepository _repository;
    private readonly UndoRedoService _undoRedoService;

    public GroupSwitchingTests()
    {
        _repository = new InMemoryWorkspaceRepository();
        var fragmentationService = new MapEditor.Infrastructure.Algorithms.FragmentationService();
        var ul22Converter = new MapEditor.Infrastructure.Algorithms.UL22Converter();
        _undoRedoService = new UndoRedoService();
        _editingService = new EditingService(_repository, fragmentationService, ul22Converter, _undoRedoService);
    }

    [Fact]
    public async Task AddGroup_ShouldAddNewGroup()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Act
        _editingService.AddGroup("Group 2");

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        Assert.Equal(2, _editingService.CurrentWorkspace.Groups.Count);
        Assert.Contains(_editingService.CurrentWorkspace.Groups, g => g.Name == "Group 2");
    }

    [Fact]
    public async Task SetActiveGroup_ShouldChangeActiveGroup()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.AddGroup("Group 2");
        var group2 = _editingService.CurrentWorkspace!.Groups.First(g => g.Name == "Group 2");

        // Act
        _editingService.SetActiveGroup(group2);

        // Assert
        Assert.Equal("Group 2", _editingService.CurrentWorkspace.ActiveGroup.Name);
        Assert.True(group2.IsActive);
    }

    [Fact]
    public async Task SetActiveGroup_ShouldDeactivatePreviousGroup()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        var defaultGroup = _editingService.CurrentWorkspace!.ActiveGroup;
        _editingService.AddGroup("Group 2");
        var group2 = _editingService.CurrentWorkspace.Groups.First(g => g.Name == "Group 2");

        // Act
        _editingService.SetActiveGroup(group2);

        // Assert
        Assert.False(defaultGroup.IsActive);
        Assert.True(group2.IsActive);
    }

    [Fact]
    public async Task SetActiveGroup_WithInvalidGroup_ShouldThrow()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        var otherWorkspace = new Workspace("Other", new Size(5, 5));
        var otherGroup = otherWorkspace.Groups[0];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _editingService.SetActiveGroup(otherGroup));
    }

    [Fact]
    public async Task AddGroup_WithDuplicateName_ShouldThrow()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.AddGroup("Group 2");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _editingService.AddGroup("Group 2"));
    }

    [Fact]
    public async Task NewWorkspace_ShouldHaveDefaultActiveGroup()
    {
        // Act
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Assert
        Assert.NotNull(_editingService.CurrentWorkspace);
        Assert.Single(_editingService.CurrentWorkspace.Groups);
        Assert.Equal("Default", _editingService.CurrentWorkspace.ActiveGroup.Name);
        Assert.True(_editingService.CurrentWorkspace.ActiveGroup.IsActive);
    }

    [Fact]
    public async Task SetActiveGroup_ShouldRaiseWorkspaceChanged()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.AddGroup("Group 2");
        var group2 = _editingService.CurrentWorkspace!.Groups.First(g => g.Name == "Group 2");

        bool eventRaised = false;
        _editingService.WorkspaceChanged += (s, e) => eventRaised = true;

        // Act
        _editingService.SetActiveGroup(group2);

        // Assert
        Assert.True(eventRaised);
    }
}
