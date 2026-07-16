using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class EditorManager : MonoBehaviour
{
    [SerializeField] private RectTransform editorRootContainer;
    public RectTransform EditorRootUIContainer { get => editorRootContainer; }

    [SerializeField] private RectTransform previewUIContainer;
    public RectTransform PreviewUIContainer { get => previewUIContainer; }
    public static EditorManager EditorInstance;
    private GameManager gameInstance;
    private PlayerInputActions inputAction;

    public Vector2 EditorMousePosition { get; private set; }

    public event Action<double> OnPreviewUpdated;
    public event Action<float> OnPlaceDeleteSizeUpdated;

    public event Action OnEditorInstantiate;

    public event Action<EditorObject> OnRenderRenderable;
    public event Action<EditorObject> OnUnrenderRenderable;

    public List<EditorObject> EditorRenderables { get; private set; }

    public event Action<EditorDynamicObject> OnEditorSelectedSelectable;
    public event Action<EditorDynamicObject> OnEditorDeselectedSelectable;
    public event Action<EditorDynamicObject> OnEditorPlaceEditorObject;
    public event Action<EditorDynamicObject> OnEditorDeleteEditorObject;
    public event Action<EditorDynamicObject> OnEditorEditEditable;

    public event Action<TimelineMarker> OnTimelineMarkerActive;
    public EditorChart CurrentEditorChart { get; private set; }
    public double EditorPreviewTime { get; private set; }
    public float EditorPlaceDeleteSize { get; private set; }
    public double CurrentBPM { get; private set; }
    private double autoScrollSensitivity_time;
    [SerializeField] private float userScrollSensitivity_size;


    [SerializeField] private int numberOfBeatSubdivisions;
    public float ScrollSensitivity_Size { get => userScrollSensitivity_size; }
    public double LookAheadTime { get => GameManager.GameInstance.GlobalSettings.EditorSettings.EditorLookaheadTime; }
    public int NumberOfBeatSubdivisions { get => numberOfBeatSubdivisions; }

    private Vector2 snappedMouseCoordinate;
    private bool mouseSnapX = false;
    private bool mouseSnapY = false;

    private List<EditorDynamicObject> currentSelectedRenderables = new();
    public List<EditorDynamicObject> CurrentSelectedRenderables { get => currentSelectedRenderables; }

    public event Action<ObjectPlaceDeleteType> OnEditorPlaceDeleteTypeChanged;
    public event Action<int> OnEditorToolkitButtonPressed;

    public TimelineMarker CurrentTimelineMarker { get; private set; }
    // yes this might be bad
    // but we don't need to generalize it, keep it simple for now
    public TMP_Text[] TextLabels = new TMP_Text[9];
    public Button[] Buttons = new Button[9];
    public TMP_InputField[] InputFields = new TMP_InputField[9];

    public const string k_DEFAULTTOOLTEXTSTRING = "Empty Tool";
    /// <summary>
    /// Defines the size of the grid. <br></br>
    /// </summary>
    public const int k_SCREENGRIDSIZE = 20;
    public const float k_SCREENGRIDSIZE_CELL = 1f / (float)k_SCREENGRIDSIZE;
    public Vector2[] RegionGridSizePositions { get; private set; }

    public const int k_MAXIMUMCOMMANDPOOL = 99;
    private LimitedStack<EditorCommand> executeCommandStack = new LimitedStack<EditorCommand>(k_MAXIMUMCOMMANDPOOL);
    private LimitedStack<EditorCommand> undoCommandStack = new LimitedStack<EditorCommand>(k_MAXIMUMCOMMANDPOOL);

    public event Func<double, double> OnRequestSnap;

    public bool IsEditorInPlaybackState { get; private set; }

    public event Action<double> OnPlaybackStart;
    public event Action OnPlaybackStopped;

    public event Action<AudioClip> OnMusicAudioClipLoaded;

    public bool IsEditorSnapMouseToGrid { get; private set; }

    private byte[] currentEditorAudioClipByteArray = new byte[0];

    public event Func<BaseChartMetadata> OnRequestBaseChartMetadata;
    public event Action<EditorChartMetadata> OnChartMetadataLoaded;

    [SerializeField] private RawImage gridCursorVisualizationImage;

    private void Awake()
    {
        EditorInstance = this;
    }

    private void OnDestroy()
    {
        inputAction.Editor.ScrollEditorTime.performed -= ScrollEditorTime_performed;
        inputAction.Editor.ScrollEditorTime_BigScroll.performed -= ScrollEditorTime_BigScroll_performed;
        inputAction.Editor.ScrollEditorBeatSubdivision.performed -= ScrollEditorBeatSubdivision_performed;
        inputAction.Editor.MouseSnapAlongX.performed -= MouseSnapAlongX_performed;
        inputAction.Editor.MouseSnapAlongY.performed -= MouseSnapAlongY_performed;
        inputAction.Editor.DeselectAllEditorObjects.performed -= DeselectAllEditorObjects_performed;
        inputAction.Editor.ScrollEditorNoteSize.performed -= ScrollEditorNoteSize_performed;
        inputAction.Editor.SelectAllVisibleEditorObjects.performed -= SelectAllVisibleEditorObjects_performed;
        inputAction.Editor.UndoEditorCommand.performed -= UndoEditorCommand_performed;
        inputAction.Editor.RedoEditorCommand.performed -= RedoEditorCommand_performed;

        EditorInstance = null;
    }
    private void Start()
    {
        gameInstance = GameManager.GameInstance;
        inputAction = gameInstance.InputActions;

        for (int i = 0; i < Buttons.Length; i++)
        {
            int index = i;
            Buttons[index].onClick.AddListener(() => InvokeEditorToolkitButtonPressed(index));
        }

        RegionGridSizePositions = new Vector2[k_SCREENGRIDSIZE * k_SCREENGRIDSIZE];
        for (int y = 0; y < k_SCREENGRIDSIZE; y++)
        {
            float yPosition = 1f / (float)k_SCREENGRIDSIZE * (float)y;
            for (int x = 0; x < k_SCREENGRIDSIZE; x++)
            {
                float xPosition = 1f / k_SCREENGRIDSIZE * (float)x;

                RegionGridSizePositions[k_SCREENGRIDSIZE * y + x].x = xPosition + 0.5f * k_SCREENGRIDSIZE_CELL;
                RegionGridSizePositions[k_SCREENGRIDSIZE * y + x].y = yPosition + 0.5f * k_SCREENGRIDSIZE_CELL;
            }
        }

        EditorPlaceDeleteSize = 0.1f;
        numberOfBeatSubdivisions = 4;
        CurrentTimelineMarker = null;
        inputAction.Editor.ScrollEditorTime.performed += ScrollEditorTime_performed;
        inputAction.Editor.ScrollEditorTime_BigScroll.performed += ScrollEditorTime_BigScroll_performed;
        inputAction.Editor.ScrollEditorBeatSubdivision.performed += ScrollEditorBeatSubdivision_performed;
        inputAction.Editor.MouseSnapAlongX.performed += MouseSnapAlongX_performed;
        inputAction.Editor.MouseSnapAlongY.performed += MouseSnapAlongY_performed;
        inputAction.Editor.DeselectAllEditorObjects.performed += DeselectAllEditorObjects_performed;
        inputAction.Editor.ScrollEditorNoteSize.performed += ScrollEditorNoteSize_performed;
        inputAction.Editor.SelectAllVisibleEditorObjects.performed += SelectAllVisibleEditorObjects_performed;
        inputAction.Editor.UndoEditorCommand.performed += UndoEditorCommand_performed;
        inputAction.Editor.RedoEditorCommand.performed += RedoEditorCommand_performed;

        StartEditor();
    }

    private void RedoEditorCommand_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        RedoEditorCommand();
    }

    private void UndoEditorCommand_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        UndoEditorCommand();
    }

    private void SelectAllVisibleEditorObjects_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        for (int i = 0; i < EditorRenderables.Count; i++)
        {
            if (EditorRenderables[i] is not EditorDynamicObject editorObj)
            {
                continue;
            }

            if (editorObj.IsSelected)
            {
                continue;
            }
        }

        Action selectAction = () =>
        {
            for (int i = 0; i < EditorRenderables.Count; i++)
            {
                if (EditorRenderables[i] is not EditorDynamicObject editorObj)
                {
                    continue;
                }

                editorObj.OnSelect();
            }
        };
        Action deselectAction = () =>
        {
            for (int i = 0; i < EditorRenderables.Count; i++)
            {
                if (EditorRenderables[i] is not EditorDynamicObject editorObj)
                {
                    continue;
                }
                editorObj.OnDeselect();
            }
        };

        GameManager.GameInstance.InvokeInformationDisplayNeeded("Select All");
        EditorCommand selectCommand = new EditorCommand(selectAction, deselectAction);
        ExecuteEditorCommand(selectCommand);
    }

    private void ScrollEditorTime_BigScroll_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        if (IsEditorInPlaybackState)
        {
            return;
        }

        double delta = GameManager.GameInstance.GlobalSettings.EditorSettings.BigScrollTimeInterval;
        if (obj.ReadValue<Vector2>().y > 0f)
        {
            delta *= 1d;
        }
        else
        {
            delta *= -1d;
        }

        UpdateEditorPreviewTimeByDelta(delta, false);
    }

    private void ScrollEditorBeatSubdivision_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        if (obj.ReadValue<Vector2>().y > 0f)
        {
            numberOfBeatSubdivisions++;
        }
        else
        {
            if (numberOfBeatSubdivisions <= 1)
            {
                return;
            }

            numberOfBeatSubdivisions--;
        }

        InvokeEditorPreviewUpdateEvent();
    }

    private void ScrollEditorNoteSize_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        float delta = userScrollSensitivity_size;
        if (obj.ReadValue<Vector2>().y > 0f)
        {
            delta *= 1f;
        }
        else
        {
            delta *= -1f;
        }

        EditorPlaceDeleteSize += delta;
        OnPlaceDeleteSizeUpdated?.Invoke(EditorPlaceDeleteSize);
    }

    private void DeselectAllEditorObjects_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        List<EditorDynamicObject> copy = new List<EditorDynamicObject>(currentSelectedRenderables);

        Action selectAction = () =>
        {
            for (int i = 0; i < copy.Count; i++)
            {
                copy[i].OnSelect();
            }
        };
        Action deselectAction = () =>
        {
            for (int i = 0; i < copy.Count; i++)
            {
                copy[i].OnDeselect();
            }
        };


        EditorCommand deselectAllCommand = new EditorCommand(deselectAction, selectAction);
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Deselect All");
        ExecuteEditorCommand(deselectAllCommand);
    }

    private void MouseSnapAlongY_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        snappedMouseCoordinate = EditorMousePosition;
        mouseSnapY = !mouseSnapY;
        mouseSnapX = false;
    }

    private void MouseSnapAlongX_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        snappedMouseCoordinate = EditorMousePosition;
        mouseSnapX = !mouseSnapX;
        mouseSnapY = false;
    }

    private void StartEditor()
    {
        Debug.Log($"Starting editor");

        if (math.abs(GameManager.GameInstance.GlobalSettings.AudioOffsetMs) >= GameManager.k_HIGHLATENCYTHRESHOLDMS)
        {
            ConfirmAction confirmAction = new ConfirmAction(() => { }, () => SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_TITLESCREENINDEX, () => Task.CompletedTask), "It is not recommend to chart with a high audio latency.\n" +
                                                                                                                                                      "Do you still want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(confirmAction);
        }

        CurrentEditorChart = new EditorChart(new(), new(), new(), new());
        EditorRenderables = new();
        OnEditorInstantiate?.Invoke();

        UpdateEditorPreviewTime(0d, true);
    }

    private void Update()
    {
        MathHelper.GetNormalizedPointInsideReferenceUI(gameInstance.MousePosition, previewUIContainer, out Vector2 normalizedMousePosition);

        if (mouseSnapX)
        {
            EditorMousePosition = new Vector2(normalizedMousePosition.x, snappedMouseCoordinate.y);
        }
        else if (mouseSnapY)
        {
            EditorMousePosition = new Vector2(snappedMouseCoordinate.x, normalizedMousePosition.y);
        }
        else
        {
            EditorMousePosition = normalizedMousePosition;
        }

        if (IsEditorSnapMouseToGrid)
        {
            MathHelper.GetSnappedPositionOnGrid(EditorMousePosition, k_SCREENGRIDSIZE, 1f, 1f, out Vector2 snappedMousePosition);
            EditorMousePosition = snappedMousePosition;
        }

        gridCursorVisualizationImage.rectTransform.anchorMin = gridCursorVisualizationImage.rectTransform.anchorMax = MathHelper.ClampVectorByComponent(EditorMousePosition, 0f, 1f);
        gridCursorVisualizationImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void ScrollEditorTime_performed(InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        if (IsEditorInPlaybackState)
        {
            return;
        }

        if (CurrentBPM <= 0d || numberOfBeatSubdivisions <= 0)
        {
            return;
        }

        autoScrollSensitivity_time = 60d / CurrentBPM / (double)numberOfBeatSubdivisions;
        double delta;
        if (obj.ReadValue<Vector2>().y > 0f)
        {
            delta = autoScrollSensitivity_time;
        }
        else
        {
            delta = -autoScrollSensitivity_time;
        }

        UpdateEditorPreviewTimeByDelta(delta, true);
    }
    public void UpdateEditorPreviewTime(double newTime, bool shouldSnap)
    {
        if (shouldSnap)
        {
            double? snappedTime = OnRequestSnap?.Invoke(newTime);

            if (snappedTime == null)
            {
                EditorPreviewTime = Math.Max(0d, newTime);
            }
            else
            {
                EditorPreviewTime = Math.Max(0d, (double)snappedTime);
            }
        }
        else
        {
            EditorPreviewTime = Math.Max(0d, newTime);
        }

        InvokeEditorPreviewUpdateEvent();
    }

    public void UpdateEditorPreviewTimeByDelta(double deltaTime, bool shouldSnap)
    {
        if (shouldSnap)
        {
            double? snappedTime = OnRequestSnap?.Invoke(EditorPreviewTime + deltaTime);

            if (snappedTime == null)
            {
                EditorPreviewTime = Math.Max(0d, EditorPreviewTime + deltaTime);
            }
            else
            {
                EditorPreviewTime = Math.Max(0d, (double)snappedTime);
            }
        }
        else
        {
            EditorPreviewTime = Math.Max(0d, EditorPreviewTime + deltaTime);
        }

        InvokeEditorPreviewUpdateEvent();
    }

    public void ExecuteEditorCommand(EditorCommand command)
    {
        executeCommandStack.Push(command);
        command.Execute();
        undoCommandStack.Clear(); // remove undo history
    }

    public void UndoEditorCommand()
    {
        bool popResult = executeCommandStack.TryPop(out EditorCommand command);
        if (!popResult)
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Undo Failed");
            return;
        }

        GameManager.GameInstance.InvokeInformationDisplayNeeded("Undo");
        undoCommandStack.Push(command);
        command.Undo();
    }

    public void RedoEditorCommand()
    {
        bool popResult = undoCommandStack.TryPop(out EditorCommand command);
        if (!popResult)
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Redo Failed");
            return;
        }

        GameManager.GameInstance.InvokeInformationDisplayNeeded("Redo");
        executeCommandStack.Push(command);
        command.Execute();
    }

    // Performance: maybe force it so that we update once? Since right now selecting all will update the editor multiple times
    // eg. selecting 100 objects at once will update the editor 100 times, when it really isn't necessary. For now it is quite fast still but
    // if performance gets really horseshit then this is another potential optimization angle
    public void InvokeEditorPreviewUpdateEvent()
    {
        OnPreviewUpdated?.Invoke(EditorPreviewTime);
    }
    public void InvokeRenderRenderableEvent(EditorObject r)
    {
        EditorRenderables.Add(r);
        OnRenderRenderable?.Invoke(r);
    }
    public void InvokeUnrenderRenderableEvent(EditorObject r)
    {
        EditorRenderables.Remove(r);
        OnUnrenderRenderable?.Invoke(r);
    }
    public void InvokeEditEditableEvent(EditorDynamicObject e)
    {
        OnEditorEditEditable?.Invoke(e);
        InvokeEditorPreviewUpdateEvent();
    }
    public void InvokeSelectSelectableEvent(EditorDynamicObject s)
    {
        currentSelectedRenderables.Add(s);
        OnEditorSelectedSelectable?.Invoke(s);
        InvokeEditorPreviewUpdateEvent();
    }
    public void InvokeDeselectSelectableEvent(EditorDynamicObject s)
    {
        currentSelectedRenderables.Remove(s);
        OnEditorDeselectedSelectable?.Invoke(s);
        InvokeEditorPreviewUpdateEvent();
    }

    public void InvokePlaceEditorObjectEvent(EditorDynamicObject pd)
    {
        OnEditorPlaceEditorObject?.Invoke(pd);
        InvokeEditorPreviewUpdateEvent();
    }

    public void InvokeDeleteEditorObjectEvent(EditorDynamicObject pd)
    {
        OnEditorDeleteEditorObject?.Invoke(pd);
        InvokeEditorPreviewUpdateEvent();
    }

    public void InvokeEditorToolkitButtonPressed(int buttonIndex)
    {
        OnEditorToolkitButtonPressed?.Invoke(buttonIndex);
    }

    public void InvokeEditorObjectTypeChanged(ObjectPlaceDeleteType type)
    {
        OnEditorPlaceDeleteTypeChanged?.Invoke(type);
    }

    public void InvokeEditorTimelineMarkerActive(TimelineMarker marker)
    {
        CurrentTimelineMarker = marker;
        CurrentBPM = marker.BPM;
        OnTimelineMarkerActive?.Invoke(marker);
    }

    public void InvokeEditorStartPlayback(double playbackSpeed)
    {
        IsEditorInPlaybackState = true;
        OnPlaybackStart?.Invoke(playbackSpeed);
    }

    public void InvokeEditorStopPlayback()
    {
        IsEditorInPlaybackState = false;
        OnPlaybackStopped?.Invoke();
    }

    public void InvokeAudioClipLoadedEvent(AudioClip clip, byte[] clipByteArray)
    {
        currentEditorAudioClipByteArray = clipByteArray;
        OnMusicAudioClipLoaded?.Invoke(clip);
    }

    public void InvokeEditorSnapMouseToGrid(bool snapState)
    {
        IsEditorSnapMouseToGrid = snapState;
    }

    public void InvokeOnEditorMetadataLoaded(EditorChartMetadata metadata)
    {
        OnChartMetadataLoaded?.Invoke(metadata);
    }

    public void SaveEditorChart()
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save To File", "", "New_Chart", GameManager.k_FILEEXTENSION);

        if (string.IsNullOrEmpty(path))
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Invalid File");
            return;
        }

        string chartJson = "";
        BaseChartMetadata baseMetadata = (BaseChartMetadata)(OnRequestBaseChartMetadata?.Invoke());

        try
        {
            chartJson = JsonConvert.SerializeObject(CurrentEditorChart, GameManager.GameInstance.JsonSerializerSettings);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert chart to JSON! Exception: \n" +
                             $"{e}");
        }

        string metadataJson = "";
        EditorChartMetadata metadata = CreateChartMetadataFromBaseMetadata(baseMetadata);

        try
        {
            metadataJson = JsonConvert.SerializeObject(metadata, GameManager.GameInstance.JsonSerializerSettings);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert metadata to JSON! Exception: \n" +
                             $"{e}");
        }

        GamePersistenceManager.SaveAsChartFile(path, chartJson, metadataJson, currentEditorAudioClipByteArray);
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Saved", 1d);
    }

    private EditorChartMetadata CreateChartMetadataFromBaseMetadata(in BaseChartMetadata baseMetadata)
    {
        double previewStartTime = GetSongPreviewStartTime();

        return new EditorChartMetadata(baseMetadata, previewStartTime);
    }

    /// <summary>
    /// Gets the song's preview start time. <br></br>
    /// If multiple markers are marked as the start time, we choose the earliest one. If none are marked, we default to 0d as the start time.
    /// </summary>
    /// <returns></returns>
    private double GetSongPreviewStartTime()
    {
        double minStartTime = double.MaxValue;
        bool hasAssigned = false;

        for (int i = 0; i < CurrentEditorChart.TimelineMarkers.Count; i++)
        {
            TimelineMarker marker = CurrentEditorChart.TimelineMarkers[i];

            if (!marker.IsPreviewStartMarker)
            {
                continue;
            }

            if (marker.RenderTime < minStartTime)
            {
                hasAssigned = true;
                minStartTime = marker.RenderTime;
            }
        }

        return !hasAssigned ? 0d : minStartTime;
    }

    public void LoadEditorChart()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Load From File", "", GameManager.k_FILEEXTENSION, false);

        if (paths.Length <= 0)
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Invalid File");
            return;
        }

        GamePersistenceManager.LoadChartFile(paths[0], out string chartJson, out string metadataJson, out byte[] audioBytes);

        JObject chartJObject = JObject.Parse(chartJson);
        JObject metadataJObject = JObject.Parse(metadataJson);

        bool validResult = GameVersionConverter.CompareChartMetadataWithCurrentVersion(in metadataJObject, out int compareResult);

        if (!validResult)
        {
            ConfirmAction invalidConfirmAction = new ConfirmAction(() => ConvertToEditorChart(chartJson, metadataJson, audioBytes), () => { }, "The selected chart has completely invalid metadata. Importing may cause errors.\n" +
                                                                                                                                               "Do you want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(invalidConfirmAction);
        }
        else if (compareResult == 1)
        {
            ConfirmAction outdatedGameConfirmAction = new ConfirmAction(() => ConvertToEditorChart(chartJson, metadataJson, audioBytes), () => { }, "The selected chart is made with a later version of the game. Importing may cause errors.\n" +
                                                                                                                                                    "Do you want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(outdatedGameConfirmAction);
        }
        else if (compareResult == -1)
        {
            bool result = GameVersionConverter.ConvertChartVersionToCurrentGameVersion(in chartJObject, in metadataJObject, out JObject convertedChartJObject, out JObject convertedmetadataJObject);
            chartJson = convertedChartJObject.ToString();
            metadataJson = convertedmetadataJObject.ToString();

            if (!result)
            {
                ConfirmAction convertConfirmAction = new ConfirmAction(() => ConvertToEditorChart(chartJson, metadataJson, audioBytes), () => { }, "The selected chart has a version mismatch and the game failed to resolve it.\n" +
                                                                                                                                                   "Do you want to continue?");

                GameManager.GameInstance.InvokeConfirmActionNeeded(convertConfirmAction);
            }
            else
            {
                Debug.Log($"Resolved version mismatch automatically");
                ConvertToEditorChart(chartJson, metadataJson, audioBytes);
            }
        }
        else
        {
            ConvertToEditorChart(chartJson, metadataJson, audioBytes);
        }
    }

    private async void ConvertToEditorChart(string chartJson, string metadataJson, byte[] audioBytes)
    {
        try
        {
            EditorChart loadedChart = JsonConvert.DeserializeObject<EditorChart>(chartJson, GameManager.GameInstance.JsonSerializerSettings);
            currentSelectedRenderables = new();
            executeCommandStack = new(k_MAXIMUMCOMMANDPOOL);
            undoCommandStack = new(k_MAXIMUMCOMMANDPOOL);
            UpdateEditorPreviewTime(0d, false);
            if (string.IsNullOrWhiteSpace(chartJson) || loadedChart == null)
            {
                CurrentEditorChart = new(new(), new(), new(), new());
            }
            else
            {
                CurrentEditorChart = loadedChart;
            }

            EditorChartMetadata metadata = JsonConvert.DeserializeObject<EditorChartMetadata>(metadataJson, GameManager.GameInstance.JsonSerializerSettings);

            if (string.IsNullOrWhiteSpace(metadataJson) || metadata == null)
            {
                InvokeOnEditorMetadataLoaded(null);
            }
            else
            {
                InvokeOnEditorMetadataLoaded(metadata);
            }

            (bool audioResult, AudioClip clip) = await GamePersistenceManager.GetAudioClipFromByteArray(audioBytes, false);

            if (!audioResult)
            {
                return;
            }

            await Awaitable.MainThreadAsync();

            // return to main thread in order to invoke this action.
            InvokeAudioClipLoadedEvent(clip, audioBytes);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse file! Exception: \n" +
                             $"{e}");
            return;
        }
    }
}



