using UnityEngine;

public class VisualHitbox : GameplayObject
{
    public VisualHitbox(Vector2 normalizedPosition, double renderTime, double normalizedSize) : base(renderTime)
    {
        NormalizedPosition = normalizedPosition;
        NormalizedSize = normalizedSize;
    }

    public Vector2 NormalizedPosition { get; protected set; }
    public double NormalizedSize { get; protected set; }

    public override void OnRender()
    {
        Debug.Log($"Rendering hitbox at {NormalizedPosition} @ t = {RenderTime}");
    }

    public override void OnUnrender()
    {
        // idk
    }
}
