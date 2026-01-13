namespace MapEditor.Domain.Editing.ValueObjects;

/// <summary>
/// Size - Value Object reprezentujący rozmiar Workspace
/// </summary>
public record Size(int Width, int Height)
{
    public Size() : this(10, 10) { }

    public int TotalCells => Width * Height;

    public bool IsValid() => Width > 0 && Height > 0;

    public override string ToString() => $"{Width}x{Height}";
}
