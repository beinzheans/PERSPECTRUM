using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to handle the audio visualization logic in the chart select screen. <br></br>
/// The visualizer should be a circle that grows in size according to low frequency band (bass / kicks) since we only want the rhythm, not melody. <br></br>
/// </summary>
public class ChartChooseAudioVisualizer : BaseDynamicSpectrumObject
{
    [SerializeField] private Image visualizerImage;
    private static readonly int k_SHADER_GLOWRADIUSKEY = Shader.PropertyToID("_GlowRadius");
    protected override void UpdateDynamicObject(float normalizedAmplitude)
    {
        Vector3 enlargedScale = Vector3.Lerp(Vector3.one, maxEnlargedScale, normalizedAmplitude * normalizedAmplitude);
        float glowRadius = Mathf.Lerp(minGlowRadius, maxGlowRadius, normalizedAmplitude * normalizedAmplitude);
        visualizerImage.material.SetFloat(k_SHADER_GLOWRADIUSKEY, glowRadius);
        visualizerImage.rectTransform.localScale = enlargedScale;
    }

    private readonly Vector3 maxEnlargedScale = new Vector3(1.025f, 1.025f, 1f);
    private const float minGlowRadius = 0.2f;
    private const float maxGlowRadius = 0.4f;
}
