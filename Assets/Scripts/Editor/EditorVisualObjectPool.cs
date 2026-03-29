using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EditorVisualObjectPool<TRenderable, TBehavior> : MonoBehaviour where TRenderable : EditorDynamicObject<TRenderable> where TBehavior : EditorSelectableUIBehaviour<TRenderable>
{
    protected EditorManager editorManager;
    [SerializeField] protected TBehavior behaviorPrefab;
    protected Queue<TBehavior> behaviorPool = new();
    protected Dictionary<TRenderable, TBehavior> activeRenderableToBehaviourMapping = new();

    [SerializeField] protected Canvas canvasParent;
    [SerializeField] protected int k_MAXOBJECTPOOLS;

    protected List<TRenderable> allRenderables = new();
    protected List<ISelectable> allSelectedRenderables = new();
    protected virtual void Start()
    {
        editorManager = EditorManager.EditorInstance;

        InstaniateBehaviorPool();
        editorManager.OnPreviewUpdated += EditorManager_OnPreviewUpdated;
        editorManager.OnEditorSelectedSelectable += EditorManager_OnEditorSelectedSelectable;
        editorManager.OnEditorDeselectedSelectable += EditorManager_OnEditorDeselectedSelectable;
    }

    private void EditorManager_OnEditorDeselectedSelectable(ISelectable obj)
    {
        if (obj is not TRenderable t)
        {
            return;
        }

        allSelectedRenderables.Remove(t);
    }

    private void EditorManager_OnEditorSelectedSelectable(ISelectable obj)
    {
        if (obj is not TRenderable t)
        {
            return;
        }

        allSelectedRenderables.Add(t);
    }

    protected void AssignAllRenderableList(List<TRenderable> renderables)
    {
        this.allRenderables = renderables;
    }

    private void EditorManager_OnPreviewUpdated(double time)
    {
        OnPreviewChangedEvent(time);
        for (int i = 0; i < allRenderables.Count; i++)
        {
            UpdateAllTRenderables(time, allRenderables[i]);
        }
    }

    protected abstract void OnPreviewChangedEvent(double time);

    private void UpdateAllTRenderables(double time, TRenderable renderable)
    {
        bool isRendered = activeRenderableToBehaviourMapping.ContainsKey(renderable);
        bool isInRange = (time + editorManager.LookAheadTime) >= renderable.RenderTime && time <= renderable.RenderTime;
        
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
                RenderRenderable_GetBehaviorFromPool(renderable);
            }
        }
    }

    protected abstract void UpdateRenderedRenderable(double time, TRenderable renderable);
    protected abstract void OnRenderRenderableEvent(TRenderable correspondingRenderable, TBehavior behavior);
    protected abstract void OnUnrenderRenderableEvent(TRenderable correspondingRenderable, TBehavior behavior);

    private void RenderableBehaviour_OnRender(TRenderable correspondingRenderable, TBehavior behavior)
    {
        behavior.AssignAssociatedSelectable(correspondingRenderable);
        behavior.gameObject.SetActive(true);

        activeRenderableToBehaviourMapping.Add(correspondingRenderable, behavior);
        correspondingRenderable.OnRender();

        OnRenderRenderableEvent(correspondingRenderable, behavior);
    }

    private void RenderableBehavior_OnUnrender(TRenderable correspondingRenderable, TBehavior behavior)
    {
        behavior.UnassignAssociatedRenderable();
        behavior.gameObject.SetActive(false);

        activeRenderableToBehaviourMapping.Remove(correspondingRenderable);
        behaviorPool.Enqueue(behavior);
        correspondingRenderable.OnUnrender();

        OnUnrenderRenderableEvent(correspondingRenderable, behavior);
    }

    private void UnrenderRenderable_ReturnBehaviorToPool(TRenderable renderable)
    {
        bool renderableMappingResult = activeRenderableToBehaviourMapping.TryGetValue(renderable, out TBehavior behavior);

        if (!renderableMappingResult)
        {
            Debug.Log($"Renderable not in active mapping, ignoring unrendering request.", gameObject);
            return;
        }

        if (!behavior.IsActive)
        {
            return;
        }

        RenderableBehavior_OnUnrender(renderable, behavior);
    }

    private void RenderRenderable_GetBehaviorFromPool(TRenderable renderable)
    {
        bool behaviorDequeueResult = behaviorPool.TryDequeue(out TBehavior behavior);

        if (!behaviorDequeueResult)
        {
            Debug.LogWarning($"Failed to dequeue behavior from pool while trying to render renderable! The limit on behaviors is {k_MAXOBJECTPOOLS}.", gameObject);
            return;
        }

        if (activeRenderableToBehaviourMapping.ContainsKey(renderable))
        {
            Debug.Log($"Renderable already in active renderable mapping, ignoring rendering request.", gameObject);
            return;
        }

        RenderableBehaviour_OnRender(renderable, behavior);
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
}
