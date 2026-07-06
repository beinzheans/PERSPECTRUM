using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class GameplayManager : MonoBehaviour
{
    [SerializeField] private RectTransform gameplayRectTransform;
    public RectTransform GameplayRectTransform { get => gameplayRectTransform; }

    [SerializeField] private Camera gameplayCamera;
    public Camera GameplayCamera { get => gameplayCamera; }
    public static GameplayManager GameplayInstance;

    public event Action<AudioClip, EditorChartMetadata> OnGameplayChartLoaded;
    public event Action OnGameplayStarted;
    public event Action OnGameplayEnded;
    public event Action OnGameplayRestarted;
    public event Action<GameplayStatisticRecord> OnGameplayReplayLoaded;

    public MouseActiveType MouseActiveType { get; private set; }

    /// <summary>
    /// How much leniency we allow before a note is considered a miss <br></br>
    /// Note that it is impossible for notes to be early.
    /// </summary>
    public const double k_LENIENCYTIMEFRAME = 0.1d;

    /// <summary>
    /// How much leniency we allow before a note is considered active. <br></br>
    /// This is to make the game FEEL more fair.
    /// </summary>
    public const double k_EARLYTIMEFRAME = 0.05d;

    /// <summary>
    /// How much extra delta size the hitbox interact size is compared to the actual hitbox size. <br></br>
    /// This is to make the game FEEL more fair.
    /// </summary>
    public const float k_HITBOXINTERACTSIZEADDDELTA = 0.025f;
    /// <summary>
    /// How long the pool looks ahead to spawn objects. This is added on top of the player's lookahead DSP time.
    /// </summary>
    public const double k_POOLLOOKAHEADTIME = 1d;

    /// <summary>
    /// How long the pool looks behind to despawn objects.
    /// </summary>
    public const double k_POOLUNRENDERTIMETHRESHOLD = 0.01d;

    /// <summary>
    /// The depth away from camera depicting the current DSP time. This affects the extent of perspective distortion.
    /// </summary>
    public const float k_HITPLANEDEPTH = 1f;

    public float GameplayFarClipPlane { get; private set; }
    public GameplayChart CurrentGameplayChart { get; private set; }
    public EditorChartMetadata CurrentMetadata { get; private set; }
    public string CurrentPath { get; private set; }
    public event Action<double> OnGameplayTimeUpdated;
    public event Action<GameplayObject> OnGameplayObjectRendered;
    public event Action<GameplayObject> OnGameplayObjectUnrendered;

    public event Action<VisualHitbox> OnHitboxBecomeActive;
    public event Action<VisualHitbox> OnHitboxMatchedHit;
    public event Action<VisualHitbox> OnHitboxMismatchedHit;
    public event Action<VisualHitbox> OnHitboxMiss;
    public event Action<VisualHitbox> OnHitboxBombHit;

    public Vector2 GameplayMousePosition { get; private set; }
    public double CurrentGameplayTime { get; private set; }

    /// <summary>
    /// The world position of where the bottom-left corner of the preview space is.
    /// </summary>
    public Vector3 WorldPositionOfPreviewMin { get; private set; }

    /// <summary>
    /// The world position of where the top-right corner of the preview space is.
    /// </summary>
    public Vector3 WorldPositionOfPreviewMax { get; private set; }

    public Vector3 WorldSizeOfPreview { get; private set; }
    public Vector2 ScreenSizeOfPreview { get; private set; }

    /// <summary>
    /// The ratio between the world coordinates and screen coordinates within the preview space
    /// </summary>
    public Vector2 WorldToScreenSizeRatioOfPreview { get; private set; }

    /// <summary>
    /// The vanishing point of the gameplay camera local to the camera, ie. the local coordinates of the center of the screen at the far clip plane w.r.t. the camera
    /// </summary>
    public Vector3 GameplayCameraVanishingLocalPoint { get; private set; }
    public int MaxHitboxCount { get; private set; }
    public int CurrentCombo { get; private set; }
    public int MissCount { get; private set; }
    /// <summary>
    /// How many hits were the correct mouse type
    /// </summary>
    public int MatchHitCount { get; private set; }

    /// <summary>
    /// How many hits were the wrong mouse type.
    /// </summary>
    public int MismatchHitCount { get; private set; }

    /// <summary>
    /// How much score weighing we give to mismatch hits relative to matched hits.
    /// </summary>
    public const double k_MISMATCHSCOREWEIGHT = 0.5d;
    public int BombHitCount { get; private set; }

    public event Action<MouseActiveType> OnMouseActiveTypeChanged;

    /// <summary>
    /// Accuracy will be a measurement of how many hitboxes we miss, ignores the hitbox types weights, ie. a pure measurement of hits vs misses
    /// </summary>
    public double CurrentAccuracy { get; private set; } = 1d;

    public const double k_MAXIMUMSCORE = 1000000d;
    private double scorePerNote = 0d;

    /// <summary>
    /// Score will be a measurement of the overall performance we have, considering the hitbox types
    /// </summary>
    public double CurrentScore { get; private set; } = 0d;

    public bool IsInReplayMode { get; private set; } = false;
    public GameplayStatisticRecord? CurrentGameplayRecord { get; private set; }

    public bool IsMetronomeDisabled;
    public double EndTime { get; private set; }

    public Mesh PlayAreaBorderMesh { get; private set; }
    private void Awake()
    {
        GameplayInstance = this;
    }

    public GameplayMarker CurrentActiveGameplayMarker { get; private set; }
    public event Action<GameplayMarker> OnGameplayMarkerUpdated;
    public event Action<double> OnGameplayMetronomeFired;
    private const float k_NEARCLIPPLANESCALE = 0.9f;
    private const float k_FARCLIPPLANESCALE = 1.25f;
    public Vector3 CurrentPlayAreaBorderScale { get; private set; }
    public Vector3 CurrentPlayAreaDisplacement { get; private set; }
    public Quaternion CurrentPlayAreaRotation { get; private set; }
    public Vector3[] LocalBorderCorners { get; private set; } = new Vector3[4]; // 0 is bottom-left corner, increment clockwise

    private void Start()
    {
        CreateGameplayReferencePoints();
        
        CurrentActiveGameplayMarker = null;

        gameplayCamera.nearClipPlane = k_HITPLANEDEPTH * k_NEARCLIPPLANESCALE;
        GameplayFarClipPlane = k_HITPLANEDEPTH + (float)(GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);
        gameplayCamera.farClipPlane = GameplayFarClipPlane * k_FARCLIPPLANESCALE;

        GameplayCameraVanishingLocalPoint = GetCameraVanishingPoint();
        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        GeneratePlayAreaMesh();
    }

    private void CreateGameplayReferencePoints()
    {
        Vector2 minScreenCoordinates = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(0f, 0f), gameplayRectTransform);
        Vector2 maxScreenCoordinates = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(1f, 1f), gameplayRectTransform);

        WorldPositionOfPreviewMin = gameplayCamera.ScreenToWorldPoint(new Vector3(minScreenCoordinates.x, minScreenCoordinates.y, k_HITPLANEDEPTH));
        WorldPositionOfPreviewMax = gameplayCamera.ScreenToWorldPoint(new Vector3(maxScreenCoordinates.x, maxScreenCoordinates.y, k_HITPLANEDEPTH));

        WorldSizeOfPreview = WorldPositionOfPreviewMax - WorldPositionOfPreviewMin;
        ScreenSizeOfPreview = maxScreenCoordinates - minScreenCoordinates;

        WorldToScreenSizeRatioOfPreview = WorldSizeOfPreview / ScreenSizeOfPreview;
    }

    private const float k_BORDERINSETTHICKNESS = 0.025f;
    private const int k_NUMBEROFVERTICES = 8;
    private const int k_NUMBEROFTRIANGLES = 8;
    private const float k_DiagonalDisplacementComponent = 0.707106781f; // precomputes the unit vector of (1,1) and stores the component (sqrt(2) / 2)


    /// <summary>
    /// Generates a mesh with a defined inset thickness at the preview borders. Refer to the border schematic to better understand this code.
    /// </summary>
    private void GeneratePlayAreaMesh()
    {
        if (k_BORDERINSETTHICKNESS * 2 > WorldSizeOfPreview.x || k_BORDERINSETTHICKNESS * 2 > WorldSizeOfPreview.y) // invalid inset
        {
            return;
        }

        Mesh mesh = new Mesh();

        Vector3 worldMin = new Vector3(WorldPositionOfPreviewMin.x, WorldPositionOfPreviewMin.y, 0f);
        Vector3 worldMax = new Vector3(WorldPositionOfPreviewMax.x, WorldPositionOfPreviewMax.y, 0f);

        Vector3 min = worldMin - k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);
        Vector3 max = worldMax - k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        // outer verts is from 0 to 3, with 0 bottom left and increment clockwise
        // inner verts is from 4 to 7, with 4 bottom left and increment clockwise. we want the inner verts to be where the border is too, hence min and max has a displacement vector

        Vector3[] verts = new Vector3[k_NUMBEROFVERTICES];
        verts[0] = min;
        verts[1] = new Vector3(min.x, max.y, 0f);
        verts[2] = max;
        verts[3] = new Vector3(max.x, min.y, 0f);

        verts[4] = verts[0] + k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);
        verts[5] = verts[1] + k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        verts[6] = verts[2] + k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        verts[7] = verts[3] + k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);

        LocalBorderCorners[0] = verts[4];
        LocalBorderCorners[1] = verts[5];
        LocalBorderCorners[2] = verts[6];
        LocalBorderCorners[3] = verts[7];

        int[] tris = new int[k_NUMBEROFTRIANGLES * 3];

        // refer to schematic
        for (int i = 0; i < k_NUMBEROFTRIANGLES / 2; i++)
        {
            int offset = 6 * i; // generate 2 triangles for each cycle

            tris[offset] = i;
            tris[offset + 1] = (i + 1) % 4;
            tris[offset + 2] = (i + 1) % 4 + 4;

            tris[offset + 3] = i;
            tris[offset + 4] = (i + 1) % 4 + 4;
            tris[offset + 5] = i + 4;
        }


        Vector2[] uvs = new Vector2[k_NUMBEROFVERTICES];

        Vector2 thicknessRelative = (Vector2.one * k_BORDERINSETTHICKNESS) / WorldSizeOfPreview; // how large the inset thickness relative to whole object scale. Note object scale is 16:9 ratio

        uvs[0] = Vector2.zero;
        uvs[1] = new Vector2(0, 1);
        uvs[2] = Vector2.one;
        uvs[3] = new Vector2(1, 0);

        uvs[4] = uvs[0] + thicknessRelative;
        uvs[5] = uvs[1] + new Vector2(thicknessRelative.x, -thicknessRelative.y);
        uvs[6] = uvs[2] + (-1f * thicknessRelative);
        uvs[7] = uvs[3] + new Vector2(-thicknessRelative.x, thicknessRelative.y);

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        PlayAreaBorderMesh = mesh;

    }
    private void GameInstance_OnGameSettingsChanged()
    {
        GameplayFarClipPlane = k_HITPLANEDEPTH + (float)(GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);
        gameplayCamera.farClipPlane = GameplayFarClipPlane * k_FARCLIPPLANESCALE;

        CreateGameplayReferencePoints();
        GeneratePlayAreaMesh();
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(stopwatchAction);
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
        GameplayInstance = null;
    }

    private void Update()
    {
        if (IsInReplayMode)
        {
            return;
        }

        MathHelper.GetNormalizedPointInsideReferenceUI(GameManager.GameInstance.MousePosition, gameplayRectTransform, out Vector2 normalizedPoint);

        GameplayMousePosition = MathHelper.ClampVectorByComponent(normalizedPoint, 0f, 1f);
    }

    public const double k_TIMEOFFSET = 1d; // this allows for offset for the chart to be earlier than the dsp time

    private void UpdateGameplayTimeByDeltatime(double dt)
    {
        CurrentGameplayTime += dt;
        GameplayCamera.transform.Translate((float)(dt * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed) * Vector3.forward);
        OnGameplayTimeUpdated?.Invoke(CurrentGameplayTime);
    }

    private void StartGameplay()
    {
        if (CurrentGameplayChart == null)
        {
            Debug.LogWarning($"No gameplay chart assigned");
            return;
        }
        MaxHitboxCount = CurrentGameplayChart.GameplayObjects.Count(x =>
        {
            if (x is not VisualHitbox hitbox)
            {
                return false;
            }

            return hitbox.HitboxType != HitboxType.BOMB;
        });

        if (IsInReplayMode)
        {
            if (!((GameplayStatisticRecord)CurrentGameplayRecord).BaseChartMetadata.Equals(CurrentMetadata.BaseMetadata))
            {
                Debug.LogWarning($"The replay metadata does not match the loaded chart metadata.\n" +
                                 $"The replay will play, but the visuals may have discrepancy.");
                GameManager.GameInstance.InvokeInformationDisplayNeeded("Metadata conflict, visuals will be wrong.", 1d);
            }

            OnGameplayReplayLoaded?.Invoke((GameplayStatisticRecord)CurrentGameplayRecord);
        }

        CurrentAccuracy = 1d;

        if (MaxHitboxCount <= 0)
        {
            scorePerNote = 0d;
            CurrentScore = k_MAXIMUMSCORE;
            return;
        }

        scorePerNote = k_MAXIMUMSCORE / MaxHitboxCount;

        MissCount = 0;
        MatchHitCount = 0;
        MismatchHitCount = 0;
        CurrentCombo = 0;
        BombHitCount = 0;
        CurrentScore = 0d;

        StartChart();
    }

    private Vector3 GetCameraVanishingPoint()
    {
        Vector2 screenPoint = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(0.5f, 0.5f), gameplayRectTransform);
        return gameplayCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, GameplayFarClipPlane));
    }

    private TimerStopwatchAction stopwatchAction;
    private void StartChart()
    {
        EndTime = CurrentGameplayChart.GameplayObjects[CurrentGameplayChart.GameplayObjects.Length - 1].RenderTime; // note it is sorted
        Action<double> timerElaspedAction = (x) => UpdateGameplayTimeByDeltatime(x);
        Action timerEndAction = () => { InvokeGameplayEndedEvent(); };

        stopwatchAction = new TimerStopwatchAction(this, timerElaspedAction, timerEndAction, k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, EndTime + k_TIMEOFFSET, true);
        DSPTimerEngine.TimerInstance.AddActionToTimer(stopwatchAction);
    }

    private void GetAccuracy()
    {
        int totalHits = MatchHitCount + MismatchHitCount - BombHitCount;
        if (totalHits + MissCount == 0)
        {
            CurrentAccuracy = 1d;
        }
        else
        {
            CurrentAccuracy = (double)(totalHits) / (totalHits + MissCount);
        }
    }

    /// <summary>
    /// This is called by <see cref="GameManager"/> when the gameplay scene loads.
    /// </summary>
    /// <param name="path"></param>
    public void InvokeGameplayStartedEvent(string path)
    {
        CurrentPath = path;
        GamePersistenceManager.LoadChartFile(path, out string chartJson, out string metadataJson, out byte[] bytes);

        if (string.IsNullOrWhiteSpace(chartJson) || string.IsNullOrWhiteSpace(metadataJson))
        {
            Debug.LogWarning($"Invalid chart JSONs, check file! Path: \n" +
                             $"{path}");
            GameManager.GameInstance.InvokeInformationDisplayNeeded("The chart is invalid!", 5d);
            return;
        }
        JObject chartJObject = JObject.Parse(chartJson);
        JObject metadataJObject = JObject.Parse(metadataJson);

        bool validResult = GameVersionConverter.CompareChartMetadataWithCurrentVersion(in metadataJObject, out int compareResult);

        if (!validResult)
        {
            Debug.LogWarning($"Loaded chart has invalid metadata, can not play chart!");
            GameManager.GameInstance.InvokeInformationDisplayNeeded("The chart has invalid metadata!", 5d);
            return;
        }
        else if (compareResult == 1)
        {
            Debug.LogWarning($"Game is outdated, can not load the chart!");
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Your game is outdated and can not play the chart!", 5d);
            return;
        }
        else if (compareResult == -1)
        {
            Debug.Log($"Loaded chart is not up to date with game version, prompting confirmation to resolve...");
            ConfirmAction action = new ConfirmAction(() =>
            {
                if (!GameVersionConverter.ConvertChartVersionToCurrentGameVersion(in chartJObject, in metadataJObject, out JObject convertedChartJObject, out JObject convertedmetadataJObject))
                {
                    Debug.LogWarning($"Loaded chart can not be automatically resolved!");
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Can not resolve mismatch! Try opening in the Editor!", 5d);
                    return;
                }

                Debug.Log($"Loaded chart version conflict is automatically resolved");
                GameManager.GameInstance.InvokeInformationDisplayNeeded("Resolved version mismatch", 1d);

                StartGameplayFromJsonString(convertedChartJObject.ToString(), convertedmetadataJObject.ToString(), bytes);
            }, () => SceneLoader.LoadSceneAtIndex(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => { }),
            "The selected chart is outdated.\n" +
            "The game will attempt to resolve mismatch, do you still want to continue?");

            GameManager.GameInstance.InvokeConfirmActionNeeded(action);
            return;
        }

        StartGameplayFromJsonString(chartJson, metadataJson, bytes);
    }

    private async void StartGameplayFromJsonString(string chartJson, string metadataJson, byte[] audioBytes)
    {
        (bool convertResult, EditorChart editorChart, AudioClip clip) = await GamePersistenceManager.ConvertFilesToEditorChart(chartJson, audioBytes);

        GamePersistenceManager.GetMetadataOfEditorChartFromJson(metadataJson, out EditorChartMetadata metadata);
        if (metadata == null)
        {
            Debug.LogWarning($"Failed to get chart metadata");
            CurrentMetadata = null;
        }

        CurrentMetadata = metadata;
        CurrentGameplayChart = MathHelper.ConvertEditorChartToGameplayChart(editorChart, clip);

        OnGameplayChartLoaded?.Invoke(clip, metadata);
        StartGameplay();
        OnGameplayStarted?.Invoke();
    }

    /// <summary>
    /// This is called by <see cref="GameManager"/> when the gameplay scene loads.
    /// </summary>
    /// <param name="path"></param>
    public void InvokeGameplayReplayStartedEvent(string path, GameplayStatisticRecord record)
    {
        IsInReplayMode = true;
        CurrentGameplayRecord = record;
        InvokeGameplayStartedEvent(path);
    }

    public void InvokeGameplayObjectRendered(GameplayObject obj)
    {
        OnGameplayObjectRendered?.Invoke(obj);
    }

    public void InvokeGameplayObjectUnrendered(GameplayObject obj)
    {
        OnGameplayObjectUnrendered?.Invoke(obj);
    }
    public void InvokeHitboxActiveEvent(VisualHitbox hitbox)
    {
        OnHitboxBecomeActive?.Invoke(hitbox);
    }
    public void InvokeHitboxMatchHitEvent(VisualHitbox hitbox)
    {
        CurrentCombo++;
        MatchHitCount++;
        GetAccuracy();

        CurrentScore += scorePerNote;
        OnHitboxMatchedHit?.Invoke(hitbox);
    }

    public void InvokeHitboxMismatchHitEvent(VisualHitbox hitbox)
    {
        CurrentCombo++;
        MismatchHitCount++;
        GetAccuracy();

        CurrentScore += scorePerNote * k_MISMATCHSCOREWEIGHT;
        OnHitboxMismatchedHit?.Invoke(hitbox);
    }

    public void InvokeHitboxMissEvent(VisualHitbox hitbox)
    {
        MissCount++;
        CurrentCombo = 0;
        GetAccuracy();

        OnHitboxMiss?.Invoke(hitbox);
    }

    public void InvokeHitboxBombHitEvent(VisualHitbox hitbox)
    {
        BombHitCount++;
        CurrentCombo = 0;
        GetAccuracy();

        CurrentScore -= scorePerNote;
        OnHitboxBombHit?.Invoke(hitbox);
    }

    public void InvokeGameplayEndedEvent()
    {
        if (MatchHitCount + MismatchHitCount + MissCount == MaxHitboxCount && CurrentPath == GameManager.GameInstance.k_TUTORIALFILEPATHSTRING)
        {
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GameEvents.HasPlayedTutorial, true);
        }

        OnGameplayEnded?.Invoke();
    }

    public void InvokeGameplayRestartEvent()
    {
        CurrentGameplayTime = 0d;
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(stopwatchAction);
        OnGameplayRestarted?.Invoke();
        StartGameplay();
        OnGameplayStarted?.Invoke();
    }

    public void InvokeMouseActiveTypeChanged(MouseActiveType newActiveType)
    {
        if (MouseActiveType == newActiveType)
        {
            return;
        }

        MouseActiveType = newActiveType;
        OnMouseActiveTypeChanged?.Invoke(this.MouseActiveType);
    }

    public void InvokeGameplayMarkerUpdate(GameplayMarker newMarker)
    {
        CurrentActiveGameplayMarker = newMarker;
        if (string.IsNullOrWhiteSpace(newMarker.DisplayMessage) || newMarker.DisplayTime <= 0d)
        {
            return;
        }

        GameManager.GameInstance.InvokeInformationDisplayNeeded(newMarker.DisplayMessage, newMarker.DisplayTime);
    }

    public void InvokeGameplayMetronomeFired(double fireTime)
    {
        OnGameplayMetronomeFired?.Invoke(fireTime);
    }

    public void SetGameMouseState(Vector2 position, MouseActiveType mouseType)
    {
        GameplayMousePosition = position;
        InvokeMouseActiveTypeChanged(mouseType);
    }

    public void AssignGameplayBorderMesh(Mesh mesh)
    {
        PlayAreaBorderMesh = mesh;
    }

    public void AssignGameplayBorderScale(Vector3 scale)
    {
        CurrentPlayAreaBorderScale = scale;
    }

    public void AssignGameplayDisplacementRotation(Vector3 displacement, Quaternion rotation)
    {
        CurrentPlayAreaDisplacement = displacement;
        CurrentPlayAreaRotation = rotation;
    }
}

public class GameplayChart
{
    public GameplayObject[] GameplayObjects { get; private set; }
    public AudioClip AudioClip { get; private set; }
    public GameplayChart(GameplayObject[] gameplayObjects, AudioClip audioClip)
    {
        GameplayObjects = gameplayObjects;
        AudioClip = audioClip;
    }
}

public enum GameplayResultRank
{
    SS = 0,
    S = 1,
    AA = 2,
    A = 3,
    B = 4,
    C = 5,
    D = 6,
    F = 7
}
