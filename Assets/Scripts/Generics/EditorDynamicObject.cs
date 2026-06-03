using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A generic class representing an editor object that is interactable.
/// </summary>
public class EditorDynamicObject : EditorObject, IPlaceDeleteable, IEditable
{
    protected EditorDynamicObject(double renderTime) : base(renderTime)
    {
        RenderTime = renderTime;
    }

    [JsonIgnore]
    public bool IsSelected { get; protected set; }

    public virtual void OnDelete()
    {
        EditorManager.EditorInstance.InvokeDeleteEditorObjectEvent(this);
    }

    public virtual void OnPlace()
    {
        EditorManager.EditorInstance.InvokePlaceEditorObjectEvent(this);
    }

    public virtual void OnSelect()
    {
        if (IsSelected)
        {
            return;
        }

        IsSelected = true;
        EditorManager.EditorInstance.InvokeSelectSelectableEvent(this);
    }

    public virtual void OnDeselect()
    {
        if (!IsSelected)
        {
            return;
        }

        IsSelected = false;
        EditorManager.EditorInstance.InvokeDeselectSelectableEvent(this);
    }
    public override EditorObject GetCopy()
    {
        return new EditorDynamicObject(RenderTime);
    }

    public virtual void Mirror(MirrorAxis axis)
    {
        return;
    }

    public void OnEdit<TClass, TValue>(Expression<Func<TClass, TValue>> editAction, TValue newValue)
    {
        if (editAction.Body is not MemberExpression expression)
        {
            return;
        }

        if (expression.Member is not PropertyInfo property)
        {
            return;
        }

        property.SetValue(this, newValue);
        EditorManager.EditorInstance.InvokeEditEditableEvent(this);
    }

    /// <summary>
    /// Gets the position of the editor object if possible. Returns false if can not define what a position means for this object.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public virtual bool GetPosition(out Vector2 position)
    {
        position = Vector2.zero;
        return false;
    }
}
