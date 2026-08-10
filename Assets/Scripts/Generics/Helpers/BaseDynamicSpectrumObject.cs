using UnityEngine;

/// <summary>
/// A class to make objects dynamic based on the frequency spectrum according to <see cref="AudioEngine"/>. <br></br>
/// Allows customized frequency band and decay factor on a per-object basis. <br></br>
/// To account for volume variance, we will use dynamic peak tracking approach so we can remap the result of <see cref="AudioListener.GetSpectrumData(float[], int, UnityEngine.FFTWindow)"/> correctly.
/// </summary>
public abstract class BaseDynamicSpectrumObject : MonoBehaviour
{
    [SerializeField] protected float lowFrequencySample = 50f;
    [SerializeField] protected float highFrequencySample = 200f;

    /// <summary>
    /// The decay factor of the <see cref="currentDynamicPeak"/>.
    /// This defines the percentage of <see cref="peakDecayFactor"/> we retain after one second.
    /// </summary>
    [SerializeField] private float peakDecayFactor = 0.5f;
    private float currentDynamicPeak;
    protected float normalizedAmplitude = 0f;

    private void Update()
    {
        AudioEngine.AudioInstance.QuerySampledAudioBetweenFrequency(lowFrequencySample, highFrequencySample, out int startSampleIndex, out int endSampleIndex);

        if (startSampleIndex > endSampleIndex)
        {
            UpdateDynamicObject(0f);
            return;
        }

        float sum = 0f;

        for (int i = startSampleIndex; i < endSampleIndex; i++)
        {
            sum += AudioEngine.AudioInstance.AmplitudeOfSample[i];
        }

        float average = sum / (endSampleIndex - startSampleIndex + 1);

        float decayedPeak = currentDynamicPeak * Mathf.Pow(peakDecayFactor, Time.deltaTime);

        currentDynamicPeak = Mathf.Max(decayedPeak, average);

        if (MathHelper.IsTwoFloatsEqualWithEpsilion(currentDynamicPeak, 0f))
        {
            return;
        }

        normalizedAmplitude = Mathf.Clamp01(average / currentDynamicPeak);
        UpdateDynamicObject(normalizedAmplitude);
    }

    /// <summary>
    /// Custom implementation of events to make the object dynamic based on the normalized amplitude
    /// </summary>
    /// <param name="normalizedAmplitude"></param>
    protected abstract void UpdateDynamicObject(float normalizedAmplitude);
}
