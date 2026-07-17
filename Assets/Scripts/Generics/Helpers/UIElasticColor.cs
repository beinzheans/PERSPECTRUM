using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIElasticColor : UIElastic
{
    public Graphic Graphic { get; private set; }
    private Color initialColor;
    protected override void Awake()
    {
        base.Awake();
        Graphic = GetComponent<Graphic>();
    }

    protected override void Start()
    {
        base.Start();
        initialColor = Graphic.color;
    }

    TimerStopwatchAction pulseTimer;
    public void PulseGraphicColor(Color newColor, double pulseTime)
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(pulseTimer);

        pulseTimer = new TimerStopwatchAction(this, x => Graphic.color = Color.Lerp(newColor, initialColor, (float)(x / pulseTime)), () => { }, 0d, pulseTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(pulseTimer);
    }
}
