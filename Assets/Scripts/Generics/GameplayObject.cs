using System;

public abstract class GameplayObject : IEquatable<GameplayObject>
{
    public GameplayObject(double renderTime)
    {
        RenderTime = renderTime;
    }

    public double RenderTime { get; protected set; }
    public bool IsRendered = false;

    /// <summary>
    /// Whether or not this object is in range of the render time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool IsInRenderRange(double time)
    {
        double maxRenderTime = time + GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime + GameplayManager.k_POOLLOOKAHEADTIME;
        return RenderTime > time && RenderTime < maxRenderTime;
    }

    /// <summary>
    /// Whether or not this object is in range of the unrender time
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool IsInUnrenderRange(double time)
    {
        double minTime = time - GameplayManager.k_POOLUNRENDERTIMETHRESHOLD;
        return RenderTime < minTime;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as GameplayObject);
    }

    public bool Equals(GameplayObject other)
    {
        return other is not null &&
               RenderTime == other.RenderTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RenderTime);
    }
}
