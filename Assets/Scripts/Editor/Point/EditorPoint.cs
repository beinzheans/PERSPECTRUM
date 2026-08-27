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

    public override void Move_AxesMirror(MoveSelectedMode axis)
    {
        NormalizedPosition = MathHelper.GetMirroredPosition(NormalizedPosition, axis);
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }
    public override void Move_TimeMirror(MoveSelectedMode moveMode, in double middleTime, in double timeOffset)
    {
        RenderTime = MathHelper.GetMirroredTime(RenderTime, middleTime, timeOffset, moveMode);
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override void Move_Rotate(MoveSelectedMode moveMode)
    {
        NormalizedPosition = MathHelper.GetRotatedPosition(NormalizedPosition, moveMode);
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override void Move_Delta(Vector2 normalizedMoveDelta)
    {
        NormalizedPosition += normalizedMoveDelta;
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override bool GetPosition(out Vector2 position)
    {
        position = NormalizedPosition;
        return true;
    }
}
