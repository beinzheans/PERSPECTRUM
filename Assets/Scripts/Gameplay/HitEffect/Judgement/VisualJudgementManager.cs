public class VisualJudgementManager : GameplayObjectPoolManager<VisualJudgementObject, VisualJudgementRenderBehavior>
{
    /// <summary>
    /// How long the judgement UI element stays on the screen.
    /// </summary>
    public const double k_JUDGEMENTALIVETIME = 0.1d;

    protected override void OnStartEvent()
    {
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnHitboxBombHit += GameplayManager_OnHitboxBombHit;
    }

    private void GameplayManager_OnGameplayTimeUpdated(double obj)
    {
        foreach (var (_, renderBehavior) in currentActiveObjectsMapping)
        {
            renderBehavior.OnUpdate();
        }
    }

    private void AddReturnPoolTimer(VisualJudgementObject visualJudgementObject)
    {
        TimerIntervalAction timerAction = new TimerIntervalAction(this, (x) => UnrenderObject_ReturnToPool(visualJudgementObject), () => { }, k_JUDGEMENTALIVETIME, 0d, 1);
        DSPTimerEngine.TimerInstance.AddActionToTimer(timerAction);
    }
    private void GameplayManager_OnHitboxBombHit(VisualHitbox obj)
    {
        VisualJudgementObject visualJudgementObject = new VisualJudgementObject(obj, gameplayManager.CurrentGameplayTime, gameplayManager.GameplayMousePosition, JudgementType.MISS);
        RenderObject_GetFromPool(visualJudgementObject);

        AddReturnPoolTimer(visualJudgementObject);
    }

    private void GameplayManager_OnHitboxMiss(VisualHitbox obj)
    {
        VisualJudgementObject visualJudgementObject = new VisualJudgementObject(obj, gameplayManager.CurrentGameplayTime, obj.NormalizedPosition, JudgementType.MISS);
        RenderObject_GetFromPool(visualJudgementObject);

        AddReturnPoolTimer(visualJudgementObject);
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        VisualJudgementObject visualJudgementObject = new VisualJudgementObject(obj, gameplayManager.CurrentGameplayTime, gameplayManager.GameplayMousePosition, JudgementType.MISMATCH);
        RenderObject_GetFromPool(visualJudgementObject);

        AddReturnPoolTimer(visualJudgementObject);
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        VisualJudgementObject visualJudgementObject = new VisualJudgementObject(obj, gameplayManager.CurrentGameplayTime, gameplayManager.GameplayMousePosition, JudgementType.MATCH);
        RenderObject_GetFromPool(visualJudgementObject);

        AddReturnPoolTimer(visualJudgementObject);
    }

    protected override void OnDestroyEvent()
    {
        gameplayManager.OnGameplayTimeUpdated -= GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxMiss -= GameplayManager_OnHitboxMiss;
        gameplayManager.OnHitboxBombHit -= GameplayManager_OnHitboxBombHit;
    }
}