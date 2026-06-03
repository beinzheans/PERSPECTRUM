using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// A class to manage timeline internal logic and UI logic.
/// </summary>
public class EditorTimelineManager : MonoBehaviour
{

    [SerializeField] private EditorBeatMarkerRenderableUI prefab;
    [SerializeField] private Canvas renderCanvas;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text bpmText;
    [SerializeField] private TMP_Text markerLabelText;
    private EditorManager editorManager;
    /// <summary>
    /// A marker to define where beat zero starts in the chart. Assumes the first marker in time is the initial marker.
    /// </summary>
    private TimelineMarker initialMarker;
    private TimelineMarker currentActiveTimelineMarker;
    private double timelineTimeLength;
    public const int k_MAXNUMBEROFBEATS = 50;
    private BeatMarker[] currentBeatMarkers = new BeatMarker[k_MAXNUMBEROFBEATS];
    private EditorBeatMarkerRenderableUI[] currentRenderableBeatMarkers = new EditorBeatMarkerRenderableUI[k_MAXNUMBEROFBEATS];

    private double currentTimelineMinTime;
    private double currentTimelineMaxTime;

    [SerializeField] private bool snapToNearestBeat = true;

    private void Start()
    {
        editorManager = EditorManager.EditorInstance;

        initialMarker = FindInitialMarker();
        currentActiveTimelineMarker = null;
        InstantiateInitialMarkers();
        editorManager.OnPreviewUpdated += EditorManager_OnPreviewUpdated;
        editorManager.OnRequestSnap += EditorManager_OnRequestSnap;
    }

    private double EditorManager_OnRequestSnap(double time)
    {
        if (!snapToNearestBeat)
        {
            return time;
        }

        bool markerResult = GetActiveTimelineMarkerAtTime(time, out TimelineMarker marker);

        if (!markerResult)
        {
            return time; // nothing to snap
        }

        bool beatResult = CalculateBeatIndexOfMarker(marker, out _, out _, out double timeOfFirstBeat);

        if (!beatResult)
        {
            return time; // can not find starting beat of marker
        }

        double dt = 60d / marker.BPM / (double)editorManager.NumberOfBeatSubdivisions;

        int beatOffset = MathHelper.CommonSenseFloor((time - timeOfFirstBeat) / dt);

        return timeOfFirstBeat + (double)beatOffset * dt;
    }

    private bool FindNextMarker(TimelineMarker marker, out TimelineMarker nextMarker)
    {
        if (marker == null)
        {
            nextMarker = null;
            return false;
        }

        int foundIndex = -1;
        double filterTime = marker.RenderTime;
        double closestMarkerTime = double.MaxValue;
        List<TimelineMarker> markers = new List<TimelineMarker>(editorManager.CurrentEditorChart.TimelineMarkers);
        for (int i = 0; i < markers.Count; i++)
        {
            if (markers[i].RenderTime <= filterTime)
            {
                continue;
            }

            if (markers[i].RenderTime < closestMarkerTime)
            {
                closestMarkerTime = markers[i].RenderTime;
                foundIndex = i;
            }
        }

        if (foundIndex == -1)
        {
            nextMarker = null;
            return false;
        }

        nextMarker = markers[foundIndex];
        return true;

    }

    private void InstantiateInitialMarkers()
    {
        for (int i = 0; i < k_MAXNUMBEROFBEATS; i++)
        {
            BeatMarker marker = new BeatMarker(BeatMarkerType.Hidden, 0d);
            currentBeatMarkers[i] = marker;
            EditorBeatMarkerRenderableUI ui = Instantiate(prefab, renderCanvas.transform, false);
            ui.gameObject.SetActive(false);
            currentRenderableBeatMarkers[i] = ui;
            currentRenderableBeatMarkers[i].AssignAssociatedRenderable(currentBeatMarkers[i]);
        }
    }
    private TimelineMarker FindInitialMarker()
    {
        double minTime = double.MaxValue;
        int minIndex = -1;
        List<TimelineMarker> markers = editorManager.CurrentEditorChart.TimelineMarkers;
        for (int i = 0; i < markers.Count; i++)
        {
            if (markers[i].RenderTime < minTime)
            {
                minIndex = i;
                minTime = markers[i].RenderTime;
            }
        }

        if (minIndex == -1)
        {
            Debug.LogWarning($"Can not find initial marker");
            return null;
        }

        return markers[minIndex];
    }

    private void EditorManager_OnPreviewUpdated(double time)
    {
        timeText.text = $"{time:F3} secs";
        GetNewActiveTimelineMarker(time);
    }

