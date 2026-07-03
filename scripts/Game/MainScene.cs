using Godot;
using System.Linq;

namespace RimworldCloneForEducation;

public class MainScene : Node2D
{
    private TileMapLayer tileMapLayer;
    private Camera2D camera;
    private CanvasLayer pawnLayer;
    private CanvasLayer designationLayer;
    private CanvasLayer uiLayer;
    private CanvasLayer itemLayer;
    private CanvasLayer buildingLayer;
    private Dictionary<int, Sprite2D> pawnSprites = new();
    private Dictionary<Vector2I, ColorRect> designationVisuals = new();
    private Dictionary<int, Sprite2D> itemSprites = new();
    private Dictionary<Vector2I, ColorRect> stockpileVisuals = new();
    private Dictionary<int, Sprite2D> buildingSprites = new();
    private Dictionary<int, (ColorRect hunger, ColorRect rest)> needBars = new();
    private Label statusLabel;
    private Label needsLabel;
    private Label resourceLabel;
    private Label buildMenuLabel;
    private Label gameSpeedLabel;
    private int selectedPawnId = -1;
    private bool isDragging = false;
    private bool isDesignatingStockpile = false;
    private bool isDesignatingMine = false;
    private bool isPlacingBuilding = false;
    private BuildingType selectedBuildingType = BuildingType.Wall;
    private Vector2I dragStartTile = Vector2I.Zero;

    private const float NeedBarWidth = 100f;
    private const float NeedBarHeight = 8f;

    private const float CameraMoveSpeed = 300f;
    private const float ZoomSpeed = 0.1f;
    private const float MinZoom = 0.5f;
    private const float MaxZoom = 4f;
    private const float PawnSize = 12f;

    public override void _Ready()
    {
        SetupScene();
        PopulateTilemap();
        SetupPawns();
    }

    private void SetupScene()
    {
        tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");
        camera = GetNode<Camera2D>("Camera2D");

        // Create designation layer for rendering designations
        designationLayer = new CanvasLayer();
        designationLayer.Layer = 0;
        AddChild(designationLayer);

        // Create item layer for rendering items
        itemLayer = new CanvasLayer();
        itemLayer.Layer = 0;
        AddChild(itemLayer);

        // Create building layer for rendering buildings
        buildingLayer = new CanvasLayer();
        buildingLayer.Layer = 0;
        AddChild(buildingLayer);

        // Create pawn layer for rendering pawns on top
        pawnLayer = new CanvasLayer();
        pawnLayer.Layer = 1;
        AddChild(pawnLayer);

        // Create UI layer
        uiLayer = new CanvasLayer();
        uiLayer.Layer = 2;
        AddChild(uiLayer);

        // Create status label
        statusLabel = new Label();
        statusLabel.GlobalPosition = Vector2.One * 10f;
        uiLayer.AddChild(statusLabel);

        // Create needs label (shows all pawns' needs)
        needsLabel = new Label();
        needsLabel.GlobalPosition = new Vector2(10f, 80f);
        uiLayer.AddChild(needsLabel);

        // Create resource label
        resourceLabel = new Label();
        resourceLabel.GlobalPosition = new Vector2(10f, 160f);
        uiLayer.AddChild(resourceLabel);

        // Create build menu label
        buildMenuLabel = new Label();
        buildMenuLabel.GlobalPosition = new Vector2(10f, 240f);
        buildMenuLabel.Text = "Build Menu:\n[W] Wall [B] Bed [T] Table [Esc] Cancel";
        uiLayer.AddChild(buildMenuLabel);

        // Create game speed label
        gameSpeedLabel = new Label();
        gameSpeedLabel.GlobalPosition = new Vector2(10f, 300f);
        uiLayer.AddChild(gameSpeedLabel);

        // Setup camera
        camera.GlobalPosition = new Vector2(
            Constants.MapWidth * 8,
            Constants.MapHeight * 8
        );
        camera.Zoom = new Vector2(2f, 2f);
    }

