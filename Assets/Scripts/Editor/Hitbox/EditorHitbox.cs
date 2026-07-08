using System;
using UnityEngine;

[Serializable]
public class EditorHitbox : EditorDynamicObject, IConvertable<VisualHitbox>
{
    public EditorHitbox(Vector2 normalizedPosition, float normalizedSize, HitboxType hitboxType, double renderTime) : base(renderTime)
    {
        NormalizedPosition = normalizedPosition;
        NormalizedSize = normalizedSize;
        HitboxType = hitboxType;
    }

    public Vector2 NormalizedPosition { get; protected set; }
    public float NormalizedSize { get; protected set; }
    public HitboxType HitboxType { get; protected set; }

    public override EditorObject GetCopy()
    {
        return new EditorHitbox(NormalizedPosition, NormalizedSize, HitboxType, RenderTime);
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

    public bool Convert(out VisualHitbox converted)
    {
        converted = new VisualHitbox(NormalizedPosition, RenderTime, NormalizedSize, HitboxType);
        return true;
    }
}

public enum HitboxType
{
    A = 0,
    B = 1,
    BOMB = 2
}
