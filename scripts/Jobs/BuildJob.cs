using Godot;

namespace RimworldCloneForEducation;

public class BuildJob : Job
{
    public Building Building { get; set; }
    private const float WorkPerTick = 5f;

    public BuildJob(Building building)
        : base(JobType.Build, (int)JobType.Build, building.Position)
    {
        Building = building;
    }

    public override bool CanPawnDoJob(Pawn pawn)
    {
        return Building != null && Building.State != BuildingState.Complete;
    }

    public override void OnJobStart(Pawn pawn)
    {
        pawn.State = PawnState.Working;
        Building.State = BuildingState.UnderConstruction;
    }

    public override void OnJobUpdate(Pawn pawn, float deltaTime)
    {
        Building.ConstructionProgress += WorkPerTick;
    }

    public override void OnJobComplete(Pawn pawn)
    {
        Building.State = BuildingState.Complete;
        pawn.State = PawnState.Idle;
    }

    public override bool IsJobComplete(Pawn pawn)
    {
        return Building.ConstructionProgress >= Building.ConstructionRequired;
    }
}
