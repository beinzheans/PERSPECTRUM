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

        gameplayManager.OnGameplayWaitingForResume += GameplayManager_OnGameplayWaitingForResume;
        gameplayManager.OnGameplayResumed += GameplayManager_OnGameplayResumed;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void GameplayManager_OnGameplayResumed()
    {
        if (metronomeTimer == null)
        {
            return;
        }

        if (gameplayManager.IsMetronomeDisabled)
        {
            return;
        }

        if (currentMarkerInGameplay == null && initialMarker != null)
        {
            double timeUntilFirstBeat = initialMarker.RenderTime - gameplayManager.CurrentGameplayTime;

            metronomeTimer.UnpauseTimer(timeUntilFirstBeat + GameplayManager.k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, true);
        }
        else
        {
            metronomeTimer.UnpauseTimer(GameplayManager.k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d);
        }
    }

    private void GameplayManager_OnGameplayWaitingForResume()
    {
        if (metronomeTimer == null)
        {
            return;
        }

        metronomeTimer.PauseTimer();
    }

    private void GameplayManager_OnGameplayEnded()
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(metronomeTimer);
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(metronomeTimer);
        currentMarkerInGameplay = null;
        gameplayManager.IsMetronomeDisabled = false;
        previousSearchIndex = 0;
    }

    private bool TryAssignInitialMarker()
    {
        if (gameplayManager.CurrentGameplayChart == null)
        {
            return false;
        }

        if (gameplayManager.StartGameplayMarkerIndex >= gameplayManager.CurrentGameplayChart.GameplayObjects.Length)
        {
            return false;
        }

        GameplayMarker marker = gameplayManager.CurrentGameplayChart.GameplayObjects[gameplayManager.StartGameplayMarkerIndex] as GameplayMarker;
        Debug.Log($"Accessing gameplay chart at index {gameplayManager.StartGameplayMarkerIndex} (t = {gameplayManager.CurrentGameplayChart.GameplayObjects[gameplayManager.StartGameplayMarkerIndex].RenderTime})");
        return true;
    }

    private void GameplayManager_OnGameplayStarted()
    {
        if (!TryAssignInitialMarker())
        {
            gameplayManager.IsMetronomeDisabled = true;
            Debug.LogWarning($"No initial marker found! Metronome will not be enabled.");
            return;
        }

        currentBPM = initialMarker.BPM * gameplayManager.CurrentGameplayModifications.GameplaySpeed;
        double offset = (initialMarker.RenderTime - gameplayManager.CurrentGameplayModifications.GameplayStartTime) / gameplayManager.CurrentGameplayModifications.GameplaySpeed;
        if (offset < 0d) // this indicates that we start after this marker. Thus we MUST calcuate the beat index at this time based on this marker. Quite difficult..
        {

        }
        metronomeTimer = new TimerIntervalAction(this, (x) => gameplayManager.InvokeGameplayMetronomeFired(gameplayManager.CurrentGameplayTime), () => { }, 
                                                 offset + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d + GameplayManager.k_STARTTIMEOFFSET, 
                                                 TimerBehavior.PERSISTENT, 
                                                 60d / currentBPM, 0);

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

        currentBPM = currentMarkerInGameplay.BPM * gameplayManager.CurrentGameplayModifications.GameplaySpeed;
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

        metronomeTimer.EditIntervalTime(intervalTime, false);
    }
}
