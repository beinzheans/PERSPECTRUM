using System;
using System.Collections.Generic;

public class EditorHitboxToolManager : EditorToolManager
{
    private const int k_CHANGEHITBOX_A = 0;
    private const int k_CHANGEHITBOX_B = 1;
    private const int k_CHANGEHITBOX_BOMB = 2;
    private const int k_CHANGESIZE = 3;
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnPositiveNegativeInput(float input)
    {
        if (toolActiveStates[k_CHANGESIZE])
        {
            float delta = editorInstance.ScrollSensitivity_Size * input;

            ChangeEditorHitboxSize(delta);
        }
    }

    protected override void OnButtonPressedEvent(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case k_CHANGEHITBOX_A:
                ChangeEditorHitboxType(HitboxType.A);
                break;
            case k_CHANGEHITBOX_B:
                ChangeEditorHitboxType(HitboxType.B);
                break;
            case k_CHANGEHITBOX_BOMB:
                ChangeEditorHitboxType(HitboxType.BOMB);
                break;
            case k_CHANGESIZE:
                break;
            default:
                break;
        }
    }
    private void ChangeEditorHitboxType(HitboxType hitboxType)
    {
        List<EditorDynamicObject> allSelected = editorInstance.CurrentSelectedRenderables;
        List<HitboxType> originalTypes = new();
        for (int i = 0; i < allSelected.Count; i++)
        {
            if (allSelected[i] is not EditorHitbox hitbox)
            {
                continue;
            }

            originalTypes.Add(hitbox.HitboxType);
        }

        Action changeHitboxAction = () =>
        {
            for (int i = 0; i < allSelected.Count; i++)
            {
                if (allSelected[i] is not EditorHitbox hitbox)
                {
                    continue;
                }

                hitbox.OnEdit<EditorHitbox, HitboxType>(x => x.HitboxType, hitboxType);
            }
        };

        Action undoAction = () =>
        {
            int index = 0;
            for (int i = 0; i < allSelected.Count; i++)
            {
                if (allSelected[i] is not EditorHitbox hitbox)
                {
                    continue;
                }

                hitbox.OnEdit<EditorHitbox, HitboxType>(x => x.HitboxType, originalTypes[index]);
                index++;
            }
        };

        EditorCommand changeCommand = new EditorCommand(changeHitboxAction, undoAction);
        editorInstance.ExecuteEditorCommand(changeCommand);
    }

    private void ChangeEditorHitboxSize(float sizeDelta)
    {
        List<EditorDynamicObject> allSelected = editorInstance.CurrentSelectedRenderables;

        Action changeHitboxAction = () =>
        {
            for (int i = 0; i < allSelected.Count; i++)
            {
                if (allSelected[i] is not EditorHitbox hitbox)
                {
                    continue;
                }

                hitbox.OnEdit<EditorHitbox, float>(x => x.NormalizedSize, hitbox.NormalizedSize + sizeDelta);
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < allSelected.Count; i++)
            {
                if (allSelected[i] is not EditorHitbox hitbox)
                {
                    continue;
                }

                hitbox.OnEdit<EditorHitbox, float>(x => x.NormalizedSize, hitbox.NormalizedSize - sizeDelta);
            }
        };

        EditorCommand changeCommand = new EditorCommand(changeHitboxAction, undoAction);
        editorInstance.ExecuteEditorCommand(changeCommand);
    }

    protected override void CheckForToolActiveState(ObjectPlaceDeleteType obj, out bool validResult)
    {
        validResult = obj == ObjectPlaceDeleteType.HitboxA || obj == ObjectPlaceDeleteType.HitboxB || obj == ObjectPlaceDeleteType.HitboxBomb;
    }
}
