using System;
using UnityEngine;
using UnityEngine.InputSystem;

public static class RebindHelper
{
    private static InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    public static void StartRebindAction(InputAction action, Action callback)
    {
        GameManager.GameInstance.InputActions.Disable();
        rebindingOperation = action.PerformInteractiveRebinding(0).
            OnComplete(x => 
            { 
                x.Dispose();
                GameManager.GameInstance.InputActions.Enable();
                GameManager.GameInstance.WriteCurrentInputActionAsJson();
                callback?.Invoke();
            }).
            OnCancel(x =>
            { 
                x.Dispose();
                GameManager.GameInstance.InputActions.Enable();
            });

        rebindingOperation.Start();
    }
}
