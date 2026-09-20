using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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
    int metronomeBeatIndex;

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

        initialMarker = gameplayManager.CurrentGameplayChart.GameplayObjects[gameplayManager.StartGameplayMarkerIndex] as GameplayMarker;
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
        double markerOffset = (initialMarker.RenderTime - gameplayManager.CurrentGameplayModifications.GameplayStartTime) / gameplayManager.CurrentGameplayModifications.GameplaySpeed;
        double offset = 0d;
        if (markerOffset < 0d) // this indicates that we start after this marker. Thus we MUST calcuate the beat index at this time based on this marker. Quite difficult..
        {
            SetMetronomeInitialIndex(out double earlyOffset);
            offset = earlyOffset / gameplayManager.CurrentGameplayModifications.GameplaySpeed;
        }
        else
        {
            metronomeBeatIndex = 0; // we start before any markers.
            offset = markerOffset;
        }

        Debug.Log($"Starting metronome at beat {metronomeBeatIndex}");
        metronomeTimer = new TimerIntervalAction(this, (x) => gameplayManager.InvokeGameplayMetronomeFired(gameplayManager.CurrentGameplayTime, metronomeBeatIndex++), () => { },
                                                 offset + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d + GameplayManager.k_STARTTIMEOFFSET,
                                                 TimerBehavior.PERSISTENT,
                                                 60d / currentBPM, 0);

        DSPTimerEngine.TimerInstance.AddActionToTimer(metronomeTimer);
    }

    /// <summary>
    /// Sets <see cref="metronomeBeatIndex"/> along with giving an offset to make sure it is synced with beat. <br></br>
    /// This <paramref name="earlyOffset"/> describes the time offset to the <see cref="metronomeBeatIndex"/> from the starting time.
    /// </summary>
    /// <param name="earlyOffset"></param>
    private void SetMetronomeInitialIndex(out double earlyOffset)
    {
        List<(double time, double BPM)> allMarkers = gameplayManager.CurrentGameplayChart.GameplayObjects.OfType<GameplayMarker>().Select(x => (x.RenderTime, x.BPM)).ToList();

        (double time, double BPM) currentMarker = (initialMarker.RenderTime, initialMarker.BPM); // this must exist.

        bool calculateResult = MathHelper.CalculateBeatIndexOfMarker(currentMarker, allMarkers, 1, out int firstBeat, out _, out double timeOfFirstBeat);

        if (!calculateResult)
        {
            Debug.LogWarning($"Could not calculate the beat index at the start time, setting the metronome index to be 0.");
            metronomeBeatIndex = 0;
            earlyOffset = 0d;
            return;
        }

        double dt = 60d / currentMarker.BPM;

        double beatOffset_accumulated = (gameplayManager.CurrentGameplayModifications.GameplayStartTime - timeOfFirstBeat) / dt;
        int beatOffset = MathHelper.CommonSenseFloor(beatOffset_accumulated);
        earlyOffset = (beatOffset - beatOffset_accumulated) * dt;
        metronomeBeatIndex = firstBeat + beatOffset;
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
