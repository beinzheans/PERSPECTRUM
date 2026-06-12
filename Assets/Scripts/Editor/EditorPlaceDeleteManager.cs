using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorPlaceDeleteManager : EditorUIBehavior
{
    private ObjectPlaceDeleteType currentEditorSelectedType = ObjectPlaceDeleteType.HitboxA;

    private PlayerInputActions inputActions;
    private EditorManager editorManager;

    protected override void Start()
    {
        base.Start();
        editorManager = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;
        UI_OnButtonPress((int)ObjectPlaceDeleteType.HitboxA);
        inputActions.Editor.PlaceEditorObject.performed += PlaceEditorObject_performed;
        inputActions.Editor.DeleteSelectedEditorObject.performed += DeleteSelectedEditorObject_performed;
    }

    private void OnDestroy()
    {
        inputActions.Editor.PlaceEditorObject.performed -= PlaceEditorObject_performed;
        inputActions.Editor.DeleteSelectedEditorObject.performed -= DeleteSelectedEditorObject_performed;
    }

    private void DeleteSelectedEditorObject_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        List<EditorDynamicObject> selectedObjs = new List<EditorDynamicObject>(editorManager.CurrentSelectedRenderables);

        Action deleteAction = () =>
        {
            for (int i = 0; i < selectedObjs.Count; i++)
            {
                selectedObjs[i].OnDeselect();
                selectedObjs[i].OnDelete();
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < selectedObjs.Count; i++)
            {
                selectedObjs[i].OnPlace();
                selectedObjs[i].OnSelect();
            }
        };

        EditorCommand command = new EditorCommand(deleteAction, undoAction);
        editorManager.ExecuteEditorCommand(command);
    }

    private void PlaceEditorObject_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (editorManager.EditorMousePosition.x < 0f || editorManager.EditorMousePosition.x > 1f || editorManager.EditorMousePosition.y < 0f || editorManager.EditorMousePosition.y > 1f)
        {
            return;
        }

        List<EditorDynamicObject> pd = new();

        switch (currentEditorSelectedType)
        {
            case ObjectPlaceDeleteType.HitboxA:
                pd.Add(new EditorHitbox(editorManager.EditorMousePosition, editorManager.EditorPlaceDeleteSize, HitboxType.A, editorManager.EditorPreviewTime));
                break;
            case ObjectPlaceDeleteType.HitboxB:
                pd.Add(new EditorHitbox(editorManager.EditorMousePosition, editorManager.EditorPlaceDeleteSize, HitboxType.B, editorManager.EditorPreviewTime));
                break;
            case ObjectPlaceDeleteType.HitboxBomb:
                pd.Add(new EditorHitbox(editorManager.EditorMousePosition, editorManager.EditorPlaceDeleteSize, HitboxType.BOMB, editorManager.EditorPreviewTime));
                break;
            case ObjectPlaceDeleteType.Point:
                pd.Add(new EditorPoint(editorManager.EditorMousePosition, editorManager.EditorPreviewTime));
                break;
            case ObjectPlaceDeleteType.Line:
                int i = 0;
                while (i < editorManager.CurrentSelectedRenderables.Count)
                {
                    ISelectable fromSelectable = editorManager.CurrentSelectedRenderables[i];

                    EditorDynamicObject fromPoint = fromSelectable as EditorDynamicObject;

                    if (!fromPoint.GetPosition(out Vector2 fromPosition))
                    {
                        i++;
                        continue;
                    }

                    bool foundToPoint = false;

                    for (int j = i + 1; j < editorManager.CurrentSelectedRenderables.Count; j++)
                    {
                        ISelectable toSelectable = editorManager.CurrentSelectedRenderables[j];

                        EditorDynamicObject toPoint = toSelectable as EditorDynamicObject;

                        if (!toPoint.GetPosition(out Vector2 toPosition))
                        {
                            continue;
                        }

                        foundToPoint = true;
                        EditorLine line = new EditorLine(fromPosition, toPosition, fromPoint.RenderTime, toPoint.RenderTime);
                        pd.Add(line);
                        i = j;
                        break;
                    }

                    if (foundToPoint)
                    {
                        continue;
                    }

                    break; // can not find toPoint, so no lines can be created.
                }

                break;
            case ObjectPlaceDeleteType.TimelineMarker:
                TimelineMarker marker = new(editorManager.EditorPreviewTime);
                marker.OnPlace(); // we let timeline marker tool handle the place logic!
                return; // don't make an command for this case
            default:
                break;
        }


        Action placeAction = () =>
        {
            for (int i = 0; i < pd.Count; i++)
            {
                pd[i].OnPlace();
            }
        };

        Action deleteAction = () =>
        {
            for (int i = 0; i < pd.Count; i++)
            {
                pd[i].OnDelete();
            }
        };


        EditorCommand placeCommand = new EditorCommand(placeAction, deleteAction);
        editorManager.ExecuteEditorCommand(placeCommand);
    }

    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)ObjectPlaceDeleteType.HitboxA || index > (int)ObjectPlaceDeleteType.TimelineMarker)
        {
            return;
        }

        buttons[(int)currentEditorSelectedType].image.color = Color.white;
        buttons[index].image.color = Color.yellow;
        currentEditorSelectedType = (ObjectPlaceDeleteType)index;
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Changed Object");
        editorManager.InvokeEditorObjectTypeChanged(currentEditorSelectedType);
    }
}

public enum ObjectPlaceDeleteType
{
    HitboxA,
    HitboxB,
    HitboxBomb,
    Point,
    Line,
    TimelineMarker
}
