public class GameplayMarker : GameplayObject
{
    public GameplayMarker(double renderTime, double BPM) : base(renderTime)
    {
        this.BPM = BPM;
    }

    public double BPM { get; protected set; }

    // can not be rendered nor unrendered
    public override void OnRender()
    {
        return;
    }

    public override void OnUnrender()
    {
        return;
    }
}
