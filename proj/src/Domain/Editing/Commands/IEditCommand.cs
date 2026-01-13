namespace MapEditor.Domain.Editing.Commands;

/// <summary>
/// Interface for undoable/redoable commands following the Command pattern
/// </summary>
public interface IEditCommand
{
    /// <summary>
    /// Execute the command
    /// </summary>
    void Execute();

    /// <summary>
    /// Undo the command, reverting its effects
    /// </summary>
    void Undo();

    /// <summary>
    /// Description of the command for UI display
    /// </summary>
    string Description { get; }
}