[Serializable]
public class EditorChart : IPlaceDeleteableContainer<EditorHitbox>, IPlaceDeleteableContainer<EditorPoint>, IPlaceDeleteableContainer<EditorLine>
{
    public EditorChart(List<EditorHitbox> hitboxes, List<EditorPoint> points, List<EditorLine> lines, List<TimelineMarker> timelineMarkers)
    {
        Hitboxes = hitboxes;
        Points = points;
        Lines = lines;
        TimelineMarkers = timelineMarkers;
    }

    public List<EditorHitbox> Hitboxes;
    public List<EditorPoint> Points;

    public List<EditorLine> Lines;
    public List<TimelineMarker> TimelineMarkers;
    public void OnPlace(EditorHitbox objectToPlace)
    {
        Hitboxes.Add(objectToPlace);
    }

    public void OnDelete(EditorHitbox objectToDelete)
    {
        Hitboxes.Remove(objectToDelete);
    }

    public void OnPlace(EditorPoint objectToPlace)
    {
        Points.Add(objectToPlace);
    }

    public void OnDelete(EditorPoint objectToDelete)
    {
        Points.Remove(objectToDelete);
    }

    public void OnPlace(EditorLine objectToPlace)
    {
        Lines.Add(objectToPlace);
    }

    public void OnDelete(EditorLine objectToDelete)
    {
        Lines.Remove(objectToDelete);
    }

