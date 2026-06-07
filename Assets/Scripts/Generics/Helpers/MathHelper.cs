using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class MathHelper
{
    /// <summary>
    /// Get a normalized point w.r.t to a reference rect given a screen point, where the bottom-left corner is (0, 0). <br></br>
    /// Gracefully handles division by zero (where the reference has size 0). Returns true if conversion result is successful.
    /// </summary>
    /// <param name="rawPoint"></param>
    /// <param name="referenceRect"></param>
    /// <param name="normalizedPoint"></param>
    /// <returns></returns>
    public static bool GetNormalizedPointInsideReferenceUI(Vector2 rawPoint, RectTransform referenceRect, out Vector2 normalizedPoint)
    {
        Vector2 size = new Vector2(referenceRect.rect.width, referenceRect.rect.height);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRect, rawPoint, null, out Vector2 relativePointToCentre);
        if (size.x == 0f && size.y == 0f)
        {
            normalizedPoint = Vector2.zero;
            return false;
        }
        else if (size.x == 0f)
        {
            normalizedPoint = new Vector2(relativePointToCentre.x, (relativePointToCentre.y + 0.5f * size.y) / size.y);
            return true;
        }
        else if (size.y == 0f)
        {
            normalizedPoint = new Vector2((relativePointToCentre.x + 0.5f * size.x) / size.x, relativePointToCentre.y);
            return true;
        }

        normalizedPoint = (relativePointToCentre + 0.5f * size) / size;
        return true;
    }

    private static Vector3[] worldCornerBuffer = new Vector3[4];
    /// <summary>
    /// Get the screen point (pixel position) given a normalized point w.r.t to a reference rect. Assumes normalized point uses (0, 0) for bottom-left corner.
    /// </summary>
    /// <param name="normalizedPoint"></param>
    /// <param name="referenceRect"></param>
    /// <returns></returns>
    public static Vector2 GetScreenPointFromNormalizedPointInsideReferenceUI(Vector2 normalizedPoint, RectTransform referenceRect)
    {
        referenceRect.GetWorldCorners(worldCornerBuffer);
        Vector2 max = worldCornerBuffer[2];
        Vector2 min = worldCornerBuffer[0];

        Vector2 result = new Vector2(normalizedPoint.x * (max.x - min.x) + min.x, normalizedPoint.y * (max.y - min.y) + min.y);
        return result;
    }

    public static Color GetTransparentVersionOfColor(Color color)
    {
        return new Color(color.r, color.g, color.b, 0f);
    }

    /// <summary>
    /// Get the from to vector in raw pixel coordinates, given normalized from to points w.r.t to a reference rect. <br></br>
    /// This is necessary because the from to vector calculated from normalized vector is NOT the same as the raw pixel from to vector.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="referenceRect"></param>
    /// <returns></returns>
    public static Vector2 GetPixelFromToVectorFromNormalizedPoints(Vector2 from, Vector2 to, RectTransform referenceRect)
    {
        Vector2 fromPixelPosition = GetScreenPointFromNormalizedPointInsideReferenceUI(from, referenceRect);
        Vector2 toPixelPosition = GetScreenPointFromNormalizedPointInsideReferenceUI(to, referenceRect);

        return toPixelPosition - fromPixelPosition;
    }

    public static Vector2 GetPixelSizeOfNormalizedSizeVector(Vector2 normalizedSizeVector, RectTransform referenceRect)
    {
        referenceRect.GetWorldCorners(worldCornerBuffer);
        Vector2 max = worldCornerBuffer[2];
        Vector2 min = worldCornerBuffer[0];

        Vector2 result = new Vector2(normalizedSizeVector.x * (max.x - min.x), normalizedSizeVector.y * (max.y - min.y));

        return result;
    }

    public const float k_FLOATCOMPAREEPSILION = 1e-5f;
    /// <summary>
    /// Returns the results when evaluating the intersection of two unit vectors while handling non-intersection case.
    /// Returns false if no intersection, otherwise returns true.
    /// </summary>
    /// <param name="aPosition"></param>
    /// <param name="bPosition"></param>
    /// <param name="aUnitDir"></param>
    /// <param name="bUnitDir"></param>
    /// <param name="aScaler"></param>
    /// <param name="bScaler"></param>
    /// <returns></returns>
    public static bool IsTwoUnitVectorsIntersect(Vector2 aPosition, Vector2 bPosition, Vector2 aUnitDir, Vector2 bUnitDir, out float aScaler, out float bScaler)
    {
        float crossProduct_result = aUnitDir.x * bUnitDir.y - aUnitDir.y * bUnitDir.x;
        if (Mathf.Abs(crossProduct_result) <= k_FLOATCOMPAREEPSILION) // cross product approx. 0, consider it parallel.
        {
            aScaler = 0f;
            bScaler = 0f;
            return false;
        }

        aScaler = (bUnitDir.y * (bPosition.x - aPosition.x) - bUnitDir.x * (bPosition.y - aPosition.y)) / crossProduct_result;
        bScaler = (aUnitDir.y * (bPosition.x - aPosition.x) - aUnitDir.x * (bPosition.y - aPosition.y)) / crossProduct_result;
        return true;
    }

    /// <summary>
    /// Returns the results when evaluating the intersection of two vectors while handling non-intersection case.
    /// Returns false if no intersection or if point of intersection is outside of either line segments, otherwise returns true.
    /// </summary>
    /// <param name="aPosition"></param>
    /// <param name="bPosition"></param>
    /// <param name="aVector"></param>
    /// <param name="bVector"></param>
    /// <param name="aScaler"></param>
    /// <param name="bScaler"></param>
    /// <returns></returns>
    public static bool IsTwoVectorsIntersect(Vector2 aPosition, Vector2 bPosition, Vector2 aVector, Vector2 bVector, out float aScaler, out float bScaler)
    {
        float crossProduct_result = aVector.x * bVector.y - aVector.y * bVector.x;
        if (Mathf.Abs(crossProduct_result) <= k_FLOATCOMPAREEPSILION) // cross product approx. 0, consider it parallel.
        {
            aScaler = 0f;
            bScaler = 0f;
            return false;
        }

        aScaler = (bVector.y * (bPosition.x - aPosition.x) - bVector.x * (bPosition.y - aPosition.y)) / crossProduct_result;
        bScaler = (aVector.y * (bPosition.x - aPosition.x) - aVector.x * (bPosition.y - aPosition.y)) / crossProduct_result;

        return aScaler >= 0f && aScaler <= 1f && bScaler >= 0f && bScaler <= 1f;
    }

    public const double k_DOUBLECOMPAREEPSILION = 1e-9d;
    /// <summary>
    /// Computes the floor of a value, accounting for potential rounding errors that lead to false results <br></br>
    /// Eg. if we evaluate 2.99999993 instead of 3 due to representation error, it will return 3 instead of 2, hence the "common sense" floor.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CommonSenseFloor(double value)
    {
        return (int)math.floor(value + k_DOUBLECOMPAREEPSILION);
    }

    /// <summary>
    /// Computes the ceil of a value, accounting for potential rounding errors that lead to false results <br></br>
    /// Eg. if we evaluate 3.0000001 instead of 3 due to representation error, it will return 3 instead of 4, hence the "common sense" ceil.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CommonSenseCeil(double value)
    {
        return (int)math.ceil(value - k_DOUBLECOMPAREEPSILION);
    }

    /// <summary>
    /// Compares two double representation of values with a pre-defined epsilion <see cref="k_DOUBLECOMPAREEPSILION"/>. <br></br>
    /// Returns true if x and y are sufficiently close to each other
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool IsTwoDoublesEqualWithEpsilion(double x, double y)
    {
        return math.abs(x - y) <= k_DOUBLECOMPAREEPSILION;
    }

    /// <summary>
    /// Compares two float representation of values with a pre-defined epsilion <see cref="k_FLOATCOMPAREEPSILION"/>. <br></br>
    /// Returns true if x and y are sufficiently close to each other
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static bool IsTwoFloatsEqualWithEpsilion(float x, float y)
    {
        return math.abs(x - y) <= k_FLOATCOMPAREEPSILION;
    }
    public static bool GetActiveTimelineMarkerAtTime(double time, List<TimelineMarker> allMarkers, out TimelineMarker resultMarker)
    {
        double filterTime = time;
        double maxTimeWithinFilterTime = -1d;
        int indexResult = -1;

        for (int i = 0; i < allMarkers.Count; i++)
        {
            if (allMarkers[i].RenderTime > filterTime)
            {
                continue;
            }

            if (allMarkers[i].RenderTime >= maxTimeWithinFilterTime)
            {
                maxTimeWithinFilterTime = allMarkers[i].RenderTime;
                indexResult = i;
            }
        }

        if (indexResult == -1)
        {
            resultMarker = null;
            return false;
        }

        resultMarker = allMarkers[indexResult];
        return true;
    }

    public static bool CalculateBeatIndexOfMarker(TimelineMarker targetMarker, List<TimelineMarker> allMarkers, int numberOfSubdivisions, out int firstBeat, out int lastBeat, out double timeOfFirstBeat)
    {
        if (numberOfSubdivisions <= 0 || allMarkers.Count <= 0)
        {
            firstBeat = -1;
            lastBeat = -1;
            timeOfFirstBeat = -1d;
            return false;
        }

        TimelineMarker[] sortedMarkers = allMarkers.OrderBy(x => x.RenderTime).ToArray();
        if (targetMarker == sortedMarkers[0])
        {
            firstBeat = 0;
            timeOfFirstBeat = targetMarker.RenderTime;

            if (sortedMarkers.Length == 1) // no next marker
            {
                lastBeat = int.MaxValue;
            }
            else
            {
                double dt = 60d / targetMarker.BPM / (double)numberOfSubdivisions;

                lastBeat = CommonSenseFloor((sortedMarkers[1].RenderTime - targetMarker.RenderTime) / dt);
            }

            return true;
        }

        double accumlatedBeatIndex = 0d;
        double lastBeatOffsetIndex = 0d;
        bool lastBeatExists = true;
        for (int i = 1; i < sortedMarkers.Length; i++)
        {
            TimelineMarker currentMarker = sortedMarkers[i];
            TimelineMarker lastMarker = sortedMarkers[i - 1];

            double previous_dt = 60d / lastMarker.BPM / (double)numberOfSubdivisions;

            accumlatedBeatIndex += (currentMarker.RenderTime - lastMarker.RenderTime) / previous_dt;

            if (currentMarker == targetMarker) // exit condition
            {
                if (i == sortedMarkers.Length - 1) // we are at final marker
                {
                    lastBeatExists = false;
                }
                else
                {
                    TimelineMarker nextMarker = sortedMarkers[i + 1];

                    double current_dt = 60d / currentMarker.BPM / (double)numberOfSubdivisions;
                    lastBeatExists = true;
                    lastBeatOffsetIndex = (nextMarker.RenderTime - currentMarker.RenderTime) / current_dt;
                }

                break;
            }
        }

        double offset_dt = 60d / targetMarker.BPM / (double)numberOfSubdivisions;
        firstBeat = CommonSenseCeil(accumlatedBeatIndex);
        lastBeat = lastBeatExists ? int.MaxValue : CommonSenseFloor(accumlatedBeatIndex + lastBeatOffsetIndex);
        timeOfFirstBeat = targetMarker.RenderTime + (firstBeat - accumlatedBeatIndex) * offset_dt;
        return true;
    }

    public static bool GetBeatIndexAtTime(double time, List<TimelineMarker> markers, int numberOfSubdivisions, out int beatIndex)
    {
        bool doesTimelineMarkerExist = GetActiveTimelineMarkerAtTime(time, markers, out TimelineMarker activeMarker);

        if (!doesTimelineMarkerExist)
        {
            beatIndex = -1;
            return false;
        }

        bool canCalculateBeatIndexOfMarker = CalculateBeatIndexOfMarker(activeMarker, markers, numberOfSubdivisions, out int firstBeat, out _, out double timeOfFirstBeat);

        if (!canCalculateBeatIndexOfMarker)
        {
            beatIndex = -1;
            return false;
        }

        double dt = 60d / activeMarker.BPM / (double)numberOfSubdivisions;

        int beatOffset = CommonSenseFloor((time - timeOfFirstBeat) / dt);

        beatIndex = firstBeat + beatOffset;
        return true;
    }

    /// <summary>
    /// Gets the snapped position of a raw position given a N x N grid defined by gridSize.
    /// If the grid defined is invalid, this function will return false
    /// </summary>
    /// <param name="rawPosition"></param>
    /// <param name="gridSize"></param>
    /// <param name="snappedPosition"></param>
    /// <returns></returns>
    public static bool GetSnappedPositionOnGrid(Vector2 rawPosition, int gridSize, float gridXLength, float gridYLength, out Vector2 snappedPosition)
    {
        if (gridXLength < 0 || IsTwoFloatsEqualWithEpsilion(gridXLength, 0f) || gridYLength < 0 || IsTwoFloatsEqualWithEpsilion(gridYLength, 0f))
        {
            snappedPosition = rawPosition;
            return false;
        }
        if (gridSize < 1)
        {
            snappedPosition = rawPosition;
            return false;
        }

        Vector2 cellSize = new Vector2(gridXLength, gridYLength) / (float)gridSize;

        float snappedX = Mathf.RoundToInt(rawPosition.x / cellSize.x) * cellSize.x;
        float snappedY = Mathf.RoundToInt(rawPosition.y / cellSize.y) * cellSize.y;

        snappedPosition = new Vector2(snappedX, snappedY);
        return true;
    }

    public static GameplayChart ConvertEditorChartToGameplayChart(EditorChart editorChart, AudioClip clip)
    {
        Comparison<GameplayObject> comparsion = (x, y) =>
        {
            if (x.RenderTime < y.RenderTime)
            {
                return 1;
            }
            else if (x.RenderTime > y.RenderTime)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        };

        VisualHitbox[] hitboxes = new VisualHitbox[editorChart.Hitboxes.Count];
        ConvertCollectionToNewTypeAsArray(editorChart.Hitboxes, ref hitboxes);


        VisualLine[] lines = new VisualLine[editorChart.Lines.Count];
        ConvertCollectionToNewTypeAsArray(editorChart.Lines, ref lines);

        GameplayMarker[] markers = new GameplayMarker[editorChart.TimelineMarkers.Count];
        ConvertCollectionToNewTypeAsArray(editorChart.TimelineMarkers, ref markers);

        List<GameplayObject> allObjects = new List<GameplayObject>();

        IEnumerable<GameplayObject> combinedObjects = allObjects.Concat(hitboxes).Concat(lines).Concat(markers);

        return new GameplayChart(combinedObjects.OrderBy(x => x.RenderTime).ToArray(), clip);
    }

    /// <summary>
    /// Applies conversion operation on every element on the original array and returns the converted types as a list
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="original"></param>
    /// <returns></returns>
    public static List<T2> ConvertCollectionToNewTypeAsList<T1, T2>(List<T1> original) where T1 : IConvertable<T2>
    {
        List<T2> result = new List<T2>(original.Count);

        for (int i = 0; i < original.Count; i++)
        {
            bool convert = original[i].Convert(out T2 converted);

            if (!convert)
            {
                continue;
            }

            result[i] = converted;
        }

        return result;
    }
    /// <summary>
    /// Applies conversion operation on every element on the original array and returns the converted types as an array
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="original"></param>
    /// <returns></returns>
    public static T2[] ConvertCollectionToNewTypeAsArray<T1, T2>(List<T1> original) where T1 : IConvertable<T2>
    {
        T2[] result = new T2[original.Count];

        for (int i = 0; i < original.Count; i++)
        {
            bool convert = original[i].Convert(out T2 converted);

            if (!convert)
            {
                continue;
            }

            result[i] = converted;
        }

        return result;
    }

    /// <summary>
    /// Applies conversion operation on every element on the original array and returns the converted types as a list. <br></br>
    /// Modifies an pre-existing list to prevent garbage generation within this method
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="original"></param>
    /// <param name="newCollection"></param>
    public static void ConvertCollectionToNewTypeAsList<T1, T2>(List<T1> original, ref List<T2> newCollection) where T1 : IConvertable<T2>
    {
        for (int i = 0; i < math.min(original.Count, newCollection.Count); i++)
        {
            bool convert = original[i].Convert(out T2 converted);

            if (!convert)
            {
                continue;
            }

            newCollection[i] = converted;
        }
    }
    /// <summary>
    /// Applies conversion operation on every element on the original array and returns the converted types as an array. <br></br>
    /// Modifies an pre-existing array to prevent garbage generation within this method
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="original"></param>
    /// <param name="newCollection"></param>
    public static void ConvertCollectionToNewTypeAsArray<T1, T2>(List<T1> original, ref T2[] newCollection) where T1 : IConvertable<T2>
    {
        for (int i = 0; i < math.min(original.Count, newCollection.Length); i++)
        {
            bool convert = original[i].Convert(out T2 converted);

            if (!convert)
            {
                continue;
            }

            newCollection[i] = converted;
        }
    }

    /// <summary>
    /// Whether or not the hitbox type matches with the mouse active type. <br></br>
    /// This will return false if the hitbox type is a bomb.
    /// </summary>
    /// <param name="hitboxType"></param>
    /// <param name="mouseActiveType"></param>
    /// <returns></returns>
    public static bool IsMouseActiveTypeCorrect(HitboxType hitboxType, MouseActiveType mouseActiveType)
    {
        return (hitboxType == HitboxType.A && mouseActiveType == MouseActiveType.A) || (hitboxType == HitboxType.B && mouseActiveType == MouseActiveType.B);
    }
}
