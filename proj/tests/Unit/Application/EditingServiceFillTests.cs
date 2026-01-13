using MapEditor.Application.Services;
using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using Xunit;

namespace MapEditor.Tests.Unit.Application;

public class EditingServiceFillTests
{
    private readonly IWorkspaceRepository _repository;
    private readonly IFragmentationService _fragmentationService;
    private readonly IUL22Converter _ul22Converter;
    private readonly UndoRedoService _undoRedoService;
    private readonly EditingService _editingService;

    public EditingServiceFillTests()
    {
        _repository = new InMemoryWorkspaceRepository();
        _fragmentationService = new MapEditor.Infrastructure.Algorithms.FragmentationService();
        _ul22Converter = new MapEditor.Infrastructure.Algorithms.UL22Converter();
        _undoRedoService = new UndoRedoService();
        _editingService = new EditingService(_repository, _fragmentationService, _ul22Converter, _undoRedoService);
    }

    [Fact]
    public void FillRegion_WithNoWorkspace_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _editingService.FillRegion(0, 0, SquareType.Grass));
    }

    [Fact]
    public async Task FillRegion_WithEmptyWorkspace_FillsEntireWorkspace()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Act - click on empty cell with fill mode
        _editingService.FillRegion(5, 5, SquareType.Grass);

        // Assert - entire empty region should be filled
        var workspace = _editingService.CurrentWorkspace;
        Assert.NotNull(workspace);
        var filledCells = workspace.Grid.GetAllCells().Where(c => c.Square != null).ToList();
        Assert.NotEmpty(filledCells);
        Assert.All(filledCells, cell => Assert.Equal(SquareType.Grass, cell.Square?.Type));
    }

    [Fact]
    public async Task FillRegion_WithSingleSquare_FillsOnlyThatSquare()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceSquare(5, 5, SquareType.Stone);

        // Act - fill the region containing (5,5)
        _editingService.FillRegion(5, 5, SquareType.Grass);

        // Assert
        var workspace = _editingService.CurrentWorkspace;
        Assert.NotNull(workspace);
        var cell = workspace.Grid.GetCell(new Point(5, 5));
        Assert.NotNull(cell.Square);
        Assert.Equal(SquareType.Grass, cell.Square.Type);
    }

    [Fact]
    public async Task FillRegion_WithConnectedRegion_FillsAllConnectedSquares()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Create a 2x2 connected region
        _editingService.PlaceSquare(3, 3, SquareType.Stone);
        _editingService.PlaceSquare(4, 3, SquareType.Stone);
        _editingService.PlaceSquare(3, 4, SquareType.Stone);
        _editingService.PlaceSquare(4, 4, SquareType.Stone);

        // Act - click on one square in the region
        _editingService.FillRegion(3, 3, SquareType.Grass);

        // Assert - all 4 squares should be filled
        var workspace = _editingService.CurrentWorkspace;
        Assert.NotNull(workspace);

        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(3, 3)).Square?.Type);
        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(4, 3)).Square?.Type);
        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(3, 4)).Square?.Type);
        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(4, 4)).Square?.Type);
    }

    [Fact]
    public async Task FillRegion_WithSeparateRegions_FillsOnlyClickedRegion()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Create two separate regions
        _editingService.PlaceSquare(2, 2, SquareType.Stone);
        _editingService.PlaceSquare(2, 3, SquareType.Stone);

        _editingService.PlaceSquare(7, 7, SquareType.Stone);
        _editingService.PlaceSquare(7, 8, SquareType.Stone);

        // Act - fill only the first region
        _editingService.FillRegion(2, 2, SquareType.Grass);

        // Assert - first region changed, second unchanged
        var workspace = _editingService.CurrentWorkspace;
        Assert.NotNull(workspace);

        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(2, 2)).Square?.Type);
        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(2, 3)).Square?.Type);

        Assert.Equal(SquareType.Stone, workspace.Grid.GetCell(new Point(7, 7)).Square?.Type);
        Assert.Equal(SquareType.Stone, workspace.Grid.GetCell(new Point(7, 8)).Square?.Type);
    }

    [Fact]
    public async Task FillRegion_WithDiagonalConnection_FillsAll8ConnectedSquares()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);

        // Create diagonal connection
        _editingService.PlaceSquare(3, 3, SquareType.Stone);
        _editingService.PlaceSquare(4, 4, SquareType.Stone); // diagonal

        // Act
        _editingService.FillRegion(3, 3, SquareType.Grass);

        // Assert - both should be filled (8-connectivity)
        var workspace = _editingService.CurrentWorkspace;
        Assert.NotNull(workspace);

        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(3, 3)).Square?.Type);
        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(4, 4)).Square?.Type);
    }

    [Fact]
    public async Task FillRegion_ClickingEmptyRegion_FillsConnectedEmptyArea()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceSquare(2, 2, SquareType.Stone);

        // Act - click on empty cell region
        _editingService.FillRegion(5, 5, SquareType.Grass);

        // Assert - original stone unchanged, empty region should be filled with Grass
        var workspace = _editingService.CurrentWorkspace;
        Assert.NotNull(workspace);
        Assert.Equal(SquareType.Stone, workspace.Grid.GetCell(new Point(2, 2)).Square?.Type);
        // The clicked position (5,5) should now have Grass (it was filled as part of the empty region)
        Assert.Equal(SquareType.Grass, workspace.Grid.GetCell(new Point(5, 5)).Square?.Type);
    }

    [Fact]
    public async Task FillRegion_RaisesWorkspaceChangedEvent()
    {
        // Arrange
        await _editingService.CreateWorkspaceAsync("Test", 10, 10);
        _editingService.PlaceSquare(3, 3, SquareType.Stone);

        bool eventRaised = false;
        _editingService.WorkspaceChanged += (sender, args) => eventRaised = true;

        // Act
        _editingService.FillRegion(3, 3, SquareType.Grass);

        // Assert
        Assert.True(eventRaised);
    }
}

// Helper in-memory repository for testing
internal class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    public Task<Workspace> CreateAsync(string name, Size size)
    {
        return Task.FromResult(new Workspace(name, size));
    }

    public Task<Workspace> LoadAsync(string filePath)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(Workspace workspace, string filePath)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string filePath)
    {
        return Task.FromResult(false);
    }
}
