using UnityEngine;

public class GameplayAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;

    [SerializeField] private AudioClip hitsoundClip;
    [SerializeField] private AudioClip missClip;

    private GameplayManager gameplayManager;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayAudioLoaded += GameplayManager_OnGameplayAudioLoaded;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
    }

    private void GameplayManager_OnGameplayAudioLoaded(AudioClip obj)
    {
        musicAudioSource.clip = obj;
    }

    private void GameplayManager_OnHitboxMiss(int obj)
    {
        AudioEngine.AudioInstance.PlayAudioClip(missClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        AudioEngine.AudioInstance.PlayAudioClip(hitsoundClip, 0d, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
    }

    private void GameplayManager_OnGameplayStarted()
    {
        AudioEngine.AudioInstance.PlayAudioSource(musicAudioSource, GameplayManager.k_TIMEOFFSET, GameManager.GameInstance.GlobalSettings.SongVolume, 0d, 1d);
    }
}
