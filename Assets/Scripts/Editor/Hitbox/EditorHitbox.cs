using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorHitbox : EditorDynamicObject<EditorHitbox>, IEquatable<EditorHitbox>
{
    public EditorHitbox(Vector2 normalizedPosition, int rawPixelSize, HitboxType hitboxType, double renderTime) : base(renderTime)
    {
        NormalizedPosition = normalizedPosition;
        RawPixelSize = rawPixelSize;
        HitboxType = hitboxType;
    }

    public Vector2 NormalizedPosition { get; protected set; }
    public int RawPixelSize { get; protected set; }
    public HitboxType HitboxType { get; protected set; }
    public bool Equals(EditorHitbox other)
    {
        return this.NormalizedPosition == other.NormalizedPosition && this.RenderTime == other.RenderTime && this.RawPixelSize == other.RawPixelSize;
    }

    public override void OnDelete(ref List<EditorHitbox> listToEdit)
    {
        base.OnDelete(ref listToEdit);
    }

    public override void OnEdit(EditorHitbox editable)
    {
        if (editable is not EditorHitbox newHitbox)
        {
            return;
        }

        NormalizedPosition = newHitbox.NormalizedPosition;
        RenderTime = newHitbox.RenderTime;
        RawPixelSize = newHitbox.RawPixelSize;
        HitboxType = newHitbox.HitboxType;

        OnRender(); // re-render the hitbox automatically
    }

    public override void OnPlace(ref List<EditorHitbox> listToEdit)
    {
        base.OnPlace(ref listToEdit);
    }

    public override void OnRender()
    {
        EditorManager.EditorInstance.InvokeRenderRenderableEvent(this);
    }
    public override void OnUnrender()
    {
        EditorManager.EditorInstance.InvokeUnrenderRenderableEvent(this);
    }

    public override void OnSelect()
    {
        base.OnSelect();
    }

}

public enum HitboxType
{
    A = 0,
    B = 1
}
