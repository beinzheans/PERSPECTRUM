using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to handle pause logic <br></br>
/// Note the settings tab is generated once during start-up using <see cref="BasePauseModule"/>. That way, we don't need to make the scene messy.
/// </summary>
public class GamePauseManager : MonoBehaviour
{
    [SerializeField] private BasePauseModule[] pauseModules = new BasePauseModule[0];
    /// <summary>
    /// Prefab for the buttons above the pause menu, we spawn them dynamically (using a layout group)
    /// </summary>
    [SerializeField] private Button PauseModuleButtonPrefab;

    private Button[] pauseModuleButtons;

    [SerializeField] private Button ReturnMainMenuButton;
    [SerializeField] private TMP_Text PauseDescriptionText;

    private GameManager gameManager;
    private bool isInPauseMenu;
    private bool originalMouseStatus;

    [SerializeField] private RectTransform pauseModuleButtonRectTransform;

    public const string k_PAUSEMENUDEFAULTDESCRIPTION = "Hover over a setting to see it's description!\n" +
                                                        "There may be more settings if you scroll down.";
    public const string k_PAUSEMENUNODESCRIPTIONPROVIDED = "No description provided.";
    private void Start()
    {
        gameManager = GameManager.GameInstance;
        isInPauseMenu = false;
        PauseDescriptionText.text = k_PAUSEMENUDEFAULTDESCRIPTION;
        SetupPauseModules();
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
        gameManager.OnPauseMenuDescriptionChanged += GameManager_OnPauseMenuDescriptionChanged;


        // resubscribe to escape action
        returnMainMenuConfirmAction = new(() =>
        {
            gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
            SceneLoader.LoadSceneAtIndex(SceneLoader.k_TITLESCREENINDEX, () => { });
            isInPauseMenu = false;
            RemovePauseMenu();
        }, () =>
        {
            gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
            gameManager.PauseCanvas.gameObject.SetActive(true);
        },
        "Are you sure you want to go back to the main menu?");
    }

    private void GameManager_OnPauseMenuDescriptionChanged(string obj)
    {
        PauseDescriptionText.text = obj;
    }

    private void SetupPauseModules()
    {
        pauseModuleButtons = new Button[pauseModules.Length];
        for (int i = 0; i < pauseModules.Length; i++)
        {
            int index = i;
            pauseModuleButtons[index] = Instantiate(PauseModuleButtonPrefab, pauseModuleButtonRectTransform, false);
            pauseModuleButtons[index].GetComponentInChildren<TMP_Text>().text = pauseModules[index].ModuleName; // this will be fine, we do it once only!
            pauseModuleButtons[index].onClick.AddListener(() => OnPauseModuleButtonClicked(index));
        }
    }

    private void OnPauseModuleButtonClicked(int index)
    {
        for (int i = 0; i < pauseModules.Length; i++)
        {
            if (i == index)
            {
                pauseModules[index].InitializeModule();
                pauseModuleButtons[i].image.color = Color.yellow;
            }
            else
            {
                pauseModules[i].DeactiviateModule();
                pauseModuleButtons[i].image.color = Color.white;
            }
        }
    }
    private void OnDestroy()
    {
        RemoveListeners();
    }
    private void EscapeMenuInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

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
        originalMouseStatus = Cursor.visible;

        gameManager.PauseCanvas.gameObject.SetActive(true);

        AddListeners();
        gameManager.InvokeGamePauseMenuEnable();
        Cursor.visible = true;
    }

    private void RemovePauseMenu()
    {
        RemoveListeners();
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InvokeGamePauseMenuDisable();
        Cursor.visible = originalMouseStatus;
    }

    private ConfirmAction returnMainMenuConfirmAction;
    private void AddListeners()
    {
        ReturnMainMenuButton.onClick.AddListener(() =>
        {
            gameManager.PauseCanvas.gameObject.SetActive(false);
            gameManager.InputActions.Gameplay.EscapeMenuInput.performed -= EscapeMenuInput_performed; // we don't want the user to be able to unpause when confirm action
            GameManager.GameInstance.InvokeConfirmActionNeeded(returnMainMenuConfirmAction);
        });
    }

    private void RemoveListeners()
    {
        ReturnMainMenuButton.onClick.RemoveAllListeners();
    }
}
