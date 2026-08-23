using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A class to handle UI behavior during gameplay, except for the cursor. <br></br>
/// The cursor UI is handled by <see cref="GameplayUICursor"/>.
/// </summary>
public class GameplayUIBehavior : MonoBehaviour
{
    private GameplayManager gameplayManager;

    [Header("Gameplay UI")]
    [SerializeField] private RectTransform gameplayUI;
    [SerializeField] private UIElasticText comboText;

    [SerializeField] private TMP_Text gameplay_chartCredit;
    [SerializeField] private TMP_Text gameplay_songCredit;
    [SerializeField] private UIElasticText gameplay_matchCount;
    [SerializeField] private UIElasticText gameplay_mismatchCount;
    [SerializeField] private UIElasticText gameplay_missCount;
    [SerializeField] private TMP_Text gameplay_accuracyPercent;
    [SerializeField] private UIElasticText gameplay_score;
    [SerializeField] private TMP_Text gameplay_chartDifficulty;
    [SerializeField] private Slider gameplay_accuracySlider;

    [SerializeField] private TMP_Text gameplay_progress;
    [SerializeField] private Slider gameplay_progressSlider;
    [SerializeField] private UIElastic gameplay_progressSlider_Handle;

    [SerializeField] private UIElastic gameplay_matchIcon;
    [SerializeField] private UIElastic gameplay_mismatchIcon;
    [SerializeField] private UIElastic gameplay_missIcon;

    [SerializeField] private UIDynamic[] dynamicUIElements = new UIDynamic[0];

    [Header("Gameplay Start UI")]
    [SerializeField] private CanvasGroup gameplayStart_UICanvasGroup;
    [SerializeField] private TMP_Text gameplayStart_colorText;
    [SerializeField] private TMP_Text gameplayStart_restartText;

    [Header("Gameplay Resume UI")]
    [SerializeField] private GameObject gameplayResume_UI;
    [SerializeField] private Image gameplayResume_background;
    [SerializeField] private UIElasticText gameplayResume_tickText;

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
            gameplayResume_tickText.gameObject.SetActive(true);

            gameplayResume_background.color = new Color(0f, 0f, 0f, k_GAMERESUMEBACKGROUNDALPHA * (float)tick / GameplayResumeManager.k_NUMBEROFLEADINTICKS);
            gameplayResume_tickText.UIText.color = new Color(1f, 1f, 1f, k_GAMERESUMEBACKGROUNDALPHA * (float)tick / GameplayResumeManager.k_NUMBEROFLEADINTICKS);
            gameplayResume_tickText.SetText(tick.ToString(), k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);