    public void OnPlace(TimelineMarker objectToPlace)
    {
        TimelineMarkers.Add(objectToPlace);
    }

    public void OnDelete(TimelineMarker objectToDelete)
    {
        TimelineMarkers.Remove(objectToDelete);
    }
}

/// <summary>
/// A class to represent all the metadata for the associated chart. <br></br>
/// It is very unlikely for there to be metadata collision, since we check for both GUID and chart equality.
/// </summary>
[Serializable]
public class EditorChartMetadata : IEquatable<EditorChartMetadata>
{
    [JsonProperty(GameManager.k_METADATABASEDATAKEY)]
    public BaseChartMetadata BaseMetadata { get; private set; }

    [DefaultValue(0d)]
    [JsonProperty(GameManager.k_CHARTSTARTPREVIEWTIMEKEY)]
    public double PreviewStartTime { get; private set; }
    public EditorChartMetadata(BaseChartMetadata baseChartMetadata, double previewStartTime)
    {
        BaseMetadata = baseChartMetadata;
        PreviewStartTime = previewStartTime;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as EditorChartMetadata);
    }

    public bool Equals(EditorChartMetadata other)
    {
        return other is not null &&
               BaseMetadata.Equals(other.BaseMetadata);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaseMetadata);
    }
}

/// <summary>
/// A struct to contain the base metadata of a chart, which is the minimal information to define or identify a chart. <br></br>
/// Additional data should be defined outside of this struct. We assume that this base struct is NOT modified and will always exist for backwards compatability. <br></br>
/// </summary>
[Serializable]
public struct BaseChartMetadata : IEquatable<BaseChartMetadata>
{
    public BaseChartMetadata(string chartName, string chartMapper, string songName, string songArtist, int chartDifficulty, string version, string gUID)
    {
        ChartName = chartName;
        ChartMapper = chartMapper;
        SongName = songName;
        SongArtist = songArtist;
        ChartDifficulty = chartDifficulty;
        Version = version;
        GUID = gUID;
    }

