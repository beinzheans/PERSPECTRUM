using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


// Fuck this shit man I'm just gonna fake the judgements. the mouse movement is convincing enough.
public class GameplayStatisticRecorder : MonoBehaviour
{
    private GameplayManager gameplayManager;

    private List<ReplayMouseInfo> replayMouseBuffer = new();
    private List<ReplayJudgementInfo> replayJudgementBuffer = new();

    public const double k_MOUSERECORDINTERVAL = 1d / 100d;
    private double lastSampleBufferTime = 0d;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnHitboxBombHit += GameplayManager_OnHitboxBombHit;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
    }

    private void GameplayManager_OnHitboxMiss(VisualHitbox obj)
    {
        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        replayJudgementBuffer.Add(new ReplayJudgementInfo(gameplayManager.CurrentGameplayTime, obj, JudgementType.MISS));
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        replayJudgementBuffer.Add(new ReplayJudgementInfo(gameplayManager.CurrentGameplayTime, obj, JudgementType.MISMATCH));
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        replayJudgementBuffer.Add(new ReplayJudgementInfo(gameplayManager.CurrentGameplayTime, obj, JudgementType.MATCH));
    }

    private void GameplayManager_OnHitboxBombHit(VisualHitbox obj)
    {
        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        replayJudgementBuffer.Add(new ReplayJudgementInfo(gameplayManager.CurrentGameplayTime, obj, JudgementType.MISS));
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        while (time - lastSampleBufferTime > k_MOUSERECORDINTERVAL || MathHelper.IsTwoDoublesEqualWithEpsilion(time - lastSampleBufferTime, k_MOUSERECORDINTERVAL))
        {
            double currentSampleTime = lastSampleBufferTime + k_MOUSERECORDINTERVAL;

            ReplayMouseInfo buffer = new ReplayMouseInfo(currentSampleTime, gameplayManager.GameplayMousePosition, gameplayManager.MouseActiveType);
            replayMouseBuffer.Add(buffer);

            lastSampleBufferTime = currentSampleTime;
        }
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        lastSampleBufferTime = 0d;
        replayMouseBuffer = new();
        replayJudgementBuffer = new();
    }

    private void GameplayManager_OnGameplayEnded()
    {
        if (gameplayManager.IsInReplayMode)
        {
            return;
        }

        if (gameplayManager.MatchHitCount + gameplayManager.MismatchHitCount + gameplayManager.MissCount < gameplayManager.MaxHitboxCount) // did not finish this play, don't save replay
        {
            return;
        }

        string timestamp = DateTime.Now.ToString(MathHelper.k_TIMESTAMPINTERNALFORMAT);

        GameplayReplay mouseReplay = new GameplayReplay(replayMouseBuffer, replayJudgementBuffer);
        GameplayStatisticRecord record = new GameplayStatisticRecord(gameplayManager.MatchHitCount,
                                                                     gameplayManager.MismatchHitCount,
                                                                     gameplayManager.MissCount,
                                                                     gameplayManager.BombHitCount,
                                                                     gameplayManager.CurrentAccuracy,
                                                                     gameplayManager.CurrentScore,
                                                                     timestamp,
                                                                     mouseReplay,
                                                                     gameplayManager.CurrentMetadata.BaseMetadata
                                                                     );

        GamePersistenceManager.SaveGameplayStatisticRecordToFile(record);
        GameManager.GameInstance.AddGameplayRecordToMapping(record);
    }
}
/// <summary>
/// A class to represent a record of the previous play. Each record will have <see cref="global::BaseChartMetadata"/> appended to it to find the corresponding chart.<br></br>
/// All records and relations should be loaded into the game when the game boots up for fast look-up in a dictionary.
/// </summary>
[Serializable]
public struct GameplayStatisticRecord
{
    public int MatchCount;
    public int MismatchCount;
    public int MissCount;
    public int BombCount;
    public double FinalAccuracy;
    public double FinalScore;
    public string RecordTimestamp;

    public GameplayReplay GameplayReplay;
    public BaseChartMetadata BaseChartMetadata;
    public GameplayStatisticRecord(int matchCount, int mismatchCount, int missCount, int bombCount, double finalAccuracy, double finalScore, string recordTimestamp, GameplayReplay gameplayReplay, BaseChartMetadata baseChartMetadata)
    {
        MatchCount = matchCount;
        MismatchCount = mismatchCount;
        MissCount = missCount;
        BombCount = bombCount;
        FinalAccuracy = finalAccuracy;
        FinalScore = finalScore;
        RecordTimestamp = recordTimestamp;
        GameplayReplay = gameplayReplay;
        BaseChartMetadata = baseChartMetadata;
    }
}

[Serializable]
public struct GameplayReplay
{
    public List<ReplayMouseInfo> ReplayMouseData;
    public List<ReplayJudgementInfo> ReplayJudgementData;
    public GameplayReplay(List<ReplayMouseInfo> replayMouseData, List<ReplayJudgementInfo> replayJudgementData)
    {
        ReplayMouseData = replayMouseData;
        ReplayJudgementData = replayJudgementData;
    }
}

[Serializable]
public struct ReplayMouseInfo
{
    [JsonProperty("time")]
    public double ReplayTime;
    [JsonProperty("pos")]
    public Vector2 NormalizedPosition;
    [JsonProperty("type")]
    public MouseActiveType MouseType;

    public ReplayMouseInfo(double replayTime, Vector2 normalizedPosition, MouseActiveType mouseType)
    {
        ReplayTime = replayTime;
        NormalizedPosition = normalizedPosition;
        MouseType = mouseType;
    }
}

[Serializable]
public struct ReplayJudgementInfo
{
    [JsonProperty("time")]
    public double ReplayTime;
    [JsonProperty("box")]
    public VisualHitbox HitHitbox;
    [JsonProperty("type")]
    public JudgementType HitJudgementType;

    public ReplayJudgementInfo(double replayTime, VisualHitbox hitHitbox, JudgementType hitJudgementType)
    {
        ReplayTime = replayTime;
        HitHitbox = hitHitbox;
        HitJudgementType = hitJudgementType;
    }
}