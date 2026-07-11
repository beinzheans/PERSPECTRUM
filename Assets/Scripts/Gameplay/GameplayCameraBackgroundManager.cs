using UnityEngine;

public class GameplayCameraBackgroundManager : MonoBehaviour
{
    private GameplayManager gameplayManager;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color pulseColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    int metronomeLoopIndex = 0;

    TimerStopwatchAction pulseAction;
    void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayMetronomeFired += GameplayManager_OnGameplayMetronomeFired;
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        metronomeLoopIndex = 0;

    }

    private const int k_CAMERABACKGROUNDPULSEBEAT = 4;
    private void GameplayManager_OnGameplayMetronomeFired(double obj)
    {
        if (gameplayManager.CurrentActiveGameplayMarker == null)
        {
            return;
        }

        if (metronomeLoopIndex == 0)
        {
            gameplayManager.GameplayCamera.backgroundColor = pulseColor;
            pulseAction = new TimerStopwatchAction(this, (x) => PulseCameraBackground(x), () => { }, 0d, GetPulseLength(), false);
            DSPTimerEngine.TimerInstance.AddActionToTimer(pulseAction);
        }

        metronomeLoopIndex = (metronomeLoopIndex + 1) % k_CAMERABACKGROUNDPULSEBEAT;
    }

    private void PulseCameraBackground(double timeElapsed)
    {
        double progress = timeElapsed / GetPulseLength();

        Color color = Color.Lerp(pulseColor, normalColor, (float)progress);
        gameplayManager.GameplayCamera.backgroundColor = color;
    }

    private double GetPulseLength()
    {
        return 60d / gameplayManager.CurrentActiveGameplayMarker.BPM * (k_CAMERABACKGROUNDPULSEBEAT / 2);

    }
}
