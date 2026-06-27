using System;
using UnityEngine;

[Serializable]
public class EditorPoint : EditorDynamicObject
{
    public EditorPoint(Vector2 normalizedPosition, double renderTime) : base(renderTime)
    {
        this.NormalizedPosition = normalizedPosition;
    }

    public Vector2 NormalizedPosition { get; protected set; }

    public override EditorObject GetCopy()
    {
        return new EditorPoint(NormalizedPosition, RenderTime);
    }

    public override void Mirror(MirrorAxis axis)
    {
        Vector2 mirrorVector = new Vector2(axis.HasFlag(MirrorAxis.Vertical) ? 1f - NormalizedPosition.x : NormalizedPosition.x, axis.HasFlag(MirrorAxis.Horizontal) ? 1f - NormalizedPosition.y : NormalizedPosition.y);
        NormalizedPosition = mirrorVector;
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override bool GetPosition(out Vector2 position)
    {
        position = NormalizedPosition;
        return true;
    }
}
