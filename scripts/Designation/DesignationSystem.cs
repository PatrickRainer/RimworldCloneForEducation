using Godot;
using System.Collections.Generic;

namespace RimworldCloneForEducation;

public class DesignationSystem
{
    private HashSet<Vector2I> chopDesignations = new();
    private HashSet<Vector2I> mineDesignations = new();
    private JobSystem jobSystem;

    public IReadOnlyCollection<Vector2I> ChopDesignations => chopDesignations;
    public IReadOnlyCollection<Vector2I> MineDesignations => mineDesignations;

    public DesignationSystem(JobSystem jobSystem)
    {
        this.jobSystem = jobSystem;
    }

    public void DesignateTreeForChopping(Vector2I position)
    {
        var tileType = GameManager.Instance.GetTile(position.X, position.Y);
        if (tileType != TileType.Tree)
            return;

        if (chopDesignations.Add(position))
        {
            var job = new ChopJob(position);
            jobSystem.AddJob(job);
        }
    }

    public void RemoveChopDesignation(Vector2I position)
    {
        chopDesignations.Remove(position);
    }

    public bool IsDesignatedForChopping(Vector2I position)
    {
        return chopDesignations.Contains(position);
    }

    public void DesignateRockForMining(Vector2I position)
    {
        var tileType = GameManager.Instance.GetTile(position.X, position.Y);
        if (tileType != TileType.Rock)
            return;

        if (mineDesignations.Add(position))
        {
            var job = new MineJob(position);
            jobSystem.AddJob(job);
        }
    }

    public void RemoveRockDesignation(Vector2I position)
    {
        mineDesignations.Remove(position);
    }

    public bool IsDesignatedForMining(Vector2I position)
    {
        return mineDesignations.Contains(position);
    }

    public void ClearAllDesignations()
    {
        chopDesignations.Clear();
        mineDesignations.Clear();
    }
}
