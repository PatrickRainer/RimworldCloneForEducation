namespace RimworldCloneForEducation;

public static class Constants
{
    public const int MapWidth = 64;
    public const int MapHeight = 64;
    public const float TicksPerSecond = 10f;
    public const float TickDuration = 1f / TicksPerSecond;

    public const float NoiseScale = 0.08f;
    public const float TreeThreshold = 0.55f;
    public const float RockThreshold = 0.35f;
    public const float BerryThreshold = 0.48f;
    public const float WaterThreshold = 0.2f;

    // Needs thresholds (0-100)
    public const float HungerThreshold = 70f;
    public const float RestThreshold = 70f;

    // Needs decay per tick
    public const float HungerDecayPerTick = 0.5f;
    public const float RestDecayPerTick = 0.3f;
    public const float RestRecoveryPerTick = 1f;
    public const float EatingRecoveryPerTick = 2f;
}

public enum TileType
{
    Grass = 0,
    Dirt = 1,
    Water = 2,
    Tree = 3,
    Rock = 4,
    BerryBush = 5,
}
