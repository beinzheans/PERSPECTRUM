using System;

public class GameplayMarker : GameplayObject, IEquatable<GameplayMarker>
{
    public GameplayMarker(double renderTime, double BPM, string displayMessage, double displayTime) : base(renderTime)
    {
        this.BPM = BPM;
        DisplayMessage = displayMessage;
        DisplayTime = displayTime;
    }

    public double BPM { get; protected set; }
    public string DisplayMessage { get; protected set; }
    public double DisplayTime { get; protected set; }
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
