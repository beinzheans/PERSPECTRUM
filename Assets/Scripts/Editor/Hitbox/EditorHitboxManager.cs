using UnityEngine;
using UnityEngine.UI;
public class EditorHitboxManager : EditorObjectPoolManager<EditorHitbox, EditorSelectableUIBehaviour<EditorHitbox>>
{
    private const int k_SELECTEDCOLORINDEX = 3;

    protected override void DeleteEditorObjectEvent(EditorHitbox obj)
    {
        editorManager.CurrentEditorChart.OnDelete(obj);
    }

    protected override void PlaceEditorObjectEvent(EditorHitbox obj)
    {
        editorManager.CurrentEditorChart.OnPlace(obj);
    }

    protected override void OnPreviewChangedEvent(double time)
    {
        AssignAllRenderableList(editorManager.CurrentEditorChart.Hitboxes);
    }

    protected override void UpdateRenderedRenderable(double time, EditorHitbox renderable)
    {
        EditorSelectableUIBehaviour<EditorHitbox> behavior = activeRenderableToBehaviourMapping[renderable];

        Color selectedColor = base.allColors[k_SELECTEDCOLORINDEX];
        Color transparentColor = base.transparentColor[(int)renderable.HitboxType];
        Color opaqueColor = base.allColors[(int)renderable.HitboxType];

        UpdateRenderableAtTime(time, renderable, behavior, transparentColor, opaqueColor, selectedColor);
    }

    protected override void OnRenderRenderableEvent(double time, EditorHitbox correspondingRenderable, EditorSelectableUIBehaviour<EditorHitbox> behavior)
    {
        Vector2 size = correspondingRenderable.NormalizedSize * GameManager.aspectRatioConversionScale;
        RawImage r = behavior.RawImage;

        r.rectTransform.anchorMax = correspondingRenderable.NormalizedPosition + 0.5f * size;
        r.rectTransform.anchorMin = correspondingRenderable.NormalizedPosition - 0.5f * size;
        r.rectTransform.sizeDelta = Vector2.zero;
        r.rectTransform.anchoredPosition = Vector2.zero;

        Color selectedColor = base.allColors[k_SELECTEDCOLORINDEX];
        Color transparentColor = base.transparentColor[(int)correspondingRenderable.HitboxType];
        Color opaqueColor = base.allColors[(int)correspondingRenderable.HitboxType];

        UpdateRenderableAtTime(time, correspondingRenderable, behavior, transparentColor, opaqueColor, selectedColor);
    }

    protected override void OnUnrenderRenderableEvent(EditorHitbox correspondingRenderable, EditorSelectableUIBehaviour<EditorHitbox> behavior)
    {
        return;
    }

    protected override void EditEditorObjectEvent(EditorHitbox renderable)
    {
        return;
    }

    protected override void OnPoolStartedEvent()
    {
        return;
    }
}