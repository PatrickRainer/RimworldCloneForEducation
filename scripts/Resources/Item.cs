using Godot;

namespace RimworldCloneForEducation;

public class Item
{
    public int Id { get; set; }
    public ItemType Type { get; set; }
    public Vector2I Position { get; set; }
    public int? HauledByPawnId { get; set; }

    private static int nextItemId = 0;

    public Item(ItemType type, Vector2I position)
    {
        Id = nextItemId++;
        Type = type;
        Position = position;
    }
}

public enum ItemType
{
    Wood,
    Stone,
    Food,
}
