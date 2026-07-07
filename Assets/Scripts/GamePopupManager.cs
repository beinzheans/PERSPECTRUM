using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A class to handle the game popup UI for confirmation messages or other info panels.
/// </summary>
public class GamePopupManager : MonoBehaviour
{
    private Queue<ConfirmAction> confirmActionQueue = new();

    [Header("Confirmation Popup")]
    [SerializeField] private GameObject popupPanel;

    [SerializeField] private TMP_Text popupDescriptionText;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button denyButton;

    [Header("Info Popup")]

    [SerializeField] private Animator infoPopupPanelAnimation;
    [SerializeField] private TMP_Text infoMessageText;
    [SerializeField] private double fadeOutTime;

    private GameManager gameInstance;

    private TimerIntervalAction fadeOutTimer;

    private Dictionary<string, string> informationMessageDisplay_KeyToStringValueMapping = new();

    private void Start()
    {
        HidePanel();
        gameInstance = GameManager.GameInstance;
        gameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        gameInstance.OnConfirmActionNeeded += EditorInstance_OnConfirmActionNeeded;
        gameInstance.OnInformationDisplayNeeded += EditorInstance_OnInformationDisplayNeeded;

        confirmButton.onClick.AddListener(() => UI_OnConfirmButtonPressed());
        denyButton.onClick.AddListener(() => UI_OnDenyButtonPressed());

        UpdateInformationDisplaySpecialKeyMapping();
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        UpdateInformationDisplaySpecialKeyMapping();
    }

    private const string k_HITBOX_A_KEYBIND_KEY = @"{HITBOX_A_KEY}";
    private const string k_HITBOX_B_KEYBIND_KEY = @"{HITBOX_B_KEY}";

    private void UpdateInformationDisplaySpecialKeyMapping()
    {
        informationMessageDisplay_KeyToStringValueMapping[k_HITBOX_A_KEYBIND_KEY] = gameInstance.InputActions.Gameplay.SwitchAInput.GetBindingDisplayString();
        informationMessageDisplay_KeyToStringValueMapping[k_HITBOX_B_KEYBIND_KEY] = gameInstance.InputActions.Gameplay.SwitchBInput.GetBindingDisplayString();
    }

    private string ParseInformationDisplayMessage(string message)
    {
        StringBuilder stringBuilder = new StringBuilder(message);

        foreach (var (key, val) in informationMessageDisplay_KeyToStringValueMapping)
        {
            stringBuilder.Replace(key, val);
        }

        return stringBuilder.ToString();
    }

    private const string k_FadeInAnimationString = "FadeIn";
    private const string k_FadeOutAnimationString = "FadeOut";

    private void EditorInstance_OnInformationDisplayNeeded(string obj, double time)
    {
        infoMessageText.text = ParseInformationDisplayMessage(obj);

        FadeInInformationPanel(time);
    }

    private void FadeInInformationPanel(double aliveTime)
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(fadeOutTimer);
        infoPopupPanelAnimation.SetTrigger(k_FadeInAnimationString);
        fadeOutTimer = new TimerIntervalAction(this, (x) => FadeOutInformationPanel(), () => { }, aliveTime, -1d);
        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeOutTimer);
    }

    private void FadeOutInformationPanel()
    {
        infoPopupPanelAnimation.ResetTrigger(k_FadeInAnimationString);
        infoPopupPanelAnimation.SetTrigger(k_FadeOutAnimationString);
    }
    private void EditorInstance_OnConfirmActionNeeded(ConfirmAction obj)
    {
        confirmActionQueue.Enqueue(obj);

        CheckForNextConfirmAction();
    }

    private void CheckForNextConfirmAction()
    {
        if (confirmActionQueue.Count <= 0)
        {
            HidePanel();
            return;
        }

        ConfirmAction obj = confirmActionQueue.Peek();

        popupDescriptionText.text = obj.MessageToDisplay;

        ShowPanel();
    }
    public void UI_OnConfirmButtonPressed()
    {
        ConfirmAction action = confirmActionQueue.Dequeue();
        action.ExecuteConfirmAction();
        CheckForNextConfirmAction();
    }

    public void UI_OnDenyButtonPressed()
    {
        ConfirmAction action = confirmActionQueue.Dequeue();
        action.ExecuteDenyAction();
        CheckForNextConfirmAction();
    }
    private void HidePanel()
    {
        if (!popupPanel.activeSelf)
        {
            return;
        }

        popupPanel.SetActive(false);
    }

    private void ShowPanel()
    {
        if (popupPanel.activeSelf)
        {
            return;
        }
        popupPanel.SetActive(true);
    }
}

/// <summary>
/// A class to represent an action to be done only when confirmed.
/// </summary>
public class ConfirmAction
{
    public Action actionToExecute_Confirm;
    public Action actionToExecute_Deny;
    public string MessageToDisplay;

    public ConfirmAction(Action actionToExecute, Action actionToDeny, string messageToDisplay)
    {
        actionToExecute_Confirm = actionToExecute;
        actionToExecute_Deny = actionToDeny;
        MessageToDisplay = messageToDisplay;
    }

    public void ExecuteConfirmAction()
    {
        actionToExecute_Confirm?.Invoke();
    }

    public void ExecuteDenyAction()
    {
        actionToExecute_Deny?.Invoke();
    }
}
