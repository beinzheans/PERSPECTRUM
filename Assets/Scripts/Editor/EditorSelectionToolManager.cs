using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorSelectionToolManager : EditorUIBehavior
{
    [SerializeField] private MoveSelectedMode moveMode;
    private EditorManager editorInstance;
    private PlayerInputActions inputActions;
    protected override void Start()
    {
        base.Start();
        editorInstance = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;
        moveMode = MoveSelectedMode.None;
        inputActions.Editor.MoveSelectedObjects.performed += MoveSelectedObjects_performed;
    }

    private void OnDestroy()
    {
        inputActions.Editor.MoveSelectedObjects.performed -= MoveSelectedObjects_performed;
    }

    private void MoveSelectedObjects_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (moveMode == MoveSelectedMode.None)
        {
            return;
        }

        List<EditorDynamicObject> selected = editorInstance.CurrentSelectedRenderables;
        MoveSelectedMode storedMoveMode = moveMode;

        MoveSelectedMode undoMoveMode = GetUndoOfMoveMode(storedMoveMode);

        // by default, we apply mirroring then rotation.

        Action moveAction = () =>
        {
            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].Move_Mirror(storedMoveMode);
                selected[i].Move_Rotate(storedMoveMode);
            }
        };

        Action undoMoveAction = () =>
        {
            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].Move_Rotate(undoMoveMode);
                selected[i].Move_Mirror(undoMoveMode);
            }
        };

        EditorCommand mirrorCommand = new EditorCommand(moveAction, undoMoveAction);
        editorInstance.ExecuteEditorCommand(mirrorCommand);
    }

    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)MoveSelectedMode.Horizontal || index > (int)MoveSelectedMode.Rotate_90_Anticlockwise)
        {
            return;
        }

        switch (index)
        {
            case (int)MoveSelectedMode.Horizontal:
                moveMode ^= MoveSelectedMode.Horizontal;
                SetButtonState(index, moveMode.HasFlag(MoveSelectedMode.Horizontal));
                break;

            case (int)MoveSelectedMode.Vertical:
                moveMode ^= MoveSelectedMode.Vertical;
                SetButtonState(index, moveMode.HasFlag(MoveSelectedMode.Vertical));
                break;
            case (int)MoveSelectedMode.Rotate_90_Clockwise:
                moveMode ^= MoveSelectedMode.Rotate_90_Clockwise;
                SetButtonState(index, moveMode.HasFlag(MoveSelectedMode.Rotate_90_Clockwise));
                break;
            case (int)MoveSelectedMode.Rotate_90_Anticlockwise:
                moveMode ^= MoveSelectedMode.Rotate_90_Anticlockwise;
                SetButtonState(index, moveMode.HasFlag(MoveSelectedMode.Rotate_90_Anticlockwise));
                break;
        }
    }
    /// <summary>
    /// Uses bitwise operations to get the undo version (inverse) of <paramref name="mode"/>.
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    private MoveSelectedMode GetUndoOfMoveMode(in MoveSelectedMode mode)
    {
        if (!(mode.HasFlag(MoveSelectedMode.Rotate_90_Clockwise) ^ mode.HasFlag(MoveSelectedMode.Rotate_90_Anticlockwise))) // we only care if exactly one flag is on
        {
            return mode;
        }

        return mode ^ (MoveSelectedMode.Rotate_90_Clockwise | MoveSelectedMode.Rotate_90_Anticlockwise);
    }
}


[Flags]
public enum MoveSelectedMode
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Rotate_90_Clockwise = 4,
    Rotate_90_Anticlockwise = 8
}
