using Godot;
using System.Collections.Generic;

namespace RimworldCloneForEducation;

public class StockpileZone
{
    public int Id { get; set; }
    public Vector2I Position { get; set; }
    public HashSet<ItemType> AcceptedTypes { get; set; } = new();
    public List<Item> StoredItems { get; set; } = new();

    private static int nextZoneId = 0;

    public StockpileZone(Vector2I position)
    {
        Id = nextZoneId++;
        Position = position;
        // Accept all item types by default
        AcceptedTypes.Add(ItemType.Wood);
        AcceptedTypes.Add(ItemType.Stone);
        AcceptedTypes.Add(ItemType.Food);
    }

    public bool CanAccept(Item item)
    {
        return AcceptedTypes.Contains(item.Type);
    }

    public void AddItem(Item item)
    {
        StoredItems.Add(item);
        item.Position = Position;
    }

    public int GetItemCount(ItemType type)
    {
        return StoredItems.FindAll(i => i.Type == type).Count;
    }
}
