using Godot;

namespace RimworldCloneForEducation;

public class SleepJob : Job
{
    private float sleepingProgress = 0f;
    private const float SleepTime = 100f;

    public SleepJob(Vector2I bedPosition)
        : base(JobType.Sleep, (int)JobType.Sleep, bedPosition)
    {
    }

    public override bool CanPawnDoJob(Pawn pawn)
    {
        // Can sleep anywhere on grass/dirt for now
        var tileType = GameManager.Instance.GetTile(TargetPosition.X, TargetPosition.Y);
        return (tileType == TileType.Grass || tileType == TileType.Dirt) && pawn.Rest > 20f;
    }

    public override void OnJobStart(Pawn pawn)
    {
        pawn.State = PawnState.Working;
        sleepingProgress = 0f;
    }

    public override void OnJobUpdate(Pawn pawn, float deltaTime)
    {
        var restBonus = GameManager.Instance.BuildingSystem.GetRestBonusAt(TargetPosition);
        var recovery = Constants.RestRecoveryPerTick * restBonus;
        sleepingProgress += recovery;
        pawn.Rest = Mathf.Max(0f, pawn.Rest - recovery);
    }

    public override void OnJobComplete(Pawn pawn)
    {
        pawn.State = PawnState.Idle;
    }

    public override bool IsJobComplete(Pawn pawn)
    {
        return sleepingProgress >= SleepTime || pawn.Rest >= 95f;
    }
}
