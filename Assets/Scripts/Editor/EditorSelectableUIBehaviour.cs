using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class EditorSelectableUIBehaviour<T> : EditorRenderableUIBehavior<T>, ICanvasRaycastFilter, IPointerClickHandler where T : EditorDynamicObject
{
    protected T currentAssociatedSelectable;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void AssignAssociatedRenderable(T renderable)
    {
        base.AssignAssociatedRenderable(renderable);
        currentAssociatedSelectable = renderable;
    }

    public override void UnassignAssociatedRenderable()
    {
        base.UnassignAssociatedRenderable();
    }


    public abstract bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Action selectAction = () => { currentAssociatedRenderable.OnSelect(); };
        Action deselectAction = () => { currentAssociatedRenderable.OnDeselect(); };

        EditorCommand command;
        if (!currentAssociatedSelectable.IsSelected)
        {
            command = new EditorCommand(selectAction, deselectAction);
        }
        else
        {
            command = new EditorCommand(deselectAction, selectAction);
        }

        EditorManager.EditorInstance.ExecuteEditorCommand(command);
    }
}
