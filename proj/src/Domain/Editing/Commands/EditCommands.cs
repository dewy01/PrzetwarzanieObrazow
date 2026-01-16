using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Domain.Editing.Commands;

/// <summary>
/// Command for placing a square on the grid
/// </summary>
public class PlaceSquareCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly Point _position;
    private readonly SquareType _squareType;
    private readonly int _rotation;
    private Square? _previousSquare;

    public string Description => $"Place {_squareType} at ({_position.X}, {_position.Y})";

    public PlaceSquareCommand(Workspace workspace, Point position, SquareType squareType, int rotation = 0)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _position = position;
        _squareType = squareType;
        _rotation = rotation;
    }

    public void Execute()
    {
        // Store previous state
        var cell = _workspace.Grid.GetCell(_position);
        _previousSquare = cell.Square;

        // Execute the command with rotation
        _workspace.ActiveGroup.PlaceSquare(_position, _squareType, _rotation);
    }

    public void Undo()
    {
        if (_previousSquare != null)
        {
            // Restore previous square with its rotation
            _workspace.ActiveGroup.PlaceSquare(_position, _previousSquare.Type, _previousSquare.Rotation);
        }
        else
        {
            // Remove the square
            _workspace.RemoveSquare(_position);
        }
    }
}

/// <summary>
/// Command for removing a square from the grid
/// </summary>
public class RemoveSquareCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly Point _position;
    private Square? _previousSquare;

    public string Description => $"Remove square at ({_position.X}, {_position.Y})";

    public RemoveSquareCommand(Workspace workspace, Point position)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _position = position;
    }

    public void Execute()
    {
        // Store previous state
        var cell = _workspace.Grid.GetCell(_position);
        _previousSquare = cell.Square;

        // Execute the command
        _workspace.RemoveSquare(_position);
    }

    public void Undo()
    {
        if (_previousSquare != null)
        {
            // Restore previous square
            _workspace.PlaceSquare(_position, _previousSquare.Type);
        }
    }
}

/// <summary>
/// Command for filling a region with a square type
/// </summary>
public class FillRegionCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly List<(Point position, Square? previousSquare)> _changes;
    private readonly SquareType _squareType;

    public string Description => $"Fill region with {_squareType} ({_changes.Count} squares)";

    public FillRegionCommand(Workspace workspace, List<Point> positions, SquareType squareType)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _squareType = squareType;

        // Store previous state for all positions
        _changes = new List<(Point, Square?)>();
        foreach (var pos in positions)
        {
            var cell = _workspace.Grid.GetCell(pos);
            _changes.Add((pos, cell.Square));
        }
    }

    public void Execute()
    {
        // Place squares at all positions
        foreach (var (position, _) in _changes)
        {
            _workspace.PlaceSquare(position, _squareType);
        }
    }

    public void Undo()
    {
        // Restore previous state for all positions
        foreach (var (position, previousSquare) in _changes)
        {
            if (previousSquare != null)
            {
                _workspace.PlaceSquare(position, previousSquare.Type);
            }
            else
            {
                _workspace.RemoveSquare(position);
            }
        }
    }
}

/// <summary>
/// Command for placing an entity on the grid
/// </summary>
public class PlaceEntityCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly Point _position;
    private readonly Shared.Enums.EntityType _entityType;
    private readonly string? _name;
    private Entity? _previousEntity;

    public string Description => $"Place {_entityType} entity at ({_position.X}, {_position.Y})";

    public PlaceEntityCommand(Workspace workspace, Point position, Shared.Enums.EntityType entityType, string? name = null)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _position = position;
        _entityType = entityType;
        _name = name;
    }

    public void Execute()
    {
        // Store previous state
        _previousEntity = _workspace.GetEntityAt(_position);

        // Execute the command
        _workspace.PlaceEntity(_position, _entityType, _name);
    }

    public void Undo()
    {
        // Remove current entity
        _workspace.RemoveEntity(_position);

        // Restore previous entity if it existed
        if (_previousEntity != null)
        {
            _workspace.PlaceEntity(_position, _previousEntity.Type, _previousEntity.Name);
        }
    }
}

/// <summary>
/// Command for removing an entity from the grid
/// </summary>
public class RemoveEntityCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly Point _position;
    private Entity? _previousEntity;

    public string Description => $"Remove entity at ({_position.X}, {_position.Y})";

    public RemoveEntityCommand(Workspace workspace, Point position)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _position = position;
    }

    public void Execute()
    {
        // Store previous state
        _previousEntity = _workspace.GetEntityAt(_position);

        // Execute the command
        _workspace.RemoveEntity(_position);
    }

    public void Undo()
    {
        if (_previousEntity != null)
        {
            _workspace.PlaceEntity(_position, _previousEntity.Type, _previousEntity.Name);
        }
    }
}

/// <summary>
/// Command for placing a preset on the grid
/// </summary>
public class PlacePresetCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly Point _position;
    private readonly Preset _preset;
    private readonly List<Square> _previousSquares;

    public string Description => $"Place preset '{_preset.Name}' at ({_position.X}, {_position.Y})";

    public PlacePresetCommand(Workspace workspace, Point position, Preset preset)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _position = position;
        _preset = preset ?? throw new ArgumentNullException(nameof(preset));
        _previousSquares = new List<Square>();
    }

    public void Execute()
    {
        // Store previous state (all squares that will be overwritten)
        _previousSquares.Clear();
        _previousSquares.AddRange(_workspace.RemovePresetSquares(_position, _preset));

        // Execute the command
        _workspace.PlacePreset(_position, _preset);
    }

    public void Undo()
    {
        // Remove preset squares
        _workspace.RemovePresetSquares(_position, _preset);

        // Restore previous squares
        foreach (var square in _previousSquares)
        {
            _workspace.PlaceSquare(square.Position, square.Type);
        }
    }
}

/// <summary>
/// Command for removing a group from the workspace
/// </summary>
public class RemoveGroupCommand : IEditCommand
{
    private readonly Workspace _workspace;
    private readonly Group _group;
    private readonly int _previousIndex;
    private readonly Group? _previousActiveGroup;
    private readonly Dictionary<Point, Square> _removedSquares;
    private readonly Dictionary<Point, Entity> _removedEntities;

    public string Description => $"Remove group '{_group.Name}'";

    public RemoveGroupCommand(Workspace workspace, Group group)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
        _group = group ?? throw new ArgumentNullException(nameof(group));
        _previousIndex = workspace.Groups.IndexOf(group);
        _previousActiveGroup = workspace.ActiveGroup != group ? workspace.ActiveGroup : null;
        _removedSquares = new Dictionary<Point, Square>(_group.Elements);
        _removedEntities = new Dictionary<Point, Entity>(_group.Entities);
    }

    public void Execute()
    {
        _workspace.RemoveGroup(_group);
    }

    public void Undo()
    {
        // Re-add the group
        _workspace.Groups.Insert(_previousIndex, _group);

        // Restore squares and entities
        foreach (var kvp in _removedSquares)
        {
            _group.Elements[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in _removedEntities)
        {
            _group.Entities[kvp.Key] = kvp.Value;
        }

        // Restore previous active group if different
        if (_previousActiveGroup != null && _previousActiveGroup != _group)
        {
            _workspace.SetActiveGroup(_previousActiveGroup);
        }
        else if (_workspace.Groups.Contains(_group))
        {
            _workspace.SetActiveGroup(_group);
        }
    }
}
