using UnityEngine;

/// <summary>
/// A class to open the escape menu for the Editor
/// </summary>
public class EditorPauseManager : MonoBehaviour
{
    private EditorManager editorManager;
    private PlayerInputActions inputActions;
    private bool IsInPauseMenu = false;


    private void Start()
    {
        editorManager = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;

        inputActions.Gameplay.EscapeMenuInput.performed += EscapeMenuInput_performed;
    }

    private void EscapeMenuInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        IsInPauseMenu = !IsInPauseMenu;
    }
}
