using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class GameManager : MonoBehaviour
{
    public readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.Auto,
    };


    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private Canvas pauseCanvas;

    public Canvas PopupCanvas { get => popupCanvas; }

    public Canvas PauseCanvas { get => pauseCanvas; }
    public const string k_METADATAFILENAME = "metadata.json";
    public const string k_CHARTFILENAME = "chart.json";
    public const string k_AUDIOFILENAME = "audio.mp3"; // let's just assume mp3 for now... IDC
    public const string k_FILEEXTENSION = "psr";
    public const string k_PLAYERSETTINGSFILENAME = "settings.json";
    public const string k_TUTORIALCHARTNAME = "tutorial";

    public static readonly Vector2 aspectRatioConversionScale = new Vector2(0.490261239f, 0.871575537f);
    public static readonly float aspectRatioFloat = 16f / 9f;
    public static readonly float aspectRatioReciprocalFloat = 9f / 16f;
    public static GameManager GameInstance;
    public Vector2 MousePosition { get; private set; }
    public PlayerInputActions InputActions { get; private set; }

    public event Action<ConfirmAction> OnConfirmActionNeeded;
    public event Action<string, double> OnInformationDisplayNeeded;

    public event Action OnPauseMenuEnable;
    public event Action OnPauseMenuDisable;
    public event Action<string> OnPauseMenuDescriptionChanged;

    public event Action OnGameSettingsChanged;
    public Mesh NoteMesh { get; private set; }
    public string CurrentVersion { get; private set; }

    public GlobalSettings GlobalSettings;

    public static GlobalSettings DefaultGlobalSettings;

    public const double k_HIGHLATENCYTHRESHOLDMS = 100d;

    /// <summary>
    /// How much we scale the normalized X position to get our audio panning.
    /// </summary>
    public const float k_AUDIOSTEREOPANNINGSCALING = 1f;
    /// <summary>
    /// Defines a mapping f: Base Metadata -> set of records. This is used for determining the relation between charts and the gameplay records.
    /// </summary>
    public Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>> ChartMetadataGUIDToGameplayRecordMapping { get; private set; }

    public string k_TUTORIALFILEPATHSTRING { get; private set; }

    /// <summary>
    /// A key to describe the base metadata field in <see cref="EditorChartMetadata"/> for access.
    /// </summary>
    public const string k_METADATABASEDATAKEY = "BaseMetadata";

    public const string k_CHARTVERSIONKEY = "Version";
    public const string k_CHARTNAMEKEY = "ChartName";
    public const string k_CHARTMAPPERKEY = "ChartMapper";
    public const string k_SONGNAMEKEY = "SongName";
    public const string k_SONGARTISTKEY = "SongArtist";
    public const string k_CHARTGUIDKEY = "GUID";
    public const string k_CHARTDIFFICULTYKEY = "ChartDifficulty";

    private UniversalRenderPipelineAsset URP_asset;
    public List<Vector2Int> AllPossibleResolutions { get; private set; }
    private void Awake()
    {
        if (GameInstance != null)
        {
            Destroy(pauseCanvas.gameObject);
            Destroy(popupCanvas.gameObject);
            Destroy(gameObject);
            return;
        }
        else
        {
            GameInstance = this;
            DontDestroyOnLoad(gameObject);
        }

        InputActions = new();
        InputActions.Enable();
    }

    private void Start()
    {
        DefaultGlobalSettings = new GlobalSettings(0d, false, 0.5f, 0.5f,
                                                  new GameSettings(3d, 1d),
                                                  new EditorSettings(1d, 1d),
                                                  new GraphicSettings(new Vector2Int(Display.main.systemWidth, Display.main.systemHeight), true, AntiAliasingMSAA.Off, 1f, true, 0),
                                                  new GameEvents(false, false));

        JsonSerializerSettings.Converters.Add(new Vector2Serializer());
        k_TUTORIALFILEPATHSTRING = Path.Combine(Application.persistentDataPath, GamePersistenceManager.k_GameChartStorageFolderName, $"{k_TUTORIALCHARTNAME}.{k_FILEEXTENSION}");

        CurrentVersion = Application.version;
        if (!MathHelper.IsStringMatchVersioningFormat(CurrentVersion))
        {
            Debug.LogWarning($"Current version is invalid format!");
        }

        if (!GamePersistenceManager.LoadGlobalSettingsFromFile(out GlobalSettings))
        {
            InvokeInformationDisplayNeeded("No global settings found");
        }
        else
        {
            InvokeInformationDisplayNeeded("Loaded settings");
        }
        UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (!asset)
        {
            Debug.LogWarning($"Current rendering pipeline is not using URP! This is horrible!!!");
        }
        else
        {
            URP_asset = asset;
        }

        SetupGraphicalSettings();
        GamePersistenceManager.ImportTutorialChartToGameStorage();
        GamePersistenceManager.CreateMetadataToRecordsMapping(out Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>> mapping);
        ChartMetadataGUIDToGameplayRecordMapping = mapping;
    }

    private void SetupGraphicalSettings()
    {
        int width = GlobalSettings.GraphicSettings.CurrentResolution.x;
        int height = GlobalSettings.GraphicSettings.CurrentResolution.y;
        FullScreenMode fullScreenMode = GlobalSettings.GraphicSettings.IsUseFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (GlobalSettings.GraphicSettings.FrameRateLimit > 0)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = GlobalSettings.GraphicSettings.FrameRateLimit;
            Screen.SetResolution(width, height, fullScreenMode);
        }
        else if (GlobalSettings.GraphicSettings.IsUseVsync)
        {
            QualitySettings.vSyncCount = 1;
            Screen.SetResolution(width, height, fullScreenMode, Screen.currentResolution.refreshRateRatio);
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Screen.SetResolution(width, height, fullScreenMode);
        }

        int msaaCount;
        switch (GlobalSettings.GraphicSettings.AntiAliasingMSAA)
        {
            case AntiAliasingMSAA.Off:
                msaaCount = 1;
                break;
            case AntiAliasingMSAA.Two:
                msaaCount = 2;
                break;
            case AntiAliasingMSAA.Four:
                msaaCount = 4;
                break;
            case AntiAliasingMSAA.Eight:
                msaaCount = 8;
                break;
            default:
                msaaCount = 1;
                break;
        }

        URP_asset.msaaSampleCount = msaaCount;
        URP_asset.renderScale = Mathf.Clamp(GlobalSettings.GraphicSettings.RenderScale, 0.25f, 1f);
    }

    private void OnApplicationQuit()
    {
        GamePersistenceManager.SaveGlobalSettingsToFile(GlobalSettings);
    }

    private void Update()
    {
        MousePosition = InputActions.Gameplay.MousePosition.ReadValue<Vector2>();
    }

    public void InvokeConfirmActionNeeded(ConfirmAction action)
    {
        OnConfirmActionNeeded?.Invoke(action);
    }

    public void InvokeInformationDisplayNeeded(string infoMessage, double time = 0.25d)
    {
        OnInformationDisplayNeeded?.Invoke(infoMessage, time);
    }

    public void RequestPlayChartEvent(string path)
    {
        if (!GlobalSettings.GameEvents.HasPlayedTutorial)
        {
            if (path != k_TUTORIALFILEPATHSTRING)
            {
                ConfirmAction loadConfirmAction = new ConfirmAction(() => SceneLoader.LoadSceneAtIndex(SceneLoader.k_GAMEPLAYINDEX, () => GameplayManager.GameplayInstance.InvokeGameplayStartedEvent(path)), () => { }, "It is recommended to play the tutorial chart first.\n" +
                                                                                                                                                                                                                         "Do you still want to continue?");
                InvokeConfirmActionNeeded(loadConfirmAction);
                return;
            }
        }

        SceneLoader.LoadSceneAtIndex(SceneLoader.k_GAMEPLAYINDEX, () => GameplayManager.GameplayInstance.InvokeGameplayStartedEvent(path));
    }

    public void RequestReplayChartEvent(string path, GameplayStatisticRecord gameplayRecord)
    {
        SceneLoader.LoadSceneAtIndex(SceneLoader.k_GAMEPLAYINDEX, () => GameplayManager.GameplayInstance.InvokeGameplayReplayStartedEvent(path, gameplayRecord));
    }
    public void InvokeGamePauseMenuEnable()
    {
        DSPTimerEngine.TimerInstance.PauseDSPTimer();
        OnPauseMenuEnable?.Invoke();
    }

    public void InvokeGamePauseMenuDisable()
    {
        DSPTimerEngine.TimerInstance.ResumeDSPTimer();
        OnPauseMenuDisable?.Invoke();
    }

    public void InvokeGamePauseDescriptionChanged(string description)
    {
        OnPauseMenuDescriptionChanged?.Invoke(description);
    }
    public void InvokeGameSettingsChanged()
    {
        SetupGraphicalSettings();
        Debug.Log($"Settings changed!");
        OnGameSettingsChanged?.Invoke();
    }
    public void AddGameplayRecordToMapping(GameplayStatisticRecord record)
    {
        GamePersistenceManager.UpdateMetadataToRecordsMapping(record, ChartMetadataGUIDToGameplayRecordMapping);
    }
}

