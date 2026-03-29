using UnityEngine;

public static class MathHelper
{
    public static Vector2 GetNormalizedPointInsideReferenceUI(Vector2 rawPoint, RectTransform referenceRect)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(referenceRect, rawPoint, null, out Vector2 relativeMousePointToRectCentre);

        return (relativeMousePointToRectCentre + new Vector2(0.5f * referenceRect.rect.width, 0.5f * referenceRect.rect.height)) / new Vector2(referenceRect.rect.width, referenceRect.rect.height);
    }

    public static Color GetTransparentVersionOfColor(Color color)
    {
        return new Color(color.r, color.g, color.b, 0f);
    }
}
