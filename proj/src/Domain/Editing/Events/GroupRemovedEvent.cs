using MapEditor.Domain.Editing.Entities;

namespace MapEditor.Domain.Editing.Events;

/// <summary>
/// Domain event: Group has been removed from Workspace
/// </summary>
public class GroupRemovedEvent
{
    public Guid GroupId { get; }
    public string GroupName { get; }
    public DateTime OccurredAt { get; }

    public GroupRemovedEvent(Guid groupId, string groupName)
    {
        GroupId = groupId;
        GroupName = groupName;
        OccurredAt = DateTime.UtcNow;
    }

    public override string ToString() => $"GroupRemoved: {GroupName} (ID: {GroupId})";
}
