using UnityEngine;

public class VisualLineRenderBehavior : GameplayObjectRenderBehavior<VisualLine>
{
    private const float k_LINETHICKNESS = 0.01f;
    protected override void OnRenderEvent()
    {
        Vector2 fromScreenPoint = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(AssociatedGameplayObject.InitialPosition, GameplayManager.GameplayInstance.GameplayRectTransform);
        Vector2 toScreenPoint = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(AssociatedGameplayObject.TerminalPosition, GameplayManager.GameplayInstance.GameplayRectTransform);

        float fromZDisplacement = (float)((AssociatedGameplayObject.InitialTime - GameplayManager.GameplayInstance.CurrentGameplayTime) * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);
        float toZDisplacement = (float)((AssociatedGameplayObject.TerminalTime - GameplayManager.GameplayInstance.CurrentGameplayTime) * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);

        Vector3 fromWorldPoint = GameplayManager.GameplayInstance.GameplayCamera.ScreenToWorldPoint(new Vector3(fromScreenPoint.x, fromScreenPoint.y, GameplayManager.k_HITPLANEDEPTH)) + new Vector3(0f, 0f, fromZDisplacement);
        Vector3 toWorldPoint = GameplayManager.GameplayInstance.GameplayCamera.ScreenToWorldPoint(new Vector3(toScreenPoint.x, toScreenPoint.y, GameplayManager.k_HITPLANEDEPTH)) + new Vector3(0f, 0f, toZDisplacement);
        float distance = Vector3.Distance(fromWorldPoint, toWorldPoint);
        Vector3 position = 0.5f * (toWorldPoint + fromWorldPoint);
        Quaternion rotation = Quaternion.LookRotation(toWorldPoint - fromWorldPoint, Vector3.up);

        Vector3 size = new Vector3(k_LINETHICKNESS, k_LINETHICKNESS, distance);
        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = size;
    }

    protected override void OnUnrenderEvent()
    {
        return;
    }

    protected override void OnUpdateEvent()
    {
        return;
    }
}