    private void GetNewActiveTimelineMarker(double time)
    {
        initialMarker = FindInitialMarker(); // try to find initial marker, since it is possible we deleted it this update

        bool findResult = GetActiveTimelineMarkerAtTime(time, out TimelineMarker marker);
        if (!findResult)
        {
            currentActiveTimelineMarker = null;

            bpmText.text = "??? BPM";
            markerLabelText.text = "Undefined Section";

            ClearBeatMarkers();
            return;
        }

        if (marker != currentActiveTimelineMarker)
        {
            editorManager.InvokeEditorTimelineMarkerActive(marker);
            markerLabelText.text = marker.MarkerLabel;
        }
        currentActiveTimelineMarker = marker;

        bpmText.text = $"{currentActiveTimelineMarker.BPM:F3} BPM\n" +
               $"1 : {editorManager.NumberOfBeatSubdivisions}";

        UpdateTimelineBeats(time);
    }


    private void ClearBeatMarkers()
    {
        for (int i = 0; i < k_MAXNUMBEROFBEATS; i++)
        {
            currentBeatMarkers[i].MarkerType = BeatMarkerType.Hidden;
            RenderBeatMarker(i);
        }
    }
    private bool GetActiveTimelineMarkerAtTime(double time, out TimelineMarker marker)
    {
        double filterTime = time;
        double maxTimeWithinFilterTime = -1d;
        int indexResult = -1;
        List<TimelineMarker> markers = editorManager.CurrentEditorChart.TimelineMarkers;
        for (int i = 0; i < markers.Count; i++)
        {
            if (markers[i].RenderTime > filterTime)
            {
                continue;
            }

            if (markers[i].RenderTime >= maxTimeWithinFilterTime)
            {
                maxTimeWithinFilterTime = markers[i].RenderTime;
                indexResult = i;
            }
        }

        if (indexResult == -1)
        {
            marker = null;
            return false;
        }

        marker = markers[indexResult];
        return true;
    }

    private void UpdateTimelineBeats(double time)
    {
        double bpm = currentActiveTimelineMarker.BPM;

        if (bpm <= 0d)
        {
            return;
        }

        if (editorManager.NumberOfBeatSubdivisions <= 0)
        {
            return;
        }

        double dt = 60d / bpm / (double)editorManager.NumberOfBeatSubdivisions;
        timelineTimeLength = (double)k_MAXNUMBEROFBEATS * dt; // maximum possible length defined mathematically

        currentTimelineMinTime = time - 0.5d * timelineTimeLength;
        currentTimelineMaxTime = time + 0.5d * timelineTimeLength;

        bool searchResult = CalculateBeatIndexOfMarker(currentActiveTimelineMarker, out int firstVisibleBeatIndex, out int lastBeat, out double timeOfFirstBeat);
        if (!searchResult)
        {
            return;
        }

        int minBeatOffset = MathHelper.CommonSenseCeil((currentTimelineMinTime - timeOfFirstBeat) / dt); // offset of the beat at the minimum time relative to first visible beat 
        for (int i = 0; i < k_MAXNUMBEROFBEATS; i++)
        {
            int beatIndex = firstVisibleBeatIndex + minBeatOffset + i;
            if (beatIndex < firstVisibleBeatIndex || beatIndex > lastBeat)
            {
                currentBeatMarkers[i].MarkerType = BeatMarkerType.Hidden;
                RenderBeatMarker(i);
                continue;
            }

            double beatTime = timeOfFirstBeat + (double)(beatIndex - firstVisibleBeatIndex) * dt;

            currentBeatMarkers[i].RenderTime = beatTime;

            if ((beatIndex) % editorManager.NumberOfBeatSubdivisions == 0)
            {
                currentBeatMarkers[i].MarkerType = BeatMarkerType.Big;
            }
            else
            {
                currentBeatMarkers[i].MarkerType = BeatMarkerType.Small;
            }

            RenderBeatMarker(i);
        }
    }

