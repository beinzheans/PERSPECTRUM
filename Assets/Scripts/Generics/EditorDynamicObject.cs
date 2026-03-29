using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;

public abstract class EditorDynamicObject<TObject> : IEditable<TObject>, IPlaceDeleteable<TObject>
{
    protected EditorDynamicObject(double renderTime)
    {
        RenderTime = renderTime;
    }

    public double RenderTime { get; protected set; }
    public bool IsSelected { get; protected set; }

    public virtual void OnDelete(ref List<TObject> listToEdit)
    {
        if (this is not TObject deletable)
        {
            return;
        }

        listToEdit.Remove(deletable);
    }

    public abstract void OnEdit(TObject editable);

    public virtual void OnPlace(ref List<TObject> listToEdit)
    {
        if (this is not TObject placeable)
        {
            return;
        }

        if (listToEdit.Contains(placeable))
        {
            return;
        }

        listToEdit.Add(placeable);
    }

    public abstract void OnRender();

    public virtual void OnSelect()
    {
        if (IsSelected)
        {
            EditorManager.EditorInstance.InvokeDeselectSelectableEvent(this);
            IsSelected = false;
        }
        else
        {
            EditorManager.EditorInstance.InvokeSelectSelectableEvent(this);
            IsSelected = true;
        }
    }

    public abstract void OnUnrender();
}
