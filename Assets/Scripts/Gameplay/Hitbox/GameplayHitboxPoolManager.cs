public class GameplayHitboxPoolManager : GameplayObjectPoolManager<VisualHitbox, VisualHitboxRenderBehavior>
{
    protected override void OnStartEvent()
    {
        base.OnStartEvent();
        gameplayManager.OnHitboxBombHit += GameplayManager_OnHitboxBombHit;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
    }

    protected override void OnDestroyEvent()
    {
        base.OnDestroyEvent();
        gameplayManager.OnHitboxBombHit -= GameplayManager_OnHitboxBombHit;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        UnrenderVisualHitbox(obj);
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        UnrenderVisualHitbox(obj);
    }

    private void GameplayManager_OnHitboxBombHit(VisualHitbox obj)
    {
        UnrenderVisualHitbox(obj);
    }

    private void UnrenderVisualHitbox(VisualHitbox hitbox)
    {
        for (int i = 0; i < gameplayManager.CurrentGameplayChart.GameplayObjects.Length; i++)
        {
            GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[i];

            if (gameplayObject is not VisualHitbox hit)
            {
                continue;
            }

            if (hit.Equals(hitbox))
            {
                if (!hit.IsRendered)
                {
                    return;
                }

                UnrenderObject_ReturnToPool(hit);

                return;
            }
        }
    }
}
