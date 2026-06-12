using System;

public class GameplayMarker : GameplayObject, IEquatable<GameplayMarker>
{
    public GameplayMarker(double renderTime, double BPM) : base(renderTime)
    {
        this.BPM = BPM;
    }

    public double BPM { get; protected set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as GameplayMarker);
    }

    public bool Equals(GameplayMarker other)
    {
        return other is not null &&
               base.Equals(other) &&
               RenderTime == other.RenderTime &&
               BPM == other.BPM;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), BPM);
    }
}
