using UnityEngine;

public abstract class GameplayObject : IRenderable
{
    public GameplayObject(double renderTime)
    {
        RenderTime = renderTime;
    }

    public double RenderTime { get; protected set; }

    public abstract void OnRender();

    public abstract void OnUnrender();
}
