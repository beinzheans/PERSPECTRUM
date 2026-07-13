using UnityEngine;

/// <summary>
/// A class responsible for handling the replay logic.
/// </summary>
public class GameplayReplayManager : MonoBehaviour
{
    private GameplayManager gameplayManager;
    private GameplayStatisticRecord currentReplayRecord;

    private ReplayMouseInfo[] replayMouseBuffer = new ReplayMouseInfo[0];
    private ReplayJudgementInfo[] replayJudgementBuffer = new ReplayJudgementInfo[0];

    int currentReplayMouseIndex = 0;
    int currentReplayJudgementIndex = 0;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayReplayLoaded += GameplayManager_OnGameplayReplayLoaded;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void GameplayManager_OnGameplayTimeUpdated(double obj)
    {
        if (!gameplayManager.IsInReplayMode)
        {
            return;
        }

        SetGameplayBufferAtTime(obj);
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        currentReplayMouseIndex = 0;
        currentReplayJudgementIndex = 0;
    }

    private void SetGameplayBufferAtTime(double time)
    {
        SetMouseBuffer(time);
        SetJudgementBuffer(time);
    }

    private void SetMouseBuffer(double time)
    {
        for (int i = currentReplayMouseIndex; i < replayMouseBuffer.Length; i++)
        {
            if (replayMouseBuffer[i].ReplayTime > time)
            {
                break;
            }

            currentReplayMouseIndex = i;
        }

        ReplayMouseInfo mouseBuffer = replayMouseBuffer[currentReplayMouseIndex];

        if (currentReplayMouseIndex >= replayMouseBuffer.Length - 1)
        {
            gameplayManager.SetGameMouseState(mouseBuffer.NormalizedPosition, mouseBuffer.MouseType);
            return;
        }

        ReplayMouseInfo nextBuffer = replayMouseBuffer[currentReplayMouseIndex + 1];

        MathHelper.LerpMouseBuffers(mouseBuffer, nextBuffer, time, out Vector2 position, out MouseActiveType mouseType);
        gameplayManager.SetGameMouseState(position, mouseType);
    }

    private void SetJudgementBuffer(double time)
    {
        while (currentReplayJudgementIndex <= replayJudgementBuffer.Length - 1)
        {
            ReplayJudgementInfo judgementBuffer = replayJudgementBuffer[currentReplayJudgementIndex];

            if (judgementBuffer.ReplayTime > time)
            {
                break;
            }

            switch (judgementBuffer.HitJudgementType)
            {
                case JudgementType.MATCH:
                    gameplayManager.InvokeHitboxMatchHitEvent(judgementBuffer.HitHitbox);
                    break;
                case JudgementType.MISMATCH:
                    gameplayManager.InvokeHitboxMismatchHitEvent(judgementBuffer.HitHitbox);
                    break;
                case JudgementType.MISS:
                    if (judgementBuffer.HitHitbox.HitboxType == HitboxType.BOMB)
                    {
                        gameplayManager.InvokeHitboxBombHitEvent(judgementBuffer.HitHitbox);
                    }
                    else
                    {
                        gameplayManager.InvokeHitboxMissEvent(judgementBuffer.HitHitbox);
                    }
                    break;
            }

            currentReplayJudgementIndex++;
        }
    }
    private void GameplayManager_OnGameplayReplayLoaded(GameplayStatisticRecord obj)
    {
        currentReplayRecord = obj;

        replayMouseBuffer = currentReplayRecord.GameplayReplay.ReplayMouseData.ToArray();
        replayJudgementBuffer = currentReplayRecord.GameplayReplay.ReplayJudgementData.ToArray();
    }
}
