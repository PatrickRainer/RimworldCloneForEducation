namespace RimworldCloneForEducation;

public class ChopJob : Job
{
    private float workProgress = 0f;
    private const float WorkRequired = 100f;
    private const float WorkPerTick = 10f;

    public ChopJob(Vector2I treePosition)
        : base(JobType.Chop, (int)JobType.Chop, treePosition)
    {
    }

    public override bool CanPawnDoJob(Pawn pawn)
    {
        var tileType = GameManager.Instance.GetTile(TargetPosition.X, TargetPosition.Y);
        return tileType == TileType.Tree;
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
        // Spawn wood items and remove designation
        GameManager.Instance.ItemSystem.SpawnItem(ItemType.Wood, TargetPosition);
        GameManager.Instance.ItemSystem.SpawnItem(ItemType.Wood, TargetPosition);
        GameManager.Instance.DesignationSystem.RemoveChopDesignation(TargetPosition);
        pawn.State = PawnState.Idle;
    }

    public override bool IsJobComplete(Pawn pawn)
    {
        return workProgress >= WorkRequired;
    }
}
