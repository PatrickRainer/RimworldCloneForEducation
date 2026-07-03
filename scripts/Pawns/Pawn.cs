using Godot;
using System.Collections.Generic;

namespace RimworldCloneForEducation;

public class Pawn
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Vector2I Position { get; set; }
    public Vector2I TargetPosition { get; set; }
    public List<Vector2I> Path { get; set; } = new();

    public PawnState State { get; set; } = PawnState.Idle;
    public float Hunger { get; set; } = 50f;
    public float Rest { get; set; } = 50f;

    public Pawn(int id, string name, Vector2I startPosition)
    {
        Id = id;
        Name = name;
        Position = startPosition;
        TargetPosition = startPosition;
    }

    public void MoveTo(Vector2I target)
    {
        TargetPosition = target;
        State = PawnState.MovingToTarget;
    }

    public void ClearPath()
    {
        Path.Clear();
        TargetPosition = Position;
        State = PawnState.Idle;
    }
}

public enum PawnState
{
    Idle,
    MovingToTarget,
    Working,
}
