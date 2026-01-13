using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Commands;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Application.Services;

/// <summary>
/// Application Service dla operacji edycji Workspace
/// </summary>
public class EditingService
{
    private readonly IWorkspaceRepository _repository;
    private readonly IFragmentationService _fragmentationService;
    private readonly IUL22Converter _ul22Converter;
    private readonly UndoRedoService _undoRedoService;
    private Workspace? _currentWorkspace;

    public EditingService(IWorkspaceRepository repository, IFragmentationService fragmentationService, IUL22Converter ul22Converter, UndoRedoService undoRedoService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _fragmentationService = fragmentationService ?? throw new ArgumentNullException(nameof(fragmentationService));
        _ul22Converter = ul22Converter ?? throw new ArgumentNullException(nameof(ul22Converter));
        _undoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));
    }

    public Workspace? CurrentWorkspace => _currentWorkspace;
    public bool CanUndo => _undoRedoService.CanUndo;
    public bool CanRedo => _undoRedoService.CanRedo;

    public event EventHandler? WorkspaceChanged;

    public async Task<Workspace> CreateWorkspaceAsync(string name, int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and height must be positive");

        _currentWorkspace = await _repository.CreateAsync(name, new Size(width, height));
        OnWorkspaceChanged();
        return _currentWorkspace;
    }

    public async Task SaveWorkspaceAsync(string filePath)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace to save");

        await _repository.SaveAsync(_currentWorkspace, filePath);
    }

    public async Task<Workspace> LoadWorkspaceAsync(string filePath)
    {
        _currentWorkspace = await _repository.LoadAsync(filePath);
        OnWorkspaceChanged();
        return _currentWorkspace;
    }

    public void PlaceSquare(int x, int y, SquareType squareType)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        var command = new PlaceSquareCommand(_currentWorkspace, new Point(x, y), squareType);
        _undoRedoService.ExecuteCommand(command);
        OnWorkspaceChanged();
    }

    public void RemoveSquare(int x, int y)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        var command = new RemoveSquareCommand(_currentWorkspace, new Point(x, y));
        _undoRedoService.ExecuteCommand(command);
        OnWorkspaceChanged();
    }

    public void AddGroup(string groupName)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        var group = new Group(groupName);
        _currentWorkspace.AddGroup(group);
        OnWorkspaceChanged();
    }

    public void SetActiveGroup(Group group)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        _currentWorkspace.SetActiveGroup(group);
        OnWorkspaceChanged();
    }

    public void PlaceEntity(int x, int y, Domain.Shared.Enums.EntityType entityType, string? name = null)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        var command = new PlaceEntityCommand(_currentWorkspace, new Point(x, y), entityType, name);
        _undoRedoService.ExecuteCommand(command);
        OnWorkspaceChanged();
    }

    public void RemoveEntity(int x, int y)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        var command = new RemoveEntityCommand(_currentWorkspace, new Point(x, y));
        _undoRedoService.ExecuteCommand(command);
        OnWorkspaceChanged();
    }

    public void FillRegion(int x, int y, SquareType squareType)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        var clickPosition = new Point(x, y);
        var grid = _currentWorkspace.Grid;
        var cell = grid.GetCell(clickPosition);

        // If clicking on a filled square, use flood fill for same square type
        if (!cell.IsEmpty && cell.Square != null)
        {
            var targetType = cell.Square.Type;
            var positions = FloodFillSameType(x, y, targetType, grid);

            if (positions.Count > 0)
            {
                var command = new FillRegionCommand(_currentWorkspace, positions, squareType);
                _undoRedoService.ExecuteCommand(command);
                OnWorkspaceChanged();
            }
            return;
        }

        // If clicking on empty space, use flood fill for empty cells
        var emptyPositions = FloodFillEmpty(x, y, grid);

        if (emptyPositions.Count > 0)
        {
            var command = new FillRegionCommand(_currentWorkspace, emptyPositions, squareType);
            _undoRedoService.ExecuteCommand(command);
            OnWorkspaceChanged();
        }
    }

    /// <summary>
    /// Flood fill that only includes connected cells of the same square type (8-connectivity)
    /// </summary>
    private List<Point> FloodFillSameType(int startX, int startY, SquareType targetType, Grid2D grid)
    {
        var positions = new List<Point>();
        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();
        var startPoint = new Point(startX, startY);

        queue.Enqueue(startPoint);
        visited.Add(startPoint);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            positions.Add(current);

            // Check all 8 neighbors (4-connectivity + diagonals for 8-connectivity)
            foreach (var (dx, dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0), (-1, -1), (-1, 1), (1, -1), (1, 1) })
            {
                var neighbor = new Point(current.X + dx, current.Y + dy);

                if (!grid.IsValidPosition(neighbor) || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                var neighborCell = grid.GetCell(neighbor);

                // Only include if it has the same square type
                if (!neighborCell.IsEmpty && neighborCell.Square?.Type == targetType)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// Flood fill that only includes connected empty cells (4-connectivity)
    /// </summary>
    private List<Point> FloodFillEmpty(int startX, int startY, Grid2D grid)
    {
        var positions = new List<Point>();
        var visited = new HashSet<Point>();
        var queue = new Queue<Point>();
        var startPoint = new Point(startX, startY);

        // Don't fill if starting point is not empty
        var startCell = grid.GetCell(startPoint);
        if (!startCell.IsEmpty)
            return positions;

        queue.Enqueue(startPoint);
        visited.Add(startPoint);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            positions.Add(current);

            // Check all 4 connected neighbors (up, down, left, right)
            foreach (var (dx, dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var neighbor = new Point(current.X + dx, current.Y + dy);

                if (!grid.IsValidPosition(neighbor) || visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                var neighborCell = grid.GetCell(neighbor);

                // Only include if it's empty
                if (neighborCell.IsEmpty)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return positions;
    }

    public async Task FillRegionAsync(int x, int y, SquareType squareType)
    {
        await Task.Run(() => FillRegion(x, y, squareType));
    }

    public void PlacePreset(int x, int y, Preset preset)
    {
        if (_currentWorkspace == null)
            throw new InvalidOperationException("No workspace loaded");

        if (preset == null)
            throw new ArgumentNullException(nameof(preset));

        var command = new PlacePresetCommand(_currentWorkspace, new Point(x, y), preset);
        _undoRedoService.ExecuteCommand(command);
        OnWorkspaceChanged();
    }

    public void Undo()
    {
        _undoRedoService.Undo();
        OnWorkspaceChanged();
    }

    public void Redo()
    {
        _undoRedoService.Redo();
        OnWorkspaceChanged();
    }

    private void OnWorkspaceChanged()
    {
        WorkspaceChanged?.Invoke(this, EventArgs.Empty);
    }
}
