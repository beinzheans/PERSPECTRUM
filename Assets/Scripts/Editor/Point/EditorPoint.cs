using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorPoint : EditorDynamicObject<EditorPoint>, IEquatable<EditorPoint>
{
    public EditorPoint(Vector2 normalizedPosition, double renderTime) : base(renderTime)
    {
        this.NormalizedPosition = normalizedPosition;
    }

    public Vector2 NormalizedPosition { get; private set; }

    public bool Equals(EditorPoint other)
    {
        return this.NormalizedPosition == other.NormalizedPosition && this.RenderTime == other.RenderTime;
    }

    public override void OnEdit(EditorPoint editable)
    {
        this.NormalizedPosition = editable.NormalizedPosition;
        this.RenderTime = editable.RenderTime;

        OnRender();
    }

    public override void OnRender()
    {

    }

    public override void OnUnrender()
    {
    }
}
