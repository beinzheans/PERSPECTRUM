using UnityEngine;

public class EditorHitboxSelectableBehavior : EditorSelectableUIBehaviour<EditorHitbox>
{
    public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return true;
    }
}
