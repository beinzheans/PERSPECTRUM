using UnityEngine;

public class GameplayAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;

    [SerializeField] private AudioClip matchHitsoundClip;
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
    }
    private void GameplayManager_OnGameplayAudioLoaded(AudioClip obj, EditorChartMetadata metadata)
    {
        musicAudioSource.clip = obj;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        AudioEngine.AudioInstance.PlayAudioClip(mismatchHitsoundClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        AudioEngine.AudioInstance.PlayAudioClip(matchHitsoundClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
    }

    private void GameplayManager_OnGameplayStarted()
    {
        AudioEngine.AudioInstance.PlayAudioSource(musicAudioSource, GameplayManager.k_TIMEOFFSET, GameManager.GameInstance.GlobalSettings.SongVolume, 0d, 1d);
    }
}
