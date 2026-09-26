using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIElasticColor : UIElastic
{
    public Graphic Graphic { get; private set; }
    private Color initialColor;
    [SerializeField] private ParticleSystem UIParticleSystem;
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

        pulseTimer = new TimerStopwatchAction(this, x => SetGraphicColor(Color.Lerp(newColor, initialColor, (float)(x / pulseTime)), false), () => { }, 0d, TimerBehavior.TEMPORARY, pulseTime, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(pulseTimer);
    }

    public void SetGraphicColor(Color newColor, bool overrideInitialColor = true)
    {
        if (overrideInitialColor)
        {
            initialColor = newColor;
        }

        Graphic.color = newColor;

        if (UIParticleSystem == null)
        {
            return;
        }

        ParticleSystem.MainModule mainModule = UIParticleSystem.main;
        mainModule.startColor = newColor;
    }
}
