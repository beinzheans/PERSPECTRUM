using UnityEngine;

public class GameplayAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;

    [SerializeField] private AudioClip matchHitsound_AClip;
    [SerializeField] private AudioClip matchHitsound_BClip;
    [SerializeField] private AudioClip mismatchHitsoundClip;

    private GameplayManager gameplayManager;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        gameplayManager.OnGameplayChartLoaded += GameplayManager_OnGameplayAudioLoaded;

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnHitboxBecomeActive += GameplayManager_OnHitboxBecomeActive;
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

        if (obj.HitboxType == HitboxType.A)
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_AClip, hitboxOffset - GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
        }
        else
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_BClip, hitboxOffset - GameManager.GameInstance.GlobalSettings.AudioOffsetMs / 1000d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
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

        gameplayManager.OnGameplayChartLoaded -= GameplayManager_OnGameplayAudioLoaded;
        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnGameplayRestarted -= GameplayManager_OnGameplayRestarted;
    }
    private void GameplayManager_OnGameplayAudioLoaded(AudioClip obj, EditorChartMetadata metadata)
    {
        musicAudioSource.clip = obj;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        if (GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds)
        {
            return;
        }

        AudioEngine.AudioInstance.PlayAudioClip(mismatchHitsoundClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        if (GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds)
        {
            return;
        }

        if (obj.HitboxType == HitboxType.A)
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_AClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
        }
        else
        {
            AudioEngine.AudioInstance.PlayAudioClip(matchHitsound_BClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
        }
    }

    private void GameplayManager_OnGameplayStarted()
    {
        AudioEngine.AudioInstance.PlayAudioSource(musicAudioSource, GameplayManager.k_TIMEOFFSET, GameManager.GameInstance.GlobalSettings.SongVolume, 0d, 1d);
    }
}
