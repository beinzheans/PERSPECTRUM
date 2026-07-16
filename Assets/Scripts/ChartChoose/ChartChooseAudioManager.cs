using UnityEngine;

public class ChartChooseAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource music_AudioSource;
    private ChartChooseManager chartChooseManager;

    private void Start()
    {
        chartChooseManager = ChartChooseManager.ChartChooseInstance;

        chartChooseManager.OnChartButtonClicked += ChartChooseManager_OnChartButtonClicked;
        GameManager.GameInstance.OnPauseMenuEnable += GameInstance_OnPauseMenuEnable;
        GameManager.GameInstance.OnPauseMenuDisable += GameInstance_OnPauseMenuDisable;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;
    }

    private void GameInstance_OnPauseMenuDisable()
    {
        music_AudioSource.UnPause();
    }

    private void GameInstance_OnPauseMenuEnable()
    {
        music_AudioSource.Pause();
    }

    private void ChartChooseManager_OnChartDeleted()
    {
        music_AudioSource.Stop();
    }

    private void OnDestroy()
    {
        chartChooseManager.OnChartButtonClicked -= ChartChooseManager_OnChartButtonClicked;
        GameManager.GameInstance.OnPauseMenuEnable -= GameInstance_OnPauseMenuEnable;
        GameManager.GameInstance.OnPauseMenuDisable -= GameInstance_OnPauseMenuDisable;
        chartChooseManager.OnChartDeleted -= ChartChooseManager_OnChartDeleted;
    }

    private async void ChartChooseManager_OnChartButtonClicked(ChartButtonBehavior obj)
    {
        GamePersistenceManager.LoadChartFile(obj.AssociatedFullFilePath, out _, out _, out byte[] audioBytes);

        (bool result, AudioClip clip) = await GamePersistenceManager.GetAudioClipFromByteArray(audioBytes, true);

        if (!result)
        {
            return;
        }

        music_AudioSource.clip = clip;
        AudioEngine.AudioInstance.PlayAudioSource(music_AudioSource, 0d, GameManager.GameInstance.GlobalSettings.SongVolume, 0d, 1d, 0f);
    }
}
