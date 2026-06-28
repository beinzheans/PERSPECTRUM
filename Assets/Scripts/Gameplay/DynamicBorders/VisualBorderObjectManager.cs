public class VisualBorderObjectManager : GameplayObjectPoolManager<VisualBorderObject, VisualBorderBehavior>
{
    protected override void OnStartEvent()
    {
        gameplayManager.OnGameplayObjectRendered += GameplayManager_OnGameplayObjectRendered;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayObjectUnrendered += GameplayManager_OnGameplayObjectUnrendered;
    }

    private void GameplayManager_OnGameplayTimeUpdated(double obj)
    {
        foreach (var (_, behavior) in currentActiveObjectsMapping)
        {
            behavior.OnUpdate();
        }
    }

    private void GameplayManager_OnGameplayObjectUnrendered(GameplayObject obj)
    {
        if (obj is not VisualHitbox hitbox)
        {
            return;
        }

        if (hitbox.HitboxType == HitboxType.BOMB)
        {
            return;
        }

        UnrenderBorders(hitbox);
    }

    private void GameplayManager_OnGameplayObjectRendered(GameplayObject obj)
    {
        if (obj is not VisualHitbox hitbox)
        {
            return;
        }

        if (hitbox.HitboxType == HitboxType.BOMB)
        {
            return;
        }

        VisualBorderObject borderObject = new VisualBorderObject(hitbox);
        RenderObject_GetFromPool(borderObject);
    }

    protected override void OnDestroyEvent()
    {
        gameplayManager.OnGameplayObjectRendered -= GameplayManager_OnGameplayObjectRendered;
        gameplayManager.OnGameplayObjectUnrendered -= GameplayManager_OnGameplayObjectUnrendered;
        gameplayManager.OnGameplayTimeUpdated -= GameplayManager_OnGameplayTimeUpdated;
    }

    private void UnrenderBorders(VisualHitbox obj)
    {
        foreach (var (data, _) in currentActiveObjectsMapping)
        {
            if (data.AssociatedHitbox.Equals(obj))
            {
                UnrenderObject_ReturnToPool(data);
                break;
            }
        }
    }
}
