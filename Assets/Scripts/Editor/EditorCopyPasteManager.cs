using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// This shit still doesn't work dumb fuck
// I suspect it's because of memory, when I create a brand new and copy paste it is fine, but when I copy and paste an already copied object, it will paste it incorrectly, just for lines?>????
// Maybe it is because defining the "render time" of the line is wrong?
public class EditorCopyPasteManager : MonoBehaviour
{
    private EditorManager editorManager;
    private PlayerInputActions inputActions;

    private List<EditorDynamicObject> currentCopy = new();
    private void Start()
    {
        editorManager = EditorManager.EditorInstance;

        inputActions = GameManager.GameInstance.InputActions;

        inputActions.Editor.CopyObjects.performed += CopyObjects_performed;
        inputActions.Editor.PasteObjects.performed += PasteObjects_performed;
        inputActions.Editor.CutObjects.performed += CutObjects_performed;
    }

    private void OnDestroy()
    {
        inputActions.Editor.CopyObjects.performed -= CopyObjects_performed;
        inputActions.Editor.PasteObjects.performed -= PasteObjects_performed;
        inputActions.Editor.CutObjects.performed -= CutObjects_performed;
    }

    private void CutObjects_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        List<EditorDynamicObject> previousCopy = currentCopy;
        List<EditorDynamicObject> tempCopy = new List<EditorDynamicObject>(editorManager.CurrentSelectedRenderables);

        Action cutCommand = () =>
        {
            currentCopy = new List<EditorDynamicObject>(tempCopy);

            for (int i = 0; i < tempCopy.Count; i++)
            {
                tempCopy[i].OnDeselect();
                tempCopy[i].OnDelete();
            }
        };

        Action undoCommand = () =>
        {
            currentCopy = previousCopy;
            for (int i = 0; i < tempCopy.Count; i++)
            {
                tempCopy[i].OnPlace();
                tempCopy[i].OnSelect();
            }
        };

        EditorCommand command = new(cutCommand, undoCommand);
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Cut");
        editorManager.ExecuteEditorCommand(command);
    }

    private void PasteObjects_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        GetCopyOfSelectedObjects(out EditorDynamicObject[] pasteObjs);
        Action pasteCommand = () =>
        {
            for (int i = 0; i < pasteObjs.Length; i++)
            {
                pasteObjs[i].OnPlace();
                pasteObjs[i].OnSelect();
            }
        };

        Action undoCommand = () =>
        {
            for (int i = 0; i < pasteObjs.Length; i++)
            {
                pasteObjs[i].OnDeselect();
                pasteObjs[i].OnDelete();
            }
        };

        EditorCommand command = new(pasteCommand, undoCommand);
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Paste");
        editorManager.ExecuteEditorCommand(command);
    }

    private void CopyObjects_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        List<EditorDynamicObject> previousCopy = currentCopy;
        List<EditorDynamicObject> tempCopy = new List<EditorDynamicObject>(editorManager.CurrentSelectedRenderables);

        Action copyAction = () =>
        {
            currentCopy = tempCopy;
            for (int i = 0; i < tempCopy.Count; i++)
            {
                tempCopy[i].OnDeselect();
            }
        };

        Action undoAction = () =>
        {
            currentCopy = previousCopy;
            for (int i = 0; i < tempCopy.Count; i++)
            {
                tempCopy[i].OnSelect();
            }
        };

        EditorCommand command = new(copyAction, undoAction);
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Copy");
        editorManager.ExecuteEditorCommand(command);
    }

    private void GetCopyOfSelectedObjects(out EditorDynamicObject[] pasteObjs)
    {
        double minTime = double.MaxValue;
        for (int i = 0; i < currentCopy.Count; i++)
        {
            minTime = math.min(minTime, currentCopy[i].RenderTime);
        }

        double timeOffset = editorManager.EditorPreviewTime - minTime;

        pasteObjs = new EditorDynamicObject[currentCopy.Count];

        for (int j = 0; j < currentCopy.Count; j++)
        {
            EditorDynamicObject copy = (EditorDynamicObject)currentCopy[j].GetCopy();
            copy.AddDeltaTime(timeOffset);
            pasteObjs[j] = copy;
        }
    }
}
