using Godot;

namespace RimworldCloneForEducation;

public class EatJob : Job
{
    private float eatingProgress = 0f;
    private const float EatingTime = 50f;

    public EatJob(Vector2I foodPosition)
        : base(JobType.Eat, (int)JobType.Eat, foodPosition)
    {
    }

    public override bool CanPawnDoJob(Pawn pawn)
    {
        var tileType = GameManager.Instance.GetTile(TargetPosition.X, TargetPosition.Y);
        return tileType == TileType.BerryBush && pawn.Hunger > 30f;
    }

    public override void OnJobStart(Pawn pawn)
    {
        pawn.State = PawnState.Working;
        eatingProgress = 0f;
    }

    public override void OnJobUpdate(Pawn pawn, float deltaTime)
    {
        var eatBonus = GameManager.Instance.BuildingSystem.GetEatBonusAt(TargetPosition);
        var recovery = Constants.EatingRecoveryPerTick * eatBonus;
        eatingProgress += recovery;
        pawn.Hunger = Mathf.Max(0f, pawn.Hunger - recovery);
    }

    public override void OnJobComplete(Pawn pawn)
    {
        pawn.State = PawnState.Idle;
    }

    public override bool IsJobComplete(Pawn pawn)
    {
        return eatingProgress >= EatingTime || pawn.Hunger <= 10f;
    }
}
