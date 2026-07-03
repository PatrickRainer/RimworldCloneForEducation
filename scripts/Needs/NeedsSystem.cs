using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RimworldCloneForEducation;

public class NeedsSystem
{
    private JobSystem jobSystem;
    private Dictionary<int, float> lastHungerJobTime = new();
    private Dictionary<int, float> lastRestJobTime = new();
    private const float MinJobCreationCooldown = 10f; // ticks between auto-jobs

    public NeedsSystem(JobSystem jobSystem)
    {
        this.jobSystem = jobSystem;
    }

    public void UpdateNeeds(List<Pawn> pawns, float deltaTime)
    {
        foreach (var pawn in pawns)
        {
            // Decay hunger and rest
            pawn.Hunger = Mathf.Min(100f, pawn.Hunger + Constants.HungerDecayPerTick);
            pawn.Rest = Mathf.Min(100f, pawn.Rest + Constants.RestDecayPerTick);

            // Check if needs require jobs
            CheckAndCreateHungerJob(pawn);
            CheckAndCreateRestJob(pawn);
        }
    }

    private void CheckAndCreateHungerJob(Pawn pawn)
    {
        if (pawn.Hunger < Constants.HungerThreshold)
            return;

        // Cooldown to prevent spamming jobs
        if (lastHungerJobTime.TryGetValue(pawn.Id, out var lastTime) &&
            (float)Time.GetTicksMsec() / 1000f - lastTime < MinJobCreationCooldown)
            return;

        // Find nearest berry bush
        var bushPosition = FindNearestResource(pawn.Position, TileType.BerryBush);
        if (bushPosition.HasValue)
        {
            var job = new EatJob(bushPosition.Value);
            jobSystem.AddJob(job);
            lastHungerJobTime[pawn.Id] = (float)Time.GetTicksMsec() / 1000f;
        }
    }

    private void CheckAndCreateRestJob(Pawn pawn)
    {
        if (pawn.Rest < Constants.RestThreshold)
            return;

        // Cooldown to prevent spamming jobs
        if (lastRestJobTime.TryGetValue(pawn.Id, out var lastTime) &&
            (float)Time.GetTicksMsec() / 1000f - lastTime < MinJobCreationCooldown)
            return;

        // Find a safe place to sleep (grass tile)
        var sleepPosition = FindNearestResource(pawn.Position, TileType.Grass);
        if (sleepPosition.HasValue)
        {
            var job = new SleepJob(sleepPosition.Value);
            jobSystem.AddJob(job);
            lastRestJobTime[pawn.Id] = (float)Time.GetTicksMsec() / 1000f;
        }
    }

    private Vector2I? FindNearestResource(Vector2I from, TileType resourceType)
    {
        // Simple search within a radius
        const int searchRadius = 20;
        Vector2I? nearest = null;
        float nearestDistance = float.MaxValue;

        for (int x = from.X - searchRadius; x <= from.X + searchRadius; x++)
        {
            for (int y = from.Y - searchRadius; y <= from.Y + searchRadius; y++)
            {
                if (GameManager.Instance.GetTile(x, y) == resourceType)
                {
                    var pos = new Vector2I(x, y);
                    float distance = ((Vector2)from).DistanceTo((Vector2)pos);
                    if (distance < nearestDistance)
                    {
                        nearest = pos;
                        nearestDistance = distance;
                    }
                }
            }
        }

        return nearest;
    }
}
