using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A generic class to implement object-pooling based rendering for the Editor.
/// </summary>
/// <typeparam name="TRenderable">Any editor object to be rendered</typeparam>
/// <typeparam name="TBehavior">The associated UI behavior for the editor object</typeparam>
public abstract class EditorObjectPoolManager<TRenderable, TBehavior> : MonoBehaviour where TRenderable : EditorObject where TBehavior : EditorRenderableUIBehavior<TRenderable>
{
    protected EditorManager editorManager;
    [SerializeField] protected TBehavior behaviorPrefab;
    protected Queue<TBehavior> behaviorPool = new();
    protected Dictionary<TRenderable, TBehavior> activeRenderableToBehaviourMapping = new();
    [SerializeField] protected Canvas canvasParent;
    [SerializeField] protected int k_MAXOBJECTPOOLS;

    [SerializeField] protected Color[] allColors;
    protected Color[] transparentColor;
    protected List<TRenderable> allRenderables = new();
    protected List<TRenderable> allSelectedRenderables = new();
    protected void Start()
    {
        InitializePool();
        OnPoolStartedEvent();
    }

    protected void InitializePool()
    {
        editorManager = EditorManager.EditorInstance;

        InstaniateBehaviorPool();
        transparentColor = allColors.Select(x => MathHelper.GetTransparentVersionOfColor(x)).ToArray();

        editorManager.OnPreviewUpdated += EditorManager_OnPreviewUpdated;
        editorManager.OnEditorSelectedSelectable += EditorManager_OnEditorSelectedSelectable;
        editorManager.OnEditorDeselectedSelectable += EditorManager_OnEditorDeselectedSelectable;
        editorManager.OnEditorPlaceEditorObject += EditorManager_OnEditorPlaceEditorObject;
        editorManager.OnEditorDeleteEditorObject += EditorManager_OnEditorDeleteEditorObject;
        editorManager.OnEditorEditEditable += EditorManager_OnEditorEditEditable;
    }

    /// <summary>
    /// Custom implementation of custom events when the pool is fully instantiated (started)
    /// </summary>
    protected abstract void OnPoolStartedEvent();

    private void EditorManager_OnEditorEditEditable(IEditable obj)
    {
        if (obj is not TRenderable renderable)
        {
            return;
        }

        EditRenderable_EditRenderable(editorManager.EditorPreviewTime, renderable);
        EditEditorObjectEvent(renderable);
    }

    /// <summary>
    /// Implementation of custom events when an editor object is edited
    /// </summary>
    /// <param name="renderable"></param>
    /// <param name="behavior"></param>
    protected abstract void EditEditorObjectEvent(TRenderable renderable);

    private void EditorManager_OnEditorDeselectedSelectable(ISelectable obj)
    {
        if (obj is not TRenderable renderable)
        {
            return;
        }

        allSelectedRenderables.Remove(renderable);
    }

    private void EditorManager_OnEditorSelectedSelectable(ISelectable obj)
    {
        if (obj is not TRenderable renderable)
        {
            return;
        }

        allSelectedRenderables.Add(renderable);
    }

    protected void EditorManager_OnEditorDeleteEditorObject(IPlaceDeleteable obj)
    {
        if (obj is not TRenderable renderable)
        {
            return;
        }

        DeleteEditorObjectEvent(renderable);
        UnrenderRenderable_ReturnBehaviorToPool(renderable);
    }

    protected void EditorManager_OnEditorPlaceEditorObject(IPlaceDeleteable obj)
    {
        if (obj is not TRenderable renderable)
        {
            return;
        }

        PlaceEditorObjectEvent(renderable);
        RenderRenderable_GetBehaviorFromPool(EditorManager.EditorInstance.EditorPreviewTime, renderable);
    }
    /// <summary>
    /// Implementation of custom events when an object is deleted
    /// </summary>
    /// <param name="obj"></param>
    protected abstract void DeleteEditorObjectEvent(TRenderable obj);
    /// <summary>
    /// Implementation of custom events when an object is placed
    /// </summary>
    /// <param name="obj"></param>
    protected abstract void PlaceEditorObjectEvent(TRenderable obj);

    protected void AssignAllRenderableList(List<TRenderable> renderables, int offset, int count)
    {
        this.allRenderables = renderables.GetRange(offset, count);
    }
    protected void AssignAllRenderableList(List<TRenderable> renderables)
    {
        this.allRenderables = new List<TRenderable>(renderables);
    }
    private void EditorManager_OnPreviewUpdated(double time)
    {
        OnPreviewChangedEvent(time);

        for (int i = 0; i < allRenderables.Count; i++)
        {
            UpdateAllTRenderables(time, allRenderables[i]);
        }
    }

    /// <summary>
    /// Implementation of custom events when the preview time changes
    /// </summary>
    /// <param name="time"></param>
    protected abstract void OnPreviewChangedEvent(double time);

