using Codice.Client.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
public class EditorHitboxVisualManager : EditorVisualObjectPool<EditorHitbox, EditorSelectableUIBehaviour<EditorHitbox>>
{
    [SerializeField] private Color HitboxAColor;
    [SerializeField] private Color HitboxBColor;
    [SerializeField] private Color SelectedHitboxColor;

    private Color hitboxAColorTransparent;
    private Color hitboxBColorTransparent;
    
    protected override void Start()
    {
        base.Start();

        hitboxAColorTransparent = MathHelper.GetTransparentVersionOfColor(HitboxAColor);
        hitboxBColorTransparent = MathHelper.GetTransparentVersionOfColor(HitboxBColor);
    }

    protected override void OnPreviewChangedEvent(double time)
    {
        AssignAllRenderableList(editorManager.CurrentEditorChart.Hitboxes);
    }

    protected override void UpdateRenderedRenderable(double time, EditorHitbox renderable)
    {
        EditorSelectableUIBehaviour<EditorHitbox> behaviour = activeRenderableToBehaviourMapping[renderable];

        if (allSelectedRenderables.Contains(renderable))
        {
            behaviour.RawImage.color = SelectedHitboxColor;
        }
        else
        {
            float progress = (float)((editorManager.LookAheadTime - (renderable.RenderTime - time)) / editorManager.LookAheadTime);
            Color initialColor;
            Color finalColor;

            initialColor = renderable.HitboxType == HitboxType.A ? hitboxAColorTransparent : hitboxBColorTransparent;
            finalColor = renderable.HitboxType == HitboxType.A ? HitboxAColor : HitboxBColor;
            Color lerpedColor = Color.Lerp(initialColor, finalColor, progress);
            behaviour.RawImage.color = lerpedColor;
        }
    }

    protected override void OnRenderRenderableEvent(EditorHitbox correspondingRenderable, EditorSelectableUIBehaviour<EditorHitbox> behavior)
    {
        Vector2 size = correspondingRenderable.RawPixelSize * Vector2.one;
        RawImage r = behavior.RawImage;

        r.rectTransform.anchorMax = r.rectTransform.anchorMin = correspondingRenderable.NormalizedPosition;
        r.rectTransform.sizeDelta = size;
        r.rectTransform.anchoredPosition = Vector2.zero;
        r.color = correspondingRenderable.HitboxType == HitboxType.A ? hitboxAColorTransparent : hitboxBColorTransparent;
    }

    protected override void OnUnrenderRenderableEvent(EditorHitbox correspondingRenderable, EditorSelectableUIBehaviour<EditorHitbox> behavior)
    {
        return;
    }
}
