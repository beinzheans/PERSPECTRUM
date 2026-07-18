using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

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

    /// <summary>
    /// Performs clamp operations on each component of a vector
    /// </summary>
    /// <param name="rawVector"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector2 ClampVectorByComponent(Vector2 rawVector, float min, float max)
    {
        float x = Mathf.Clamp(rawVector.x, min, max);
        float y = Mathf.Clamp(rawVector.y, min, max);

        return new Vector2(x, y);
    }

    /// <summary>
    /// Performs clamp operations on each component of a vector
    /// </summary>
    /// <param name="rawVector"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static Vector2 ClampVectorByComponent(Vector2 rawVector, float xMin, float xMax, float yMin, float yMaX)
    {
        float x = Mathf.Clamp(rawVector.x, xMin, xMax);
        float y = Mathf.Clamp(rawVector.y, yMin, yMaX);

        return new Vector2(x, y);
    }


    private static Vector3[] worldCornerBuffer = new Vector3[4];
    /// <summary>
    /// Get the screen point (pixel position) given a normalized point w.r.t a reference rect. Assumes normalized point uses (0, 0) for bottom-left corner.
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
    /// Get the fromto vector in raw pixel coordinates, given normalized fromPoint toPoint points w.r.t toPoint a reference rect. <br></br>
    /// This is necessary because the fromto vector calculated fromPoint normalized vector is NOT the same as the raw pixel fromto vector.
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
        if (IsTwoVectorsParallel(aUnitDir, bUnitDir))
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
        if (IsTwoVectorsParallel(aVector, bVector))
        {
            aScaler = 0f;
            bScaler = 0f;
            return false;
        }

        aScaler = (bVector.y * (bPosition.x - aPosition.x) - bVector.x * (bPosition.y - aPosition.y)) / crossProduct_result;
        bScaler = (aVector.y * (bPosition.x - aPosition.x) - aVector.x * (bPosition.y - aPosition.y)) / crossProduct_result;

        return aScaler >= 0f && aScaler <= 1f && bScaler >= 0f && bScaler <= 1f;
    }

    /// <summary>
    /// Whether or not two vectors are parallel. Note pointing exact opposite direction is considered as parallel
    /// </summary>
    /// <param name="aVector"></param>
    /// <param name="bVector"></param>
    /// <returns></returns>
    public static bool IsTwoVectorsParallel(Vector2 aVector, Vector2 bVector)
    {
        float crossProduct_result = aVector.x * bVector.y - aVector.y * bVector.x;
        return IsTwoFloatsEqualWithEpsilion(crossProduct_result, 0f); // cross product approx. 0 means we consider it toPoint be in same direction.
    }

    /// <summary>
    /// Whether or not three points are collinear.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    public static bool IsPointsCollinear(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 v1 = p2 - p1;
        Vector2 v2 = p3 - p2;
        Vector2 v3 = p3 - p1;

        return IsTwoVectorsParallel(v2, v1) && IsTwoVectorsParallel(v3, v1); // this is sufficient to prove collinear, v1 is the shared point.
    }

    public const double k_DOUBLECOMPAREEPSILION = 1e-9d;
    /// <summary>
    /// Computes the floor of a value, accounting for potential rounding errors that lead toPoint false results <br></br>
    /// Eg. if we evaluate 2.99999993 instead of 3 due toPoint representation error, it will return 3 instead of 2, hence the "common sense" floor.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CommonSenseFloor(double value)
    {
        return (int)math.floor(value + k_DOUBLECOMPAREEPSILION);
    }

    /// <summary>
    /// Computes the ceil of a value, accounting for potential rounding errors that lead toPoint false results <br></br>
    /// Eg. if we evaluate 3.0000001 instead of 3 due toPoint representation error, it will return 3 instead of 4, hence the "common sense" ceil.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CommonSenseCeil(double value)
    {
        return (int)math.ceil(value - k_DOUBLECOMPAREEPSILION);
    }

    /// <summary>
    /// Compares two double representation of values with a pre-defined epsilion <see cref="k_DOUBLECOMPAREEPSILION"/>. <br></br>
    /// Returns true if x and y are sufficiently close toPoint each other
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
    /// Returns true if x and y are sufficiently close toPoint each other
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

    private static Dictionary<int, GameplayResultRank> overallScoreThresholdsToRankMapping = new Dictionary<int, GameplayResultRank>()
    {
        { 1000000, GameplayResultRank.SS },
        { 980000 , GameplayResultRank.S },
        { 950000 , GameplayResultRank.AA},
        { 900000 , GameplayResultRank.A },
        { 800000 , GameplayResultRank.B },
        { 700000 , GameplayResultRank.C },
        { 0      , GameplayResultRank.D },
        { int.MinValue, GameplayResultRank.F }
    };

    private static Dictionary<GameplayResultRank, string> rankToStringMapping = new Dictionary<GameplayResultRank, string>()
    {
        { GameplayResultRank.SS, "SS"},
        { GameplayResultRank.S, "S" },
        { GameplayResultRank.AA, "AA" },
        { GameplayResultRank.A, "A" },
        { GameplayResultRank.B, "B" },
        { GameplayResultRank.C, "C" },
        { GameplayResultRank.D, "D" },
        { GameplayResultRank.F, "F" }
    };
    public static GameplayResultRank ConvertOverallScoreToRank(double score)
    {
        int checkScore = (int)math.round(score);
        foreach (int s in overallScoreThresholdsToRankMapping.Keys)
        {
            if (checkScore < s)
            {
                continue;
            }

            return overallScoreThresholdsToRankMapping[s];
        }

        throw new Exception("Invalid rank thresholds");
    }

    public static string ConvertRankToString(GameplayResultRank rank)
    {
        return rankToStringMapping.GetValueOrDefault(rank, "N.A.");
    }
    public static Spline GenerateSplineFromConstructionLineAndVertexPoint(EditorLine line, EditorPoint point)
    {
        float3[] knots = new float3[3];
        knots[0] = new float3(line.FromNormalizedPosition, 0f);
        knots[1] = new float3(point.NormalizedPosition, 0f);
        knots[2] = new float3(line.ToNormalizedPosition, 0f);

        Spline result = new(knots, TangentMode.AutoSmooth, false);
        return result;
    }

    public const string k_TIMESTAMPINTERNALFORMAT = "yyyyMMdd_HHmmss";
    /// <summary>
    /// Converts a timestamp in <see cref="k_TIMESTAMPINTERNALFORMAT"/> format to human readable time. Returns empty string if can not parse.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string ConvertTimestampToHumanReadableTime(string timestamp)
    {
        if (!DateTime.TryParseExact(timestamp, k_TIMESTAMPINTERNALFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
        {
            return "";
        }

        return result.ToString();
    }

    /// <summary>
    /// Perform lerp between two mouse buffers. We assume that the instanteous velocity between buffers is a constant velocity which is sufficient for small <see cref="GameplayStatisticRecorder.k_MOUSERECORDINTERVAL"/>.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="next"></param>
    /// <param name="time"></param>
    /// <param name="newPosition"></param>
    /// <param name="newMouseType"></param>
    public static void LerpMouseBuffers(ReplayMouseInfo current, ReplayMouseInfo next, double time, out Vector2 newPosition, out MouseActiveType newMouseType)
    {
        double dt = time - current.ReplayTime;
        double progress = dt / (next.ReplayTime - current.ReplayTime);

        Vector2 pos = Vector2.Lerp(current.NormalizedPosition, next.NormalizedPosition, (float)progress);

        newPosition = pos;

        if (current.MouseType == next.MouseType)
        {
            newMouseType = current.MouseType;
        }
        else
        {
            newMouseType = progress <= 0.5f ? current.MouseType : next.MouseType;
        }
    }

    /// <summary>
    /// Compares two version numbers to see which is the latest, assuming <paramref name="a"/> and <paramref name="b"/> has a valid version number (see <see cref="IsStringMatchVersioningFormat(string)"/>). <br></br>
    /// Returns 1 if <paramref name="a"/> is later than <paramref name="b"/>. <br></br>
    /// Returns 0 if <paramref name="a"/> is the same as <paramref name="b"/> or if the format is invalid. <br></br>
    /// Returns -1 if <paramref name="a"/> is earlier than <paramref name="b"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int CompareGameVersions(string a, string b)
    {
        bool aConvertResult = TryConvertStringToVersioning(a, out int aMajor, out int aMinor, out int aRevision);
        bool bConvertResult = TryConvertStringToVersioning(b, out int bMajor, out int bMinor, out int bRevision);

        if (!aConvertResult || !bConvertResult)
        {
            return 0;
        }

        // this is not the best code but it is the simplest way to compare versioning.

        if (aMajor > bMajor)
        {
            return 1;
        }
        else if (aMajor < bMajor)
        {
            return -1;
        }

        if (aMinor > bMinor)
        {
            return 1;
        }
        else if (aMinor < bMinor)
        {
            return -1;
        }

        if (aRevision > bRevision)
        {
            return 1;
        }
        else if (aRevision < bRevision)
        {
            return -1;
        }

        return 0;
    }

    private static string versioningRegex = @"[0-9]+.[0-9]+.[0-9]+";
    public static bool TryConvertStringToVersioning(string s, out int major, out int minor, out int revision)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            major = minor = revision = 0;
            return false;
        }

        if (!Regex.IsMatch(s, versioningRegex))
        {
            major = minor = revision = 0;
            return false;
        }

        // this will always be valid since we regex match the pattern beforehand. 
        string[] intString = s.Split('.');
        major = int.Parse(intString[0]);
        minor = int.Parse(intString[1]);
        revision = int.Parse(intString[2]);
        return true;
    }

    public static bool IsStringMatchVersioningFormat(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        return Regex.IsMatch(s, versioningRegex);
    }

    /// <summary>
    /// The maximum extent we allow the panning for audio.
    /// </summary>
    private const float k_AUDIOPANNINGMAX = 0.75f;
    /// <summary>
    /// Converts a normalized position to audio panning to create illusion of space. That is, a mapping f: [0, 1] -> [-<see cref="k_AUDIOPANNINGMAX"/>, <see cref="k_AUDIOPANNINGMAX"/>]. <br></br>
    /// Uses the sigmoid function to compute panning instead of linear.
    /// </summary>
    /// <param name="normalizedPosition"></param>
    /// <returns></returns>
    public static float GetAudioPanningFromPosition(Vector2 normalizedPosition)
    {
        return EvaluateSigmoidFunction(normalizedPosition.x, -k_AUDIOPANNINGMAX, k_AUDIOPANNINGMAX, 10f, 0.5f);
    }

    /// <summary>
    /// Evaluates a sigmoid function f: [-inf, inf] -> (<paramref name="min"/>, <paramref name="max"/>). <br></br>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="k">The steepness coefficient. This also defines the direction of the function.</param>
    /// <param name="x0">The exact input such that the function returns the mid-point of <paramref name="min"/> and <paramref name="max"/>.</param>
    /// <returns></returns>
    public static float EvaluateSigmoidFunction(float x, float min, float max, float k, float x0)
    {
        float num = max - min;
        float denom = 1f + math.exp(-k * (x - x0));

        return num / denom + min;
    }

    public static Vector2 GetMirroredPosition(in Vector2 pos, MoveSelectedMode mode)
    {
        return new Vector2(mode.HasFlag(MoveSelectedMode.Vertical) ? 1f - pos.x : pos.x, mode.HasFlag(MoveSelectedMode.Horizontal) ? 1f - pos.y : pos.y);
    }

    public static Vector2 GetRotatedPosition(in Vector2 pos, MoveSelectedMode mode)
    {
        if (!(mode.HasFlag(MoveSelectedMode.Rotate_90_Clockwise) ^ mode.HasFlag(MoveSelectedMode.Rotate_90_Anticlockwise)))
        {
            return pos; // do nothing, we only care if exactly one rotation is done
        }

        // we precompute the matrix transformation, hence the "magic numbers", though they are just a result of precompute matrix.
        // note we use normal math convention, so anticlockwise is positive, so consider this if you want to derive it yourself

        if (mode.HasFlag(MoveSelectedMode.Rotate_90_Clockwise))
        {
            return new Vector2(GameManager.aspectRatioReciprocalFloat * pos.y + 7f / 32f, -GameManager.aspectRatioFloat * pos.x + 25f / 18f);
        }
        else
        {
            return new Vector2(-GameManager.aspectRatioReciprocalFloat * pos.y + 25f / 32f, GameManager.aspectRatioFloat * pos.x - 7f / 18f);
        }
    }

}