using System;

/// <summary>
/// A generic class to describe any editor object.
/// </summary>
public class EditorObject : IRenderable
{
    protected EditorObject(double renderTime)
    {
        RenderTime = renderTime;
    }

    public double RenderTime;

    public virtual void OnRender()
    {
        EditorManager.EditorInstance.InvokeRenderRenderableEvent(this);
    }

    public virtual void OnUnrender()
    {
        EditorManager.EditorInstance.InvokeUnrenderRenderableEvent(this);
    }

    public virtual EditorObject GetCopy()
    {
        return new EditorObject(RenderTime);
    }

    public virtual void AddDeltaTime(double deltaTime)
    {
        RenderTime += deltaTime;
    }
}