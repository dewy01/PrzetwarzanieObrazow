namespace MapEditor.Application.Services;

/// <summary>
/// Service do importu obrazów i tworzenia Preset
/// </summary>
public interface IImageImportService
{
    /// <summary>
    /// Importuje obraz z pliku i konwertuje do macierzy RGB
    /// </summary>
    Task<byte[,]> LoadImageAsync(string filePath);

    /// <summary>
    /// Tworzy Preset z obrazu używając fragmentacji
    /// </summary>
    Task<Domain.Editing.Entities.Preset> CreatePresetFromImageAsync(
        string imagePath,
        string presetName,
        int threshold = 128);
}
