using Godot;

namespace RimworldCloneForEducation;

public abstract class Job
{
    public int Id { get; }
    public JobType Type { get; }
    public int Priority { get; }
    public Vector2I TargetPosition { get; set; }
    public int? AssignedPawnId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;

    private static int nextJobId = 0;

    protected Job(JobType type, int priority, Vector2I targetPosition)
    {
        Id = nextJobId++;
        Type = type;
        Priority = priority;
        TargetPosition = targetPosition;
    }

    public abstract bool CanPawnDoJob(Pawn pawn);
    public abstract void OnJobStart(Pawn pawn);
    public abstract void OnJobUpdate(Pawn pawn, float deltaTime);
    public abstract void OnJobComplete(Pawn pawn);
    public abstract bool IsJobComplete(Pawn pawn);
}

public enum JobType
{
    Eat = 10,
    Sleep = 9,
    Build = 8,
    Chop = 7,
    Mine = 6,
    Haul = 5,
}

public enum JobStatus
{
    Pending,
    Active,
    Complete,
    Failed,
}
