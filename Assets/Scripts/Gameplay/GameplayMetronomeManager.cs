using UnityEngine;

/// <summary>
/// A class to handle metronome pulses during gameplay, accounting for BPM changes.
/// </summary>
public class GameplayMetronomeManager : MonoBehaviour
{
    private GameplayManager gameplayManager;

    private GameplayMarker initialMarker;
    private GameplayMarker currentMarker;
    int previousSearchIndex = 0;

    TimerIntervalAction metronomeTimer;
    private double currentBPM;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;

    }

    private bool FindInitialMarker()
    {
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
            Debug.LogWarning($"No initial marker found! Metronome will not be enabled.");
            return;
        }

        currentMarker = initialMarker;
        gameplayManager.InvokeGameplayMarkerUpdate(currentMarker);
        metronomeTimer = new TimerIntervalAction(this, (x) => gameplayManager.InvokeGameplayMetronomeFired(gameplayManager.CurrentGameplayTime), () => { }, initialMarker.RenderTime + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, 60d / initialMarker.BPM);
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

            if (currentMarker == marker) // do not assign if we somehow search the same marker. This shouldn't happen since we set the lower bounds
            {
                continue;
            }

            Debug.Log($"Assigned current marker @ t = {marker.RenderTime} & BPM {marker.BPM}");
            assigned = true;
            currentMarker = marker;
        }

        if (!assigned) // nothing to update
        {
            return;
        }

        if (currentMarker == null)
        {
            return;
        }

        currentBPM = currentMarker.BPM;
        UpdateMetronomeTimer();
        gameplayManager.InvokeGameplayMarkerUpdate(currentMarker);
    }

    private void UpdateMetronomeTimer()
    {
        if (currentMarker == null)
        {
            return;
        }

        if (currentBPM == 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(currentBPM, 0d))
        {
            return;
        }

        double intervalTime = 60d / currentBPM;

        metronomeTimer.EditIntervalTime(intervalTime, true);
    }


}
