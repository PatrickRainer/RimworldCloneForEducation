using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RimworldCloneForEducation;

public class JobSystem
{
    private List<Job> jobs = new();
    private Dictionary<int, Job> activePawnJobs = new(); // pawnId -> current job

    public IReadOnlyList<Job> Jobs => jobs.AsReadOnly();

    public void AddJob(Job job)
    {
        jobs.Add(job);
        jobs = jobs.OrderByDescending(j => j.Priority).ToList();
    }

    public void Update(List<Pawn> pawns, float deltaTime)
    {
        // Update active jobs
        var completedJobs = new List<int>();
        foreach (var (pawnId, job) in activePawnJobs)
        {
            var pawn = pawns.FirstOrDefault(p => p.Id == pawnId);
            if (pawn == null)
                continue;

            // Check if pawn reached target
            if (pawn.Position != job.TargetPosition)
                continue;

            job.OnJobUpdate(pawn, deltaTime);

            if (job.IsJobComplete(pawn))
            {
                job.OnJobComplete(pawn);
                jobs.Remove(job);
                completedJobs.Add(pawnId);
            }
        }

        // Remove completed jobs
        foreach (var pawnId in completedJobs)
        {
            activePawnJobs.Remove(pawnId);
        }

        // Assign jobs to idle pawns
        foreach (var pawn in pawns)
        {
            if (pawn.State != PawnState.Idle || activePawnJobs.ContainsKey(pawn.Id))
                continue;

            var availableJob = FindJobForPawn(pawn);
            if (availableJob != null)
            {
                AssignJobToPawn(pawn, availableJob);
            }
        }
    }

    private Job FindJobForPawn(Pawn pawn)
    {
        foreach (var job in jobs)
        {
            if (job.AssignedPawnId == null && job.CanPawnDoJob(pawn))
            {
                return job;
            }
        }
        return null;
    }

    private void AssignJobToPawn(Pawn pawn, Job job)
    {
        job.AssignedPawnId = pawn.Id;
        job.Status = JobStatus.Active;
        activePawnJobs[pawn.Id] = job;

        // Move pawn to job target
        var pathfinding = GameManager.Instance.Pathfinding;
        pawn.Path = pathfinding.FindPath(pawn.Position, job.TargetPosition);
        if (pawn.Path.Count > 0)
        {
            pawn.State = PawnState.MovingToTarget;
            pawn.TargetPosition = job.TargetPosition;
            job.OnJobStart(pawn);
        }
        else
        {
            activePawnJobs.Remove(pawn.Id);
            jobs.Remove(job);
        }
    }

    public void RemoveJob(int jobId)
    {
        var job = jobs.FirstOrDefault(j => j.Id == jobId);
        if (job != null)
        {
            if (job.AssignedPawnId.HasValue)
            {
                activePawnJobs.Remove(job.AssignedPawnId.Value);
            }
            jobs.Remove(job);
        }
    }
}
