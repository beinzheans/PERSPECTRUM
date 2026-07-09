using System.Data.Common;
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

        GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds, true);
        string chartFilePath = Path.Combine(Application.streamingAssetsPath, $"{k_CALIBRATIONCHARTNAME}.{GameManager.k_FILEEXTENSION}");

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        offsetSlider.value = (float)(GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d);
        offsetSlider.onValueChanged.AddListener((x) => { GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.AudioOffsetMs, (double)(1000f * x)); UpdateOffsetText(); });
        ShowPopupDialogBeforeStartingGameplay(chartFilePath);
        UpdateOffsetText();

        GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GameEvents.HasAdjustedOffset, true);
    }

    private void ShowPopupDialogBeforeStartingGameplay(string filePath)
    {
        if (!GameManager.GameInstance.GlobalSettings.GameEvents.HasAdjustedOffset)
        {
            // maybe in the future I can add a "dependency chain" logic for the timer. But let's do that later, this will work

            TimerIntervalAction dialog_one = new TimerIntervalAction(this, x => GameManager.GameInstance.InvokeInformationDisplayNeeded("Adjust your offset so that the note borders touch the outermost yellow border on the beat.", 5d), () => { }, 0d, -1d);
            TimerIntervalAction dialog_two = new TimerIntervalAction(this, x => GameManager.GameInstance.InvokeInformationDisplayNeeded("Use the slider below to adjust your offset. For a specific value, type it in the Settings menu.", 5d), () => { }, 6d, -1d);
            TimerIntervalAction dialog_three = new TimerIntervalAction(this, x => GameManager.GameInstance.InvokeInformationDisplayNeeded("Leave this screen using the Settings menu by pressing ESC.", 5d), () => { }, 12d, -1d);
            TimerIntervalAction startAction = new TimerIntervalAction(this, x => gameplayManager.RequestGameplayStartedEvent(filePath), () => { }, 18d, -1d);

            DSPTimerEngine.TimerInstance.AddActionToTimer(dialog_one);
            DSPTimerEngine.TimerInstance.AddActionToTimer(dialog_two);
            DSPTimerEngine.TimerInstance.AddActionToTimer(dialog_three);
            DSPTimerEngine.TimerInstance.AddActionToTimer(startAction);
        }
        else
        {
            TimerIntervalAction startAction = new TimerIntervalAction(this, x => gameplayManager.RequestGameplayStartedEvent(filePath), () => { }, k_CALIBRATIONWAITTIME, -1d);

            DSPTimerEngine.TimerInstance.AddActionToTimer(startAction);
        }
    }

    private void GameplayManager_OnGameplayStarted()
    {
        offsetSlider.interactable = false;
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Check offset now", GameplayManager.k_TIMEOFFSET);
    }

    private void GameplayManager_OnGameplayEnded()
    {
        offsetSlider.interactable = true;
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Adjust offset now", GameplayManager.k_TIMEOFFSET);
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
        GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds, predictiveHitsoundStorage);
    }
}
