using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RimworldCloneForEducation;

public class StockpileSystem
{
    private List<StockpileZone> stockpiles = new();

    public IReadOnlyList<StockpileZone> Stockpiles => stockpiles.AsReadOnly();

    public void CreateStockpile(Vector2I position)
    {
        // Prevent overlapping stockpiles
        if (stockpiles.Any(s => s.Position == position))
            return;

        var zone = new StockpileZone(position);
        stockpiles.Add(zone);
    }

    public void RemoveStockpile(int zoneId)
    {
        var zone = stockpiles.FirstOrDefault(s => s.Id == zoneId);
        if (zone != null)
        {
            stockpiles.Remove(zone);
        }
    }

    public StockpileZone FindNearestStockpile(Vector2I position)
    {
        if (stockpiles.Count == 0)
            return null;

        StockpileZone nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var zone in stockpiles)
        {
            float distance = ((Vector2)position).DistanceTo((Vector2)zone.Position);
            if (distance < nearestDistance)
            {
                nearest = zone;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    public bool IsInStockpile(Item item)
    {
        return stockpiles.Any(s => s.Position == item.Position &&
            s.StoredItems.Contains(item));
    }

    public int GetResourceCount(ItemType type)
    {
        int total = 0;
        foreach (var stockpile in stockpiles)
        {
            total += stockpile.GetItemCount(type);
        }
        return total;
    }
}
