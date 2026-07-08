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

    public override void Move_Mirror(MoveSelectedMode axis)
    {
        NormalizedPosition = MathHelper.GetMirroredPosition(NormalizedPosition, axis);
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override void Move_Rotate(MoveSelectedMode moveMode)
    {
        NormalizedPosition = MathHelper.GetRotatedPosition(NormalizedPosition, moveMode);
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }
    public override bool GetPosition(out Vector2 position)
    {
        position = NormalizedPosition;
        return true;
    }
}
