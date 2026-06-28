using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to handle the audio calibration scene. This requires <see cref="GameplayManager"/> and <see cref="GameplayPlayareaBorderManager"/> purely for the appearence of the gameplay scene. <br></br>
/// Note this class will set prescheuled hitsounds to be true and restore the original state when destroyed.
/// </summary>
[RequireComponent(typeof(GameplayManager), typeof(GameplayPlayareaBorderManager))]
public class AudioCalibrationManager : MonoBehaviour
{
    private GameplayManager gameplayManager;
    private const string k_CALIBRATIONCHARTNAME = "calibration";

    private const double k_CALIBRATIONWAITTIME = 3d * GameplayManager.k_TIMEOFFSET;

    [SerializeField] private AudioClip matchHitsound_AClip;

    [SerializeField] private TMP_Text offsetText;
    [SerializeField] private Slider offsetSlider;
    private bool predictiveHitsoundStorage; // store the user preference before modifying it

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        predictiveHitsoundStorage = GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds;
        GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds = true;
        string chartFilePath = Path.Combine(Application.streamingAssetsPath, $"{k_CALIBRATIONCHARTNAME}.{GameManager.k_FILEEXTENSION}");

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        offsetSlider.value = (float)(GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d);
        offsetSlider.onValueChanged.AddListener((x) => { GameManager.GameInstance.GlobalSettings.AudioOffsetMs = (double)(1000f * x); UpdateOffsetText(); });

        TimerIntervalAction startAction = new TimerIntervalAction(this, x => gameplayManager.InvokeGameplayStartedEvent(chartFilePath), () => { }, k_CALIBRATIONWAITTIME, -1d);
        DSPTimerEngine.TimerInstance.AddActionToTimer(startAction);

        UpdateOffsetText();
    }



    private void GameplayManager_OnGameplayStarted()
    {
        offsetSlider.interactable = false;
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Check offset now");
    }

    private void GameplayManager_OnGameplayEnded()
    {
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Adjust offset now");
        offsetSlider.interactable = true;
        TimerIntervalAction restartAction = new TimerIntervalAction(this, x => gameplayManager.InvokeGameplayRestartEvent(), () => { }, k_CALIBRATIONWAITTIME, -1d);
        DSPTimerEngine.TimerInstance.AddActionToTimer(restartAction);
    }

    private void UpdateOffsetText()
    {
        offsetText.text = $"Offset: {GameManager.GameInstance.GlobalSettings.AudioOffsetMs:F2} ms";
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded -= GameplayManager_OnGameplayEnded;
        GameManager.GameInstance.GlobalSettings.GameEvents.HasAdjustedOffset = true;
        GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds = predictiveHitsoundStorage;
    }
}