            tick--;
        }, () => { }, GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, TimerBehavior.TEMPORARY, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(tickAction);
    }

    private void GameplayManager_OnGameplayWaitingForResume()
    {
        gameplayResume_tickText.gameObject.SetActive(false);
        gameplayStart_UICanvasGroup.gameObject.SetActive(true);
        gameplayResume_UI.SetActive(true);
        tick = GameplayResumeManager.k_NUMBEROFLEADINTICKS;
        gameplayResume_background.color = new Color(0f, 0f, 0f, k_GAMERESUMEBACKGROUNDALPHA);

        SetupStartGameplayUI(GameplayManager.k_TIMEOFFSET);
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(resumeUI_setInactiveTimer);
    }

    private TimerIntervalAction resumeUI_setInactiveTimer;
    private void GameplayManager_OnGameplayResumed()
    {
        resumeUI_setInactiveTimer = new TimerIntervalAction(this, (x) => gameplayResume_UI.SetActive(false), () => { }, GameplayManager.k_TIMEOFFSET + GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, TimerBehavior.TEMPORARY, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(resumeUI_setInactiveTimer);

    }

    private readonly Vector2 k_DEFAULTGAMEPLAYBOUNCESIZE = new Vector2(0.9f, 1.2f);
    private readonly Vector2 k_MISSGAMEPLAYBOUNCESIZE = new Vector2(1.2f, 0.9f);
    private readonly double k_DEFAULTGAMEPLAYBOUNCETIME = 0.05d;
    private readonly double k_DEFAULTGAMEPLAYICONBOUNCETIME = 0.1d;
    private void UpdateGameplayStatistics()
    {
        gameplay_accuracyPercent.text = $"{gameplayManager.CurrentAccuracy * 100d:F2}%";
        gameplay_accuracySlider.value = (float)gameplayManager.CurrentAccuracy;
        gameplay_score.SetText(((int)math.round(gameplayManager.CurrentScore)).ToString(), k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_progress.text = $"{gameplayManager.MatchHitCount + gameplayManager.MismatchHitCount + gameplayManager.MissCount} | {gameplayManager.MaxHitboxCount}";
        gameplay_progressSlider.value = (float)(gameplayManager.MatchHitCount + gameplayManager.MismatchHitCount + gameplayManager.MissCount) / gameplayManager.MaxHitboxCount;
        gameplay_progressSlider_Handle.PulseElasticSize(k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);

    }
    private void GameplayManager_OnHitboxBombHit(VisualHitbox hitbox)
    {
        comboText.SetText("0", k_MISSGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_missCount.SetText($"{gameplayManager.MissCount} | {gameplayManager.BombHitCount}", k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_missIcon.PulseElasticSize(k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYICONBOUNCETIME);
        UpdateGameplayStatistics();
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
        comboText.SetText(gameplayManager.CurrentCombo.ToString(), new Vector2(0.95f, 1.1f), k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_mismatchCount.SetText(gameplayManager.MismatchHitCount.ToString(), k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_mismatchIcon.PulseElasticSize(k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYICONBOUNCETIME);
        UpdateGameplayStatistics();
    }

    private void GameplayManager_OnGameplayEnded()
    {
        gameplayUI.gameObject.SetActive(false);
        gameplayStart_UICanvasGroup.gameObject.SetActive(false);
        endscreenUI.SetActive(true);
        SetupEndscreenUI();
    }

    private void GameplayManager_OnGameplayStarted()
    {
        gameplayUI.gameObject.SetActive(true);
        gameplayStart_UICanvasGroup.gameObject.SetActive(true);
        endscreenUI.SetActive(false);
        SetupGameplayUI();
        SetupStartGameplayUI(GameplayManager.k_STARTTIMEOFFSET);
    }

    private void GameplayManager_OnHitboxMiss(VisualHitbox obj)
    {
        comboText.SetText("0", k_MISSGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_missCount.SetText($"{gameplayManager.MissCount} | {gameplayManager.BombHitCount}", k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_missIcon.PulseElasticSize(k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYICONBOUNCETIME);
        UpdateGameplayStatistics();
    }

    private void GameplayManager_OnHitboxHit(VisualHitbox obj)
    {
        comboText.SetText(gameplayManager.CurrentCombo.ToString(), k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_matchCount.SetText(gameplayManager.MatchHitCount.ToString(), k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYBOUNCETIME);
        gameplay_matchIcon.PulseElasticSize(k_DEFAULTGAMEPLAYBOUNCESIZE, k_DEFAULTGAMEPLAYICONBOUNCETIME);
        UpdateGameplayStatistics();
    }


    private void SetupGameplayUI()
    {
        comboText.SetTextWithoutElastic("0");
        gameplay_chartCredit.text = gameplayManager.CurrentMetadata.BaseMetadata.ChartName;
        gameplay_songCredit.text = $"{gameplayManager.CurrentMetadata.BaseMetadata.SongArtist} - {gameplayManager.CurrentMetadata.BaseMetadata.SongName}";
        gameplay_chartDifficulty.text = $"Difficulty {gameplayManager.CurrentMetadata.BaseMetadata.ChartDifficulty}";
        gameplay_matchCount.SetTextWithoutElastic("0");
        gameplay_mismatchCount.SetTextWithoutElastic("0");
        gameplay_missCount.SetTextWithoutElastic("0 | 0");
        gameplay_accuracyPercent.text = "100.00%";
        gameplay_score.SetTextWithoutElastic("0");

        gameplay_progress.text = $"0 | {gameplayManager.MaxHitboxCount}";
        gameplay_progressSlider.value = 0f;
        gameplay_accuracySlider.value = 1f;
    }

    private const double k_GAMEPLAYSTART_FADEINTIME = 0.25d;
    private const double k_GAMEPLAYSTART_FADEOUTTIME = 0.25d;

    TimerStopwatchAction fadeInTimer;
    TimerStopwatchAction fadeOutTimer;
    TimerIntervalAction setInactiveTimer;
    private void SetupStartGameplayUI(double aliveTime)
    {
        if (k_GAMEPLAYSTART_FADEINTIME + k_GAMEPLAYSTART_FADEOUTTIME > aliveTime)
        {
            Debug.LogWarning($"Tried to show the start UI, but the alive time parameter was too short.");
            return;
        }

        gameplayStart_UICanvasGroup.alpha = 0f;

        fadeInTimer = new TimerStopwatchAction(this, x =>
        {
            double t = x / k_GAMEPLAYSTART_FADEINTIME;
            gameplayStart_UICanvasGroup.alpha = Mathf.Lerp(0f, 1f, (float)t);
        }, () => { }, 0d, TimerBehavior.TEMPORARY, k_GAMEPLAYSTART_FADEINTIME, false);

        fadeOutTimer = new TimerStopwatchAction(this, x =>
        {
            double t = x / k_GAMEPLAYSTART_FADEOUTTIME;
            gameplayStart_UICanvasGroup.alpha = Mathf.Lerp(1f, 0f, (float)t);
        }, () => { }, aliveTime - k_GAMEPLAYSTART_FADEOUTTIME, TimerBehavior.TEMPORARY, k_GAMEPLAYSTART_FADEOUTTIME, false);

        setInactiveTimer = new TimerIntervalAction(this, x => gameplayStart_UICanvasGroup.gameObject.SetActive(false), () => { }, aliveTime, TimerBehavior.TEMPORARY, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeInTimer);
        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeOutTimer);
        DSPTimerEngine.TimerInstance.AddActionToTimer(setInactiveTimer);

        gameplayStart_colorText.text = "Switch Color\n" +
                                       $"{GameManager.GameInstance.InputActions.Gameplay.SwitchAInput.GetBindingDisplayString()} | {GameManager.GameInstance.InputActions.Gameplay.SwitchBInput.GetBindingDisplayString()}";

        gameplayStart_restartText.text = "Restart\n" +
                                         GameManager.GameInstance.InputActions.Gameplay.RestartInput.GetBindingDisplayString();

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

    private const float k_UIDISPLACEMENTSCALER = 0.5f;
    private void Update()
    {
        Vector2 UIDisplacement = k_UIDISPLACEMENTSCALER * gameplayManager.CurrentPlayAreaDisplacement / gameplayManager.WorldToScreenSizeRatioOfPreview;

        for (int i = 0; i < dynamicUIElements.Length; i++)
        {
            dynamicUIElements[i].Displace(UIDisplacement);
        }
    }
}
