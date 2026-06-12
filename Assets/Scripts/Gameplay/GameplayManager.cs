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
    public const float k_HITBOXINTERACTSIZEADDDELTA = 0.05f;
    /// <summary>
    /// How long the pool looks ahead to spawn objects. This is added on top of the player's lookahead metronomeFireDSPTime.
    /// </summary>
    public const double k_POOLLOOKAHEADTIME = 1d;

    /// <summary>
    /// How long the pool looks behind to despawn objects.
    /// </summary>
    public const double k_POOLUNRENDERTIMETHRESHOLD = 0.01d;

    /// <summary>
    /// The depth away from camera depicting the current metronomeFireDSPTime. This affects the extent of perspective distortion
    /// </summary>
    public const float k_HITPLANEDEPTH = 1f;
    public GameplayChart CurrentGameplayChart { get; private set; }
    public EditorChartMetadata CurrentMetadata { get; private set; }
    public string CurrentPath { get; private set; }
    public event Action<double> OnGameplayTimeUpdated;
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
    /// Accuracy will be a measurement of how many hitboxes we miss, ignores the hitbox types weights
    /// </summary>
    public double CurrentAccuracy { get; private set; } = 1d;
    public const double k_MAXIMUMSCORE = 1000000d;
    private double scorePerNote = 0d;

    /// <summary>
    /// Score will be a measurement of the overall performance we have, considering the hitbox types
    /// </summary>
    public double CurrentScore { get; private set; } = 0d;

    private void Awake()
    {
        GameplayInstance = this;
    }

    public GameplayMarker CurrentActiveGameplayMarker { get; private set; }
    private TimerIntervalAction gameplayMetronome;
    public event Action<GameplayMarker> OnGameplayMarkerUpdated;
    public event Action<double> OnGameplayMetronomeFired;

    private void Start()
    {
        Vector2 minScreenCoordinates = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(0f, 0f), gameplayRectTransform);
        Vector2 maxScreenCoordinates = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(1f, 1f), gameplayRectTransform);

        WorldPositionOfPreviewMin = gameplayCamera.ScreenToWorldPoint(new Vector3(minScreenCoordinates.x, minScreenCoordinates.y, k_HITPLANEDEPTH));
        WorldPositionOfPreviewMax = gameplayCamera.ScreenToWorldPoint(new Vector3(maxScreenCoordinates.x, maxScreenCoordinates.y, k_HITPLANEDEPTH));

        WorldSizeOfPreview = WorldPositionOfPreviewMax - WorldPositionOfPreviewMin;
        ScreenSizeOfPreview = maxScreenCoordinates - minScreenCoordinates;

        WorldToScreenSizeRatioOfPreview = WorldSizeOfPreview / ScreenSizeOfPreview * GameManager.aspectRatioConversionScale;

        CurrentActiveGameplayMarker = null;
        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        gameplayCamera.farClipPlane = k_HITPLANEDEPTH + (float)(GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);
    }

    private void OnDestroy()
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(stopwatchAction);
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
        GameplayInstance = null;
    }

    private void Update()
    {
        MathHelper.GetNormalizedPointInsideReferenceUI(GameManager.GameInstance.MousePosition, gameplayRectTransform, out Vector2 normalizedPoint);

        GameplayMousePosition = normalizedPoint;
    }

    public const double k_TIMEOFFSET = 1d; // this allows for offset for the chart to be earlier than the metronomeFireDSPTime

    private void UpdateGameplayTimeByDeltatime(double dt)
    {
        CurrentGameplayTime += dt;
        GameplayCamera.transform.Translate((float)(dt * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed) * Vector3.forward);
        OnGameplayTimeUpdated?.Invoke(CurrentGameplayTime);
    }

    private const float k_NEARCLIPPLANESCALE = 0.5f;
    private void StartGameplay()
    {
        if (CurrentGameplayChart == null)
        {
            Debug.LogWarning($"No gameplay chart assigned");
            return;
        }

        gameplayCamera.nearClipPlane = k_HITPLANEDEPTH * k_NEARCLIPPLANESCALE;
        gameplayCamera.farClipPlane = k_HITPLANEDEPTH + (float)(GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);

        MaxHitboxCount = CurrentGameplayChart.GameplayObjects.Count(x =>
        {
            if (x is not VisualHitbox hitbox)
            {
                return false;
            }

            return hitbox.HitboxType != HitboxType.BOMB;
        });

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
        CurrentCombo = 0;
        BombHitCount = 0;
        Cursor.visible = false;

        StartChart();
    }

    private TimerStopwatchAction stopwatchAction;
    private void StartChart()
    {
        double endTime = CurrentGameplayChart.GameplayObjects[CurrentGameplayChart.GameplayObjects.Length - 1].RenderTime; // note it is sorted

        Action<double> timerElaspedAction = (x) => UpdateGameplayTimeByDeltatime(x);
        Action timerEndAction = () => { InvokeGameplayEndedEvent(); };

        stopwatchAction = new TimerStopwatchAction(this, timerElaspedAction, timerEndAction, k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, endTime + k_TIMEOFFSET, true);
        DSPTimerEngine.TimerInstance.AddActionToTimer(stopwatchAction);
    }

    private void GetAccuracy()
    {
        int totalHits = MatchHitCount + MismatchHitCount - BombHitCount;
        CurrentAccuracy = (double)(totalHits) / (totalHits + MissCount);
    }

    public async void InvokeGameplayStartedEvent(string path)
    {
        CurrentPath = path;
        SaveLoadManager.LoadChartFile(path, out string chartJson, out _, out byte[] bytes);

        (bool convertResult, EditorChart editorChart, AudioClip clip) = await SaveLoadManager.ConvertFilesToEditorChart(chartJson, bytes);

        SaveLoadManager.GetMetadataOfEditorChartPath(path, out EditorChartMetadata metadata);
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
        CurrentGameplayChart = null;
        OnGameplayEnded?.Invoke();

        Cursor.visible = true;
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
    }

    public void InvokeGameplayMetronomeFired(double metronomeFireTime)
    {
        OnGameplayMetronomeFired?.Invoke(metronomeFireTime);
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
