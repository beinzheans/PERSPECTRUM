using UnityEngine;
using UnityEngine.UI;

public class EditorPointObjectManager : EditorObjectPoolManager<EditorPoint, EditorSelectableUIBehaviour<EditorPoint>>
{
    private const int k_POINTCOLORINDEX = 0;
    private const int k_SELECTEDCOLORINDEX = 1;

    protected override void DeleteEditorObjectEvent(EditorPoint obj)
    {
        editorManager.CurrentEditorChart.OnDelete(obj);
    }

    protected override void EditEditorObjectEvent(EditorPoint renderable)
    {
        return;
    }

    protected override void OnPoolStartedEvent()
    {
        return;
    }

    protected override void OnPreviewChangedEvent(double time)
    {
        AssignAllRenderableList(editorManager.CurrentEditorChart.Points);
    }

    protected override void OnRenderRenderableEvent(double time, EditorPoint correspondingRenderable, EditorSelectableUIBehaviour<EditorPoint> behavior)
    {
        RawImage r = behavior.RawImage;
        r.rectTransform.anchorMin = r.rectTransform.anchorMax = correspondingRenderable.NormalizedPosition;
        r.rectTransform.anchoredPosition = Vector2.zero;


        Color selectedColor = base.allColors[k_SELECTEDCOLORINDEX];
        Color transparentColor = base.transparentColor[k_POINTCOLORINDEX];
        Color opaqueColor = base.allColors[k_POINTCOLORINDEX];

        UpdateRenderableAtTime(time, correspondingRenderable, behavior, transparentColor, opaqueColor, selectedColor);
    }

    protected override void OnUnrenderRenderableEvent(EditorPoint correspondingRenderable, EditorSelectableUIBehaviour<EditorPoint> behavior)
    {
        return;
    }

    protected override void PlaceEditorObjectEvent(EditorPoint obj)
    {
        editorManager.CurrentEditorChart.OnPlace(obj);
    }

    protected override void UpdateRenderedRenderable(double time, EditorPoint renderable)
    {
        EditorSelectableUIBehaviour<EditorPoint> behavior = activeRenderableToBehaviourMapping[renderable];

        Color selectedColor = base.allColors[k_SELECTEDCOLORINDEX];
        Color transparentColor = base.transparentColor[k_POINTCOLORINDEX];
        Color opaqueColor = base.allColors[k_POINTCOLORINDEX];

        UpdateRenderableAtTime(time, renderable, behavior, transparentColor, opaqueColor, selectedColor);
    }
}
