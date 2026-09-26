using Steamworks;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// A class to handle pause logic <br></br>
/// Note the settings tab is generated once during start-up using <see cref="BaseDataModule"/>. That way, we don't need to make the scene messy.
/// </summary>
public class GamePauseManager : BaseUIPopupContent
{
    [SerializeField] private Button ReturnMainMenuButton;
    [SerializeField] private Button ContinueGameButton;

    [SerializeField] private TMP_Text PauseDescriptionText;

    private GameManager gameManager;
    private bool isInPauseMenu;
    private bool originalMouseStatus;

    public const string k_PAUSEMENUDEFAULTDESCRIPTION = "Hover over a setting to see it's description!\n" +
                                                        "There may be more settings if you scroll down.";
    public const string k_PAUSEMENUNODESCRIPTIONPROVIDED = "No description provided.";

    private Callback<GameOverlayActivated_t> STEAM_gameOverlapCallback;

    private bool isBlockPauseMenu;
    protected override void Start()
    {
        base.Start();
        gameManager = GameManager.GameInstance;
        isInPauseMenu = false;
        PauseDescriptionText.text = k_PAUSEMENUDEFAULTDESCRIPTION;
        gameManager.PauseCanvas.gameObject.SetActive(false);
        gameManager.InputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
        gameManager.OnPauseMenuDescriptionChanged += GameManager_OnPauseMenuDescriptionChanged;
        gameManager.OnRequestOverridePauseMenuActiveState += OverridePauseMenuState;
        gameManager.OnConfirmPanelShow += GameManager_OnConfirmPanelShow;
        gameManager.OnConfirmPanelHide += GameManager_OnConfirmPanelHide;
        returnMainMenuConfirmAction = new(() =>
        {
            gameManager.RequestOverrideGamePauseState(false);
            SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_TITLESCREENINDEX, () => Task.CompletedTask);
        }, () =>
        {
            gameManager.PauseCanvas.gameObject.SetActive(true);
        },
        "Are you sure you want to go back to the main menu?");

        SceneLoader.SceneLoaderInstance.OnSceneLoadRequestReceived += SceneLoaderInstance_OnSceneLoadRequestReceived;
        SceneLoader.SceneLoaderInstance.OnSceneLoadRequestFinished += SceneLoaderInstance_OnSceneLoadRequestFinished;
        if (!SteamManager.Initialized)
        {
            return;
        }

        STEAM_gameOverlapCallback = Callback<GameOverlayActivated_t>.Create(STEAM_GetGameOverlayActivatedState);
    }

    private void SceneLoaderInstance_OnSceneLoadRequestFinished()
    {
        isBlockPauseMenu = false;
    }

    private void SceneLoaderInstance_OnSceneLoadRequestReceived()
    {
        isBlockPauseMenu = true;
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
        isBlockPauseMenu = true;
    }

    private void GameManager_OnConfirmPanelHide()
    {
        isBlockPauseMenu = false;
    }

    private void OverridePauseMenuState(bool obj)
    {
        if (isBlockPauseMenu)
        {
            return;
        }

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        RemoveListeners();

        GameManager.GameInstance.InputActions.Gameplay.EscapeMenuInput.performed -= EscapeMenuInput_performed;
        SceneLoader.SceneLoaderInstance.OnSceneLoadRequestReceived -= SceneLoaderInstance_OnSceneLoadRequestReceived;
        SceneLoader.SceneLoaderInstance.OnSceneLoadRequestFinished -= SceneLoaderInstance_OnSceneLoadRequestFinished;

        STEAM_gameOverlapCallback?.Dispose();
        STEAM_gameOverlapCallback = null;
    }

    private void EscapeMenuInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isBlockPauseMenu)
        {
            return;
        }

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
        SwitchActiveDataModuleAtIndex(0); // force it back to first module
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
