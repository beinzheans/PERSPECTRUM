using UnityEngine;

public class GameplayStatisticRecorder : MonoBehaviour
{
    private GameplayManager gameplayManager;

    /// <summary>
    /// Accuracy will be a measurement of how many hitboxes we miss, ignores the hitbox types weights
    /// </summary>
    private double currentAccuracy = 1d;
    public const double k_MAXIMUMSCORE = 1000000d;
    private double scorePerNote = 0d;

    /// <summary>
    /// Score will be a measurement of the overall performance we have, considering the hitbox types
    /// </summary>
    private double currentScore = 0d;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
    }
    private void GameplayManager_OnGameplayEnded()
    {
    }

    private void GameplayManager_OnHitboxBombHit(int obj)
    {
        currentScore -= scorePerNote;
        GetAccuracy();
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        currentScore += scorePerNote * GameplayManager.k_MISMATCHSCOREWEIGHT;
        GetAccuracy();
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        currentScore += scorePerNote;
        GetAccuracy();
    }

    private void GetAccuracy()
    {
        int totalHits = gameplayManager.MatchHitCount + gameplayManager.MismatchHitCount - gameplayManager.BombHitCount;
        currentAccuracy = (double)(totalHits) / (totalHits + gameplayManager.MissCount);
    }
    private void GameplayManager_OnGameplayStarted()
    {
        currentAccuracy = 1d;

        if (gameplayManager.MaxHitboxCount <= 0)
        {
            scorePerNote = 0d;
            currentScore = k_MAXIMUMSCORE;
            return;
        }
        scorePerNote = k_MAXIMUMSCORE / gameplayManager.MaxHitboxCount;
    }
}
