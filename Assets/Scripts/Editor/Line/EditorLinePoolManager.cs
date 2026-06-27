using Radishmouse;
using UnityEngine;

public class EditorLinePoolManager : EditorObjectPoolManager<EditorLine, EditorLineSelectableUI>
{
    private const int k_BASECOLOR = 0;
    private const int k_SELECTEDCOLOR = 1;
    protected override void DeleteEditorObjectEvent(EditorLine obj)
    {
        editorManager.CurrentEditorChart.OnDelete(obj);
    }

    protected override void OnPreviewChangedEvent(double time)
    {
        AssignAllRenderableList(editorManager.CurrentEditorChart.Lines);
    }

    protected override void OnRenderRenderableEvent(double time, EditorLine correspondingRenderable, EditorLineSelectableUI behavior)
    {
        UILineRenderer lineRenderer = behavior.LineRenderer;
        lineRenderer.points = new Vector2[2] { correspondingRenderable.FromNormalizedPosition, correspondingRenderable.ToNormalizedPosition };

        Color selectedColor = base.allColors[k_SELECTEDCOLOR];
        Color transparentColor = base.transparentColor[k_BASECOLOR];
        Color opaqueColor = base.allColors[k_BASECOLOR];

        UpdateRenderableAtTime(time, correspondingRenderable, behavior, transparentColor, opaqueColor, selectedColor);
    }

    protected override void OnUnrenderRenderableEvent(EditorLine correspondingRenderable, EditorLineSelectableUI behavior)
    {
        UILineRenderer lineRenderer = behavior.LineRenderer;

        lineRenderer.points = new Vector2[2];
        lineRenderer.Draw();
    }

    protected override void PlaceEditorObjectEvent(EditorLine obj)
    {
        editorManager.CurrentEditorChart.OnPlace(obj);
    }

    protected override void UpdateRenderedRenderable(double time, EditorLine renderable)
    {
        EditorLineSelectableUI behavior = activeRenderableToBehaviourMapping[renderable];

        Color selectedColor = base.allColors[k_SELECTEDCOLOR];
        Color transparentColor = base.transparentColor[k_BASECOLOR];
        Color opaqueColor = base.allColors[k_BASECOLOR];

        UpdateRenderableAtTime(time, renderable, behavior, transparentColor, opaqueColor, selectedColor);
    }

    protected override void UpdateRenderableAtTime(double time, EditorLine renderable, EditorLineSelectableUI behavior, Color transparentColor, Color opaqueColor, Color selectedColor)
    {
        if (renderable.IsSelected)
        {
            behavior.LineRenderer.color = selectedColor;
        }
        else
        {
            float progress = (float)((editorManager.LookAheadTime - (renderable.RenderTime - time)) / editorManager.LookAheadTime);

            Color lerpedColor = Color.Lerp(transparentColor, opaqueColor, progress);

            behavior.LineRenderer.color = lerpedColor;
        }

        behavior.LineRenderer.renderContainer = EditorManager.EditorInstance.PreviewUIContainer;
        behavior.LineRenderer.Draw();
    }

    protected override void EditEditorObjectEvent(EditorLine renderable)
    {
        return;
    }

    protected override void OnPoolStartedEvent()
    {
        return;
    }
}