    private void PopulateTilemap()
    {
        var tileSet = new TileSet();
        var source = new TileSetAtlasSource();
        source.TextureRegionSize = Vector2I.One * 16;
        source.Texture = GenerateTileTexture();

        for (int i = 0; i < 6; i++)
        {
            source.CreateAlternativeTile(new Vector2I(i, 0));
        }

        tileSet.AddSource(source, 0);
        tileMapLayer.TileSet = tileSet;

        // Populate tiles from map data
        for (int x = 0; x < Constants.MapWidth; x++)
        {
            for (int y = 0; y < Constants.MapHeight; y++)
            {
                TileType tileType = GameManager.Instance.GetTile(x, y);
                var sourceId = new Vector2I(0, 0);
                var atlasCoords = new Vector2I((int)tileType, 0);
                var tile = new Vector2I(x, y);

                tileMapLayer.SetCell(0, tile, sourceId, atlasCoords);
            }
        }
    }

    private void SetupPawns()
    {
        foreach (var pawn in GameManager.Instance.PawnManager.Pawns)
        {
            var sprite = new Sprite2D();
            sprite.Texture = GeneratePawnTexture(pawn.Id);
            sprite.Scale = new Vector2(PawnSize, PawnSize);
            sprite.GlobalPosition = pawn.Position * 16f + Vector2.One * 8f;
            pawnLayer.AddChild(sprite);
            pawnSprites[pawn.Id] = sprite;
        }
    }

    private void RenderStockpiles()
    {
        // Clear old visuals
        foreach (var rect in stockpileVisuals.Values)
        {
            rect.QueueFree();
        }
        stockpileVisuals.Clear();

        // Render stockpile zones
        foreach (var zone in GameManager.Instance.StockpileSystem.Stockpiles)
        {
            var rect = new ColorRect();
            rect.Color = new Color(0.5f, 0.5f, 1f, 0.2f);
            rect.Size = Vector2.One * 16f;
            rect.GlobalPosition = zone.Position * 16f;
            designationLayer.AddChild(rect);
            stockpileVisuals[zone.Position] = rect;
        }
    }

    private void RenderItems()
    {
        // Clear old item sprites
        foreach (var sprite in itemSprites.Values)
        {
            sprite.QueueFree();
        }
        itemSprites.Clear();

        // Render items on the map
        foreach (var item in GameManager.Instance.ItemSystem.Items)
        {
            var sprite = new Sprite2D();
            sprite.Texture = GenerateItemTexture(item.Type);
            sprite.Scale = Vector2.One * 4f;
            sprite.GlobalPosition = item.Position * 16f + Vector2.One * 8f;
            itemLayer.AddChild(sprite);
            itemSprites[item.Id] = sprite;
        }
    }

    private ImageTexture GenerateItemTexture(ItemType type)
    {
        var image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        var color = type switch
        {
            ItemType.Wood => new Color(0.6f, 0.3f, 0f),
            ItemType.Stone => new Color(0.5f, 0.5f, 0.5f),
            ItemType.Food => new Color(1f, 0.8f, 0.2f),
            _ => Colors.White,
        };
        image.SetPixel(0, 0, color);
        return ImageTexture.CreateFromImage(image);
    }

    private void RenderBuildings()
    {
        // Clear old building sprites
        foreach (var sprite in buildingSprites.Values)
        {
            sprite.QueueFree();
        }
        buildingSprites.Clear();

        // Render buildings
        foreach (var building in GameManager.Instance.BuildingSystem.Buildings)
        {
            var sprite = new Sprite2D();
            sprite.Texture = GenerateBuildingTexture(building.Type, building.State);
            sprite.Scale = Vector2.One * 14f;
            sprite.GlobalPosition = building.Position * 16f + Vector2.One * 8f;
            buildingLayer.AddChild(sprite);
            buildingSprites[building.Id] = sprite;
        }
    }

