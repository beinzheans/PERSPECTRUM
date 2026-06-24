using UnityEngine;

/// <summary>
/// A class to handle a 4/4 sig. metronome pulses during gameplay, accounting for BPM changes defined by <see cref="GameplayMarker"/>. <br></br>
/// This class will precompute the times when the metronome will fire at the beginning of the gameplay.
/// </summary>
public class GameplayMetronomeManager : MonoBehaviour
{
    private GameplayManager gameplayManager;

    private GameplayMarker initialMarker;
    private GameplayMarker currentMarkerInGameplay;
    int previousSearchIndex = 0;

    TimerIntervalAction metronomeTimer;
    private double currentBPM;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;

    }

    private void GameplayManager_OnGameplayEnded()
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(metronomeTimer);
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        gameplayManager.IsMetronomeDisabled = false;
        previousSearchIndex = 0;
    }

    private bool FindInitialMarker()
    {
        if (gameplayManager.CurrentGameplayChart == null)
        {
            return false;
        }

        bool findResult = false;


        for (int i = 0; i < gameplayManager.CurrentGameplayChart.GameplayObjects.Length; i++)
        {
            GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[i];

            if (gameplayObject is GameplayMarker marker)
            {
                initialMarker = marker;
                findResult = true;
                previousSearchIndex = i;
                break;
            }
        }

        return findResult;
    }

    private void GameplayManager_OnGameplayStarted()
    {
        if (!FindInitialMarker())
        {
            gameplayManager.IsMetronomeDisabled = true;
            Debug.LogWarning($"No initial marker found! Metronome will not be enabled.");
            return;
        }

        metronomeTimer = new TimerIntervalAction(this, (x) => gameplayManager.InvokeGameplayMetronomeFired(gameplayManager.CurrentGameplayTime), () => { }, initialMarker.RenderTime + GameplayManager.k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, 60d / initialMarker.BPM);
        DSPTimerEngine.TimerInstance.AddActionToTimer(metronomeTimer);
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        AssignCurrentMarkerAndUpdate(time);
    }

    private void AssignCurrentMarkerAndUpdate(double time)
    {
        bool assigned = false;
        for (int i = previousSearchIndex; i < gameplayManager.CurrentGameplayChart.GameplayObjects.Length; i++)
        {
            GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[i];

            if (gameplayObject.RenderTime > time) // do not search anymore
            {
                previousSearchIndex = i;
                break;
            }

            if (gameplayObject is not GameplayMarker marker)
            {
                continue;
            }

            if (currentMarkerInGameplay == marker) // do not assign if we somehow search the same marker. This shouldn't happen since we set the lower bounds
            {
                continue;
            }

            assigned = true;
            currentMarkerInGameplay = marker;
        }

        if (!assigned) // nothing to update
        {
            return;
        }

        if (currentMarkerInGameplay == null)
        {
            gameplayManager.IsMetronomeDisabled = true;
            return;
        }

        currentBPM = currentMarkerInGameplay.BPM;
        UpdateMetronomeTimer();
        gameplayManager.InvokeGameplayMarkerUpdate(currentMarkerInGameplay);
    }

    private void UpdateMetronomeTimer()
    {
        if (MathHelper.IsTwoDoublesEqualWithEpsilion(currentBPM, 0d))
        {
            gameplayManager.IsMetronomeDisabled = true;
            return;
        }

        gameplayManager.IsMetronomeDisabled = false;
        double intervalTime = 60d / currentBPM;

        metronomeTimer.EditIntervalTime(intervalTime, true);
    }
}
