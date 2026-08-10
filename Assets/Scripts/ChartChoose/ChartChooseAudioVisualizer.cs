using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A class to handle the audio visualization logic in the chart select screen. <br></br>
/// The visualizer should be a circle that grows in size according to low frequency band (bass / kicks) since we only want the rhythm, not melody. <br></br>
/// To account for volume variance, we will use dynamic peak tracking approach so we can remap the result of <see cref="AudioListener.GetSpectrumData(float[], int, UnityEngine.FFTWindow)"/> correctly.
/// </summary>
public class ChartChooseAudioVisualizer : MonoBehaviour
{
    private const int k_NUMBEROFSAMPLES = 1024;
    private float[] amplitudeOfSample = new float[k_NUMBEROFSAMPLES];

    private float sampleFrequencyWidth;

    [SerializeField] private float lowFrequencySample = 50f;
    [SerializeField] private float highFrequencySample = 200f;

    /// <summary>
    /// The decay factor of the <see cref="currentDynamicPeak"/>.
    /// This defines the percentage of <see cref="peakDecayFactor"/> we retain after one second.
    /// </summary>
    [SerializeField] private float peakDecayFactor = 0.5f;
    private float currentDynamicPeak;
    private float normalizedAmplitude = 0f;
    [Header("UI")]
    [SerializeField] private RectTransform visualizerRectTransform;
    [SerializeField] private float visualizerReturnTime;

    private TimerStopwatchAction visualizerReturnStopwatch;

    private void Start()
    {
        sampleFrequencyWidth = AudioSettings.outputSampleRate / (2f * k_NUMBEROFSAMPLES);
    }

    private void Update()
    {
        int startSampleIndex = Mathf.Max(Mathf.FloorToInt(lowFrequencySample / sampleFrequencyWidth), 0);
        int endSampleIndex = Mathf.Min(Mathf.CeilToInt(highFrequencySample / sampleFrequencyWidth), k_NUMBEROFSAMPLES - 1);

        if (startSampleIndex > endSampleIndex)
        {
            return;
        } 

        AudioListener.GetSpectrumData(amplitudeOfSample, 0, FFTWindow.BlackmanHarris);

        float sum = 0f;

        for (int i = startSampleIndex; i < endSampleIndex; i++)
        {
            sum += amplitudeOfSample[i];
        }

        float average = sum / (endSampleIndex - startSampleIndex + 1);

        float decayedPeak = currentDynamicPeak * Mathf.Pow(peakDecayFactor, Time.deltaTime);

        currentDynamicPeak = Mathf.Max(decayedPeak, average);

        if (MathHelper.IsTwoFloatsEqualWithEpsilion(currentDynamicPeak, 0f))
        {
            return;
        }

        normalizedAmplitude = Mathf.Clamp01(average / currentDynamicPeak);
        visualizerReturnStopwatch = new TimerStopwatchAction(this, ChangeVisualizerScale, () => { }, 0d, visualizerReturnTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(visualizerReturnStopwatch);
    }

    private readonly Vector3 maxEnlargedScale = new Vector3(1.1f, 1.1f, 1f);
    private void ChangeVisualizerScale(double time)
    {
        if (MathHelper.IsTwoDoublesEqualWithEpsilion(visualizerReturnTime, 0d))
        {
            visualizerRectTransform.localScale = Vector3.one;
            return;
        }

        Vector3 enlargedScale = Vector3.Lerp(Vector3.one, maxEnlargedScale, normalizedAmplitude);
        Vector3 scale = Vector3.Lerp(enlargedScale, Vector3.one, (float)(time / visualizerReturnTime));
        visualizerRectTransform.localScale = scale;
    }
}
