using Godot;
using System.Linq;

namespace RimworldCloneForEducation;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    private float tickAccumulator = 0f;
    private TileType[,] mapData;
    private PathfindingSystem pathfinding;
    private PawnManager pawnManager;
    private JobSystem jobSystem;
    private DesignationSystem designationSystem;
    private NeedsSystem needsSystem;
    private StockpileSystem stockpileSystem;
    private ItemSystem itemSystem;
    private BuildingSystem buildingSystem;
    private float gameSpeedMultiplier = 1f;
    private bool isPaused = false;

    public float GameSpeedMultiplier => gameSpeedMultiplier;
    public bool IsPaused => isPaused;

    public PawnManager PawnManager => pawnManager;
    public JobSystem JobSystem => jobSystem;
    public DesignationSystem DesignationSystem => designationSystem;
    public PathfindingSystem Pathfinding => pathfinding;
    public NeedsSystem NeedsSystem => needsSystem;
    public StockpileSystem StockpileSystem => stockpileSystem;
    public ItemSystem ItemSystem => itemSystem;
    public BuildingSystem BuildingSystem => buildingSystem;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Ready()
    {
        var generator = new MapGenerator();
        mapData = generator.GenerateMap();

        pathfinding = new PathfindingSystem();
        pawnManager = new PawnManager(pathfinding);
        jobSystem = new JobSystem();
        designationSystem = new DesignationSystem(jobSystem);
        needsSystem = new NeedsSystem(jobSystem);
        stockpileSystem = new StockpileSystem();
        itemSystem = new ItemSystem(jobSystem, stockpileSystem);
        buildingSystem = new BuildingSystem(pathfinding, jobSystem);

        // Create default stockpile near center of map
        stockpileSystem.CreateStockpile(new Vector2I(Constants.MapWidth / 2, Constants.MapHeight / 2));

        // Setup pathfinding obstacles
        for (int x = 0; x < Constants.MapWidth; x++)
        {
            for (int y = 0; y < Constants.MapHeight; y++)
            {
                if (mapData[x, y] == TileType.Water)
                {
                    pathfinding.SetObstacle(new Vector2I(x, y), true);
                }
            }
        }

        pawnManager.SpawnPawns();
    }

    public override void _Process(double delta)
    {
        if (isPaused)
            return;

        var adjustedDelta = (float)delta * gameSpeedMultiplier;
        tickAccumulator += adjustedDelta;

        while (tickAccumulator >= Constants.TickDuration)
        {
            OnTick((float)delta);
            tickAccumulator -= Constants.TickDuration;
        }
    }

    private void OnTick(float delta)
    {
        var pawnList = pawnManager.Pawns.ToList();
        pawnManager.UpdatePawns(delta);
        needsSystem.UpdateNeeds(pawnList, delta);
        jobSystem.Update(pawnList, delta);
        itemSystem.Update(delta);
        buildingSystem.Update(delta);
    }

    public TileType GetTile(int x, int y)
    {
        if (x < 0 || x >= Constants.MapWidth || y < 0 || y >= Constants.MapHeight)
            return TileType.Water;

        return mapData[x, y];
    }

    public void SetGameSpeed(float multiplier)
    {
        gameSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }
}
