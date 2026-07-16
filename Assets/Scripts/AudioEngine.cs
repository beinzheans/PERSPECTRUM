using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// A class to handle all audio logic in the game. <br></br>
/// Note the automatically generated audio sources used by <see cref="PlayAudioClip(UnityEngine.AudioClip, double, float)"/> will be 2D sources. <br></br>
/// To play 3D audio or audio that is independent, use <see cref="PlayAudioSource(UnityEngine.AudioSource, double, float)"/> that specifies a custom audio source.
/// </summary>
public class AudioEngine : MonoBehaviour
{
    public static AudioEngine AudioInstance;

    [SerializeField] private int k_MAXIMUMNUMBEROFSOURCES = 99;

    [SerializeField] private AudioSource audioSourcePrefab;

    private AudioSource[] audioSourcePool;
    private int poolIndex = 0;

    private void Awake()
    {
        if (AudioInstance == null)
        {
            AudioInstance = this;
            InstantiateAudioPool();
            return;
        }

        Destroy(gameObject);
    }

    private void InstantiateAudioPool()
    {
        audioSourcePool = new AudioSource[k_MAXIMUMNUMBEROFSOURCES];
        for (int i = 0; i < k_MAXIMUMNUMBEROFSOURCES; i++)
        {
            audioSourcePool[i] = Instantiate(audioSourcePrefab, transform, false);
        }
    }

    /// <summary>
    /// Plays an specified audio clip using a pre-generated audio source with an optional offset. <br></br>
    /// This is useful for one-shot audio.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="playOffsetTime"></param>
    public void PlayAudioClip(AudioClip clip, double playOffsetTime, float volume, double playbackSpeed, float panning)
    {
        if (playOffsetTime < 0d)
        {
            return;
        }

        poolIndex = (poolIndex + 1) % k_MAXIMUMNUMBEROFSOURCES; // cycle through the pool index
        AudioSource source = audioSourcePool[poolIndex];
        source.pitch = (float)playbackSpeed;
        source.clip = clip;
        source.volume = volume;
        source.panStereo = panning;
        audioSourcePool[poolIndex].PlayScheduled(DSPTimerEngine.TimerInstance.CurrentDSPTime + playOffsetTime);
    }

    /// <summary>
    /// Plays an specified audio source with an optional offset. <br></br>
    /// This is useful for more controlled audio that has their own dedicated audio source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="playOffsetTime"></param>
    /// <param name="playStartTime">The time at which the audio source starts playing.</param>
    public void PlayAudioSource(AudioSource source, double playOffsetTime, float volume, double playStartTime, double playbackSpeed, float panning)
    {
        if (playOffsetTime < 0d)
        {
            return;
        }

        if (source.clip == null)
        {
            return;
        }

        int seekSamples = (int)math.round(playStartTime * source.clip.frequency);

        if (seekSamples > source.clip.samples) // invalid seek time, don't play anything
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Preview Longer than Audio");
            return;
        }

        source.timeSamples = seekSamples;
        source.pitch = (float)playbackSpeed;
        source.volume = volume;
        source.panStereo = panning;
        source.PlayScheduled(AudioSettings.dspTime + playOffsetTime);
    }

    private TimerStopwatchAction fadeInStopwatch;
    public void FadeInAudioSource(AudioSource source, float maxVolume, double fadeInTime, Action callback)
    {
        fadeInTime = math.max(0.01d, fadeInTime);
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(fadeInStopwatch);
        fadeInStopwatch = new TimerStopwatchAction(source, x =>
        {
            source.volume = math.lerp(0f, maxVolume, (float)(x / fadeInTime));
        }, () => callback?.Invoke(), 0d, fadeInTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeInStopwatch);
    }

    private TimerStopwatchAction fadeOutStopwatch;

    public void FadeOutAudioSource(AudioSource source, double fadeOutTime, Action callback)
    {
        fadeOutTime = math.max(0.01d, fadeOutTime);
        float startingVolume = source.volume;
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(fadeOutStopwatch);
        fadeOutStopwatch = new TimerStopwatchAction(source, x =>
        {
            source.volume = math.lerp(startingVolume, 0f, (float)(x / fadeOutTime));
        }, () => callback?.Invoke(), 0d, fadeOutTime, false);

        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeOutStopwatch);
    }

    public void EditAudioSource(AudioSource source, float volume)
    {
        source.volume = volume;
    }

    
    public async Task<(bool result, AudioClip clip)> GetAudioClipFromLocalFile(string fullFilePath)
    {
        Uri request = new Uri("file://" + fullFilePath);

        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(request, AudioType.MPEG);

        UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load audio clip from local file");
            webRequest.Dispose();
            return (false, null);
        }

        AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(webRequest);

        webRequest.Dispose();
        return (true, loadedClip);
    }

    public async Task<(bool result, AudioClip clip)> GetAudioClipFromLocalFileStreaming(string fullFilePath)
    {
        Uri request = new Uri("file://" + fullFilePath);

        UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(request, AudioType.MPEG);
        DownloadHandlerAudioClip downloadHandler = webRequest.downloadHandler as DownloadHandlerAudioClip;
        downloadHandler.streamAudio = true;

        UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();

        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to stream audio clip from local file");
            webRequest.Dispose();
            return (false, null);
        }

        AudioClip loadedClip = DownloadHandlerAudioClip.GetContent(webRequest);
        webRequest.Dispose();
        return (true, loadedClip);


    }
}