    private void UpdateAllTRenderables(double time, TRenderable renderable)
    {
        bool isRendered = activeRenderableToBehaviourMapping.ContainsKey(renderable);
        bool isInRange = ((time + editorManager.LookAheadTime) > renderable.RenderTime || MathHelper.IsTwoDoublesEqualWithEpsilion(time + editorManager.LookAheadTime, renderable.RenderTime))
                        && (time < renderable.RenderTime || MathHelper.IsTwoDoublesEqualWithEpsilion(time, renderable.RenderTime));

        if (isRendered)
        {
            if (!isInRange)
            {
                UnrenderRenderable_ReturnBehaviorToPool(renderable);
                return;
            }

            UpdateRenderedRenderable(time, renderable);
        }
        else
        {
            if (isInRange)
            {
                RenderRenderable_GetBehaviorFromPool(time, renderable);
            }
        }
    }

    /// <summary>
    /// Implementation of custom logic handling the updating the currently active renderable.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="renderable"></param>
    protected abstract void UpdateRenderedRenderable(double time, TRenderable renderable);

    /// <summary>
    /// Implementation of custom events when a renderable is first rendered.
    /// </summary>
    /// <param name="correspondingRenderable"></param>
    /// <param name="behavior"></param>
    protected abstract void OnRenderRenderableEvent(double time, TRenderable correspondingRenderable, TBehavior behavior);
    /// <summary>
    /// Implementation of custom events when a renderable is unrendered
    /// </summary>
    /// <param name="correspondingRenderable"></param>
    /// <param name="behavior"></param>
    protected abstract void OnUnrenderRenderableEvent(TRenderable correspondingRenderable, TBehavior behavior);

    private void RenderRenderable_SetBehavior(double time, TRenderable correspondingRenderable, TBehavior behavior)
    {
        behavior.AssignAssociatedRenderable(correspondingRenderable);

        activeRenderableToBehaviourMapping.Add(correspondingRenderable, behavior);

        OnRenderRenderableEvent(time, correspondingRenderable, behavior);
        correspondingRenderable.OnRender();
    }

    private void UnrenderRenderable_ResetBehavior(TRenderable correspondingRenderable, TBehavior behavior)
    {
        behavior.UnassignAssociatedRenderable();

        activeRenderableToBehaviourMapping.Remove(correspondingRenderable);
        behaviorPool.Enqueue(behavior);

        OnUnrenderRenderableEvent(correspondingRenderable, behavior);
        correspondingRenderable.OnUnrender();
    }

    private void UnrenderRenderable_ReturnBehaviorToPool(TRenderable renderable)
    {
        bool renderableMappingResult = activeRenderableToBehaviourMapping.TryGetValue(renderable, out TBehavior behavior);

        if (!renderableMappingResult)
        {
            return;
        }

        UnrenderRenderable_ResetBehavior(renderable, behavior);
    }

    private void RenderRenderable_GetBehaviorFromPool(double time, TRenderable renderable)
    {
        bool behaviorDequeueResult = behaviorPool.TryDequeue(out TBehavior behavior);

        if (!behaviorDequeueResult)
        {
            Debug.LogWarning($"Failed to dequeue behavior from pool while trying to render renderable! The limit on behaviors is {k_MAXOBJECTPOOLS}.", gameObject);
            return;
        }

        if (activeRenderableToBehaviourMapping.ContainsKey(renderable))
        {
            return;
        }

        RenderRenderable_SetBehavior(time, renderable, behavior);
    }

    private void EditRenderable_EditRenderable(double time, TRenderable renderable)
    {
        bool behaviorDequeueResult = activeRenderableToBehaviourMapping.TryGetValue(renderable, out TBehavior behavior);

        if (!behaviorDequeueResult)
        {
            return;
        }

        UnrenderRenderable_ReturnBehaviorToPool(renderable);
        RenderRenderable_GetBehaviorFromPool(time, renderable);
    }

    private void InstaniateBehaviorPool()
    {
        for (int i = 0; i < k_MAXOBJECTPOOLS; i++)
        {
            TBehavior behavior = Instantiate(behaviorPrefab, canvasParent.transform, false);
            behavior.gameObject.SetActive(false);
            behaviorPool.Enqueue(behavior);
        }
    }

    /// <summary>
    /// A function to assign color to current rendered renderables.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="renderable"></param>
    /// <param name="behavior"></param>
    /// <param name="transparentColor"></param>
    /// <param name="opaqueColor"></param>
    protected virtual void UpdateRenderableAtTime(double time, TRenderable renderable, TBehavior behavior, Color transparentColor, Color opaqueColor, Color selectedColor)
    {
        if (renderable is not EditorDynamicObject s)
        {
            float progress = (float)((editorManager.LookAheadTime - (renderable.RenderTime - time)) / editorManager.LookAheadTime);

            Color lerpedColor = Color.Lerp(transparentColor, opaqueColor, progress);
            behavior.RawImage.color = lerpedColor;
            return;
        }

        if (s.IsSelected)
        {
            if (!editorManager.CurrentSelectedRenderables.Contains(s))
            {
                Debug.Log($"Selection desync detected: list thinks the object is NOT selected but object thinks itself is selected");
            }

            behavior.RawImage.color = selectedColor;
        }
        else
        {
            if (editorManager.CurrentSelectedRenderables.Contains(s))
            {
                Debug.Log($"Selection desync detected: list thinks the object is selected but object thinks itself is NOT selected");
            }

            float progress = (float)((editorManager.LookAheadTime - (renderable.RenderTime - time)) / editorManager.LookAheadTime);

            Color lerpedColor = Color.Lerp(transparentColor, opaqueColor, progress);
            behavior.RawImage.color = lerpedColor;
        }
    }
}
