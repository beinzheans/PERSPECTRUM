using UnityEngine;

public class VisualLine : GameplayObject
{
    public VisualLine(Vector2 initialPosition, Vector2 terminalPosition, double renderTime) : base(renderTime)
    {
        InitialPosition = initialPosition;
        TerminalPosition = terminalPosition;
    }

    public Vector2 InitialPosition { get; protected set; }
    public Vector2 TerminalPosition { get; protected set; }

    public override void OnRender()
    {
        Debug.Log($"Rendering some line from {InitialPosition} to {TerminalPosition} @ t = {RenderTime}");
    }

    public override void OnUnrender()
    {
        // test
    }
}