    private readonly Vector2 smallMarkerSize = new Vector2(3f, 25f);
    private readonly Vector2 bigMarkerSize = new Vector2(3f, 50f);
    private void RenderBeatMarker(int index)
    {
        BeatMarker currentBeatMarker = currentBeatMarkers[index];
        if (currentBeatMarker.MarkerType == BeatMarkerType.Hidden)
        {
            currentRenderableBeatMarkers[index].gameObject.SetActive(false);
            return;
        }

        currentRenderableBeatMarkers[index].gameObject.SetActive(true);
        RectTransform r = currentRenderableBeatMarkers[index].RawImage.rectTransform;

        r.anchorMax = r.anchorMin = new Vector2((float)((currentBeatMarker.RenderTime - currentTimelineMinTime) / timelineTimeLength), 0.5f);

        if (currentBeatMarker.MarkerType == BeatMarkerType.Small)
        {
            r.sizeDelta = smallMarkerSize;
        }
        else
        {
            r.sizeDelta = bigMarkerSize;
        }
    }

    private bool CalculateBeatIndexOfMarker(TimelineMarker marker, out int firstBeat, out int lastBeat, out double timeOfFirstBeat)
    {
        if (editorManager.NumberOfBeatSubdivisions <= 0)
        {
            firstBeat = -1;
            lastBeat = -1;
            timeOfFirstBeat = -1d;
            return false;
        }

        if (marker == initialMarker) // base case
        {
            firstBeat = 0;
            timeOfFirstBeat = marker.RenderTime;

            bool findNextResult = FindNextMarker(marker, out TimelineMarker nextMarker);
            if (!findNextResult)
            {
                lastBeat = int.MaxValue;
                return true;
            }

            double dt = 60d / marker.BPM / (double)editorManager.NumberOfBeatSubdivisions;

            lastBeat = MathHelper.CommonSenseFloor((nextMarker.RenderTime - marker.RenderTime) / dt);
            return true;
        }
        List<TimelineMarker> copy = editorManager.CurrentEditorChart.TimelineMarkers.OrderBy(x => x.RenderTime).ToList();

        double accumulatedBeatIndex = 0d;
        double lastBeatOffset = 0d;
        bool shouldHaveLastBeat = true;

        for (int i = 1; i < copy.Count; i++)
        {
            TimelineMarker currentMarker = copy[i];
            TimelineMarker previousMarker = copy[i - 1];
            double previous_dt = 60d / previousMarker.BPM / (double)editorManager.NumberOfBeatSubdivisions;

            accumulatedBeatIndex += (currentMarker.RenderTime - previousMarker.RenderTime) / previous_dt;

            if (currentMarker == marker)
            {
                if (i == copy.Count - 1) // do not compute last index if we are at the final marker
                {
                    shouldHaveLastBeat = false;
                    break;
                }

                shouldHaveLastBeat = true;
                TimelineMarker nextMarker = copy[i + 1];

                double current_dt = 60d / currentMarker.BPM / (double)editorManager.NumberOfBeatSubdivisions;
                lastBeatOffset = (nextMarker.RenderTime - currentMarker.RenderTime) / current_dt;

                break;
            }
        }

        double offset_dt = 60d / marker.BPM / (double)editorManager.NumberOfBeatSubdivisions;
        firstBeat = MathHelper.CommonSenseCeil(accumulatedBeatIndex);
        lastBeat = shouldHaveLastBeat ? MathHelper.CommonSenseFloor(accumulatedBeatIndex + lastBeatOffset) : int.MaxValue;
        timeOfFirstBeat = marker.RenderTime + (firstBeat - accumulatedBeatIndex) * offset_dt;
        return true;
    }
}

/// <summary>
/// A class to describe a marker on the timeline for BPM changes with a label. <br></br>
/// </summary>
/// 
[Serializable]
public class TimelineMarker : EditorDynamicObject, IConvertable<GameplayMarker>
{
    public TimelineMarker(double markerTime) : base(markerTime)
    {
    }

    [JsonConstructor]
    public TimelineMarker(double markerTime, string markerLabel, double BPM) : base(markerTime)
    {
        MarkerLabel = markerLabel;
        this.BPM = BPM;
    }

    public void AssignMarkerValues(string label, double BPM)
    {
        MarkerLabel = label;
        this.BPM = BPM;
    }

    public bool Convert(out GameplayMarker converted)
    {
        converted = new GameplayMarker(RenderTime, BPM);
        return true;
    }

    public string MarkerLabel { get; private set; }
    public double BPM { get; private set; }
}

/// <summary>
/// A class to describe a marker on the timeline to visualize the beat.
/// </summary>
public class BeatMarker : EditorObject
{
    public BeatMarker(BeatMarkerType type, double markerTime) : base(markerTime)
    {
        this.MarkerType = type;
    }

    public BeatMarkerType MarkerType;
}

public enum BeatMarkerType
{
    Hidden = 0,
    Small = 1,
    Big = 2
}