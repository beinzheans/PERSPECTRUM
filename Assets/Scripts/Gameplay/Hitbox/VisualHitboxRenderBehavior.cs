using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualHitboxRenderBehavior : GameplayObjectRenderBehavior<VisualHitbox>
{
    private static readonly int SHADER_HITBOXID = Shader.PropertyToID("_HitboxType_Float");
    private static readonly int SHADER_NORMALIZEDPROGRESSID = Shader.PropertyToID("_NormalizedProgress");

    protected override void OnRenderEvent()
    {
        Vector2 screenPoint = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(AssociatedGameplayObject.NormalizedPosition, GameplayManager.GameplayInstance.GameplayRectTransform);

        Vector3 spawnPoint = GameplayManager.GameplayInstance.GameplayCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, GameplayManager.k_HITPLANEDEPTH));
        float zTimeDisplacement = (float)((AssociatedGameplayObject.RenderTime - GameplayManager.GameplayInstance.CurrentGameplayTime) * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed);

        Vector2 screenSize = MathHelper.GetPixelSizeOfNormalizedSizeVector(AssociatedGameplayObject.NormalizedSize * Vector2.one, GameplayManager.GameplayInstance.GameplayRectTransform);
        Vector3 worldSize = screenSize * GameplayManager.GameplayInstance.WorldToScreenSizeRatioOfPreview;

        transform.position = spawnPoint + new Vector3(0f, 0f, zTimeDisplacement);
        transform.localScale = worldSize + new Vector3(0f, 0f, 1f);

        meshRenderer.GetPropertyBlock(propertyBlock);

        float hitboxIDFloat;

        if (AssociatedGameplayObject.HitboxType == HitboxType.A) hitboxIDFloat = 0f;
        else if (AssociatedGameplayObject.HitboxType == HitboxType.B) hitboxIDFloat = 1f;
        else hitboxIDFloat = 2f;

        propertyBlock.SetFloat(SHADER_HITBOXID, hitboxIDFloat);

        propertyBlock.SetFloat(SHADER_NORMALIZEDPROGRESSID, GetNormalizedProgressFloat());
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    protected override void OnUnrenderEvent()
    {
        return;
    }

    protected override void OnUpdateEvent()
    {
        meshRenderer.GetPropertyBlock(propertyBlock);
        
        propertyBlock.SetFloat(SHADER_NORMALIZEDPROGRESSID, GetNormalizedProgressFloat());
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private float GetNormalizedProgressFloat()
    {
        double lookaheadTime = GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime;
        double progress = (math.max(0d, (GameplayManager.GameplayInstance.CurrentGameplayTime + lookaheadTime - AssociatedGameplayObject.RenderTime) / lookaheadTime));

        // use (25^x - 1) / 24 graph for exponential curve

        return (float)((math.pow(25d, progress) - 1) / 24d);
    }

}
