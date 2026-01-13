namespace MapEditor.Domain.Shared.Enums;

/// <summary>
/// Typ Entity - elementy nie-terenowe (wrogowie, start, koniec)
/// </summary>
public enum EntityType
{
    None = 0,
    Player = 1,
    Enemy = 2,
    StartPoint = 3,
    EndPoint = 4,
    Checkpoint = 5,
    Collectible = 6
}
