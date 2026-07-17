using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

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

    protected virtual void Start()
    {
        GameVirtualCursor.GameVirtualCursorInstance.OnVirtualCursorClickedUIElement += GameVirtualCursorInstance_OnVirtualCursorClickedUIElement;
    }

    protected virtual void OnDestroy()
    {
        GameVirtualCursor.GameVirtualCursorInstance.OnVirtualCursorClickedUIElement -= GameVirtualCursorInstance_OnVirtualCursorClickedUIElement;
    }

    protected readonly Vector2 k_DEFAULTSCALEWHENCLICKED = new Vector2(0.95f, 1.05f);
    protected const double k_DEFAULTSCALETIMERWHENCLICKED = 0.1d;
    private void GameVirtualCursorInstance_OnVirtualCursorClickedUIElement(GameObject gameObject)
    {
        if (gameObject != this.gameObject)
        {
            return;
        }

        PulseElasticSize(k_DEFAULTSCALEWHENCLICKED, k_DEFAULTSCALETIMERWHENCLICKED);
    }

    public virtual void PulseElasticSize(Vector2 elasticScale, double elasticTime)
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(elasticStopwatch);
        double internal_elasticTime = math.max(0.001d, elasticTime);
        elasticStopwatch = new TimerStopwatchAction(this, x => AnimateElasticSize(x, elasticScale, internal_elasticTime), () => { }, 0d, internal_elasticTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(elasticStopwatch);
    }

    protected void AnimateElasticSize(double time, Vector2 elasticSize, double elasticTime)
    {
        Vector2 size = Vector2.Lerp(elasticSize, Vector2.one, (float)(time / elasticTime));
        RectTransform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