[Serializable]
public class GlobalSettings
{
    public double AudioOffsetMs { get; private set; }

    public bool UsePrescheduledHitsounds { get; private set; }

    public float SongVolume { get; private set; }
    public float HitsoundVolume { get; private set; }

    public GameSettings GameSettings { get; private set; }
    public EditorSettings EditorSettings { get; private set; }
    public GraphicSettings GraphicSettings { get; private set; }
    public GameEvents GameEvents { get; private set; }


    // we are going to trust that the settings file has valid inputs. Lol
    public GlobalSettings(double audioOffsetMs, bool usePrescheduledHitsounds, float songVolume, float hitsoundVolume, GameSettings gameSettings, EditorSettings editorSettings, GraphicSettings graphicSettings, GameEvents gameEvents)
    {
        AudioOffsetMs = audioOffsetMs;
        UsePrescheduledHitsounds = usePrescheduledHitsounds;
        SongVolume = songVolume;
        HitsoundVolume = hitsoundVolume;

        GameSettings = gameSettings;
        EditorSettings = editorSettings;
        GraphicSettings = graphicSettings;
        GameEvents = gameEvents;
    }

    /// <summary>
    /// Edits the current settings using an expression and invokes <see cref="GameManager.OnGameSettingsChanged"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the setting to edit</typeparam>
    /// <param name="editAction">The expression tree used to edit. Write the property you want to target here.</param>
    /// <param name="newValue">The new value you want to assign to your target property.</param>

