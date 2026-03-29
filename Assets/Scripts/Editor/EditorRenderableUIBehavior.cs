using UnityEngine;
using UnityEngine.UI;

public class EditorRenderableUIBehavior<T> : MonoBehaviour where T : IRenderable
{
    protected bool isActive;
    public bool IsActive { get => isActive; }
    protected RawImage rawImage;
    public RawImage RawImage { get => rawImage; }

    protected T currentAssociatedRenderable;

    protected virtual void Awake()
    {
        rawImage = GetComponent<RawImage>();
        isActive = false;
    }

    public void AssignAssociatedRenderable(T renderable)
    {
        currentAssociatedRenderable = renderable;
        gameObject.SetActive(true);
        isActive = true;
    }

    public void UnassignAssociatedRenderable()
    {
        gameObject.SetActive(false);
        isActive = false;
    }
   
}
