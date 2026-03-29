using System;
using UnityEngine;
using UnityEngine.UI;

public class EditorSelectableUIBehaviour<T> : EditorRenderableUIBehavior<T> where T : ISelectable
{
    protected Button button;
    protected T currentAssociatedSelectable;

    protected override void Awake()
    {
        base.Awake();

        button = GetComponent<Button>();
    }

    public void AssignAssociatedSelectable(T h)
    {
        AssignAssociatedRenderable(h);
        button.onClick.AddListener(() => h.OnSelect());
    }

    public void UnassignCurrentAssociatedSelectable()
    {
        UnassignAssociatedRenderable();
        button.onClick.RemoveAllListeners();
    }
}
