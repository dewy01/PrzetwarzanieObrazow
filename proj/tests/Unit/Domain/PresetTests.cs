using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Domain;

public class PresetTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreatePreset()
    {
        // Arrange
        var name = "Bridge";
        var size = new Size(5, 3);
        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Wood),
            new SquareDefinition(new Point(1, 0), SquareType.Wood),
            new SquareDefinition(new Point(2, 0), SquareType.Wood)
        };

        // Act
        var preset = new Preset(name, size, squares);

        // Assert
        Assert.NotEqual(Guid.Empty, preset.Id);
        Assert.Equal(name, preset.Name);
        Assert.Equal(size, preset.Size);
        Assert.Equal(3, preset.Squares.Count);
        Assert.Equal(new Point(0, 0), preset.OriginPoint);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrow(string? invalidName)
    {
        // Arrange
        var size = new Size(5, 5);
        var squares = new List<SquareDefinition>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Preset(invalidName!, size, squares));
    }

    [Fact]
    public void Constructor_WithNegativeSize_ShouldThrow()
    {
        // Arrange
        var name = "Test";
        var size = new Size(-1, 5);
        var squares = new List<SquareDefinition>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Preset(name, size, squares));
    }

    [Fact]
    public void Constructor_WithCustomOrigin_ShouldSetOrigin()
    {
        // Arrange
        var name = "Test";
        var size = new Size(5, 5);
        var squares = new List<SquareDefinition>();
        var origin = new Point(10, 10);

        // Act
        var preset = new Preset(name, size, squares, null, origin);

        // Assert
        Assert.Equal(origin, preset.OriginPoint);
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var preset = new Preset("Old Name", new Size(5, 5), new List<SquareDefinition>());

        // Act
        preset.Rename("New Name");

        // Assert
        Assert.Equal("New Name", preset.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithInvalidName_ShouldThrow(string? invalidName)
    {
        // Arrange
        var preset = new Preset("Test", new Size(5, 5), new List<SquareDefinition>());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => preset.Rename(invalidName!));
    }

    [Fact]
    public void SetOrigin_WithValidPoint_ShouldUpdateOrigin()
    {
        // Arrange
        var preset = new Preset("Test", new Size(5, 5), new List<SquareDefinition>());
        var newOrigin = new Point(5, 5);

        // Act
        preset.SetOrigin(newOrigin);

        // Assert
        Assert.Equal(newOrigin, preset.OriginPoint);
    }

    [Fact]
    public void SquareDefinition_GetAbsolutePosition_ShouldCalculateCorrectly()
    {
        // Arrange
        var relativePos = new Point(2, 3);
        var squareDef = new SquareDefinition(relativePos, SquareType.Grass);
        var presetOrigin = new Point(10, 10);

        // Act
        var absolutePos = squareDef.GetAbsolutePosition(presetOrigin);

        // Assert
        Assert.Equal(12, absolutePos.X);
        Assert.Equal(13, absolutePos.Y);
    }

    [Fact]
    public void SquareDefinition_WithRotation_ShouldNormalizeRotation()
    {
        // Arrange & Act
        var squareDef = new SquareDefinition(new Point(0, 0), SquareType.Grass, 450);

        // Assert
        Assert.Equal(90, squareDef.Rotation); // 450 % 360 = 90
    }

    [Fact]
    public void Preset_WithMultipleSquares_ShouldStoreAll()
    {
        // Arrange
        var squares = new List<SquareDefinition>
        {
            new SquareDefinition(new Point(0, 0), SquareType.Grass),
            new SquareDefinition(new Point(1, 0), SquareType.Wood),
            new SquareDefinition(new Point(2, 0), SquareType.Stone),
            new SquareDefinition(new Point(0, 1), SquareType.Sand),
            new SquareDefinition(new Point(1, 1), SquareType.Metal)
        };

        // Act
        var preset = new Preset("Multi", new Size(3, 2), squares);

        // Assert
        Assert.Equal(5, preset.Squares.Count);
        Assert.Contains(preset.Squares, s => s.Type == SquareType.Grass);
        Assert.Contains(preset.Squares, s => s.Type == SquareType.Wood);
        Assert.Contains(preset.Squares, s => s.Type == SquareType.Stone);
        Assert.Contains(preset.Squares, s => s.Type == SquareType.Sand);
        Assert.Contains(preset.Squares, s => s.Type == SquareType.Metal);
    }
}
