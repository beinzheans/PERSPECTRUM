using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to handle the hitbox logic during gameplay.
/// </summary>
public class GameplayLogicManager : MonoBehaviour
{
    private GameplayManager gameplayManager;
    private MouseActiveType currentActiveMouseType = MouseActiveType.NONE;
    List<VisualHitbox> currentActiveHitboxes = new();
    private int currentObjectIndex = 0;
    private PlayerInputActions inputActions;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        inputActions = GameManager.GameInstance.InputActions;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;

        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        inputActions.Gameplay.SwitchAInput.performed += SwitchAInput_performed;
        inputActions.Gameplay.SwitchAInput.canceled += SwitchAInput_canceled;

        inputActions.Gameplay.SwitchBInput.performed += SwitchBInput_performed;
        inputActions.Gameplay.SwitchBInput.canceled += SwitchBInput_canceled;


    }

    private void GameplayManager_OnGameplayRestarted()
    {
        currentActiveHitboxes = new();
        currentObjectIndex = 0;
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayTimeUpdated -= GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayRestarted -= GameplayManager_OnGameplayRestarted;

        if (gameplayManager.IsInReplayMode)
        {
            return;
        }
        inputActions.Gameplay.SwitchAInput.performed -= SwitchAInput_performed;
        inputActions.Gameplay.SwitchAInput.canceled -= SwitchAInput_canceled;

        inputActions.Gameplay.SwitchBInput.performed -= SwitchBInput_performed;
        inputActions.Gameplay.SwitchBInput.canceled -= SwitchBInput_canceled;


    }
    private void SwitchBInput_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        currentActiveMouseType = inputActions.Gameplay.SwitchAInput.IsPressed() ? MouseActiveType.A : MouseActiveType.NONE;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void SwitchBInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        currentActiveMouseType = MouseActiveType.B;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void SwitchAInput_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        currentActiveMouseType = inputActions.Gameplay.SwitchBInput.IsPressed() ? MouseActiveType.B : MouseActiveType.NONE;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void SwitchAInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        currentActiveMouseType = MouseActiveType.A;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        double maxInteractTime = time + GameplayManager.k_EARLYTIMEFRAME;

        if (!gameplayManager.IsInReplayMode)
        {
            currentActiveHitboxes.RemoveAll(x =>
            {
                if (x.IsPlayerMissed(time))
                {
                    if (x.HitboxType != HitboxType.BOMB)
                    {
                        gameplayManager.InvokeHitboxMissEvent(x);
                    }

                    return true;
                }
                else if (x.RenderTime > maxInteractTime)
                {
                    return false;
                }
                else if (x.IsMousePositionSuccessfullyInside())
                {
                    if (x.HitboxType == HitboxType.BOMB)
                    {
                        gameplayManager.InvokeHitboxBombHitEvent(x);
                    }
                    else if (MathHelper.IsMouseActiveTypeCorrect(x.HitboxType, gameplayManager.MouseActiveType))
                    {
                        gameplayManager.InvokeHitboxMatchHitEvent(x);
                    }
                    else if (x.RenderTime <= time) // only make it a "mismatch" after the early buffer.
                    {
                        gameplayManager.InvokeHitboxMismatchHitEvent(x);
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        UpdateCurrentActiveHitboxList(time);
    }
    private void UpdateCurrentActiveHitboxList(double time)
    {
        double maxInteractTime = time + GameplayManager.k_EARLYTIMEFRAME + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d; // add on top of offset so predictive hitsounds will work

        while (true)
        {
            if (currentObjectIndex >= gameplayManager.CurrentGameplayChart.GameplayObjects.Length)
            {
                break;
            }

            GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[currentObjectIndex];

            if (gameplayObject.RenderTime > maxInteractTime)
            {
                break;
            }

            if (gameplayObject is VisualHitbox hitbox)
            {
                if (hitbox.RenderTime <= maxInteractTime)
                {
                    gameplayManager.InvokeHitboxActiveEvent(hitbox);
                    currentActiveHitboxes.Add(hitbox);
                }
            }

            currentObjectIndex++;
        }
    }
}
public enum MouseActiveType
{
    NONE = 0,
    A = 1,
    B = 2,
}