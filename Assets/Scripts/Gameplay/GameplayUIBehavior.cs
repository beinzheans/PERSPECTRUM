using System.Threading.Tasks;
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
    [SerializeField] private TMP_Text gameplay_missCount;
    [SerializeField] private TMP_Text gameplay_accuracyPercent;
    [SerializeField] private TMP_Text gameplay_score;
    [SerializeField] private TMP_Text gameplay_chartDifficulty;
    [SerializeField] private Slider gameplay_accuracySlider;

    [SerializeField] private TMP_Text gameplay_progress;
    [SerializeField] private Slider gameplay_progressSlider;


    [Header("Gameplay Resume UI")]
    [SerializeField] private GameObject gameplayResume_UI;
    [SerializeField] private Image gameplayResume_background;
    [SerializeField] private TMP_Text gameplayResume_tickText;

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

        gameplayManager.OnGameplayWaitingForResume += GameplayManager_OnGameplayWaitingForResume;
        gameplayManager.OnGameplayResumeTick += GameplayManager_OnGameplayResumeTick;
        gameplayManager.OnGameplayResumed += GameplayManager_OnGameplayResumed;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxBombHit += GameplayManager_OnHitboxBombHit;
    }


    private int tick = GameplayResumeManager.k_NUMBEROFLEADINTICKS;
    private const float k_GAMERESUMEBACKGROUNDALPHA = 0.5f;
    private void GameplayManager_OnGameplayResumeTick()
    {
        if (tick == 0)
        {
            return;
        }

        TimerIntervalAction tickAction = new TimerIntervalAction(this, (x) =>
        {
            gameplayResume_background.color = new Color(0f, 0f, 0f, k_GAMERESUMEBACKGROUNDALPHA * (float)tick / GameplayResumeManager.k_NUMBEROFLEADINTICKS);
            gameplayResume_tickText.color = new Color(1f, 1f, 1f, k_GAMERESUMEBACKGROUNDALPHA * (float)tick / GameplayResumeManager.k_NUMBEROFLEADINTICKS);
            gameplayResume_tickText.text = tick.ToString();

            tick--;
        }, () => { }, GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(tickAction);
    }

    private void GameplayManager_OnGameplayWaitingForResume()
    {
        gameplayResume_UI.SetActive(true);
        tick = GameplayResumeManager.k_NUMBEROFLEADINTICKS;
        gameplayResume_background.color = new Color(0f, 0f, 0f, k_GAMERESUMEBACKGROUNDALPHA);
        gameplayResume_tickText.color = new Color(1f, 1f, 1f, k_GAMERESUMEBACKGROUNDALPHA);
        gameplayResume_tickText.text = tick.ToString();
    }

    private void GameplayManager_OnGameplayResumed()
    {
        TimerIntervalAction tickAction = new TimerIntervalAction(this, (x) => gameplayResume_UI.SetActive(false), () => { }, GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(tickAction);

    }

    private void UpdateGameplayStatistics()
    {
        gameplay_accuracyPercent.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        gameplay_accuracySlider.value = (float)gameplayManager.CurrentAccuracy;
        gameplay_score.text = ((int)math.round(gameplayManager.CurrentScore)).ToString();
        gameplay_progress.text = $"{gameplayManager.MatchHitCount + gameplayManager.MismatchHitCount + gameplayManager.MissCount} | {gameplayManager.MaxHitboxCount}";
        gameplay_progressSlider.value= (float)(gameplayManager.MatchHitCount + gameplayManager.MismatchHitCount + gameplayManager.MissCount) / gameplayManager.MaxHitboxCount;
    }
    private void GameplayManager_OnHitboxBombHit(VisualHitbox hitbox)
    {
        comboText.text = "0";
        gameplay_missCount.text = $"{gameplayManager.MissCount} | {gameplayManager.BombHitCount}";
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayWaitingForResume -= GameplayManager_OnGameplayWaitingForResume;
        gameplayManager.OnGameplayResumeTick -= GameplayManager_OnGameplayResumeTick;
        gameplayManager.OnGameplayResumed -= GameplayManager_OnGameplayResumed;

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
        UpdateGameplayStatistics();
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
        gameplay_missCount.text = $"{gameplayManager.MissCount} | {gameplayManager.BombHitCount}";
        UpdateGameplayStatistics();
    }

    private void GameplayManager_OnHitboxHit(VisualHitbox obj)
    {
        comboText.text = gameplayManager.CurrentCombo.ToString();
        gameplay_matchCount.text = gameplayManager.MatchHitCount.ToString();
        UpdateGameplayStatistics();
    }


    private void SetupGameplayUI()
    {
        comboText.text = "0";
        gameplay_chartCredit.text = gameplayManager.CurrentMetadata.BaseMetadata.ChartName;
        gameplay_songCredit.text = $"{gameplayManager.CurrentMetadata.BaseMetadata.SongArtist} - {gameplayManager.CurrentMetadata.BaseMetadata.SongName}";
        gameplay_chartDifficulty.text = $"Difficulty {gameplayManager.CurrentMetadata.BaseMetadata.ChartDifficulty}";
        gameplay_matchCount.text = "0";
        gameplay_mismatchCount.text = "0";
        gameplay_missCount.text = "0 | 0";
        gameplay_accuracyPercent.text = "100.00%";
        gameplay_score.text = "0";

        gameplay_progress.text = $"0 | {gameplayManager.MaxHitboxCount}";
        gameplay_progressSlider.value = 0f;
        gameplay_accuracySlider.value = 1f;
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
        SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => Task.CompletedTask);
    }

    public void UI_OnRetryButton()
    {
        gameplayManager.InvokeGameplayRestartEvent();
    }
}