    private ImageTexture GenerateBuildingTexture(BuildingType type, BuildingState state)
    {
        var image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        var color = (type, state) switch
        {
            (BuildingType.Wall, BuildingState.Blueprint) => new Color(0.3f, 0.3f, 0.3f, 0.5f),
            (BuildingType.Wall, BuildingState.UnderConstruction) => new Color(0.4f, 0.3f, 0.2f),
            (BuildingType.Wall, BuildingState.Complete) => new Color(0.2f, 0.2f, 0.2f),
            (BuildingType.Bed, BuildingState.Blueprint) => new Color(0.5f, 0.2f, 0.2f, 0.5f),
            (BuildingType.Bed, BuildingState.UnderConstruction) => new Color(0.7f, 0.3f, 0.3f),
            (BuildingType.Bed, BuildingState.Complete) => new Color(0.6f, 0.2f, 0.2f),
            (BuildingType.Table, BuildingState.Blueprint) => new Color(0.4f, 0.2f, 0.1f, 0.5f),
            (BuildingType.Table, BuildingState.UnderConstruction) => new Color(0.6f, 0.3f, 0.1f),
            (BuildingType.Table, BuildingState.Complete) => new Color(0.5f, 0.25f, 0f),
            _ => Colors.White,
        };
        image.SetPixel(0, 0, color);
        return ImageTexture.CreateFromImage(image);
    }

    private void UpdatePawnVisuals()
    {
        var pawns = GameManager.Instance.PawnManager.Pawns;

        foreach (var pawn in pawns)
        {
            if (pawnSprites.TryGetValue(pawn.Id, out var sprite))
            {
                sprite.GlobalPosition = pawn.Position * 16f + Vector2.One * 8f;

                // Highlight selected pawn
                sprite.SelfModulate = selectedPawnId == pawn.Id
                    ? new Color(1f, 1f, 0.5f)
                    : Colors.White;
            }
        }

        // Update status label
        if (selectedPawnId >= 0)
        {
            var pawn = GameManager.Instance.PawnManager.Pawns.FirstOrDefault(p => p.Id == selectedPawnId);
            if (pawn != null)
            {
                statusLabel.Text = $"{pawn.Name}\nState: {pawn.State}\nPos: ({pawn.Position.X}, {pawn.Position.Y})";
            }
        }
        else
        {
            statusLabel.Text = isPlacingBuilding
                ? $"Placing {selectedBuildingType}... (Esc to cancel)"
                : "Click pawn to select\nDrag on trees to chop\nShift+drag stockpile  Ctrl+drag mine\nSpace=pause  1/3=speed";
        }

        // Update needs label
        var needsText = "Colonist Needs:\n";
        foreach (var pawn in pawns)
        {
            var hungerBar = GenerateBar((int)pawn.Hunger);
            var restBar = GenerateBar((int)pawn.Rest);
            needsText += $"{pawn.Name}: H[{hungerBar}] R[{restBar}]\n";
        }
        needsLabel.Text = needsText;

        // Update item visuals
        RenderItems();
        RenderStockpiles();
        RenderBuildings();

        // Update resource label
        var wood = GameManager.Instance.StockpileSystem.GetResourceCount(ItemType.Wood);
        var stone = GameManager.Instance.StockpileSystem.GetResourceCount(ItemType.Stone);
        var food = GameManager.Instance.StockpileSystem.GetResourceCount(ItemType.Food);
        var onGround = GameManager.Instance.ItemSystem.Items.Count;
        resourceLabel.Text = $"Resources:\nWood: {wood}  Stone: {stone}  Food: {food}\nOn Ground: {onGround}";

        // Update game speed label
        var speedStatus = GameManager.Instance.IsPaused ? "PAUSED" : $"{GameManager.Instance.GameSpeedMultiplier:F1}x";
        gameSpeedLabel.Text = $"Game Speed: {speedStatus}";
    }

    private string GenerateBar(int value)
    {
        // Generate a simple ASCII progress bar (0-10 scale)
        int filled = Mathf.Clamp(value / 10, 0, 10);
        return new string('█', filled) + new string('░', 10 - filled);
    }

    private ImageTexture GeneratePawnTexture(int pawnId)
    {
        var image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        var colors = new Color[]
        {
            new Color(1f, 0.2f, 0.2f), // Red
            new Color(0.2f, 1f, 0.2f), // Green
            new Color(0.2f, 0.2f, 1f), // Blue
        };
        image.SetPixel(0, 0, colors[pawnId % colors.Length]);
        return ImageTexture.CreateFromImage(image);
    }

