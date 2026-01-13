using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Tests.Unit.Domain;

public class EntityTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {
        // Arrange & Act
        var position = new Point(5, 5);
        var entity = new Entity(position, EntityType.Player);

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(position, entity.Position);
        Assert.Equal(EntityType.Player, entity.Type);
        Assert.Equal("Player", entity.Name);
        Assert.True((DateTime.UtcNow - entity.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_WithCustomName_ShouldSetName()
    {
        // Arrange & Act
        var position = new Point(3, 7);
        var entity = new Entity(position, EntityType.Enemy, "Boss");

        // Assert
        Assert.Equal("Boss", entity.Name);
    }

    [Fact]
    public void Constructor_WithNoneType_ShouldThrow()
    {
        // Arrange
        var position = new Point(0, 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Entity(position, EntityType.None));
    }

    [Fact]
    public void MoveTo_WithValidPosition_ShouldUpdatePosition()
    {
        // Arrange
        var entity = new Entity(new Point(1, 1), EntityType.Player);
        var newPosition = new Point(5, 5);

        // Act
        entity.MoveTo(newPosition);

        // Assert
        Assert.Equal(newPosition, entity.Position);
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var entity = new Entity(new Point(0, 0), EntityType.Enemy);

        // Act
        entity.Rename("MegaBoss");

        // Assert
        Assert.Equal("MegaBoss", entity.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithInvalidName_ShouldThrow(string invalidName)
    {
        // Arrange
        var entity = new Entity(new Point(0, 0), EntityType.Player);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => entity.Rename(invalidName));
    }

    [Fact]
    public void CreatedAt_ShouldBeUtc()
    {
        // Arrange & Act
        var entity = new Entity(new Point(0, 0), EntityType.Checkpoint);

        // Assert
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAt.Kind);
    }

    [Theory]
    [InlineData(EntityType.Player)]
    [InlineData(EntityType.Enemy)]
    [InlineData(EntityType.StartPoint)]
    [InlineData(EntityType.EndPoint)]
    [InlineData(EntityType.Checkpoint)]
    [InlineData(EntityType.Collectible)]
    public void Constructor_WithAllValidTypes_ShouldSucceed(EntityType type)
    {
        // Arrange
        var position = new Point(0, 0);

        // Act
        var entity = new Entity(position, type);

        // Assert
        Assert.Equal(type, entity.Type);
        Assert.Equal(type.ToString(), entity.Name);
    }
}
