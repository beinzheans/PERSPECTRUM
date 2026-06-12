using UnityEngine;

public class VisualJudgementRenderBehavior : GameplayObjectRenderBehavior<VisualJudgementObject>
{
    private JudgementUI judgementUI;
    private RectTransform rectTransform;

    protected override void OnAwake()
    {
        rectTransform = GetComponent<RectTransform>();
        judgementUI = GetComponent<JudgementUI>();
    }

    protected override void OnRenderEvent()
    {
        rectTransform.anchorMin = rectTransform.anchorMax = AssociatedGameplayObject.NormalizedPosition;
        rectTransform.anchoredPosition = Vector2.zero;

        switch (AssociatedGameplayObject.JudgementType)
        {
            case JudgementType.MATCH:
                judgementUI.JudgementType_Float = 0f;
                break;
            case JudgementType.MISMATCH:
                judgementUI.JudgementType_Float = 1f;
                break;
            case JudgementType.MISS:
                judgementUI.JudgementType_Float = 2f;
                break;
        }

        judgementUI.NormalizedProgress = GetNormalizedProgress();

        judgementUI.UpdateJudgementMarker();
    }

    protected override void OnUnrenderEvent()
    {
        return;
    }

    protected override void OnUpdateEvent()
    {
        judgementUI.NormalizedProgress = GetNormalizedProgress();

        judgementUI.UpdateJudgementMarker();
    }

    private float GetNormalizedProgress()
    {
        return (float)((GameplayManager.GameplayInstance.CurrentGameplayTime - AssociatedGameplayObject.RenderTime) / VisualJudgementManager.k_JUDGEMENTALIVETIME);
    }
}
