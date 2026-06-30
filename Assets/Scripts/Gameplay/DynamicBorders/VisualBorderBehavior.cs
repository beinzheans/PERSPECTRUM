using Unity.Mathematics;
using UnityEngine;

public class VisualBorderBehavior : GameplayObjectRenderBehavior<VisualBorderObject>
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private static readonly int SHADER_HITBOXID = Shader.PropertyToID("_HitboxType_Float");
    private static readonly int SHADER_NORMALIZEDPROGRESSID = Shader.PropertyToID("_NormalizedProgress");
    protected override void OnAwake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.GetPropertyBlock(propertyBlock);
    }
    protected override void OnRenderEvent()
    {
        meshFilter.mesh = GameplayManager.GameplayInstance.PlayAreaBorderMesh;

        Vector3 zTimeDisplacement = new Vector3(0f, 0f, GameplayManager.k_HITPLANEDEPTH + (float)((AssociatedGameplayObject.RenderTime - GameplayManager.GameplayInstance.CurrentGameplayTime) * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed));

        transform.localScale = GameplayManager.GameplayInstance.CurrentPlayAreaBorderScale;
        transform.SetPositionAndRotation(GameplayManager.GameplayInstance.GameplayCamera.transform.position + zTimeDisplacement + GameplayManager.GameplayInstance.CurrentPlayAreaDisplacement, GameplayManager.GameplayInstance.CurrentPlayAreaRotation);

        float hitboxType_float;

        if (AssociatedGameplayObject.AssociatedHitbox.HitboxType == HitboxType.A) hitboxType_float = 0f;
        else if (AssociatedGameplayObject.AssociatedHitbox.HitboxType == HitboxType.B) hitboxType_float = 1f;
        else hitboxType_float = 2f;

        propertyBlock.SetFloat(SHADER_HITBOXID, hitboxType_float);
        propertyBlock.SetFloat(SHADER_NORMALIZEDPROGRESSID, GetNormalizedProgressFloat());

        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    protected override void OnUnrenderEvent()
    {
        return;
    }

    protected override void OnUpdateEvent()
    {
        transform.localScale = GameplayManager.GameplayInstance.CurrentPlayAreaBorderScale;
        transform.SetPositionAndRotation(GameplayManager.GameplayInstance.CurrentPlayAreaDisplacement + new Vector3(0f, 0f, transform.position.z), GameplayManager.GameplayInstance.CurrentPlayAreaRotation);
        propertyBlock.SetFloat(SHADER_NORMALIZEDPROGRESSID, GetNormalizedProgressFloat());
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
    private float GetNormalizedProgressFloat()
    {
        double lookaheadTime = GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime;
        double progress = (math.max(0d, (GameplayManager.GameplayInstance.CurrentGameplayTime + lookaheadTime - AssociatedGameplayObject.RenderTime) / lookaheadTime));

        return (float)progress; // use linear function
    }

}
