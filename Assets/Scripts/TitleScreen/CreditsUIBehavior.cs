using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A script to control the credits behavior. <br></br>
/// This is generated each time when the title screen scene is loaded. using the information defined by <see cref="CreditsInfo"/> to allow for flexible addition of new credit fields. <br></br>
/// </summary>
public class CreditsUIBehavior : MonoBehaviour
{
    [SerializeField] private Canvas CreditsCanvas;

    [SerializeField] private CreditsInfo[] allCredits;

    [SerializeField] private TMP_Text textPrefab_Field;
    [SerializeField] private TMP_Text textPrefab_Names;
    [SerializeField] private RectTransform creditsContentRectTransform;
    [SerializeField] private ScrollRect creditsScrollRect;

    private TimerStopwatchAction startAutoscrollTimer;
    private void Start()
    {
        InstantiateCreditText();
    }

    private void InstantiateCreditText()
    {
        for (int i = 0; i < allCredits.Length; i++)
        {
            GameObject.Instantiate(textPrefab_Field, creditsContentRectTransform.transform, false).SetText(allCredits[i].CreditTitle);

            
            string nameText = (allCredits[i].CreditNames == null || allCredits[i].CreditNames.Length <= 0) ? "-" : string.Join("\n", allCredits[i].CreditNames);
            GameObject.Instantiate(textPrefab_Names, creditsContentRectTransform.transform, false).SetText(nameText);
        }

        creditsScrollRect.verticalNormalizedPosition = 1f;
    }

    public void UI_ScrollRectOnBeginScroll()
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(startAutoscrollTimer);
    }

    private const double k_TIMEUNTILAUTOSCROLL = 1d;
    private const float k_SCROLLSPEEDNORMALIZED = -0.025f;
    public void UI_ScrollRectOnEndScroll()
    {
        HandleAutoscroll();
    }

    private void HandleAutoscroll()
    {
        startAutoscrollTimer = new TimerStopwatchAction(this, x => creditsScrollRect.verticalNormalizedPosition += k_SCROLLSPEEDNORMALIZED * (float)x, () => { }, k_TIMEUNTILAUTOSCROLL, TimerBehavior.TEMPORARY, double.MaxValue, true);
        DSPTimerEngine.TimerInstance.AddActionToTimer(startAutoscrollTimer);
    }

    public void UI_OnCreditsButtonPressed()
    {
        CreditsCanvas.gameObject.SetActive(true);
        creditsScrollRect.verticalNormalizedPosition = 1f;
        HandleAutoscroll();
    }

    public void UI_OnCreditsExitButtonPressed()
    {
        CreditsCanvas.gameObject.SetActive(false);
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(startAutoscrollTimer);
    }
}

/// <summary>
/// A struct to define what a credit is. <br></br>
/// </summary>
/// 
[Serializable]
public struct CreditsInfo
{
    [SerializeField] private string creditTitle;
    [SerializeField] private string[] creditNames;
    public string CreditTitle { get => creditTitle; }
    public string[] CreditNames { get => creditNames; }
}
