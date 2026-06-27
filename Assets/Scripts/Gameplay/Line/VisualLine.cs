using System;
using UnityEngine;

public class VisualLine : GameplayObject, IEquatable<VisualLine>
{
    public VisualLine(Vector2 initialPosition, Vector2 terminalPosition, double initialTime, double terminalTime) : base(initialTime)
    {
        InitialPosition = initialPosition;
        TerminalPosition = terminalPosition;

        InitialTime = initialTime;
        TerminalTime = terminalTime;
    }

    public Vector2 InitialPosition { get; protected set; }
    public Vector2 TerminalPosition { get; protected set; }

    public double InitialTime { get; protected set; }
    public double TerminalTime { get; protected set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as VisualLine);
    }

    public bool Equals(VisualLine other)
    {
        return other is not null &&
               InitialPosition.Equals(other.InitialPosition) &&
               TerminalPosition.Equals(other.TerminalPosition) &&
               InitialTime == other.InitialTime &&
               TerminalTime == other.TerminalTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InitialPosition, TerminalPosition, InitialTime, TerminalTime);
    }
}
