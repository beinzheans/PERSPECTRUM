using UnityEngine;
using UnityEngine.UI;

public class EditorRenderableUIBehavior<T> : MonoBehaviour where T : EditorObject
{
    protected bool isActive;
    public bool IsActive { get => isActive; }
    protected RawImage rawImage;
    public RawImage RawImage { get => rawImage; }

    protected T currentAssociatedRenderable;
    protected RectTransform rectTransform;
    protected virtual void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        isActive = false;
    }

    public virtual void AssignAssociatedRenderable(T renderable)
    {
        currentAssociatedRenderable = renderable;
        gameObject.SetActive(true);
        isActive = true;
    }

    public virtual void UnassignAssociatedRenderable()
    {
        gameObject.SetActive(false);
        isActive = false;
    }

}
