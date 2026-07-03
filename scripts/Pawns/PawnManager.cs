using Godot;
using System.Collections.Generic;

namespace RimworldCloneForEducation;

public class PawnManager
{
    private List<Pawn> pawns = new();
    private PathfindingSystem pathfinding;
    private const float MoveSpeed = 1f; // tiles per tick
    private Dictionary<int, float> pawnMovementProgress = new();

    public IReadOnlyList<Pawn> Pawns => pawns.AsReadOnly();

    public PawnManager(PathfindingSystem pathfinding)
    {
        this.pathfinding = pathfinding;
    }

    public void SpawnPawns()
    {
        // Spawn 3 colonists at random valid locations
        var random = GD.Randi();
        var startPositions = new List<Vector2I>();

        for (int i = 0; i < 3; i++)
        {
            Vector2I pos;
            int attempts = 0;
            do
            {
                pos = new Vector2I(
                    (int)(GD.Randf() * Constants.MapWidth),
                    (int)(GD.Randf() * Constants.MapHeight)
                );
                attempts++;
            } while (IsObstructed(pos) && attempts < 10);

            if (attempts < 10)
            {
                var pawn = new Pawn(i, $"Colonist {i + 1}", pos);
                pawns.Add(pawn);
            }
        }
    }

    public void UpdatePawns(float delta)
    {
        foreach (var pawn in pawns)
        {
            UpdatePawnMovement(pawn, delta);
        }
    }

    private void UpdatePawnMovement(Pawn pawn, float delta)
    {
        if (pawn.State != PawnState.MovingToTarget)
            return;

        if (pawn.Path.Count == 0)
        {
            pawn.State = PawnState.Idle;
            pawnMovementProgress.Remove(pawn.Id);
            return;
        }

        if (!pawnMovementProgress.ContainsKey(pawn.Id))
            pawnMovementProgress[pawn.Id] = 0f;

        // Move towards first waypoint in path
        var target = pawn.Path[0];
        Vector2 direction = (Vector2)(target - pawn.Position);
        float distance = direction.Length();

        pawnMovementProgress[pawn.Id] += MoveSpeed * delta;

        if (pawnMovementProgress[pawn.Id] >= distance)
        {
            // Reached waypoint
            pawn.Position = target;
            pawn.Path.RemoveAt(0);
            pawnMovementProgress[pawn.Id] = 0f;

            if (pawn.Path.Count == 0)
            {
                pawn.State = PawnState.Idle;
                pawnMovementProgress.Remove(pawn.Id);
            }
        }
    }

    public void MovePawnTo(int pawnId, Vector2I targetPosition)
    {
        var pawn = pawns.FirstOrDefault(p => p.Id == pawnId);
        if (pawn == null)
            return;

        var path = pathfinding.FindPath(pawn.Position, targetPosition);
        if (path.Count > 0)
        {
            pawn.Path = path;
            pawn.MoveTo(targetPosition);
        }
    }

    private bool IsObstructed(Vector2I pos)
    {
        var tileType = GameManager.Instance.GetTile(pos.X, pos.Y);
        return tileType == TileType.Water;
    }
}
