using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EditorLine : EditorDynamicObject, IConvertable<VisualLine>
{
    public EditorLine(Vector2 fromNormalizedPosition, Vector2 toNormalizedPosition, double fromTime, double toTime) : base(fromTime)
    {
        FromNormalizedPosition = fromNormalizedPosition;
        ToNormalizedPosition = toNormalizedPosition;
        FromTime = fromTime;
        ToTime = toTime;
    }

    public Vector2 FromNormalizedPosition { get; protected set; }
    public Vector2 ToNormalizedPosition { get; protected set; }
    public double FromTime { get; protected set; }

    public double ToTime { get; protected set; }

    public Vector2 GetFromToVector()
    {
        return ToNormalizedPosition - FromNormalizedPosition;
    }

    /// <summary>
    /// Get time of this line as a vector. Positive values indicate the line is moving in the +t direction. <br></br>
    /// Typically lines should not be in -t direction.
    /// </summary>
    /// <returns></returns>
    public double GetTimeLengthDirection()
    {
        return ToTime - FromTime;
    }

    public EditorPoint EvaluatePositionAtProgress(float progress)
    {
        Vector2 dir = GetFromToVector();
        double time = GetTimeLengthDirection();

        Vector2 newPosition = FromNormalizedPosition + dir * progress;
        double newTime = FromTime + time * progress;

        return new EditorPoint(newPosition, newTime);
    }

    public EditorPoint EvaluatePositionAtTime(double time)
    {
        float progress = (float)((time - FromTime) / GetTimeLengthDirection());
        return EvaluatePositionAtProgress(progress);
    }

    public void EvaluateLineAtProgress(float progress, out Vector2 position, out double time)
    {
        Vector2 dir = GetFromToVector();
        double timeDir = GetTimeLengthDirection();

        Vector2 newPosition = FromNormalizedPosition + dir * progress;
        double newTime = FromTime + timeDir * progress;

        time = newTime;
        position = newPosition;
    }
    /// <summary>
    /// Subdivides a line into number of beats and adds the subdivision result to a preallocated list.
    /// </summary>
    /// <param name="numberOfBeats"></param>
    /// <param name="skipLastPoint">Whether or not to skip evaluating the last point</param>
    /// <param name="cacheResult"></param>
    public void SubdivideLineSegmentWithEndpoints(int numberOfBeats, bool skipLastPoint, ref List<EditorPoint> cacheResult)
    {
        if (numberOfBeats <= 1)
        {
            return;
        }

        float dt;
        if (skipLastPoint)
        {
            dt = 1f / numberOfBeats;

            for (int i = 1; i < numberOfBeats; i++) // skip the first point and last point
            {
                float t = dt * i;
                cacheResult.Add(EvaluatePositionAtProgress(t));
            }
        }
        else
        {
            dt = 1f / (numberOfBeats - 1);

            for (int i = 1; i < numberOfBeats - 1; i++)
            {
                float t = dt * i;
                cacheResult.Add(EvaluatePositionAtProgress(t));
            }
        }

        return;
    }

    public override EditorObject GetCopy()
    {
        return new EditorLine(FromNormalizedPosition, ToNormalizedPosition, FromTime, ToTime);
    }

    public override void Mirror(MirrorAxis axis)
    {
        FromNormalizedPosition = new Vector2(axis.HasFlag(MirrorAxis.Vertical) ? 1f - FromNormalizedPosition.x : FromNormalizedPosition.x, axis.HasFlag(MirrorAxis.Horizontal) ? 1f - FromNormalizedPosition.y : FromNormalizedPosition.y);
        ToNormalizedPosition = new Vector2(axis.HasFlag(MirrorAxis.Vertical) ? 1f - ToNormalizedPosition.x : ToNormalizedPosition.x, axis.HasFlag(MirrorAxis.Horizontal) ? 1f - ToNormalizedPosition.y : ToNormalizedPosition.y);

        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override void AddDeltaTime(double deltaTime)
    {
        FromTime += deltaTime;
        ToTime += deltaTime;
        RenderTime += deltaTime;
    }

    public bool Convert(out VisualLine converted)
    {
        converted = new VisualLine(FromNormalizedPosition, ToNormalizedPosition, FromTime, ToTime);
        return true;
    }
}