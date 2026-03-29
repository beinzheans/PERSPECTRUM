using System.Collections.Generic;
using UnityEngine;

public class EditorHitboxPlaceDeleteManager : MonoBehaviour
{
    private EditorManager editorInstance;

    private PlayerInputActions inputActions;

    private void Start()
    {
        editorInstance = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;

        inputActions.Editor.PlaceHitboxA.performed += PlaceHitboxA_performed;
        inputActions.Editor.PlaceHitboxB.performed += PlaceHitboxB_performed;

        inputActions.Editor.DeleteHitbox.performed += DeleteHitbox_performed;
    }

    private void DeleteHitbox_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        editorInstance.DeleteSelectedEditorHitboxList();
    }

    private void PlaceHitboxB_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        PlaceHitbox(HitboxType.B);
    }

    private void PlaceHitboxA_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        PlaceHitbox(HitboxType.A);
    }

    private void PlaceHitbox(HitboxType type)
    {
        EditorHitbox h = new(editorInstance.EditorMousePosition, 100, type, editorInstance.EditorPreviewTime);
        editorInstance.PlaceEditorHitbox(h);
    }
}
