using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class to handle the logic during gameplay
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

        inputActions.Gameplay.SwitchAInput.performed += SwitchAInput_performed;
        inputActions.Gameplay.SwitchAInput.canceled += SwitchAInput_canceled;

        inputActions.Gameplay.SwitchBInput.performed += SwitchBInput_performed;
        inputActions.Gameplay.SwitchBInput.canceled += SwitchBInput_canceled;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void SwitchBInput_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentActiveMouseType = inputActions.Gameplay.SwitchAInput.IsPressed() ? MouseActiveType.A : MouseActiveType.NONE;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void SwitchBInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentActiveMouseType = MouseActiveType.B;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void SwitchAInput_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentActiveMouseType = inputActions.Gameplay.SwitchBInput.IsPressed() ? MouseActiveType.B : MouseActiveType.NONE;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void SwitchAInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentActiveMouseType = MouseActiveType.A;
        gameplayManager.InvokeMouseActiveTypeChanged(currentActiveMouseType);
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        int missCount = 0;
        int bombCount = 0;

        double maxInteractTime = time - GameplayManager.k_EARLYTIMEFRAME;

        currentActiveHitboxes.RemoveAll(x =>
        {
            if (x.IsPlayerMissed(time))
            {
                if (x.HitboxType != HitboxType.BOMB)
                {
                    missCount++;
                }

                return true;
            }
            else if (x.IsMousePositionSuccessfullyInside())
            {
                if (x.HitboxType == HitboxType.BOMB)
                {
                    bombCount++;
                }
                else if (MathHelper.IsMouseActiveTypeCorrect(x.HitboxType, currentActiveMouseType))
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

        if (missCount > 0)
        {
            gameplayManager.InvokeHitboxMissEvent(missCount);
        }

        if (bombCount > 0)
        {
            gameplayManager.InvokeHitboxBombHitEvent(bombCount);
        }

        UpdateCurrentActiveHitboxList(time);
    }

    private void UpdateCurrentActiveHitboxList(double time)
    {
        if (currentObjectIndex >= gameplayManager.CurrentGameplayChart.GameplayObjects.Length)
        {
            return;
        }

        double maxInteractTime = time + GameplayManager.k_EARLYTIMEFRAME;

        GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[currentObjectIndex];

        if (gameplayObject is not VisualHitbox hitbox)
        {
            currentObjectIndex++;
            return;
        }

        if (hitbox.RenderTime <= maxInteractTime)
        {
            currentObjectIndex++;
            currentActiveHitboxes.Add(hitbox);
        }
    }
}

public enum MouseActiveType
{
    NONE = 0,
    A = 1,
    B = 2
}