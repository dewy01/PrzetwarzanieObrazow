using MapEditor.Domain.Editing.Commands;

namespace MapEditor.Application.Services;

/// <summary>
/// Service for managing undo/redo operations using the Command pattern
/// </summary>
public class UndoRedoService
{
    private readonly Stack<IEditCommand> _undoStack = new();
    private readonly Stack<IEditCommand> _redoStack = new();
    private readonly int _maxHistorySize;

    public UndoRedoService(int maxHistorySize = 100)
    {
        _maxHistorySize = maxHistorySize;
    }

    /// <summary>
    /// Can undo the last command
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Can redo the last undone command
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Number of commands in undo history
    /// </summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Number of commands in redo history
    /// </summary>
    public int RedoCount => _redoStack.Count;

    /// <summary>
    /// Execute a command and add it to history
    /// </summary>
    public void ExecuteCommand(IEditCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        // Execute the command
        command.Execute();

        // Add to undo stack
        _undoStack.Push(command);

        // Limit history size
        if (_undoStack.Count > _maxHistorySize)
        {
            // Remove oldest command
            var commands = _undoStack.ToList();
            commands.RemoveAt(commands.Count - 1);
            _undoStack.Clear();
            foreach (var cmd in commands.AsEnumerable().Reverse())
            {
                _undoStack.Push(cmd);
            }
        }

        // Clear redo stack when new command is executed
        _redoStack.Clear();
    }

    /// <summary>
    /// Undo the last command
    /// </summary>
    public void Undo()
    {
        if (!CanUndo)
            throw new InvalidOperationException("Nothing to undo");

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
    }

    /// <summary>
    /// Redo the last undone command
    /// </summary>
    public void Redo()
    {
        if (!CanRedo)
            throw new InvalidOperationException("Nothing to redo");

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
    }

    /// <summary>
    /// Clear all history
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>
    /// Get description of the next undo command
    /// </summary>
    public string? GetUndoDescription()
    {
        return CanUndo ? _undoStack.Peek().Description : null;
    }

    /// <summary>
    /// Get description of the next redo command
    /// </summary>
    public string? GetRedoDescription()
    {
        return CanRedo ? _redoStack.Peek().Description : null;
    }
}
