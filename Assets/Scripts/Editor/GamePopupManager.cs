using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to handle the game popup UI for confirmation messages or other info panels.
/// </summary>
public class GamePopupManager : MonoBehaviour
{
    private ConfirmAction currentConfirmAction;

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

    private void Start()
    {
        HidePanel();
        gameInstance = GameManager.GameInstance;

        gameInstance.OnConfirmActionNeeded += EditorInstance_OnConfirmActionNeeded;
        gameInstance.OnInformationDisplayNeeded += EditorInstance_OnInformationDisplayNeeded;
    }

    private const string k_AnimationString = "Popup";

    private void EditorInstance_OnInformationDisplayNeeded(string obj)
    {
        infoMessageText.text = obj;
        infoPopupPanelAnimation.SetTrigger(k_AnimationString);
    }

    private void EditorInstance_OnConfirmActionNeeded(ConfirmAction obj)
    {
        currentConfirmAction = obj;
        popupDescriptionText.text = obj.MessageToDisplay;

        ShowPanel();
    }

    public void UI_OnConfirmButtonPressed()
    {
        if (currentConfirmAction == null)
        {
            return;
        }

        currentConfirmAction.ExecuteAction();
        gameInstance.InvokeInformationDisplayNeeded("Confirmed Action");
        HidePanel();
    }

    public void UI_OnDenyButtonPressed()
    {
        if (currentConfirmAction == null)
        {
            return;
        }

        currentConfirmAction = null;
        gameInstance.InvokeInformationDisplayNeeded("Cancelled Action");
        HidePanel();
    }
    private void HidePanel()
    {
        popupPanel.SetActive(false);
    }

    private void ShowPanel()
    {
        popupPanel.SetActive(true);
    }
}

/// <summary>
/// A class to represent an action to be done only when confirmed.
/// </summary>
public class ConfirmAction
{
    public Action actionToExecute;
    public string MessageToDisplay;

    public ConfirmAction(Action actionToExecute, string messageToDisplay)
    {
        this.actionToExecute = actionToExecute;
        MessageToDisplay = messageToDisplay;
    }

    public void ExecuteAction()
    {
        actionToExecute?.Invoke();
    }
}
