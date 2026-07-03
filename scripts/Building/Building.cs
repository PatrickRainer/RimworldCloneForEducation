using Godot;

namespace RimworldCloneForEducation;

public class Building
{
    public int Id { get; set; }
    public BuildingType Type { get; set; }
    public Vector2I Position { get; set; }
    public BuildingState State { get; set; } = BuildingState.Blueprint;
    public float ConstructionProgress { get; set; } = 0f;
    public float ConstructionRequired { get; set; } = 100f;

    private static int nextBuildingId = 0;

    public Building(BuildingType type, Vector2I position)
    {
        Id = nextBuildingId++;
        Type = type;
        Position = position;
    }

    public bool IsComplete => State == BuildingState.Complete;
}

public enum BuildingType
{
    Wall,
    Bed,
    Table,
}

public enum BuildingState
{
    Blueprint,
    UnderConstruction,
    Complete,
}