    public void EditSettings<TValue>(Expression<Func<TValue>> editAction, TValue newValue)
    {
        if (editAction.Body is not MemberExpression expression)
        {
            return;
        }

        if (expression.Member is not PropertyInfo property)
        {
            return;
        }

        Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression.Expression, typeof(object)));
        object targetInstance = lambda.Compile()();

        property.SetValue(targetInstance, newValue);
        GameManager.GameInstance.InvokeGameSettingsChanged();
    }
}

[Serializable]
public class GameSettings
{
    public double GameScrollSpeed { get; private set; }
    public double GameLookaheadTime { get; private set; }
    public GameSettings(double gameScrollSpeed, double gameLookaheadTime)
    {
        GameScrollSpeed = gameScrollSpeed;
        GameLookaheadTime = gameLookaheadTime;
    }
}

[Serializable]
public class EditorSettings
{
    public double BigScrollTimeInterval { get; private set; }
    public double EditorLookaheadTime { get; private set; }

    public EditorSettings(double bigScrollTimeInterval, double editorLookaheadTime)
    {
        BigScrollTimeInterval = bigScrollTimeInterval;
        EditorLookaheadTime = editorLookaheadTime;
    }
}

[Serializable]
public class GraphicSettings
{
    /// <summary>
    /// x is width, y is height
    /// </summary>
    public Vector2Int CurrentResolution { get; private set; }
    public bool IsUseFullScreen { get; private set; }
    public AntiAliasingMSAA AntiAliasingMSAA { get; private set; }
    public float RenderScale { get; private set; }
    public bool IsUseVsync { get; private set; }
    /// <summary>
    /// Set to non-positive for no limit (unless if VSync is on), positive ints for FPS limit (overrides VSync).
    /// </summary>
    public int FrameRateLimit { get; private set; }

    public GraphicSettings(Vector2Int currentResolution, bool isUseFullscreen, AntiAliasingMSAA antiAliasingMSAA, float renderScale, bool isUseVsync, int frameRateLimit)
    {
        CurrentResolution = currentResolution;
        IsUseFullScreen = isUseFullscreen;
        AntiAliasingMSAA = antiAliasingMSAA;
        RenderScale = renderScale;
        IsUseVsync = isUseVsync;
        FrameRateLimit = frameRateLimit;
    }
}
[Serializable]
public class GameEvents
{
    public bool HasAdjustedOffset { get; private set; }
    public bool HasPlayedTutorial { get; private set; }
    public GameEvents(bool hasAdjustedOffset, bool isFirstTimePlayingChart)
    {
        this.HasAdjustedOffset = hasAdjustedOffset;
        this.HasPlayedTutorial = isFirstTimePlayingChart;
    }
}

public enum AntiAliasingMSAA
{
    Off,
    Two,
    Four,
    Eight
}
