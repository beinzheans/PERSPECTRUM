using System;
using UnityEngine;

public class GameplayRestartManager : MonoBehaviour
{
    private GameplayManager gameplayManager;

    private bool shouldBlockRestart = false;
    void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        GameManager.GameInstance.OnPauseMenuEnable += GameInstance_OnPauseMenuEnable;
        GameManager.GameInstance.InputActions.Gameplay.RestartInput.performed += RestartInput_performed;
        gameplayManager.OnGameplayWaitingForResume += GameplayManager_OnGameplayWaitingForResume;
        gameplayManager.OnGameplayResumed += GameplayManager_OnGameplayResumed;
    }

    private TimerIntervalAction stopBlockingTimer;
    private void GameplayManager_OnGameplayResumed()
    {
        stopBlockingTimer = new TimerIntervalAction(this, x => shouldBlockRestart = false, () => { }, GameplayManager.k_TIMEOFFSET, TimerBehavior.TEMPORARY,0);
        DSPTimerEngine.TimerInstance.AddActionToTimer(stopBlockingTimer);
    }

    private void GameplayManager_OnGameplayWaitingForResume()
    {
        shouldBlockRestart = true;
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(stopBlockingTimer);
    }

    private void GameInstance_OnPauseMenuEnable()
    {
        shouldBlockRestart = true;
    }

    private void RestartInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        if (shouldBlockRestart)
        {
            return;
        }

        gameplayManager.InvokeGameplayRestartEvent();
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Restarted", GameplayManager.k_TIMEOFFSET);
    }
}
