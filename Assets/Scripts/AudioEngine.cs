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

    [SerializeField] private int MAXNUMBEROFSOURCES = 99;
    [SerializeField] private AudioSource audioSourcePrefab;

    private const int k_NUMBEROFSAMPLES = 2048;
    public float[] AmplitudeOfSample { get; private set; } = new float[k_NUMBEROFSAMPLES];

    private float sampleFrequencyWidth;

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
        audioSourcePool = new AudioSource[MAXNUMBEROFSOURCES];
        for (int i = 0; i < MAXNUMBEROFSOURCES; i++)
        {
            audioSourcePool[i] = Instantiate(audioSourcePrefab, transform, false);
        }
    }

    private void Start()
    {
        sampleFrequencyWidth = AudioSettings.outputSampleRate / (2f * k_NUMBEROFSAMPLES);
    }



    private void Update()
    {
        AudioListener.GetSpectrumData(AmplitudeOfSample, 0, FFTWindow.BlackmanHarris);
    }

    /// <summary>
    /// Gets the start and end indices given a frequency band defined by <paramref name="lowFrequency"/> and <paramref name="highFrequency"/>. <br></br>
    /// Since <see cref="AudioListener.GetSpectrumData(float[], int, FFTWindow)"/> samples at discrete frequencies, the error is half of <see cref="sampleFrequencyWidth"/>.
    /// </summary>
    /// <param name="lowFrequency"></param>
    /// <param name="highFrequency"></param>
    public void QuerySampledAudioBetweenFrequency(float lowFrequency, float highFrequency, out int startIndex, out int endIndex)
    {
        startIndex = Mathf.Max(Mathf.FloorToInt(lowFrequency / sampleFrequencyWidth), 0);
        endIndex = Mathf.Min(Mathf.CeilToInt(highFrequency / sampleFrequencyWidth), k_NUMBEROFSAMPLES - 1);
    }

    /// <summary>
    /// Plays an specified audio clip using a pre-generated audio source with an optional offset. <br></br>
    /// This is useful for one-shot audio.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="playOffsetTime"></param>
    public void PlayAudioClip(AudioClip clip, double playOffsetTime, float volume, double playbackSpeed, float panning, bool useLogScale = true)
    {
        if (playOffsetTime < 0d)
        {
            return;
        }

        poolIndex = (poolIndex + 1) % MAXNUMBEROFSOURCES; // cycle through the pool index

        AudioSource source = audioSourcePool[poolIndex];
        source.pitch = (float)playbackSpeed;
        source.clip = clip;
        source.volume = useLogScale ? RemapLinearVolumeToScale(volume) : volume;
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
    public void PlayAudioSource(AudioSource source, double playOffsetTime, float volume, double playStartTime, double playbackSpeed, float panning, bool useLogScale = true)
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

        if (seekSamples > source.clip.samples)
        { // invalid seek time, don't play anything
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Seek time Longer than Audio");
            return;
        }

        source.timeSamples = seekSamples;
        source.pitch = (float)playbackSpeed;
        source.volume = useLogScale ? RemapLinearVolumeToScale(volume) : volume;
        source.panStereo = panning;
        source.PlayScheduled(AudioSettings.dspTime + playOffsetTime);
    }

    private TimerStopwatchAction fadeInStopwatch;
    public void FadeInAudioSource(AudioSource source, float maxVolume, double fadeInTime, Action callback, bool useLogScale = true)
    {
        if (source == null)
        {
            return;
        }

        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(fadeOutStopwatch);

        fadeInTime = math.max(0.01d, fadeInTime);
        fadeInStopwatch = new TimerStopwatchAction(source, x =>
        {
            double progress = x / fadeInTime;
            float volume = math.lerp(0f, maxVolume, MathF.Cbrt((float)progress));
            source.volume = useLogScale ? RemapLinearVolumeToScale(volume) : volume;
        }, () => { }, 0d, TimerBehavior.TEMPORARY, fadeInTime, false);

        TimerIntervalAction callbackTimer = new TimerIntervalAction(source, x => callback?.Invoke(), () => { }, fadeInTime, TimerBehavior.TEMPORARY, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeInStopwatch);
        DSPTimerEngine.TimerInstance.AddActionToTimer(callbackTimer);
    }

    private TimerStopwatchAction fadeOutStopwatch;

    public void FadeOutAudioSource(AudioSource source, double fadeOutTime, Action callback, bool useLogScale = true)
    {
        if (source == null)
        {
            return;
        }

        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(fadeInStopwatch);
        fadeOutTime = math.max(0.01d, fadeOutTime);
        float startingVolume = RemapScaledVolumeToLinearVolume(source.volume);
        fadeOutStopwatch = new TimerStopwatchAction(source, x =>
        {
            double progress = x / fadeOutTime;
            float volume = math.lerp(startingVolume, 0f, MathF.Cbrt((float)progress));
            source.volume = useLogScale ? RemapLinearVolumeToScale(volume) : volume;
        }, () => { }, 0d, TimerBehavior.TEMPORARY, fadeOutTime, false);
        TimerIntervalAction callbackTimer = new TimerIntervalAction(source, x => callback?.Invoke(), () => { }, fadeOutTime, TimerBehavior.TEMPORARY, 0d);

        DSPTimerEngine.TimerInstance.AddActionToTimer(fadeOutStopwatch);
        DSPTimerEngine.TimerInstance.AddActionToTimer(callbackTimer);

    }

    public void EditAudioSource(AudioSource source, float volume, bool useLogScale = true)
    {
        source.volume = useLogScale ? RemapLinearVolumeToScale(volume) : volume;
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

    /// <summary>
    /// Converts a linear volume in [0, 1] into a scale that uses dB (log scale). <br></br>
    /// For optimization to reduce the number of <see cref="Mathf.Pow(float, float)"/> calls, we use a cubic curve instead.
    /// </summary>
    /// <param name="linearVolume"></param>
    /// <returns></returns>
    private float RemapLinearVolumeToScale(float linearVolume)
    {
        return linearVolume * linearVolume * linearVolume;
    }

    private float RemapScaledVolumeToLinearVolume(float scaledVolume)
    {
        return MathF.Cbrt(scaledVolume);
    }
}
