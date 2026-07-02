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
    public double GetTimeVector()
    {
        return ToTime - FromTime;
    }

    public EditorPoint EvaluatePositionAtProgress(float progress)
    {
        Vector2 dir = GetFromToVector();
        double time = GetTimeVector();

        Vector2 newPosition = FromNormalizedPosition + dir * progress;
        double newTime = FromTime + time * progress;

        return new EditorPoint(newPosition, newTime);
    }

    public double EvaluateTimeAtProgress(float progress)
    {
        return FromTime + GetTimeVector() * progress;
    }
    public EditorPoint EvaluatePositionAtTime(double time)
    {
        float progress = (float)((time - FromTime) / GetTimeVector());
        return EvaluatePositionAtProgress(progress);
    }

    public void EvaluateLineAtProgress(float progress, out Vector2 position, out double time)
    {
        Vector2 dir = GetFromToVector();
        double timeDir = GetTimeVector();

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

    public override void Move_Mirror(MoveSelectedMode axis)
    {
        FromNormalizedPosition = MathHelper.GetMirroredPosition(FromNormalizedPosition, axis);
        ToNormalizedPosition = MathHelper.GetMirroredPosition(ToNormalizedPosition, axis);

        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    public override void Move_Rotate(MoveSelectedMode moveMode)
    {
        FromNormalizedPosition = MathHelper.GetRotatedPosition(FromNormalizedPosition, moveMode);
        ToNormalizedPosition = MathHelper.GetRotatedPosition(ToNormalizedPosition, moveMode);
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