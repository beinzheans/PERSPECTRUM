using Radishmouse;
using UnityEngine;

[RequireComponent(typeof(UILineRenderer))]
public class EditorLineSelectableUI : EditorSelectableUIBehaviour<EditorLine>
{
    private const float k_LINESELECTLENIENCYFACTOR = 4f;
    public UILineRenderer LineRenderer { get; protected set; }
    public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        Vector2 normalizedMousePosition = EditorManager.EditorInstance.EditorMousePosition;
        Vector2 dir = currentAssociatedSelectable.GetFromToVector();
        Vector2 mouseDir = normalizedMousePosition - currentAssociatedSelectable.FromNormalizedPosition;

        Vector2 nearestPoint = currentAssociatedSelectable.FromNormalizedPosition + Vector2.Dot(dir, mouseDir) / dir.sqrMagnitude * dir;

        float sqrDistance = MathHelper.GetPixelFromToVectorFromNormalizedPoints(nearestPoint, normalizedMousePosition, LineRenderer.renderContainer).sqrMagnitude;

        return sqrDistance <= 0.25f * k_LINESELECTLENIENCYFACTOR * k_LINESELECTLENIENCYFACTOR * LineRenderer.thickness * LineRenderer.thickness;
    }

    protected override void Awake()
    {
        base.Awake();
        LineRenderer = GetComponent<UILineRenderer>();
        LineRenderer.isNormalizedCoordinates = true;
        LineRenderer.points = new Vector2[2];
    }
}
