using UnityEngine;

/// <summary>
/// A class to handle music during editor playback.
/// </summary>
public class EditorMusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;
    private EditorManager editorManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        editorManager = EditorManager.EditorInstance;

        editorManager.OnMusicAudioClipLoaded += EditorManager_OnMusicAudioClipLoaded;
        editorManager.OnPlaybackStart += EditorManager_OnPlaybackStart;
        editorManager.OnPlaybackStopped += EditorManager_OnPlaybackStopped;
    }

    private void EditorManager_OnPlaybackStopped()
    {
        musicAudioSource.Stop();
    }

    private void EditorManager_OnPlaybackStart()
    {
        if (musicAudioSource.clip == null)
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("No audio clip");
            return;
        }

        AudioEngine.AudioInstance.PlayAudioSource(musicAudioSource, 0d, 1f, editorManager.EditorPreviewTime, editorManager.PlaybackSpeed);
    }

    private void EditorManager_OnMusicAudioClipLoaded(AudioClip obj)
    {
        if (obj == null)
        {
            musicAudioSource.clip = null;
            return;
        }
        GameManager.GameInstance.InvokeInformationDisplayNeeded("Loaded audio clip");
        musicAudioSource.clip = obj;
    }
}
