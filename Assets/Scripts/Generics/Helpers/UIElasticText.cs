using TMPro;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A generic class to add a functionality to bounce the text when the text is updated.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class UIElasticText : MonoBehaviour
{
    public TMP_Text UIText { get; private set; }
    private TimerStopwatchAction elasticStopwatch;

    private void Awake()
    {
        UIText = GetComponent<TMP_Text>();
    }

    public void SetText(string text, Vector2 elasticSize, double elasticTime)
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(elasticStopwatch);
        double internal_elasticTime = math.max(0.01d, elasticTime);
        elasticStopwatch = new TimerStopwatchAction(this, x => AnimateElasticText(x, elasticSize, internal_elasticTime), () => { }, 0d, internal_elasticTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(elasticStopwatch);
        UIText.SetText(text);
    }

    public void SetTextWithoutElastic(string text)
    {
        UIText.SetText(text);
    }

    private void AnimateElasticText(double time, Vector2 elasticSize, double elasticTime)
    {
        Vector2 size = Vector2.Lerp(elasticSize, Vector2.one, (float)(time / elasticTime));
        UIText.rectTransform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
