using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A class to handle the FPS display. <br></br>
/// Uses running average to compute the FPS that will be displayed.
/// </summary>
public class GameFPSDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int numberOfSamples;
    [SerializeField] private CanvasGroup fpsDisplayCanvasGroup;

    [SerializeField] private TMP_Text fpsDisplayText;

    [SerializeField] private float updateDisplayTimeInterval;
    private int numberOfSamples_internal; // we do this so any modifications during gameplay will not affect the FPS. we strictly require that number of samples to be fixed.
    private float[] sampledDeltaTimes;
    private int sampleIndex;

    private float calculatedFps = 0f;

    private float displayTimeInterval_internal;
    private void Awake()
    {
        numberOfSamples_internal = Mathf.Max(1, numberOfSamples);
        sampledDeltaTimes = new float[numberOfSamples_internal];
    }

    private void Start()
    {
        displayTimeInterval_internal = updateDisplayTimeInterval;
        fadeOutTimer = new TimerStopwatchAction(this, x => fpsDisplayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, (float)(x / k_FPSDISPLAYFADETIMER)), () => { }, 0d, k_FPSDISPLAYFADETIMER, false);
        fadeInTimer = new TimerStopwatchAction(this, x => fpsDisplayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, (float)(x / k_FPSDISPLAYFADETIMER)), () => { }, 0d, k_FPSDISPLAYFADETIMER, false);

        CheckFPSDisplayState();
        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
    }

    private void CheckFPSDisplayState()
    {
        if (GameManager.GameInstance.GlobalSettings.ShowFPSCounter)
        {
            ShowFPSDisplay();
        }
        else
        {
            HideFPSDisplay();
        }
    }

    private void OnDestroy()
    {
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        CheckFPSDisplayState();
    }

    private void Update()
    {
        sampledDeltaTimes[sampleIndex] = Time.unscaledDeltaTime;

        sampleIndex = (sampleIndex + 1) % numberOfSamples_internal;

        if (!GameManager.GameInstance.GlobalSettings.ShowFPSCounter)
        {
            // we exit at this stage only, since we still want to sample even if we don't want the FPS counter to be shown. That way if we enable it the buffer will be filled
            return;
        }

        displayTimeInterval_internal -= Time.unscaledDeltaTime;

        if (displayTimeInterval_internal < 0f || MathHelper.IsTwoFloatsEqualWithEpsilion(displayTimeInterval_internal, 0f))
        {
            CalculateFPSFromSamples();
            UpdateFPSDisplay();
            displayTimeInterval_internal += updateDisplayTimeInterval;
        }
    }

    private void CalculateFPSFromSamples()
    {
        float totalTime = 0f;

        for (int i = 0; i < numberOfSamples_internal; i++)
        {
            totalTime += sampledDeltaTimes[i];
        }

        calculatedFps = MathHelper.IsTwoFloatsEqualWithEpsilion(totalTime, 0f) ? 0f : numberOfSamples_internal / totalTime; // set to zero FPS if the total time is somehow 0
    }

    private void UpdateFPSDisplay()
    {
        fpsDisplayText.SetText($"{calculatedFps:F1} FPS");
    }

    private const float k_FPSDISPLAYFADETIMER = 0.25f;
    private TimerStopwatchAction fadeOutTimer;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!GameManager.GameInstance.GlobalSettings.ShowFPSCounter)
        {
            return;
        }

        fadeOutTimer.ResetTimer();
        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeOutTimer);
    }

    private TimerStopwatchAction fadeInTimer;
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!GameManager.GameInstance.GlobalSettings.ShowFPSCounter)
        {
            return;
        }

        fadeInTimer.ResetTimer();
        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeInTimer);
    }

    private void ShowFPSDisplay()
    {
        fpsDisplayCanvasGroup.alpha = 1f;
    }

    private void HideFPSDisplay()
    {
        fpsDisplayCanvasGroup.alpha = 0f;
    }
}
