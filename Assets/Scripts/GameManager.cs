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

    public static readonly Vector2 aspectRatioConversionScale = new Vector2(0.490261239f, 0.871575537f);

    public static GameManager GameInstance;
    public Vector2 MousePosition { get; private set; }
    public PlayerInputActions InputActions { get; private set; }

    public event Action<ConfirmAction> OnConfirmActionNeeded;
    public event Action<string> OnInformationDisplayNeeded;

    public string CurrentVersion { get; private set; }
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
        CurrentVersion = "1.0.0";

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

