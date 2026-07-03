using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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

    public event Action OnGameSettingsChanged;
    public Mesh NoteMesh { get; private set; }
    public string CurrentVersion { get; private set; }

    public GlobalSettings GlobalSettings;

    public static readonly GlobalSettings DefaultGlobalSettings = new GlobalSettings(0d, false, 0.5f, 0.5f, new GameSettings(3d, 1d), new EditorSettings(1d, 1d), new GameEvents(false, false));
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
            DontDestroyOnLoad(popupCanvas.gameObject);
            DontDestroyOnLoad(pauseCanvas.gameObject);
        }

        InputActions = new();
        InputActions.Enable();
    }

    private void Start()
    {
        JsonSerializerSettings.Converters.Add(new Vector2Serializer());
        k_TUTORIALFILEPATHSTRING = Path.Combine(Application.persistentDataPath, GamePersistenceManager.k_GameChartStorageFolderName, $"{k_TUTORIALCHARTNAME}.{k_FILEEXTENSION}");

        // let's just by default enable v-sync
        // in the future we will make settings to allow users to choose
        // (in the settings rehaul update)
        RefreshRate refreshRate = Screen.currentResolution.refreshRateRatio;

        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreenMode, refreshRate);

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

        GamePersistenceManager.ImportTutorialChartToGameStorage();
        GamePersistenceManager.CreateMetadataToRecordsMapping(out Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>> mapping);
        ChartMetadataGUIDToGameplayRecordMapping = mapping;
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

    public void InvokeGameSettingsChanged()
    {
        OnGameSettingsChanged?.Invoke();
    }

    public void AddGameplayRecordToMapping(GameplayStatisticRecord record)
    {
        GamePersistenceManager.UpdateMetadataToRecordsMapping(record, ChartMetadataGUIDToGameplayRecordMapping);
    }
}

[Serializable]
public struct GlobalSettings
{
    public double AudioOffsetMs;
    public bool UsePrescheduledHitsounds;
    public float SongVolume;
    public float HitsoundVolume;

    public GameSettings GameSettings;
    public EditorSettings EditorSettings;
    public GameEvents GameEvents;

    // we are going to trust that the settings file has valid inputs. Lol
    public GlobalSettings(double audioOffsetMs, bool usePrescheduledHitsounds, float songVolume, float hitsoundVolume, GameSettings gameSettings, EditorSettings editorSettings, GameEvents gameEvents)
    {
        AudioOffsetMs = audioOffsetMs;
        UsePrescheduledHitsounds = usePrescheduledHitsounds;
        SongVolume = songVolume;
        HitsoundVolume = hitsoundVolume;
        GameSettings = gameSettings;
        EditorSettings = editorSettings;
        GameEvents = gameEvents;
    }
}

[Serializable]
public struct GameSettings
{
    public double GameScrollSpeed;
    public double GameLookaheadTime;
    public GameSettings(double gameScrollSpeed, double gameLookaheadTime)
    {
        GameScrollSpeed = gameScrollSpeed;
        GameLookaheadTime = gameLookaheadTime;
    }
}

[Serializable]
public struct EditorSettings
{
    public double BigScrollTimeInterval;
    public double EditorLookaheadTime;

    public EditorSettings(double bigScrollTimeInterval, double editorLookaheadTime)
    {
        BigScrollTimeInterval = bigScrollTimeInterval;
        EditorLookaheadTime = editorLookaheadTime;
    }
}

[Serializable]
public struct GameEvents
{
    public bool HasAdjustedOffset;
    public bool HasPlayedTutorial;
    public GameEvents(bool hasAdjustedOffset, bool isFirstTimePlayingChart)
    {
        this.HasAdjustedOffset = hasAdjustedOffset;
        this.HasPlayedTutorial = isFirstTimePlayingChart;
    }
}