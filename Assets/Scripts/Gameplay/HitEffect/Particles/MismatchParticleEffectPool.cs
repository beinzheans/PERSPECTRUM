using UnityEngine;

public class MismatchParticleEffectPool : ParticleEffectPool
{
    protected override void OnStartEvent()
    {
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
    }

    protected override void OnDestroyEvent()
    {
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        PlayParticles();
    }

    protected override void OnBeforeParticlePlayEvent(ref ParticleSystem particleSystem)
    {
        particleSystem.transform.localPosition = GetLocalPositionFromHitbox();
    }
    private Vector3 GetLocalPositionFromHitbox()
    {
        Vector2 screenSpace = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(gameplayManager.GameplayMousePosition, gameplayManager.GameplayRectTransform);

        Vector3 screenSpaceWithDepth = new Vector3(screenSpace.x, screenSpace.y, GameplayManager.k_HITPLANEDEPTH);
        Vector3 worldPosition = gameplayManager.GameplayCamera.ScreenToWorldPoint(screenSpaceWithDepth);
        return gameplayManager.GameplayCamera.transform.InverseTransformPoint(worldPosition);
    }
}
