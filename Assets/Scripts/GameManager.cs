using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class GameManager : MonoBehaviour
{
    public readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        DefaultValueHandling = DefaultValueHandling.Populate,
        MissingMemberHandling = MissingMemberHandling.Error
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

    /// <summary>
    /// A cache to create a mapping f: Input Action -> Keyboard modifiers. <br></br>
    /// This will be automatically generated when <see cref="GameManager"/> is started, based on <see cref="InputActions"/>.
    /// </summary>
    public static Dictionary<InputAction, KeyboardModifiers> InputActionModifierMappingCache { get; private set; } = new();

    private const string k_MODIFIERKEY = "modifier";
    private const string k_CTRLMODIFIERKEY = "ctrl";
    private const string k_SHIFTMODIFIERKEY = "shift";
    private const string k_ALTMODIFIERKEY = "alt";

    /// <summary>
    /// Creates <see cref="InputActionModifierMappingCache"/> by iterating through all action maps, then all actions, then searching for any modifiers in the bindings.
    /// </summary>
    private void CreateInputActionModifierCache()
    {
        InputActionModifierMappingCache.Clear();

        foreach (var actionMaps in InputActions.asset.actionMaps)
        {
            foreach (var action in actionMaps.actions)
            {
                KeyboardModifiers modifiers = KeyboardModifiers.NONE;

                ReadOnlyArray<InputBinding> bindings = action.bindings;

                for (int i = 0; i < bindings.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(bindings[i].name))
                    {
                        continue;
                    }

                    if (!bindings[i].isPartOfComposite)
                    {
                        continue;
                    }

                    string path = bindings[i].path;

                    if (path.Contains(k_CTRLMODIFIERKEY)) modifiers ^= KeyboardModifiers.CTRL;
                    else if (path.Contains(k_SHIFTMODIFIERKEY)) modifiers ^= KeyboardModifiers.SHIFT;
                    else if (path.Contains(k_ALTMODIFIERKEY)) modifiers ^= KeyboardModifiers.ALT;
                }

                InputActionModifierMappingCache.Add(action, modifiers);
            }
        }
    }

    /// <summary>
    /// Whether or not the correct modifiers are being pressed for the provided <paramref name="action"/>. <br></br>
    /// Returns true if the keyboard modifier is correct. Otherwise returns false. Also returns false if no cache hit or if <see cref="Keyboard.current"/> is null.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public bool IsCorrectKeyboardModifierForInputAction(InputAction action)
    {
        bool cacheResult = InputActionModifierMappingCache.TryGetValue(action, out KeyboardModifiers cachedKeyboardModifiers);

        if (!cacheResult)
        {
            return false;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return false;

        return (cachedKeyboardModifiers.HasFlag(KeyboardModifiers.CTRL) == keyboard.ctrlKey.isPressed) &&
               (cachedKeyboardModifiers.HasFlag(KeyboardModifiers.SHIFT) == keyboard.shiftKey.isPressed) &&
               (cachedKeyboardModifiers.HasFlag(KeyboardModifiers.ALT) == keyboard.altKey.isPressed);

    }
    private void Start()
    {
        CreateInputActionModifierCache();
        string defaultKeybindJson = InputActions.SaveBindingOverridesAsJson();

        DefaultGlobalSettings = new GlobalSettings(0d, 1f, false, 0.25f, 0.5f, defaultKeybindJson,
                                                  new GameSettings(3d, 1d),
                                                  new EditorSettings(1d, 1d),
                                                  new GraphicSettings(new Vector2Int(Display.main.systemWidth, Display.main.systemHeight), true, AntiAliasingMSAA.Off, 1f, true, 0),
                                                  new GameEvents(false, false));

        JsonSerializerSettings.Converters.Add(new Vector2Serializer());
        k_TUTORIALFILEPATHSTRING = Path.Combine(Application.streamingAssetsPath, $"{k_TUTORIALCHARTNAME}.{k_FILEEXTENSION}");

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

        InputActions.LoadBindingOverridesFromJson(GlobalSettings.KeybindJson);

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
            Application.targetFrameRate = Mathf.Max(GlobalSettings.GraphicSettings.FrameRateLimit, 30);
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
        WriteCurrentInputActionAsJson();
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

    /// <summary>
    /// Request to play the chart at the designated <paramref name="path"/>. This will automatically load the scene and invoke <see cref="GameplayManager.RequestGameplayStartedEvent(string)"/>.
    /// </summary>
    /// <param name="path"></param>
    public void RequestPlayChartEvent(string path)
    {
        if (!GlobalSettings.GameEvents.HasPlayedTutorial)
        {
            if (path != k_TUTORIALFILEPATHSTRING)
            {
                ConfirmAction loadConfirmAction = new ConfirmAction(() => SceneLoader.LoadSceneAtIndex(SceneLoader.k_GAMEPLAYINDEX, () => GameplayManager.GameplayInstance.RequestGameplayStartedEvent(path)), () => { }, "It is recommended to play the tutorial chart first.\n" +
                                                                                                                                                                                                                         "Do you still want to continue?");
                InvokeConfirmActionNeeded(loadConfirmAction);
                return;
            }
        }

        SceneLoader.LoadSceneAtIndex(SceneLoader.k_GAMEPLAYINDEX, () => GameplayManager.GameplayInstance.RequestGameplayStartedEvent(path));
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
        StartCoroutine(InvokeGameSettingsChanged_Internal());
    }

    private IEnumerator InvokeGameSettingsChanged_Internal()
    {
        SetupGraphicalSettings();

        yield return null; // wait for the current frame to be done (set resolution is done at the end of current frame)
        yield return new WaitForEndOfFrame(); // wait for the canvas / gui to be updated
        OnGameSettingsChanged?.Invoke(); // finally invoke the event for the listeners do to their own logic

    }
    public void AddGameplayRecordToMapping(GameplayStatisticRecord record)
    {
        GamePersistenceManager.UpdateMetadataToRecordsMapping(record, ChartMetadataGUIDToGameplayRecordMapping);
    }

    public void WriteCurrentInputActionAsJson()
    {
        string json = InputActions.SaveBindingOverridesAsJson();

        GlobalSettings.EditSettings(() => GlobalSettings.KeybindJson, json);
    }
}

[Serializable]
public class GlobalSettings
{
    [DefaultValue(0f)]
    public double AudioOffsetMs { get; private set; }

    [DefaultValue(1f)]
    public float MouseSensitivityScaleFactor { get; private set; }

    [DefaultValue(false)]
    public bool UsePrescheduledHitsounds { get; private set; }

    [DefaultValue(0.25f)]
    public float SongVolume { get; private set; }
    [DefaultValue(0.5f)]
    public float HitsoundVolume { get; private set; }

    public string KeybindJson { get; private set; }

    [JsonProperty(Required = Required.Always)]
    public GameSettings GameSettings { get; private set; }

    [JsonProperty(Required = Required.Always)]
    public EditorSettings EditorSettings { get; private set; }

    [JsonProperty(Required = Required.Always)]
    public GraphicSettings GraphicSettings { get; private set; }

    [JsonProperty(Required = Required.Always)]
    public GameEvents GameEvents { get; private set; }


    // we are going to trust that the settings file has valid inputs. Lol
    public GlobalSettings(double audioOffsetMs, float mouseSensitivityScaleFactor, bool usePrescheduledHitsounds, float songVolume, float hitsoundVolume, string keybindJson, GameSettings gameSettings, EditorSettings editorSettings, GraphicSettings graphicSettings, GameEvents gameEvents)
    {
        AudioOffsetMs = audioOffsetMs;
        MouseSensitivityScaleFactor = mouseSensitivityScaleFactor;
        UsePrescheduledHitsounds = usePrescheduledHitsounds;
        SongVolume = songVolume;
        HitsoundVolume = hitsoundVolume;
        KeybindJson = keybindJson;

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
    [DefaultValue(3d)]
    public double GameScrollSpeed { get; private set; }

    [DefaultValue(1d)]
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
    [DefaultValue(1d)]
    public double BigScrollTimeInterval { get; private set; }

    [DefaultValue(1d)]
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

    [DefaultValue(true)]
    public bool IsUseFullScreen { get; private set; }

    [DefaultValue(AntiAliasingMSAA.Off)]
    public AntiAliasingMSAA AntiAliasingMSAA { get; private set; }

    [DefaultValue(1f)]
    public float RenderScale { get; private set; }

    [DefaultValue(true)]
    public bool IsUseVsync { get; private set; }
    /// <summary>
    /// Set to non-positive for no limit (unless if VSync is on), positive ints for FPS limit (overrides VSync).
    /// </summary>
    
    [DefaultValue(0)]
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
    [DefaultValue(false)]
    public bool HasAdjustedOffset { get; private set; }

    [DefaultValue(false)]
    public bool HasPlayedTutorial { get; private set; }
    public GameEvents(bool hasAdjustedOffset, bool hasPlayedTutorial)
    {
        this.HasAdjustedOffset = hasAdjustedOffset;
        this.HasPlayedTutorial = hasPlayedTutorial;
    }
}

public enum AntiAliasingMSAA
{
    Off,
    Two,
    Four,
    Eight
}

[Flags]
public enum KeyboardModifiers
{
    NONE = 0,
    CTRL = 1,
    SHIFT = 2,
    ALT = 4
}
