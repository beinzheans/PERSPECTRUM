using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using UnityEngine.InputSystem;
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
    [SerializeField] private Button ContinueGameButton;

    [SerializeField] private TMP_Text PauseDescriptionText;

    private GameManager gameManager;
    private bool isInPauseMenu;
    private bool originalMouseStatus;

    [SerializeField] private RectTransform pauseModuleButtonRectTransform;

    public const string k_PAUSEMENUDEFAULTDESCRIPTION = "Hover over a setting to see it's description!\n" +
                                                        "There may be more settings if you scroll down.";
    public const string k_PAUSEMENUNODESCRIPTIONPROVIDED = "No description provided.";

    private Callback<GameOverlayActivated_t> STEAM_gameOverlapCallback;
    private void Start() 
    {
        gameManager = GameManager.GameInstance;
        isInPauseMenu = false;
        PauseDescriptionText.text = k_PAUSEMENUDEFAULTDESCRIPTION;
        SetupPauseModules();
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
        gameManager.OnPauseMenuDescriptionChanged += GameManager_OnPauseMenuDescriptionChanged;
        gameManager.OnRequestOverridePauseMenuActiveState += OverridePauseMenuState;
        gameManager.OnConfirmPanelShow += GameManager_OnConfirmPanelShow;
        gameManager.OnConfirmPanelHide += GameManager_OnConfirmPanelHide;
        returnMainMenuConfirmAction = new(() =>
        {
            SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_TITLESCREENINDEX, () => Task.CompletedTask);
            gameManager.RequestOverrideGamePauseState(false);
        }, () =>
        {
            gameManager.PauseCanvas.gameObject.SetActive(true);
        },
        "Are you sure you want to go back to the main menu?");

        if (!SteamManager.Initialized)
        {
            return;
        }

        STEAM_gameOverlapCallback = Callback<GameOverlayActivated_t>.Create(STEAM_GetGameOverlayActivatedState);
    }

    private void STEAM_GetGameOverlayActivatedState(GameOverlayActivated_t state)
    {
        if (state.m_bActive == 1)
        {
            gameManager.RequestOverrideGamePauseState(true);
        }
        else
        {
            InputSystem.ResetDevice(Keyboard.current); // reset our keyboard device! The steam overlay eats the inputs which messes with the Input system
        }
    }

    // we don't want to be able to bring the pause menu when waiting for a confirm action!
    private void GameManager_OnConfirmPanelShow()
    {
        gameManager.InputActions.Gameplay.EscapeMenuInput.performed -= EscapeMenuInput_performed;
    }

    // resubscribe to action listener
    private void GameManager_OnConfirmPanelHide()
    {
        gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
    }

    private void OverridePauseMenuState(bool obj)
    {
        if (isInPauseMenu == obj)
        {
            return;
        }

        isInPauseMenu = obj;

        if (isInPauseMenu)
        {
            SetupPauseMenu();
        }
        else
        {
            RemovePauseMenu();
        }
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

        OnPauseModuleButtonClicked(0); // by default set the first module to be active first
    }

    private void OnPauseModuleButtonClicked(int index)
    {
        for (int i = 0; i < pauseModules.Length; i++)
        {
            pauseModules[i].DeactiviateModule(); // remove all listeners, since it could be stale.

            if (i == index)
            {
                pauseModules[index].InitializeModule();
                pauseModuleButtons[i].image.color = Color.yellow;
            }
            else
            {
                pauseModuleButtons[i].image.color = Color.white;
            }
        }
    }
    private void OnDestroy()
    {
        RemoveListeners();

        STEAM_gameOverlapCallback?.Dispose();
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
        originalMouseStatus = GameVirtualCursor.GameVirtualCursorInstance.MouseVisibleState;

        gameManager.PauseCanvas.gameObject.SetActive(true);

        AddListeners();
        OnPauseModuleButtonClicked(0); // force it back to first module
        gameManager.InvokeGamePauseMenuEnable();
        GameVirtualCursor.GameVirtualCursorInstance.ShowVirtualMouse();
    }

    private void RemovePauseMenu()
    {
        RemoveListeners();
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InvokeGamePauseMenuDisable();

        if (originalMouseStatus) GameVirtualCursor.GameVirtualCursorInstance.ShowVirtualMouse();
        else GameVirtualCursor.GameVirtualCursorInstance.HideVirtualMouse();
    }

    private ConfirmAction returnMainMenuConfirmAction;
    private void AddListeners()
    {
        ReturnMainMenuButton.onClick.AddListener(() =>
        {
            gameManager.PauseCanvas.gameObject.SetActive(false);
            GameManager.GameInstance.InvokeConfirmActionNeeded(returnMainMenuConfirmAction);
        });

        ContinueGameButton.onClick.AddListener(() => gameManager.RequestOverrideGamePauseState(false)); // we can call it privately, but it's best to invoke the game manager event in case other scripts need to listen!
    }

    private void RemoveListeners()
    {
        ReturnMainMenuButton.onClick.RemoveAllListeners();
        ContinueGameButton.onClick.RemoveAllListeners();
    }

    
}
