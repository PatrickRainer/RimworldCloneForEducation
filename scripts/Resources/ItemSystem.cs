using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RimworldCloneForEducation;

public class ItemSystem
{
    private List<Item> items = new();
    private JobSystem jobSystem;
    private StockpileSystem stockpileSystem;

    public IReadOnlyList<Item> Items => items.AsReadOnly();

    public ItemSystem(JobSystem jobSystem, StockpileSystem stockpileSystem)
    {
        this.jobSystem = jobSystem;
        this.stockpileSystem = stockpileSystem;
    }

    public void SpawnItem(ItemType type, Vector2I position)
    {
        var item = new Item(type, position);
        items.Add(item);

        // Auto-create haul job for items
        var nearestStockpile = stockpileSystem.FindNearestStockpile(position);
        if (nearestStockpile != null)
        {
            var job = new HaulJob(item, nearestStockpile);
            jobSystem.AddJob(job);
        }
    }

    public void RemoveItem(int itemId)
    {
        var item = items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            items.Remove(item);
        }
    }

    public Item GetItemAt(Vector2I position)
    {
        return items.FirstOrDefault(i => i.Position == position && i.HauledByPawnId == null);
    }

    public List<Item> GetItemsAt(Vector2I position)
    {
        return items.Where(i => i.Position == position && i.HauledByPawnId == null).ToList();
    }

    public void Update(float deltaTime)
    {
        // Remove items in stockpiles
        var itemsToRemove = items.Where(i => i.HauledByPawnId == null &&
            stockpileSystem.IsInStockpile(i)).ToList();

        foreach (var item in itemsToRemove)
        {
            RemoveItem(item.Id);
        }
    }
}
