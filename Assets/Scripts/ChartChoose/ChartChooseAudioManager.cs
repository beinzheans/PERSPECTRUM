using Newtonsoft.Json.Linq;
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
        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        music_AudioSource.volume = GameManager.GameInstance.GlobalSettings.SongVolume;
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
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(playAction);
        AudioEngine.AudioInstance.FadeOutAudioSource(music_AudioSource, k_MUSICFADETIME, () => music_AudioSource.Stop());
    }

    private void OnDestroy()
    {
        chartChooseManager.OnChartButtonClicked -= ChartChooseManager_OnChartButtonClicked;
        GameManager.GameInstance.OnPauseMenuEnable -= GameInstance_OnPauseMenuEnable;
        GameManager.GameInstance.OnPauseMenuDisable -= GameInstance_OnPauseMenuDisable;
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
        chartChooseManager.OnChartDeleted -= ChartChooseManager_OnChartDeleted;
    }

    private const double k_MUSICFADETIME = 0.5d;
    private const double k_MUSICPREVIEWTIME = 15d;

    private TimerIntervalAction playAction;
    private async void ChartChooseManager_OnChartButtonClicked(ChartButtonBehavior obj)
    {
        GamePersistenceManager.LoadChartFile(obj.AssociatedFullFilePath, out _, out string metadataJson, out byte[] audioBytes);

        (bool result, AudioClip clip) = await GamePersistenceManager.GetAudioClipFromByteArray(audioBytes, true);


        JObject metadataJObject = JObject.Parse(metadataJson);
        JObject chartJObject = new JObject();
        bool convertResult = GameVersionConverter.ConvertChartVersionToCurrentGameVersion(in chartJObject, in metadataJObject, out _, out JObject convertedmetadataJObject);

        if (!convertResult)
        {
            return;
        }

        GamePersistenceManager.GetMetadataOfEditorChartFromJson(convertedmetadataJObject.ToString(), out EditorChartMetadata metadata);

        if (!result)
        {
            return;
        }

        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(playAction);
        playAction = new TimerIntervalAction(this, x =>
        {
            AudioEngine.AudioInstance.FadeOutAudioSource(music_AudioSource, k_MUSICFADETIME, 
                () => {
                    music_AudioSource.Stop();

                    music_AudioSource.clip = null;
                    music_AudioSource.clip = clip; // we do this because we are handling streaming audio clip.

                    AudioEngine.AudioInstance.PlayAudioSource(music_AudioSource, 0d, 0f, metadata.PreviewStartTime, 1d, 0f);
                    AudioEngine.AudioInstance.FadeInAudioSource(music_AudioSource, GameManager.GameInstance.GlobalSettings.SongVolume, k_MUSICFADETIME, () => { });
                });
        }, () => { }, 0d, k_MUSICPREVIEWTIME, 0);

        DSPTimerEngine.TimerInstance.AddActionToTimer(playAction);
    }
}
