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

        List<(double time, double BPM)> allMarkers = editorManager.CurrentEditorChart.TimelineMarkers.Select(x => (x.RenderTime, x.BPM)).ToList();
        bool markerResult = MathHelper.GetActiveTimelineMarkerAtTime(time, allMarkers, out int index);

        if (!markerResult)
        {
            return time; // nothing to snap
        }

        (double time, double BPM) marker = allMarkers[index];
        bool beatResult = MathHelper.CalculateBeatIndexOfMarker(marker, allMarkers, editorManager.NumberOfBeatSubdivisions, out _, out _, out double timeOfFirstBeat);

        if (!beatResult)
        {
            return time; // can not find starting beat of marker
        }

        double dt = 60d / marker.BPM / (double)editorManager.NumberOfBeatSubdivisions;

        int beatOffset = MathHelper.CommonSenseFloor((time - timeOfFirstBeat) / dt);

        return timeOfFirstBeat + (double)beatOffset * dt;
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
        List<(double time, double BPM)> allMarkers = editorManager.CurrentEditorChart.TimelineMarkers.Select(x => (x.RenderTime, x.BPM)).ToList();

        bool findResult = MathHelper.GetActiveTimelineMarkerAtTime(time, allMarkers, out int index);

        if (!findResult)
        {
            currentActiveTimelineMarker = null;

            bpmText.text = "??? BPM";
            markerLabelText.text = "Undefined Section";
            ClearBeatMarkers();
            return;
        }

        TimelineMarker marker = editorManager.CurrentEditorChart.TimelineMarkers[index];

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
        List<(double time, double BPM)> allMarkers = editorManager.CurrentEditorChart.TimelineMarkers.Select(x => (x.RenderTime, x.BPM)).ToList();

        (double time, double BPM) marker = (currentActiveTimelineMarker.RenderTime, currentActiveTimelineMarker.BPM);

        bool searchResult = MathHelper.CalculateBeatIndexOfMarker(marker, allMarkers, editorManager.NumberOfBeatSubdivisions, out int firstVisibleBeatIndex, out int lastBeat, out double timeOfFirstBeat);
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
}

/// <summary>
/// A class to describe a marker on the timeline for BPM changes with a label with an option to display text. <br></br>
/// </summary>
/// 
[Serializable]
public class TimelineMarker : EditorDynamicObject, IConvertable<GameplayMarker>
{
    public TimelineMarker(double markerTime) : base(markerTime)
    {
    }

    [JsonConstructor]
    public TimelineMarker(double markerTime, string markerLabel, double BPM, string displayMessage, double displayTime, bool isPreviewStartMarker) : base(markerTime)
    {
        MarkerLabel = markerLabel;
        this.BPM = BPM;
        DisplayMessage = displayMessage;
        DisplayTime = displayTime;
        IsPreviewStartMarker = isPreviewStartMarker;
    }

    public void AssignMarkerValues(string label, double BPM, string displayMessage, double displayTime, bool isPreviewStartMarker)
    {
        MarkerLabel = label;
        this.BPM = BPM;
        DisplayMessage = displayMessage;
        DisplayTime = displayTime;
        IsPreviewStartMarker = isPreviewStartMarker;
    }

    public bool Convert(out GameplayMarker converted)
    {
        converted = new GameplayMarker(RenderTime, BPM, DisplayMessage, DisplayTime);
        return true;
    }

    public string MarkerLabel { get; private set; }
    public double BPM { get; private set; }
    public string DisplayMessage { get; private set; }
    public double DisplayTime { get; private set; }

    public bool IsPreviewStartMarker { get; private set; }
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