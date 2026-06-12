using System;
using UnityEngine;

public class VisualJudgementObject : GameplayObject, IEquatable<VisualJudgementObject>
{
    public VisualJudgementObject(VisualHitbox hitbox, double renderTime, Vector2 normalizedPosition, JudgementType judgementType) : base(renderTime)
    {
        this.hitbox = hitbox;
        NormalizedPosition = normalizedPosition;
        JudgementType = judgementType;
    }

    private VisualHitbox hitbox; // this field is to make the hashcode different.
    public Vector2 NormalizedPosition;
    public JudgementType JudgementType;

    public override bool Equals(object obj)
    {
        return Equals(obj as VisualJudgementObject);
    }

    public bool Equals(VisualJudgementObject other)
    {
        return other is not null &&
               base.Equals(other) &&
               hitbox.Equals(hitbox) &&
               RenderTime == other.RenderTime &&
               NormalizedPosition.Equals(other.NormalizedPosition) &&
               JudgementType == other.JudgementType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), hitbox, NormalizedPosition, JudgementType);
    }
}

public enum JudgementType
{
    MATCH = 0,
    MISMATCH = 1,
    MISS = 2
}
