using Godot;

namespace RimworldCloneForEducation;

public class MapGenerator
{
    private FastNoiseLite noise;

    public MapGenerator()
    {
        noise = new FastNoiseLite();
        noise.Seed = (int)GD.Randi();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        noise.Frequency = Constants.NoiseScale;
    }

    public TileType[,] GenerateMap()
    {
        var tiles = new TileType[Constants.MapWidth, Constants.MapHeight];

        for (int x = 0; x < Constants.MapWidth; x++)
        {
            for (int y = 0; y < Constants.MapHeight; y++)
            {
                float value = noise.GetNoise2D(x, y);
                // Normalize from [-1, 1] to [0, 1]
                value = (value + 1f) / 2f;

                tiles[x, y] = ClassifyTile(value);
            }
        }

        return tiles;
    }

    private TileType ClassifyTile(float value)
    {
        return value switch
        {
            > Constants.TreeThreshold => TileType.Tree,
            > Constants.BerryThreshold => TileType.BerryBush,
            > Constants.RockThreshold => TileType.Rock,
            > Constants.WaterThreshold => TileType.Water,
            > 0.3f => TileType.Dirt,
            _ => TileType.Grass,
        };
    }
}
