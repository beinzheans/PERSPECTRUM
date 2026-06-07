using SFB;
using System;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [SerializeField] private Canvas popupCanvas;
    public Canvas PopupCanvas { get => popupCanvas; }
    public const string k_METADATAFILENAME = "metadata.json";
    public const string k_CHARTFILENAME = "chart.json";
    public const string k_AUDIOFILENAME = "audio.mp3"; // let's just assume mp3 for now... IDC
    public const string k_FILEEXTENSION = "mychart";
    public const string k_PLAYERSETTINGSFILENAME = "settings.json";

    public static readonly Vector2 aspectRatioConversionScale = new Vector2(0.490261239f, 0.871575537f);

    public static GameManager GameInstance;
    public Vector2 MousePosition { get; private set; }
    public PlayerInputActions InputActions { get; private set; }

    public event Action<ConfirmAction> OnConfirmActionNeeded;
    public event Action<string> OnInformationDisplayNeeded;

    public Mesh NoteMesh { get; private set; }
    public string CurrentVersion { get; private set; }

    public GlobalSettings GlobalSettings;

    public static readonly GlobalSettings DefaultGlobalSettings = new GlobalSettings(0d, 1f, 1f, new GameSettings(1d, 1d), new EditorSettings(1d, 1d));
    private void Awake()
    {
        if (GameInstance != null)
        {
            Destroy(popupCanvas.gameObject);
            Destroy(gameObject);
            return;
        }
        else
        {
            GameInstance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(popupCanvas.gameObject);
        }

        InputActions = new();
        InputActions.Enable();
    }

    private void Start()
    {
        Application.targetFrameRate = -1;
        CurrentVersion = "1.0.0";
        if (SaveLoadManager.LoadGlobalSettingsFromFile(out GlobalSettings))
        {
            InvokeInformationDisplayNeeded("No global settings found");
        }
        else
        {
            InvokeInformationDisplayNeeded("Loaded settings");
        }
    }

    private void OnApplicationQuit()
    {
        SaveLoadManager.SaveGlobalSettingsToFile(GlobalSettings);
    }

    private void Update()
    {
        MousePosition = InputActions.Gameplay.MousePosition.ReadValue<Vector2>();
    }

    public void InvokeConfirmActionNeeded(ConfirmAction action)
    {
        OnConfirmActionNeeded?.Invoke(action);
    }

    public void InvokeInformationDisplayNeeded(string infoMessage)
    {
        OnInformationDisplayNeeded?.Invoke(infoMessage);

    }

    public void RequestPlayChartEvent(string path)
    {
        Debug.Log($"Requested to play {path}");

        SceneLoader.LoadSceneAtIndex(2, () => GameplayManager.GameplayInstance.InvokeGameplayStartedEvent(path));
    }
}

[Serializable]
public struct GlobalSettings
{
    public double AudioOffsetMs;
    public float SongVolume;
    public float HitsoundVolume;

    public GameSettings GameSettings;
    public EditorSettings EditorSettings;

    // we are going to trust that the settings file has valid inputs. Lol
    public GlobalSettings(double audioOffsetMs, float songVolume, float hitsoundVolume, GameSettings gameSettings, EditorSettings editorSettings)
    {
        AudioOffsetMs = audioOffsetMs;
        SongVolume = songVolume;
        HitsoundVolume = hitsoundVolume;
        GameSettings = gameSettings;
        EditorSettings = editorSettings;
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