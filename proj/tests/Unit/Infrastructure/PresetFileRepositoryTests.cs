using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using MapEditor.Infrastructure.Repositories;

namespace MapEditor.Tests.Unit.Infrastructure;

public class PresetFileRepositoryTests
{
    private readonly string _testDirectory;
    private readonly PresetFileRepository _repository;

    public PresetFileRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "PresetTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDirectory);
        _repository = new PresetFileRepository();
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateFile()
    {
        // Arrange
        var preset = CreateTestPreset();
        var filePath = Path.Combine(_testDirectory, "test.preset.json");

        // Act
        await _repository.SaveAsync(preset, filePath);

        // Assert
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task LoadAsync_ShouldRestorePreset()
    {
        // Arrange
        var originalPreset = CreateTestPreset();
        var filePath = Path.Combine(_testDirectory, "test.preset.json");
        await _repository.SaveAsync(originalPreset, filePath);

        // Act
        var loadedPreset = await _repository.LoadAsync(filePath);

        // Assert
        Assert.Equal(originalPreset.Name, loadedPreset.Name);
        Assert.Equal(originalPreset.Size.Width, loadedPreset.Size.Width);
        Assert.Equal(originalPreset.Size.Height, loadedPreset.Size.Height);
        Assert.Equal(originalPreset.OriginPoint.X, loadedPreset.OriginPoint.X);
        Assert.Equal(originalPreset.OriginPoint.Y, loadedPreset.OriginPoint.Y);
        Assert.Equal(originalPreset.Squares.Count, loadedPreset.Squares.Count);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var preset = CreateTestPreset();
        var filePath = Path.Combine(_testDirectory, "test.preset.json");
        await _repository.SaveAsync(preset, filePath);

        // Act
        var exists = await _repository.ExistsAsync(filePath);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.preset.json");

        // Act
        var exists = await _repository.ExistsAsync(filePath);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPresetsInDirectory()
    {
        // Arrange
        await _repository.SaveAsync(CreateTestPreset("Preset1"), Path.Combine(_testDirectory, "p1.preset.json"));
        await _repository.SaveAsync(CreateTestPreset("Preset2"), Path.Combine(_testDirectory, "p2.preset.json"));
        await _repository.SaveAsync(CreateTestPreset("Preset3"), Path.Combine(_testDirectory, "p3.preset.json"));

        // Act
        var presets = await _repository.GetAllAsync(_testDirectory);

        // Assert
        Assert.Equal(3, presets.Count);
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_ShouldThrow()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.preset.json");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => _repository.LoadAsync(filePath));
    }

    private Preset CreateTestPreset(string name = "TestPreset")
    {
        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass),
            new SquareDefinition(new Point(1, 0), SquareType.Wood),
            new SquareDefinition(new Point(2, 0), SquareType.Stone)
        };

        return new Preset(name, new Size(3, 1), squares, new Point(5, 5));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
