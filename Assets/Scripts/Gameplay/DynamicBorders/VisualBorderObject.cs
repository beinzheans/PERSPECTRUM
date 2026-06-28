using System;
using System.Collections.Generic;

public class VisualBorderObject : GameplayObject, IEquatable<VisualBorderObject>
{
    public VisualBorderObject(VisualHitbox hitbox) : base(hitbox.RenderTime)
    {
        AssociatedHitbox = hitbox;
    }

    public VisualHitbox AssociatedHitbox { get; private set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as VisualBorderObject);
    }

    public bool Equals(VisualBorderObject other)
    {
        return other is not null &&
               base.Equals(other) &&
               EqualityComparer<VisualHitbox>.Default.Equals(AssociatedHitbox, other.AssociatedHitbox);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), AssociatedHitbox);
    }
}
