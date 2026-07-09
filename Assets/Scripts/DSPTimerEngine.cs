using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to handle timer logic using <see cref="AudioSettings.dspTime"/> for precision. <br></br>
/// The timer engine every update loop will first remove any <see cref="TimerAction"/>, then execute <see cref="TimerAction"/> currently active.
/// </summary>
public class DSPTimerEngine : MonoBehaviour
{
    public static DSPTimerEngine TimerInstance;
    public double InitialDSPTime { get; private set; }
    private double previousDSPTime;
    public double CurrentDSPTime { get; private set; }

    List<TimerAction> registeredAudioActions = new();

    private Queue<TimerAction> audioActionsToRemove = new();
    private Queue<TimerAction> audioActionsToRegister = new();

    private void Awake()
    {
        if (DSPTimerEngine.TimerInstance == null)
        {
            TimerInstance = this;
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

        // add timers before trying to update

        while (audioActionsToRegister.Count > 0)
        {
            TimerAction register = audioActionsToRegister.Dequeue();
            registeredAudioActions.Add(register);
        }

        for (int i = 0; i < registeredAudioActions.Count; i++)
        {
            TimerAction action = registeredAudioActions[i];

            action.UpdateAction();
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
    /// Adds a new non-null <see cref="TimerAction"/> to the timer engine to execute.
    /// </summary>
    public void AddActionToTimer(TimerAction action)
    {
        if (action == null)
        {
            return;
        }

        if (registeredAudioActions.Contains(action))
        {
            return;
        }

        audioActionsToRegister.Enqueue(action);
    }

    /// <summary>
    /// Removes a non-null <see cref="TimerAction"/> from the timer engine if it is registered.
    /// </summary>
    public void RemoveActionFromTimer(TimerAction action)
    {
        if (action == null)
        {
            return;
        }

        if (!registeredAudioActions.Contains(action))
        {
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
    /// The Unity object that created this timer
    /// </summary>
    protected UnityEngine.Object TimerCaller;
    protected Action<double> ActionToExecute;
    protected Action OnUnregisterEvent;
    /// <summary>
    /// How long before we execute the first action
    /// </summary>
    protected double startOffsetTime;

    /// <summary>
    /// The execution time for the next cycle in DSP time
    /// </summary>
    public double ExecuteTime { get; protected set; }

    /// <summary>
    /// The starting time in DSP time
    /// </summary>
    public double StartTime { get; protected set; }

    /// <summary>
    /// How long the timer has been running
    /// </summary>
    public double ElapsedTime { get; protected set; }
    public TimerAction(UnityEngine.Object timerCaller, Action<double> actionToExecute, Action onUnregisterEvent, double startOffsetTime)
    {
        TimerCaller = timerCaller;
        ActionToExecute = actionToExecute;
        OnUnregisterEvent = onUnregisterEvent;
        this.startOffsetTime = startOffsetTime;

        ExecuteTime = AudioSettings.dspTime + startOffsetTime;
        StartTime = ExecuteTime;
    }


    /// <summary>
    /// Updates the currect action if the caller is not null
    /// </summary>
    public abstract void UpdateAction();

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

    /// <summary>
    /// Whether or not the timer invokes the action using elapsed time or using the DSP execute time.
    /// </summary>
    private bool useElapsedTimeForAction;
    public TimerIntervalAction(UnityEngine.Object timerCaller, Action<double> actionToExecute, Action unregisterEvent, double startOffsetTime, double repeatIntervalTime, bool useElapsedTimeForAction = false) : base(timerCaller, actionToExecute, unregisterEvent, startOffsetTime)
    {
        this.repeatIntervalTime = repeatIntervalTime;
        this.useElapsedTimeForAction = useElapsedTimeForAction;
    }

    public override void UpdateAction()
    {
        if (TimerCaller == null) // don't update if the caller becomes null
        {
            Debug.Log($"Removed timer from null caller");

            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        double lookaheadTime = DSPTimerEngine.TimerInstance.CurrentDSPTime;

        while (lookaheadTime > ExecuteTime)
        {
            ActionToExecute?.Invoke(useElapsedTimeForAction ? ElapsedTime : ExecuteTime);

            if (repeatIntervalTime < 0d)
            {
                DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
                ExecuteTime = double.MaxValue;
                return;
            }

            ExecuteTime += repeatIntervalTime;
            ElapsedTime += repeatIntervalTime;
        }
    }

    /// <summary>
    /// Resets the timer's interval timing for re-activation
    /// </summary>
    public override void ResetTimer()
    {
        ExecuteTime = AudioSettings.dspTime + startOffsetTime;
        StartTime = AudioSettings.dspTime;
        ElapsedTime = 0d;
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
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/> that involves incremental time.
/// </summary>
public class TimerStopwatchAction : TimerAction
{
    /// <summary>
    /// Whether or not the timer returns delta time instead of total elapsed time
    /// </summary>
    public bool MeasureDeltaTime { get; private set; }
    private double previousDSPTime;
    private double maxTimeElapsed;
    public TimerStopwatchAction(UnityEngine.Object timerCaller, Action<double> executeAction, Action unregisterEvent, double startOffsetTime, double maxTimeElapsed, bool measureDeltaTime) : base(timerCaller, executeAction, unregisterEvent, startOffsetTime)
    {
        this.maxTimeElapsed = maxTimeElapsed;
        previousDSPTime = ExecuteTime;
        MeasureDeltaTime = measureDeltaTime;
    }

    public override void UpdateAction()
    {
        if (TimerCaller == null)
        {
            Debug.Log($"Removed timer from null caller");
            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        double currentDSPTime = DSPTimerEngine.TimerInstance.CurrentDSPTime;
        if (currentDSPTime < ExecuteTime)
        {
            return;
        }

        if (ElapsedTime >= maxTimeElapsed)
        {
            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        double dt = currentDSPTime - previousDSPTime;
        ElapsedTime += dt;
        ActionToExecute?.Invoke(MeasureDeltaTime ? dt : ElapsedTime);

        previousDSPTime = currentDSPTime;
    }

    /// <summary>
    /// Resets the stopwatch's internal states and execution time for any future re-activation.
    /// </summary>
    public override void ResetTimer()
    {
        ElapsedTime = 0d;

        ExecuteTime = AudioSettings.dspTime + startOffsetTime;
        StartTime = AudioSettings.dspTime;
        previousDSPTime = ExecuteTime;
    }
}