    [JsonProperty(GameManager.k_CHARTNAMEKEY)]
    public string ChartName { get; private set; }

    [JsonProperty(GameManager.k_CHARTMAPPERKEY)]
    public string ChartMapper { get; private set; }

    [JsonProperty(GameManager.k_SONGNAMEKEY)]
    public string SongName { get; private set; }

    [JsonProperty(GameManager.k_SONGARTISTKEY)]
    public string SongArtist { get; private set; }

    [JsonProperty(GameManager.k_CHARTDIFFICULTYKEY)]
    public int ChartDifficulty { get; private set; }

    [JsonProperty(GameManager.k_CHARTVERSIONKEY)]
    public string Version { get; private set; }

    [JsonProperty(GameManager.k_CHARTGUIDKEY)]
    public string GUID { get; private set; }

    public override bool Equals(object obj)
    {
        return obj is BaseChartMetadata metadata && Equals(metadata);
    }

    public bool Equals(BaseChartMetadata other) // don't use version for equals. Because when we convert, we directly change the versions field too.
    {
        return ChartName == other.ChartName &&
               ChartMapper == other.ChartMapper &&
               SongName == other.SongName &&
               SongArtist == other.SongArtist &&
               ChartDifficulty == other.ChartDifficulty &&
               GUID == other.GUID;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ChartName, ChartMapper, SongName, SongArtist, ChartDifficulty); // don't use GUID nor version for hash code
    }
}

/// <summary>
/// A class to describe the editor undo/redo order. Each command will have their own execute and undo command.
/// </summary>
public class EditorCommand : ICommand
{
    private Action executeCommand;
    private Action undoCommand;

    public EditorCommand(Action executeCommand, Action undoCommand)
    {
        this.executeCommand = executeCommand;
        this.undoCommand = undoCommand;
    }

    /// <summary>
    /// Invokes the execute command. This function should be called only in <see cref="EditorManager.ExecuteEditorCommand(EditorCommand)"/>, and directly calling this method will lead to unexpected results.
    /// </summary>
    public void Execute()
    {
        executeCommand?.Invoke();
    }

    /// <summary>
    /// Invokes the undo command. This function should be called only in <see cref="EditorManager.UndoEditorCommand"/>
    /// </summary>
    public void Undo()
    {
        undoCommand?.Invoke();
    }
}
