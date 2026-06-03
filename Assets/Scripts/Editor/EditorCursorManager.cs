using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A class to manage the cursor in the Editor. <br></br>
/// We want to make the mouse cursor normal outside of the preview section.
/// </summary>
public class EditorCursorManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Texture2D previewMouseTexture;

    public void OnPointerEnter(PointerEventData eventData)
    {
        EditorManager.EditorInstance.InvokeEditorShowSpecialCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EditorManager.EditorInstance.InvokeEditorHideSpecialCursor();
    }
}
