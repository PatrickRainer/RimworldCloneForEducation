using Godot;
using System.Collections.Generic;

namespace RimworldCloneForEducation;

public class PathfindingSystem
{
    private AStarGrid2D aStarGrid;

    public PathfindingSystem()
    {
        aStarGrid = new AStarGrid2D();
        aStarGrid.Region = new Rect2I(0, 0, Constants.MapWidth, Constants.MapHeight);
        aStarGrid.CellSize = Vector2I.One;
        aStarGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
        aStarGrid.UpdateFreeCells();
    }

    public void SetObstacle(Vector2I position, bool isObstacle)
    {
        if (!IsValidPosition(position))
            return;

        aStarGrid.SetCellSolid(position, isObstacle);
    }

    public List<Vector2I> FindPath(Vector2I start, Vector2I end)
    {
        if (!IsValidPosition(start) || !IsValidPosition(end))
            return new List<Vector2I>();

        var path = aStarGrid.GetIdPath(start, end);
        var result = new List<Vector2I>();

        foreach (var point in path)
        {
            result.Add((Vector2I)point);
        }

        return result;
    }

    private bool IsValidPosition(Vector2I pos)
    {
        return pos.X >= 0 && pos.X < Constants.MapWidth &&
               pos.Y >= 0 && pos.Y < Constants.MapHeight;
    }
}
