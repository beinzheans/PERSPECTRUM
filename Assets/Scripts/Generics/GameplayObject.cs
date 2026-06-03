public abstract class GameplayObject : IRenderable
{
    public GameplayObject(double renderTime)
    {
        RenderTime = renderTime;
    }

    public double RenderTime { get; protected set; }

    public abstract void OnRender();

    /// <summary>
    /// Whether or not 
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool IsInRange(double time)
    {
        return MathHelper.IsTwoDoublesEqualWithEpsilion(time, RenderTime) || MathHelper.IsTwoDoublesEqualWithEpsilion(time, RenderTime + GameplayManager.k_LENIENCYTIMEFRAME) || (time > RenderTime && time < RenderTime + GameplayManager.k_LENIENCYTIMEFRAME);
    }
    public abstract void OnUnrender();
}
