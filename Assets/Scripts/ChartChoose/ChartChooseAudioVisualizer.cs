using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// A class to handle the audio visualization logic in the chart select screen. <br></br>
/// The visualizer should be a circle that grows in size according to low frequency band (bass / kicks) since we only want the rhythm, not melody. <br></br>
/// </summary>
public class ChartChooseAudioVisualizer : BaseDynamicSpectrumObject
{
    [SerializeField] private RectTransform visualizerRectTransform;

    protected override void UpdateDynamicObject(float normalizedAmplitude)
    {
        Vector3 enlargedScale = Vector3.Lerp(Vector3.one, maxEnlargedScale, normalizedAmplitude * normalizedAmplitude);
        visualizerRectTransform.localScale = enlargedScale;
    }

    private readonly Vector3 maxEnlargedScale = new Vector3(1.15f, 1.15f, 1f);
}
