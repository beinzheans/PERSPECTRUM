using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A class to handle the elasticity of UI elements. That is, to allow UI elements to bounce or shrink.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIElastic : MonoBehaviour
{
    public RectTransform RectTransform { get; private set; }
    protected TimerStopwatchAction elasticStopwatch;

    protected virtual void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public virtual void SetElasticTimer(Vector2 elasticSize, double elasticTime)
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(elasticStopwatch);
        double internal_elasticTime = math.max(0.001d, elasticTime);
        elasticStopwatch = new TimerStopwatchAction(this, x => AnimateElasticSize(x, elasticSize, internal_elasticTime), () => { }, 0d, internal_elasticTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(elasticStopwatch);
    }

    protected void AnimateElasticSize(double time, Vector2 elasticSize, double elasticTime)
    {
        Vector2 size = Vector2.Lerp(elasticSize, Vector2.one, (float)(time / elasticTime));
        RectTransform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
