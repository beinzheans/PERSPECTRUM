using UnityEngine;

public abstract class GameplayObjectRenderBehavior<T> : MonoBehaviour where T : GameplayObject
{
    protected MeshRenderer meshRenderer;
    public T AssociatedGameplayObject { get; protected set; }

    public MaterialPropertyBlock propertyBlock;

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public void OnRender(T associatedGameplayObject)
    {
        this.AssociatedGameplayObject = associatedGameplayObject;
        gameObject.SetActive(true);

        OnRenderEvent();
    }

    public void OnUpdate()
    {
        OnUpdateEvent();
    }

    /// <summary>
    /// Custom implementation of events when the object is rendered. 
    /// </summary>
    protected abstract void OnRenderEvent();
    public void OnUnrender()
    {
        this.AssociatedGameplayObject = null;
        gameObject.SetActive(false);

        OnUnrenderEvent();
    }
    /// <summary>
    /// Custom implementation of events when the object is rendered.
    /// </summary>
    protected abstract void OnUnrenderEvent();

    /// <summary>
    /// Custom implementation of events when the object needs to be updated.
    /// </summary>
    protected abstract void OnUpdateEvent();

}
