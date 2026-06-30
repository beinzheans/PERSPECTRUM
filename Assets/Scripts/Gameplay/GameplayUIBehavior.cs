using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIBehavior : MonoBehaviour
{
    private GameplayManager gameplayManager;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayUI;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text gameplay_chartCredit;
    [SerializeField] private TMP_Text gameplay_songCredit;
    [SerializeField] private TMP_Text gameplay_matchCount;
    [SerializeField] private TMP_Text gameplay_mismatchCount;
    [SerializeField] private TMP_Text gameplay_bombCount;
    [SerializeField] private TMP_Text gameplay_missCount;
    [SerializeField] private TMP_Text gameplay_accuracyPercent;
    [SerializeField] private TMP_Text gameplay_score;

    [Header("Endscreen UI")]
    [SerializeField] private GameObject endscreenUI;
    [SerializeField] private TMP_Text chartName;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text matchText;
    [SerializeField] private TMP_Text mismatchText;
    [SerializeField] private TMP_Text missText;
    [SerializeField] private TMP_Text bombText;
    [SerializeField] private Slider accuracySlider;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Button ReturnButton;
    [SerializeField] private Button RetryButton;

    void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxBombHit += GameplayManager_OnHitboxBombHit;
    }

    private void GameplayManager_OnHitboxBombHit(VisualHitbox hitbox)
    {
        comboText.text = "0";
        gameplay_bombCount.text = gameplayManager.BombHitCount.ToString();
        gameplay_accuracyPercent.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        gameplay_score.text = ((int)math.round(gameplayManager.CurrentScore)).ToString();

    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded -= GameplayManager_OnGameplayEnded;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxHit;
        gameplayManager.OnHitboxMiss -= GameplayManager_OnHitboxMiss;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxBombHit -= GameplayManager_OnHitboxBombHit;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        comboText.text = gameplayManager.CurrentCombo.ToString();
        gameplay_mismatchCount.text = gameplayManager.MismatchHitCount.ToString();
        gameplay_accuracyPercent.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        gameplay_score.text = ((int)math.round(gameplayManager.CurrentScore)).ToString();

    }

    private void GameplayManager_OnGameplayEnded()
    {
        gameplayUI.SetActive(false);
        endscreenUI.SetActive(true);
        SetupEndscreenUI();
    }

    private void GameplayManager_OnGameplayStarted()
    {
        gameplayUI.SetActive(true);
        endscreenUI.SetActive(false);
        SetupGameplayUI();
    }

    private void GameplayManager_OnHitboxMiss(VisualHitbox obj)
    {
        comboText.text = "0";
        gameplay_missCount.text = gameplayManager.MissCount.ToString();
        gameplay_accuracyPercent.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        gameplay_score.text = ((int)math.round(gameplayManager.CurrentScore)).ToString();
    }

    private void GameplayManager_OnHitboxHit(VisualHitbox obj)
    {
        comboText.text = gameplayManager.CurrentCombo.ToString();
        gameplay_matchCount.text = gameplayManager.MatchHitCount.ToString();
        gameplay_accuracyPercent.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        gameplay_score.text = ((int)math.round(gameplayManager.CurrentScore)).ToString();
    }


    private void SetupGameplayUI()
    {
        comboText.text = "0";
        gameplay_chartCredit.text = gameplayManager.CurrentMetadata.BaseMetadata.ChartName;
        gameplay_songCredit.text = $"{gameplayManager.CurrentMetadata.BaseMetadata.SongArtist} - {gameplayManager.CurrentMetadata.BaseMetadata.SongName}";
        gameplay_matchCount.text = "0";
        gameplay_mismatchCount.text = "0";
        gameplay_bombCount.text = "0";
        gameplay_missCount.text = "0";
        gameplay_accuracyPercent.text = "100%";
        gameplay_score.text = "0";
    }

    private void SetupEndscreenUI()
    {
        chartName.text = gameplayManager.CurrentMetadata.BaseMetadata.ChartName;
        scoreText.text = ((int)math.round(gameplayManager.CurrentScore)).ToString();
        GameplayResultRank rank = MathHelper.ConvertOverallScoreToRank(gameplayManager.CurrentScore);
        rankText.text = MathHelper.ConvertRankToString(rank);

        difficultyText.text = $"Difficulty {gameplayManager.CurrentMetadata.BaseMetadata.ChartDifficulty}";
        matchText.text = $"Matches: {gameplayManager.MatchHitCount}";
        mismatchText.text = $"Mismatches: {gameplayManager.MismatchHitCount}";
        missText.text = $"Misses: {gameplayManager.MissCount}";
        bombText.text = $"Bomb Hits: {gameplayManager.BombHitCount}";
        accuracyText.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        accuracySlider.value = (float)gameplayManager.CurrentAccuracy;
    }
    public void UI_OnReturnButton()
    {
        SceneLoader.LoadSceneAtIndex(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => { });
    }

    public void UI_OnRetryButton()
    {
        gameplayManager.InvokeGameplayRestartEvent();
    }
}
