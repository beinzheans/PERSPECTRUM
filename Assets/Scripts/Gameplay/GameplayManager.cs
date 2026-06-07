using System;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
public class GameplayManager : MonoBehaviour
{
    [SerializeField] private RectTransform gameplayRectTransform;
    public RectTransform GameplayRectTransform { get => gameplayRectTransform; }

    [SerializeField] private Camera gameplayCamera;
    public Camera GameplayCamera { get => gameplayCamera; }
    public static GameplayManager GameplayInstance;

    public event Action<AudioClip> OnGameplayAudioLoaded;
    public event Action OnGameplayStarted;
    public event Action OnGameplayEnded;

    public MouseActiveType MouseActiveType { get; private set; }

    /// <summary>
    /// How much leniency we allow before a note is considered a miss <br></br>
    /// Note that it is impossible for notes to be early.
    /// </summary>
    public const double k_LENIENCYTIMEFRAME = 0.2d;

    /// <summary>
    /// How much leniency we allow before a note is considered active. <br></br>
    /// This is to make the game FEEL more fair.
    /// </summary>
    public const double k_EARLYTIMEFRAME = 0.1d;

    /// <summary>
    /// How much we scale the hitbox interact size w.r.t. the actual hitbox size. <br></br>
    /// This is to make the game FEEL more fair.
    /// </summary>
    public const float k_HITBOXINTERACTSIZEBUFFERSCALE = 1.25f;
    /// <summary>
    /// How long the pool looks ahead to spawn objects. This is added on top of the player's lookahead time.
    /// </summary>
    public const double k_POOLLOOKAHEADTIME = 1d;

    /// <summary>
    /// How long the pool looks behind to despawn objects.
    /// </summary>
    public const double k_POOLUNRENDERTIMETHRESHOLD = 1d;

    /// <summary>
    /// The depth away from camera depicting the current time. This affects the extent of perspective distortion
    /// </summary>
    public const float k_HITPLANEDEPTH = 1f;
    public GameplayChart CurrentGameplayChart { get; private set; }

    public event Action<double> OnGameplayTimeUpdated;
    public event Action<VisualHitbox> OnHitboxMatchedHit;
    public event Action<VisualHitbox> OnHitboxMismatchedHit;
    public event Action<int> OnHitboxMiss;
    public event Action<int> OnHitboxBombHit;

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
    public const float k_MismatchWeighting = 0.5f;
    public int BombHitCount { get; private set; }

    public event Action<MouseActiveType> OnMouseActiveTypeChanged;

    private void Awake()
    {
        GameplayInstance = this;
    }

    private void Start()
    {
        Vector2 minScreenCoordinates = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(0f, 0f), gameplayRectTransform);
        Vector2 maxScreenCoordinates = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(1f, 1f), gameplayRectTransform);

        WorldPositionOfPreviewMin = gameplayCamera.ScreenToWorldPoint(new Vector3(minScreenCoordinates.x, minScreenCoordinates.y, k_HITPLANEDEPTH));
        WorldPositionOfPreviewMax = gameplayCamera.ScreenToWorldPoint(new Vector3(maxScreenCoordinates.x, maxScreenCoordinates.y, k_HITPLANEDEPTH));

        WorldSizeOfPreview = WorldPositionOfPreviewMax - WorldPositionOfPreviewMin;
        ScreenSizeOfPreview = maxScreenCoordinates - minScreenCoordinates;

        WorldToScreenSizeRatioOfPreview = WorldSizeOfPreview / ScreenSizeOfPreview * GameManager.aspectRatioConversionScale;
    }

    private void OnDestroy()
    {
        GameplayInstance = null;
    }

    private void Update()
    {
        MathHelper.GetNormalizedPointInsideReferenceUI(GameManager.GameInstance.MousePosition, gameplayRectTransform, out Vector2 normalizedPoint);

        GameplayMousePosition = normalizedPoint;
    }

    public const double k_TIMEOFFSET = 1d; // this allows for offset for the chart to be earlier than the time

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

        Debug.Log($"Starting gameplay");

        gameplayCamera.nearClipPlane = k_HITPLANEDEPTH;
        gameplayCamera.farClipPlane = k_HITPLANEDEPTH + (float)(GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);

        MaxHitboxCount = CurrentGameplayChart.GameplayObjects.Count(x =>
        {
            if (x is not VisualHitbox hitbox)
            {
                return false;
            }

            return hitbox.HitboxType != HitboxType.BOMB;
        });

        MissCount = 0;
        MatchHitCount = 0;
        CurrentCombo = 0;
        BombHitCount = 0;
        Cursor.visible = false;

        StartChart();
    }

    private void StartChart()
    {
        double endTime = CurrentGameplayChart.GameplayObjects[CurrentGameplayChart.GameplayObjects.Length - 1].RenderTime; // note it is sorted
        Action<double> timerElaspedAction = (x) => UpdateGameplayTimeByDeltatime(x);
        Action timerEndAction = () => { InvokeGameplayEndedEvent(); };

        TimerStopwatchAction stopwatchAction = new TimerStopwatchAction(timerElaspedAction, timerEndAction, k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs, endTime + k_TIMEOFFSET, true);
        DSPTimerEngine.TimerInstance.AddActionToTimer(stopwatchAction);
 
    }

    public async void InvokeGameplayStartedEvent(string path)
    {
        SaveLoadManager.LoadChartFile(path, out string chartJson, out _, out byte[] bytes);

        (bool convertResult, EditorChart editorChart, AudioClip clip) = await SaveLoadManager.ConvertFilesToEditorChart(chartJson, bytes);

        CurrentGameplayChart = MathHelper.ConvertEditorChartToGameplayChart(editorChart, clip);

        OnGameplayAudioLoaded?.Invoke(clip);
        StartGameplay();
        OnGameplayStarted?.Invoke();
    }

    public void InvokeHitboxMatchHitEvent(VisualHitbox hitbox)
    {
        CurrentCombo++;
        MatchHitCount++;
        OnHitboxMatchedHit?.Invoke(hitbox);
    }

    public void InvokeHitboxMismatchHitEvent(VisualHitbox hitbox)
    {
        CurrentCombo++;
        MismatchHitCount++;
        OnHitboxMismatchedHit?.Invoke(hitbox);
    }
    public void InvokeHitboxMissEvent(int numberOfMisses)
    {
        MissCount += numberOfMisses;
        CurrentCombo = 0;
        OnHitboxMiss?.Invoke(numberOfMisses);
    }

    public void InvokeHitboxBombHitEvent(int numberOfBombHits)
    {
        BombHitCount += numberOfBombHits;
        CurrentCombo = 0;
        OnHitboxBombHit?.Invoke(numberOfBombHits);
    }

    public void InvokeGameplayEndedEvent()
    {
        CurrentGameplayChart = null;
        OnGameplayEnded?.Invoke();

        Cursor.visible = true;
        Debug.Log($"Ended gameplay!");
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
