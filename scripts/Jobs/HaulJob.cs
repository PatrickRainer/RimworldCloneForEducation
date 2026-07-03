using Godot;

namespace RimworldCloneForEducation;

public class HaulJob : Job
{
    public Item ItemToHaul { get; set; }
    public StockpileZone TargetStockpile { get; set; }
    private float workProgress = 0f;
    private const float WorkRequired = 50f;

    public HaulJob(Item item, StockpileZone stockpile)
        : base(JobType.Haul, (int)JobType.Haul, item.Position)
    {
        ItemToHaul = item;
        TargetStockpile = stockpile;
        TargetPosition = stockpile.Position;
    }

    public override bool CanPawnDoJob(Pawn pawn)
    {
        return ItemToHaul != null && TargetStockpile != null;
    }

    public override void OnJobStart(Pawn pawn)
    {
        pawn.State = PawnState.Working;
        ItemToHaul.HauledByPawnId = pawn.Id;
        workProgress = 0f;
    }

    public override void OnJobUpdate(Pawn pawn, float deltaTime)
    {
        // Move item with pawn
        ItemToHaul.Position = pawn.Position;
        workProgress += 5f; // Quick haul mechanic
    }

    public override void OnJobComplete(Pawn pawn)
    {
        if (ItemToHaul != null && TargetStockpile != null)
        {
            TargetStockpile.AddItem(ItemToHaul);
            ItemToHaul.HauledByPawnId = null;
        }
        pawn.State = PawnState.Idle;
    }

    public override bool IsJobComplete(Pawn pawn)
    {
        return workProgress >= WorkRequired;
    }
}
