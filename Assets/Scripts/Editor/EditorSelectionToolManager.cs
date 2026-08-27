using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorSelectionToolManager : EditorUIBehavior
{
    [SerializeField] private MoveSelectedMode moveMode;
    private EditorManager editorInstance;
    private PlayerInputActions inputActions;

    private bool startRecordingMouseDelta;
    private Vector2 initialNormalizedMousePosition;

    private List<Vector2> initialSelectedObjectPositions = new List<Vector2>();
    protected override void Start()
    {
        base.Start();
        editorInstance = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;
        moveMode = MoveSelectedMode.None;
        inputActions.Editor.MoveSelectedObjects_Special.performed += MoveSelectedObjects_Special_performed;
        inputActions.Editor.MoveSelectedObjects_MouseDelta.performed += MoveSelectedObjects_MouseDelta_performed;
        inputActions.Editor.MoveSelectedObjects_MouseDelta.canceled += MoveSelectedObjects_MouseDelta_canceled;
    }

    private void MoveSelectedObjects_MouseDelta_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        startRecordingMouseDelta = false;

        List<EditorDynamicObject> selectedObjects = new List<EditorDynamicObject>(editorInstance.CurrentSelectedRenderables);

        List<Vector2> finalSelectedObjectPositions = selectedObjects.Select(x => { x.GetPosition(out Vector2 position); return position; }).ToList();
        List<Vector2> initialSelectedObjectPositions = new List<Vector2>(this.initialSelectedObjectPositions);
        Action executeCommand = () =>
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                bool result = selectedObjects[i].GetPosition(out Vector2 position);
                if (!result)
                {
                    Debug.LogWarning($"Selected object has no implementation for getting position, yet there was attempt to move by mouse delta.");
                    continue;
                }

                selectedObjects[i].Move_Delta(finalSelectedObjectPositions[i] - position);
            }
        };

        Action undoCommand = () =>
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                bool result = selectedObjects[i].GetPosition(out Vector2 position);
                if (!result)
                {
                    Debug.LogWarning($"Selected object has no implementation for getting position, yet there was attempt to move by mouse delta.");
                    continue;
                }

                selectedObjects[i].Move_Delta(initialSelectedObjectPositions[i] - position);
            }
        };


        EditorCommand command = new EditorCommand(executeCommand, undoCommand);
        editorInstance.ExecuteEditorCommand(command);
    }

    private void MoveSelectedObjects_MouseDelta_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        startRecordingMouseDelta = true;
        initialNormalizedMousePosition = editorInstance.EditorMousePosition;

        initialSelectedObjectPositions = editorInstance.CurrentSelectedRenderables.Select(x => { x.GetPosition(out Vector2 position); return position; }).ToList();
}

    private void Update()
    {
        if (!startRecordingMouseDelta)
        {
            return;
        }

        Vector2 delta = editorInstance.EditorMousePosition - initialNormalizedMousePosition;

        if (MathHelper.IsTwoFloatsEqualWithEpsilion(delta.magnitude, 0f))
        {
            return;
        }

        for (int i = 0; i < editorInstance.CurrentSelectedRenderables.Count; i++)
        {
            editorInstance.CurrentSelectedRenderables[i].Move_Delta(delta);
        }

        initialNormalizedMousePosition = editorInstance.EditorMousePosition;
    }

    private void OnDestroy()
    {
        inputActions.Editor.MoveSelectedObjects_Special.performed -= MoveSelectedObjects_Special_performed;
    }

    private void MoveSelectedObjects_Special_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

        if (moveMode == MoveSelectedMode.None)
        {
            return;
        }

        List<EditorDynamicObject> selected = editorInstance.CurrentSelectedRenderables;

        if (selected.Count <= 0)
        {
            return;
        }

        MoveSelectedMode storedMoveMode = moveMode;

        MoveSelectedMode undoMoveMode = GetUndoOfMoveMode(storedMoveMode);

        // by default, we apply mirroring then rotation.

        Action moveAction = () =>
        {
            GetTimeValuesFromSelectedList(in selected, out double minTime, out double averageTime, out double maxTime);
            double timeOffset = minTime - MathHelper.GetMirroredTime(maxTime, averageTime, 0d, MoveSelectedMode.Time);

            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].Move_AxesMirror(storedMoveMode);
                selected[i].Move_TimeMirror(storedMoveMode, in averageTime, in timeOffset);
                selected[i].Move_Rotate(storedMoveMode);
            }
        };

        Action undoMoveAction = () =>
        {
            GetTimeValuesFromSelectedList(in selected, out double minTime, out double averageTime, out double maxTime);
            double timeOffset = minTime - MathHelper.GetMirroredTime(maxTime, averageTime, 0d, MoveSelectedMode.Time);

            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].Move_Rotate(undoMoveMode);
                selected[i].Move_AxesMirror(undoMoveMode);
                selected[i].Move_TimeMirror(undoMoveMode, in averageTime, in timeOffset);
            }
        };

        EditorCommand mirrorCommand = new EditorCommand(moveAction, undoMoveAction);
        editorInstance.ExecuteEditorCommand(mirrorCommand);
    }

    private void GetTimeValuesFromSelectedList(in List<EditorDynamicObject> selected, out double minTime, out double averageTime, out double maxTime)
    {
        if (selected.Count <= 0)
        {
            Debug.LogWarning($"Attempted to get time values from an empty list.");
            minTime = averageTime = maxTime = 0d;
            return;
        }

        double sum = 0d;
        double minCache = double.MaxValue;
        double maxCache = double.MinValue;
        for (int i = 0; i < selected.Count; i++)
        {
            double time = selected[i].RenderTime;

            sum += time;

            if (time < minCache)
            {
                minCache = time;
            }

            if (time > maxCache)
            {
                maxCache = time;
            }
        }

        minTime = minCache;
        averageTime = sum / selected.Count;
        maxTime = maxCache;
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
            case (int)MoveSelectedMode.Time:
                moveMode ^= MoveSelectedMode.Time;
                SetButtonState(index, moveMode.HasFlag(MoveSelectedMode.Time));
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
    Time = 4,
    Rotate_90_Clockwise = 8,
    Rotate_90_Anticlockwise = 16
}
