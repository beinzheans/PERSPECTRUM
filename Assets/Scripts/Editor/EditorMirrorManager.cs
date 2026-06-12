using System;
using System.Collections.Generic;
using UnityEngine;

public class EditorMirrorManager : EditorUIBehavior
{
    [SerializeField] private MirrorAxis mirrorAxis;
    private EditorManager editorInstance;
    private PlayerInputActions inputActions;
    protected override void Start()
    {
        base.Start();
        editorInstance = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;
        mirrorAxis = MirrorAxis.None;
        inputActions.Editor.MirrorObjects.performed += MirrorObjects_performed;
    }

    private void OnDestroy()
    {
        inputActions.Editor.MirrorObjects.performed -= MirrorObjects_performed;
    }
    private void MirrorObjects_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (mirrorAxis == MirrorAxis.None)
        {
            return;
        }

        List<EditorDynamicObject> selected = editorInstance.CurrentSelectedRenderables;
        MirrorAxis storedAxis = mirrorAxis;
        Action mirrorAction = () =>
        {
            for (int i = 0; i < selected.Count; i++)
            {
                selected[i].Mirror(storedAxis);
            }
        };

        EditorCommand mirrorCommand = new EditorCommand(mirrorAction, mirrorAction);
        editorInstance.ExecuteEditorCommand(mirrorCommand);
    }

    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)MirrorAxis.Horizontal || index > (int)MirrorAxis.Vertical)
        {
            return;
        }

        if (index == (int)MirrorAxis.Horizontal)
        {
            mirrorAxis ^= MirrorAxis.Horizontal;
            SetButtonState(index, mirrorAxis.HasFlag(MirrorAxis.Horizontal));
        }
        else if (index == (int)MirrorAxis.Vertical)
        {
            mirrorAxis ^= MirrorAxis.Vertical;
            SetButtonState(index, mirrorAxis.HasFlag(MirrorAxis.Vertical));
        }
    }
}

[Flags]
public enum MirrorAxis
{
    None = 0,
    Horizontal = 1,
    Vertical = 2
}
