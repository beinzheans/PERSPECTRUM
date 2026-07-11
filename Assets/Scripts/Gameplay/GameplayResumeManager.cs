using UnityEngine;

/// <summary>
/// A class to handle the resume timer when unpaused during gameplay.
/// </summary>
public class GameplayResumeManager : MonoBehaviour
{
    private GameplayManager gameplayManager;

    private TimerIntervalAction resumeTimer;

    public const double k_DEFAULTTICKINTERVAL = 0.5d;
    public const int k_NUMBEROFLEADINTICKS = 4;

    private double tickInterval = k_DEFAULTTICKINTERVAL;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        resumeTimer = new TimerIntervalAction(this, x => gameplayManager.InvokeGameplayResumeTick(), () => gameplayManager.InvokeGameplayResumeTimerEnded(), 0d, k_DEFAULTTICKINTERVAL, k_NUMBEROFLEADINTICKS + 1);

        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
    }

    private void GameplayManager_OnGameplayStarted()
    {
        GameManager.GameInstance.OnPauseMenuDisable += GameInstance_OnPauseMenuDisable;
    }

    private void GameplayManager_OnGameplayEnded()
    {
        GameManager.GameInstance.OnPauseMenuDisable -= GameInstance_OnPauseMenuDisable;
    }

    private void GameInstance_OnPauseMenuDisable()
    {
        if (gameplayManager.IsInReplayMode) // no need to timer if in replay mode
        {
            return;
        }

        Debug.Log("Starting resume timer...");

        resumeTimer.ResetTimer();

        tickInterval = (gameplayManager.CurrentActiveGameplayMarker == null || MathHelper.IsTwoDoublesEqualWithEpsilion(gameplayManager.CurrentActiveGameplayMarker.BPM, 0d)) ? k_DEFAULTTICKINTERVAL : 60d / gameplayManager.CurrentActiveGameplayMarker.BPM;
        resumeTimer.EditIntervalTime(tickInterval, true);
        DSPTimerEngine.TimerInstance.AddActionToTimer(resumeTimer);

        gameplayManager.InvokeGameplayResumeTimerStarted();
    }

    private void OnDestroy()
    {
        GameManager.GameInstance.OnPauseMenuDisable -= GameInstance_OnPauseMenuDisable;
    }
}