    private ImageTexture GenerateTileTexture()
    {
        var image = Image.CreateEmpty(96, 16, false, Image.Format.Rgba8);

        // Generate 6 colored tiles (16x16 each)
        var colors = new Color[]
        {
            new Color(0.2f, 0.8f, 0.2f), // Grass
            new Color(0.6f, 0.4f, 0.2f), // Dirt
            new Color(0.2f, 0.3f, 0.8f), // Water
            new Color(0.1f, 0.5f, 0.1f), // Tree
            new Color(0.5f, 0.5f, 0.5f), // Rock
            new Color(0.4f, 0.8f, 0.3f), // Berry Bush
        };

        for (int tileIdx = 0; tileIdx < 6; tileIdx++)
        {
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    image.SetPixel(tileIdx * 16 + x, y, colors[tileIdx]);
                }
            }
        }

        return ImageTexture.CreateFromImage(image);
    }

    public override void _Input(InputEvent @event)
    {
        // Handle keyboard shortcuts for build menu
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            HandleBuildMenuKey(keyEvent.Keycode);
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    HandleLeftMouseDown();
                }
                else
                {
                    HandleLeftMouseUp();
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                HandleRightClick(mouseEvent.GlobalPosition);
            }
        }
        else if (@event is InputEventMouseMotion && isDragging)
        {
            HandleLeftDrag();
        }
    }

    private void HandleBuildMenuKey(Key key)
    {
        switch (key)
        {
            case Key.W:
                isPlacingBuilding = true;
                selectedBuildingType = BuildingType.Wall;
                break;
            case Key.B:
                isPlacingBuilding = true;
                selectedBuildingType = BuildingType.Bed;
                break;
            case Key.T:
                isPlacingBuilding = true;
                selectedBuildingType = BuildingType.Table;
                break;
            case Key.Escape:
                isPlacingBuilding = false;
                break;
            case Key.Space:
                GameManager.Instance.TogglePause();
                break;
            case Key.Key1:
                GameManager.Instance.SetGameSpeed(1f);
                break;
            case Key.Key3:
                GameManager.Instance.SetGameSpeed(3f);
                break;
        }
    }

    private void HandleLeftMouseDown()
    {
        var mousePos = GetGlobalMousePosition();
        var clickedTile = (Vector2I)(mousePos / 16f);

        if (isPlacingBuilding)
        {
            // Place building at clicked location
            var building = GameManager.Instance.BuildingSystem.PlaceBuilding(selectedBuildingType, clickedTile);
            if (building != null)
            {
                isPlacingBuilding = false;
            }
            return;
        }

        int clickedPawnId = GetPawnAtPosition(mousePos);

        if (clickedPawnId >= 0)
        {
            selectedPawnId = selectedPawnId == clickedPawnId ? -1 : clickedPawnId;
        }
        else
        {
            // Start drag designation
            isDragging = true;
            isDesignatingStockpile = Input.IsKeyPressed(Key.Shift);
            isDesignatingMine = Input.IsKeyPressed(Key.Ctrl);
            dragStartTile = clickedTile;
        }
    }

    private void HandleLeftMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            ClearDesignationVisuals();
        }
    }

    private void HandleLeftDrag()
    {
        var mousePos = GetGlobalMousePosition();
        var currentTile = (Vector2I)(mousePos / 16f);

        var minX = Mathf.Min(dragStartTile.X, currentTile.X);
        var maxX = Mathf.Max(dragStartTile.X, currentTile.X);
        var minY = Mathf.Min(dragStartTile.Y, currentTile.Y);
        var maxY = Mathf.Max(dragStartTile.Y, currentTile.Y);

        ClearDesignationVisuals();

        if (isDesignatingStockpile)
        {
            // Designate stockpile zones
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var tile = new Vector2I(x, y);
                    GameManager.Instance.StockpileSystem.CreateStockpile(tile);
                    ShowStockpileVisual(tile);
                }
            }
        }
        else if (isDesignatingMine)
        {
            // Designate rocks for mining
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (GameManager.Instance.GetTile(x, y) == TileType.Rock)
                    {
                        var tile = new Vector2I(x, y);
                        GameManager.Instance.DesignationSystem.DesignateRockForMining(tile);
                        ShowMineDesignationVisual(tile);
                    }
                }
            }
        }
        else
        {
            // Designate trees for chopping
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (GameManager.Instance.GetTile(x, y) == TileType.Tree)
                    {
                        var tile = new Vector2I(x, y);
                        GameManager.Instance.DesignationSystem.DesignateTreeForChopping(tile);
                        ShowDesignationVisual(tile);
                    }
                }
            }
        }
    }

    private void ShowStockpileVisual(Vector2I tile)
    {
        if (designationVisuals.ContainsKey(tile))
            return;

        var rect = new ColorRect();
        rect.Color = new Color(0.5f, 0.5f, 1f, 0.3f);
        rect.Size = Vector2.One * 16f;
        rect.GlobalPosition = tile * 16f;
        designationLayer.AddChild(rect);
        designationVisuals[tile] = rect;
    }

    private void ShowDesignationVisual(Vector2I tile)
    {
        if (designationVisuals.ContainsKey(tile))
            return;

        var rect = new ColorRect();
        rect.Color = new Color(1f, 1f, 0f, 0.3f);
        rect.Size = Vector2.One * 16f;
        rect.GlobalPosition = tile * 16f;
        designationLayer.AddChild(rect);
        designationVisuals[tile] = rect;
    }

    private void ShowMineDesignationVisual(Vector2I tile)
    {
        if (designationVisuals.ContainsKey(tile))
            return;

        var rect = new ColorRect();
        rect.Color = new Color(0.8f, 0.6f, 0.2f, 0.3f);
        rect.Size = Vector2.One * 16f;
        rect.GlobalPosition = tile * 16f;
        designationLayer.AddChild(rect);
        designationVisuals[tile] = rect;
    }

    private void ClearDesignationVisuals()
    {
        foreach (var rect in designationVisuals.Values)
        {
            rect.QueueFree();
        }
        designationVisuals.Clear();
    }

    private void HandleRightClick(Vector2 globalPosition)
    {
        if (selectedPawnId < 0)
            return;

        var mousePos = GetGlobalMousePosition();
        var targetTile = (Vector2I)(mousePos / 16f);

        GameManager.Instance.PawnManager.MovePawnTo(selectedPawnId, targetTile);
    }

    private int GetPawnAtPosition(Vector2 worldPos)
    {
        foreach (var pawn in GameManager.Instance.PawnManager.Pawns)
        {
            var pawnWorldPos = pawn.Position * 16f + Vector2.One * 8f;
            if (pawnWorldPos.DistanceTo(worldPos) < 12f)
            {
                return pawn.Id;
            }
        }
        return -1;
    }

    public override void _Process(double delta)
    {
        HandleCameraMovement((float)delta);
        HandleZoom();
        UpdatePawnVisuals();
    }

    private void HandleCameraMovement(float delta)
    {
        var velocity = Vector2.Zero;

        if (Input.IsActionPressed("ui_up"))
            velocity.Y -= 1;
        if (Input.IsActionPressed("ui_down"))
            velocity.Y += 1;
        if (Input.IsActionPressed("ui_left"))
            velocity.X -= 1;
        if (Input.IsActionPressed("ui_right"))
            velocity.X += 1;

        if (velocity != Vector2.Zero)
        {
            camera.GlobalPosition += velocity.Normalized() * CameraMoveSpeed * delta;
        }
    }

    private void HandleZoom()
    {
        if (Input.IsMouseButtonPressed(MouseButton.WheelUp))
        {
            camera.Zoom = (camera.Zoom + Vector2.One * ZoomSpeed).Clamp(
                Vector2.One * MinZoom,
                Vector2.One * MaxZoom
            );
        }

        if (Input.IsMouseButtonPressed(MouseButton.WheelDown))
        {
            camera.Zoom = (camera.Zoom - Vector2.One * ZoomSpeed).Clamp(
                Vector2.One * MinZoom,
                Vector2.One * MaxZoom
            );
        }
    }
}
