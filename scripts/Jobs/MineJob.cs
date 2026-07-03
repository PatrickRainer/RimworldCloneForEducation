using Godot;

namespace RimworldCloneForEducation;

public class MineJob : Job
{
    private float workProgress = 0f;
    private const float WorkRequired = 120f;
    private const float WorkPerTick = 8f;

    public MineJob(Vector2I rockPosition)
        : base(JobType.Mine, (int)JobType.Mine, rockPosition)
    {
    }

    public override bool CanPawnDoJob(Pawn pawn)
    {
        var tileType = GameManager.Instance.GetTile(TargetPosition.X, TargetPosition.Y);
        return tileType == TileType.Rock;
    }

    public override void OnJobStart(Pawn pawn)
    {
        pawn.State = PawnState.Working;
        workProgress = 0f;
    }

    public override void OnJobUpdate(Pawn pawn, float deltaTime)
    {
        workProgress += WorkPerTick;
    }

    public override void OnJobComplete(Pawn pawn)
    {
        // Spawn stone items and remove designation
        GameManager.Instance.ItemSystem.SpawnItem(ItemType.Stone, TargetPosition);
        GameManager.Instance.ItemSystem.SpawnItem(ItemType.Stone, TargetPosition);
        GameManager.Instance.DesignationSystem.RemoveRockDesignation(TargetPosition);
        pawn.State = PawnState.Idle;
    }

    public override bool IsJobComplete(Pawn pawn)
    {
        return workProgress >= WorkRequired;
    }
}
