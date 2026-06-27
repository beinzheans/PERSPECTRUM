using UnityEngine;

public class EditorPointSelectableUI : EditorSelectableUIBehaviour<EditorPoint>
{
    public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out Vector2 localPoint);

        return Vector2.SqrMagnitude(localPoint) <= 16 * 16;
    }
}
