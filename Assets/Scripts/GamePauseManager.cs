using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField OffsetInputField;
    [SerializeField] private TMP_InputField GameScrollSpeed;
    [SerializeField] private TMP_InputField GameLookaheadTime;
    [SerializeField] private TMP_InputField EditorLookaheadTime;
    [SerializeField] private Slider SongVolumeSlider;
    [SerializeField] private Slider HitsoundVolumeSlider;

    [SerializeField] private Button ReturnMainMenuButton;

    private GameManager gameManager;
    private bool isInPauseMenu;

    private void Start()
    {
        gameManager = GameManager.GameInstance;
        isInPauseMenu = false;
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;

        returnMainMenuConfirmAction = new(() =>
        {
            gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
            SceneLoader.LoadSceneAtIndex(0, () => { });
            isInPauseMenu = false;
            RemovePauseMenu();
        }, () =>
        {
            gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
            gameManager.PauseCanvas.gameObject.SetActive(true);
        },
        "Are you sure you want to go back to the main menu?");
    }
    private void EscapeMenuInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isInPauseMenu = !isInPauseMenu;

        if (isInPauseMenu)
        {
            SetupPauseMenu();
        }
        else
        {
            RemovePauseMenu();
        }
    }

    private void SetupPauseMenu()
    {
        OffsetInputField.text = gameManager.GlobalSettings.AudioOffsetMs.ToString("F1");
        GameScrollSpeed.text = gameManager.GlobalSettings.GameSettings.GameScrollSpeed.ToString("F2");
        GameLookaheadTime.text = gameManager.GlobalSettings.GameSettings.GameLookaheadTime.ToString("F2");
        EditorLookaheadTime.text = gameManager.GlobalSettings.EditorSettings.EditorLookaheadTime.ToString("F2");
        SongVolumeSlider.value = gameManager.GlobalSettings.SongVolume;
        HitsoundVolumeSlider.value = gameManager.GlobalSettings.HitsoundVolume;

        gameManager.PauseCanvas.gameObject.SetActive(true);

        AddListeners();
        gameManager.InvokeGamePauseMenuEnable();
    }

    private void RemovePauseMenu()
    {
        RemoveListeners();
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InvokeGamePauseMenuDisable();
    }

    private ConfirmAction returnMainMenuConfirmAction;
    private void AddListeners()
    {
        OffsetInputField.onValueChanged.AddListener(x =>
        {
            if (double.TryParse(x, out double newOffset))
            {
                gameManager.GlobalSettings.AudioOffsetMs = newOffset;
                gameManager.InvokeGameSettingsChanged();
            }
        });

        GameScrollSpeed.onValueChanged.AddListener(x =>
        {
            if (double.TryParse(x, out double newSpeed))
            {
                gameManager.GlobalSettings.GameSettings.GameScrollSpeed = newSpeed;
                gameManager.InvokeGameSettingsChanged();
            }
        });

        GameLookaheadTime.onValueChanged.AddListener(x =>
        {
            if (double.TryParse(x, out double newLookaheadTime))
            {
                gameManager.GlobalSettings.GameSettings.GameLookaheadTime = newLookaheadTime;
                gameManager.InvokeGameSettingsChanged();
            }
        });

        EditorLookaheadTime.onValueChanged.AddListener(x =>
        {
            if (double.TryParse(x, out double newLookaheadTime))
            {
                gameManager.GlobalSettings.EditorSettings.EditorLookaheadTime = newLookaheadTime;
                gameManager.InvokeGameSettingsChanged();
            }
        });

        SongVolumeSlider.onValueChanged.AddListener(x =>
        {
            gameManager.GlobalSettings.SongVolume = x;
            gameManager.InvokeGameSettingsChanged();
        });

        HitsoundVolumeSlider.onValueChanged.AddListener(x =>
        {
            gameManager.GlobalSettings.HitsoundVolume = x;
            gameManager.InvokeGameSettingsChanged();
        });

        ReturnMainMenuButton.onClick.AddListener(() =>
        {
            gameManager.PauseCanvas.gameObject.SetActive(false);
            gameManager.InputActions.Gameplay.EscapeMenuInput.performed -= EscapeMenuInput_performed;
            GameManager.GameInstance.InvokeConfirmActionNeeded(returnMainMenuConfirmAction);
        });
    }

    private void RemoveListeners()
    {
        OffsetInputField.onValueChanged.RemoveAllListeners();
        GameScrollSpeed.onValueChanged.RemoveAllListeners();
        GameLookaheadTime.onValueChanged.RemoveAllListeners();
        EditorLookaheadTime.onValueChanged.RemoveAllListeners();
        SongVolumeSlider.onValueChanged.RemoveAllListeners();
        HitsoundVolumeSlider.onValueChanged.RemoveAllListeners();
        ReturnMainMenuButton.onClick.RemoveAllListeners();
    }
}
