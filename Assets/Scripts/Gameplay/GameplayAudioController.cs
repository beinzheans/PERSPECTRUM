using Unity.Mathematics;
using UnityEngine;

public class GameplayAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;

    [SerializeField] private AudioClip resumeTickClip;
    [SerializeField] private AudioClip matchHitsound_AClip;
    [SerializeField] private AudioClip matchHitsound_BClip;
    [SerializeField] private AudioClip mismatchHitsoundClip;

    private GameplayManager gameplayManager;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        GameManager.GameInstance.OnPauseMenuEnable += GameInstance_OnPauseMenuEnable;
        gameplayManager.OnGameplayChartLoaded += GameplayManager_OnGameplayAudioLoaded;
        gameplayManager.OnGameplayWaitingForResume += GameplayManager_OnGameplayWaitingForResume;
        gameplayManager.OnGameplayResumeTick += GameplayManager_OnGameplayResumeTick;
        gameplayManager.OnGameplayResumed += GameplayManager_OnGameplayResumed;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnHitboxBecomeActive += GameplayManager_OnHitboxBecomeActive;
    }

    private void GameInstance_OnPauseMenuEnable()
    {
        musicAudioSource.Stop();
    }

    private int tick = 0;
    private void GameplayManager_OnGameplayResumeTick()
    {
        if (tick >= GameplayResumeManager.k_NUMBEROFLEADINTICKS)
        {
            return;
        }

        AudioEngine.AudioInstance.PlayAudioClip(resumeTickClip, 0d, GameManager.GameInstance.GlobalSettings.UIVolume, 1f, 0f);
        tick++;
    }

    private void GameplayManager_OnGameplayResumed()
    {
        AudioEngine.AudioInstance.PlayAudioSource(musicAudioSource, 0d, GameManager.GameInstance.GlobalSettings.SongVolume, gameplayManager.CurrentGameplayTime, 1f, 0f);
    }

    private void GameplayManager_OnGameplayWaitingForResume()
    {
        tick = 0;
    }



    private void GameplayManager_OnHitboxBecomeActive(VisualHitbox obj)
    {
        if (!GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds)
        {
            return;
        }

        if (obj.HitboxType == HitboxType.BOMB)
        {
            return;
        }

        double hitboxOffset = obj.RenderTime - gameplayManager.CurrentGameplayTime;
        float panning = MathHelper.GetAudioPanningFromPosition(obj.NormalizedPosition);

        if (obj.HitboxType == HitboxType.A)
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_AClip, hitboxOffset - GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d, panning);
        }
        else
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_BClip, hitboxOffset - GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d, panning);
        }
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        musicAudioSource.Stop();
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        musicAudioSource.volume = GameManager.GameInstance.GlobalSettings.SongVolume;
    }

    private void OnDestroy()
    {
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
        GameManager.GameInstance.OnPauseMenuEnable -= GameInstance_OnPauseMenuEnable;
        gameplayManager.OnGameplayChartLoaded -= GameplayManager_OnGameplayAudioLoaded;
        gameplayManager.OnGameplayWaitingForResume -= GameplayManager_OnGameplayWaitingForResume;
        gameplayManager.OnGameplayResumed -= GameplayManager_OnGameplayResumed;

        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnGameplayRestarted -= GameplayManager_OnGameplayRestarted;
    }
    private void GameplayManager_OnGameplayAudioLoaded(AudioClip obj, Texture2D texture, EditorChartMetadata metadata)
    {
        musicAudioSource.clip = obj;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        if (GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds)
        {
            return;
        }

        AudioEngine.AudioInstance.PlayAudioClip(mismatchHitsoundClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d, MathHelper.GetAudioPanningFromPosition(obj.NormalizedPosition));
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        if (GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds)
        {
            return;
        }

        float panning = MathHelper.GetAudioPanningFromPosition(obj.NormalizedPosition);

        double hitboxOffset = obj.RenderTime - gameplayManager.CurrentGameplayTime;
        if (obj.HitboxType == HitboxType.A)
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_AClip, math.max(0d, hitboxOffset), GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d, panning);
        }
        else
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_BClip, math.max(0d, hitboxOffset), GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d, panning);
        }
    }

    private void GameplayManager_OnGameplayStarted()
    {
        AudioEngine.AudioInstance.PlayAudioSource(musicAudioSource, GameplayManager.k_TIMEOFFSET, GameManager.GameInstance.GlobalSettings.SongVolume, 0d, 1d, 0f);
    }
}
