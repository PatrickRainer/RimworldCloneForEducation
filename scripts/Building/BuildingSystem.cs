using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RimworldCloneForEducation;

public class BuildingSystem
{
    private List<Building> buildings = new();
    private PathfindingSystem pathfinding;
    private JobSystem jobSystem;

    public IReadOnlyList<Building> Buildings => buildings.AsReadOnly();

    public BuildingSystem(PathfindingSystem pathfinding, JobSystem jobSystem)
    {
        this.pathfinding = pathfinding;
        this.jobSystem = jobSystem;
    }

    public Building PlaceBuilding(BuildingType type, Vector2I position)
    {
        // Check if position is already occupied
        if (buildings.Any(b => b.Position == position))
            return null;

        var building = new Building(type, position);
        buildings.Add(building);

        // Create build job
        var job = new BuildJob(building);
        jobSystem.AddJob(job);

        return building;
    }

    public void Update(float deltaTime)
    {
        // Update wall pathfinding obstacles
        foreach (var building in buildings)
        {
            if (building.Type == BuildingType.Wall && building.IsComplete)
            {
                pathfinding.SetObstacle(building.Position, true);
            }
        }

        // Remove completed buildings that are ready for cleanup
        var completedBuildings = buildings.Where(b => b.IsComplete).ToList();
    }

    public Building GetBuildingAt(Vector2I position)
    {
        return buildings.FirstOrDefault(b => b.Position == position);
    }

    public float GetRestBonusAt(Vector2I position)
    {
        var building = GetBuildingAt(position);
        if (building != null && building.Type == BuildingType.Bed && building.IsComplete)
        {
            return 1.5f; // 1.5x rest recovery on bed
        }
        return 1f;
    }

    public float GetEatBonusAt(Vector2I position)
    {
        var building = GetBuildingAt(position);
        if (building != null && building.Type == BuildingType.Table && building.IsComplete)
        {
            return 1.3f; // 1.3x eating speed at table
        }
        return 1f;
    }

    public void RemoveBuilding(int buildingId)
    {
        var building = buildings.FirstOrDefault(b => b.Id == buildingId);
        if (building != null)
        {
            if (building.Type == BuildingType.Wall)
            {
                pathfinding.SetObstacle(building.Position, false);
            }
            buildings.Remove(building);
        }
    }
}
