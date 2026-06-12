using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to handle timer logic using <see cref="AudioSettings.dspTime"/> for precision. <br></br>
/// The timer will internally keep track of time elapsed and allow for other scripts to hook onto this class for timing-based actions.
/// </summary>
public class DSPTimerEngine : MonoBehaviour
{
    public static DSPTimerEngine TimerInstance;
    public double InitialDSPTime { get; private set; }
    private double previousDSPTime;
    public double CurrentDSPTime { get; private set; }
    public const double k_DSPLookaheadTime = 0d; // a buffer so we can use PlaySchedule() in the future.

    List<TimerAction> registeredAudioActions = new();

    private Queue<TimerAction> audioActionsToRemove = new();
    private void Awake()
    {
        if (DSPTimerEngine.TimerInstance == null)
        {
            TimerInstance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (AudioSettings.dspTime == previousDSPTime)
        {
            return;
        }

        CurrentDSPTime = AudioSettings.dspTime;
        previousDSPTime = CurrentDSPTime;

        // remove timers before trying to update
        while (audioActionsToRemove.Count > 0)
        {
            TimerAction remove = audioActionsToRemove.Dequeue();
            remove.OnActionRemoved();
            registeredAudioActions.Remove(remove);
        }

        for (int i = 0; i < registeredAudioActions.Count; i++)
        {
            TimerAction action = registeredAudioActions[i];

            action.UpdateAction(CurrentDSPTime, k_DSPLookaheadTime);
        }
    }

    /// <summary>
    /// Resets the timer to zero by reassigning <see cref="InitialDSPTime"/>.
    /// </summary>
    public void ResetTimer()
    {
        InitialDSPTime = AudioSettings.dspTime;
        previousDSPTime = AudioSettings.dspTime;
    }

    /// <summary>
    /// Gets the current time elasped from DSP time in seconds since the last reset.
    /// </summary>
    /// <returns></returns>
    public double GetTimer()
    {
        return CurrentDSPTime - InitialDSPTime;
    }

    /// <summary>
    /// Adds a new <see cref="TimerIntervalAction"/> to the timer engine to execute
    /// </summary>
    public void AddActionToTimer(TimerAction action)
    {
        registeredAudioActions.Add(action);
    }

    /// <summary>
    /// Removes an action from the timer engine. Note that removal happen only after one clock cycle. <br></br>
    /// Ignores actions that is not registered.
    /// </summary>
    public void RemoveActionFromTimer(TimerAction action)
    {
        if (!registeredAudioActions.Contains(action))
        {
            Debug.Log($"Ignored");
            return;
        }

        audioActionsToRemove.Enqueue(action);
    }
    public void PauseDSPTimer()
    {
        AudioListener.pause = true;
    }

    public void ResumeDSPTimer()
    {
        AudioListener.pause = false;
    }
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/>
/// </summary>
public abstract class TimerAction
{
    /// <summary>
    /// The object that created this timer
    /// </summary>
    protected object TimerCaller;
    protected Action<double> ActionToExecute;
    protected Action OnUnregisterEvent;
    /// <summary>
    /// How long before we execute the first action
    /// </summary>
    protected double startOffsetTime;

    /// <summary>
    /// The execution time for the next cycle
    /// </summary>
    public double ExecuteTime { get; protected set; }

    public TimerAction(object timerCaller, Action<double> actionToExecute, Action onUnregisterEvent, double startOffsetTime)
    {
        TimerCaller = timerCaller;
        ActionToExecute = actionToExecute;
        OnUnregisterEvent = onUnregisterEvent;
        this.startOffsetTime = startOffsetTime;

        ExecuteTime = AudioSettings.dspTime + startOffsetTime;
    }


    /// <summary>
    /// Updates the currect action
    /// </summary>
    /// <param name="currentDSPTime"></param>
    /// <param name="lookaheadDSPTime"></param>
    public abstract void UpdateAction(double currentDSPTime, double lookaheadDSPTime);

    /// <summary>
    /// Resets the current timer
    /// </summary>
    public abstract void ResetTimer();
    /// <summary>
    /// Invoke the action to be performed when this action is unregistered from the <see cref="DSPTimerEngine"/>
    /// </summary>
    public void OnActionRemoved()
    {
        OnUnregisterEvent?.Invoke();
    }
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/> on a regular interval
/// </summary>
public class TimerIntervalAction : TimerAction
{
    /// <summary>
    /// How long before each action should be repeated. Define negative values for one-shot actions.
    /// </summary>
    private double repeatIntervalTime;

    public TimerIntervalAction(object timerCaller, Action<double> actionToExecute, Action unregisterEvent, double startOffsetTime, double repeatIntervalTime) : base(timerCaller, actionToExecute, unregisterEvent, startOffsetTime)
    {
        this.repeatIntervalTime = repeatIntervalTime;
    }

    public override void UpdateAction(double currentDSPTime, double lookaheadDSPTime)
    {
        if (TimerCaller == null) // don't update if the caller becomes null
        {
            Debug.Log($"Removed timer due to null");

            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        double lookaheadTime = currentDSPTime + lookaheadDSPTime;

        while (lookaheadTime > ExecuteTime)
        {
            ActionToExecute?.Invoke(ExecuteTime);

            if (repeatIntervalTime < 0d)
            {
                DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
                ExecuteTime = double.MaxValue;
                return;
            }

            ExecuteTime += repeatIntervalTime;
        }
    }

    /// <summary>
    /// Resets the timer's interval timing for re-activation
    /// </summary>
    public override void ResetTimer()
    {
        ExecuteTime = AudioSettings.dspTime + startOffsetTime;
    }

    /// <summary>
    /// Update the current regular interval timing of this action.
    /// </summary>
    /// <param name="newIntervalTime"></param>
    /// <param name="immediateChange">Whether or not this interval update affects the current waiting cycle</param>
    public void EditIntervalTime(double newIntervalTime, bool immediateChange)
    {
        if (!immediateChange)
        {
            repeatIntervalTime = newIntervalTime;
            return;
        }

        double delta = newIntervalTime - repeatIntervalTime;
        ExecuteTime += delta;
        repeatIntervalTime = newIntervalTime;
    }

    /// <summary>
    /// Gets the progress in [0, 1] until the next tick.
    /// </summary>
    /// <returns></returns>
    public double GetCurrentNormalizedProgress()
    {
        return 1d - (ExecuteTime - AudioSettings.dspTime) / repeatIntervalTime;
    }
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/> that involves incremental time.
/// </summary>
public class TimerStopwatchAction : TimerAction
{
    public double TimeElapsed { get; private set; }

    /// <summary>
    /// Whether or not the timer returns delta time instead of total elapsed time
    /// </summary>
    public bool MeasureDeltaTime { get; private set; }
    private double previousDSPTime;
    private double maxTimeElapsed;
    public TimerStopwatchAction(object timerCaller, Action<double> executeAction, Action unregisterEvent, double startOffsetTime, double maxTimeElapsed, bool measureDeltaTime) : base(timerCaller, executeAction, unregisterEvent, startOffsetTime)
    {
        TimeElapsed = 0d;

        this.maxTimeElapsed = maxTimeElapsed;
        previousDSPTime = ExecuteTime;
        MeasureDeltaTime = measureDeltaTime;
    }

    public override void UpdateAction(double currentDSPTime, double lookaheadDSPTime)
    {
        if (TimerCaller == null)
        {
            Debug.Log($"Removed timer due to null");
            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        if (currentDSPTime < ExecuteTime)
        {
            return;
        }

        if (TimeElapsed >= maxTimeElapsed)
        {
            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        double dt = currentDSPTime - previousDSPTime;
        TimeElapsed += dt;
        ActionToExecute?.Invoke(MeasureDeltaTime ? dt : TimeElapsed);

        previousDSPTime = currentDSPTime;
    }

    /// <summary>
    /// Resets the stopwatch's internal states and execution time for any future re-activation.
    /// </summary>
    public override void ResetTimer()
    {
        TimeElapsed = 0d;

        ExecuteTime = AudioSettings.dspTime + startOffsetTime;
        previousDSPTime = ExecuteTime;
    }
}
