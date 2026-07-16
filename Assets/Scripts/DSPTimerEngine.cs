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
            if (register.TimerCaller == null)
            {
                Debug.Log($"Ignored timer registeration due to null caller");
                return;
            }

            registeredAudioActions.Add(register);
        }

        for (int i = 0; i < registeredAudioActions.Count; i++)
        {
            TimerAction action = registeredAudioActions[i];

            action.UpdateAction();
        }
    }

    /// <summary>
    /// Resets the global timer to zero by reassigning <see cref="InitialDSPTime"/>.
    /// </summary>
    public void ResetTimer()
    {
        InitialDSPTime = AudioSettings.dspTime;
        previousDSPTime = AudioSettings.dspTime;
    }

    /// <summary>
    /// Adds a new non-null <see cref="TimerAction"/> to the timer engine to execute. <br></br>
    /// Note that creating a new instance of a <see cref="TimerAction"/> is considered a different timer.
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

    /// <summary>
    /// Pauses all DSP timers globally.
    /// </summary>

    private HashSet<TimerAction> alreadyPausedTimers = new();
    public void PauseDSPTimer()
    {
        alreadyPausedTimers = new();

        for (int i = 0; i < registeredAudioActions.Count; i++)
        {
            TimerAction timerAction = registeredAudioActions[i];
            if (timerAction.IsTimerPaused)
            {
                alreadyPausedTimers.Add(timerAction);
                continue;
            }

            timerAction.PauseTimer();
        }
    }

    /// <summary>
    /// Resumes all DSP timers globally, except for those that were already paused before <see cref="PauseDSPTimer"/> is called.
    /// </summary>
    public void ResumeDSPTimer()
    {
        for (int i = 0; i < registeredAudioActions.Count; i++)
        {
            TimerAction timerAction = registeredAudioActions[i];

            if (alreadyPausedTimers.Contains(timerAction))
            {
                continue;
            }

            timerAction.UnpauseTimer(0d);
        }
    }
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/>
/// </summary>
public abstract class TimerAction : IEquatable<TimerAction>
{
    /// <summary>
    /// The Unity object that created this timer
    /// </summary>
    public UnityEngine.Object TimerCaller { get; protected set; }
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

    public bool IsTimerPaused { get; protected set; }

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
    public void UpdateAction()
    {
        if (TimerCaller == null) // don't update if the caller becomes null
        {
            Debug.Log($"Removed timer from null caller");

            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(this);
            return;
        }

        if (IsTimerPaused)
        {
            return;
        }

        OnTimerUpdated();
    }

    /// <summary>
    /// Custom implementation of events when the timer is updated.
    /// </summary>
    protected abstract void OnTimerUpdated();

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

    /// <summary>
    /// Pauses the timer, preventing the timer from updating. <br></br>
    /// Use this when you want a specific timer to be inactive when <see cref="DSPTimerEngine"/> is not paused.
    /// </summary>
    public void PauseTimer()
    {
        if (IsTimerPaused)
        {
            return;
        }

        OnTimerPausedEvent();
        IsTimerPaused = true;
    }

    /// <summary>
    /// Unpauses the timer with an additional offset time before resuming.
    /// </summary>
    public void UnpauseTimer(double offset)
    {
        if (!IsTimerPaused)
        {
            return;
        }

        OnTimerUnpausedEvent(offset);
        IsTimerPaused = false;
    }

    /// <summary>
    /// Custom implementation of events when the timer is paused.
    /// </summary>
    protected abstract void OnTimerPausedEvent();

    /// <summary>
    /// Custom implementations of events when the timer is unpaused.
    /// </summary>
    protected abstract void OnTimerUnpausedEvent(double offset);

    public override bool Equals(object obj)
    {
        return Equals(obj as TimerAction);
    }

    public bool Equals(TimerAction other)
    {
        return other is not null &&
               EqualityComparer<UnityEngine.Object>.Default.Equals(TimerCaller, other.TimerCaller) &&
               EqualityComparer<Action<double>>.Default.Equals(ActionToExecute, other.ActionToExecute) &&
               EqualityComparer<Action>.Default.Equals(OnUnregisterEvent, other.OnUnregisterEvent) &&
               startOffsetTime == other.startOffsetTime;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TimerCaller, ActionToExecute, OnUnregisterEvent, startOffsetTime);
    }
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/> on a regular interval
/// </summary>
public class TimerIntervalAction : TimerAction, IEquatable<TimerIntervalAction>
{
    /// <summary>
    /// How long before each action should be repeated
    /// </summary>
    private double repeatIntervalTime;

    /// <summary>
    /// How many executions the timer will perform. Define zero or negative values for no limit.
    /// </summary>
    private int numberOfExecutions;

    /// <summary>
    /// Whether or not the timer invokes the action using elapsed time or using the DSP execute time.
    /// </summary>
    private bool useElapsedTimeForAction;

    /// <summary>
    /// How much time until the next execution in DSP time. This is calculated only when <see cref="PauseTimer"/> is called.
    /// </summary>
    protected double pauseTimeProgress;

    public TimerIntervalAction(UnityEngine.Object timerCaller, Action<double> actionToExecute, Action unregisterEvent, double startOffsetTime, double repeatIntervalTime, int numberOfExecutions = 1, bool useElapsedTimeForAction = false) : base(timerCaller, actionToExecute, unregisterEvent, startOffsetTime)
    {
        this.repeatIntervalTime = repeatIntervalTime;
        this.numberOfExecutions = numberOfExecutions;
        this.useElapsedTimeForAction = useElapsedTimeForAction;
        internal_executionCount = 0;
    }

    private int internal_executionCount;

    protected override void OnTimerUpdated()
    {
        double lookaheadTime = DSPTimerEngine.TimerInstance.CurrentDSPTime;

        while (lookaheadTime > ExecuteTime)
        {
            ActionToExecute?.Invoke(useElapsedTimeForAction ? ElapsedTime : ExecuteTime);

            internal_executionCount++;

            if (internal_executionCount >= numberOfExecutions && numberOfExecutions > 0)
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
        internal_executionCount = 0;
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
    protected override void OnTimerPausedEvent()
    {
        pauseTimeProgress = ExecuteTime - AudioSettings.dspTime;
    }

    protected override void OnTimerUnpausedEvent(double offset)
    {
        ExecuteTime = AudioSettings.dspTime + offset + pauseTimeProgress;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TimerIntervalAction);
    }

    public bool Equals(TimerIntervalAction other)
    {
        return other is not null &&
               base.Equals(other) &&
               repeatIntervalTime == other.repeatIntervalTime &&
               numberOfExecutions == other.numberOfExecutions &&
               useElapsedTimeForAction == other.useElapsedTimeForAction;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), repeatIntervalTime, numberOfExecutions, useElapsedTimeForAction);
    }
}

/// <summary>
/// A class to represent an action to be performed by <see cref="DSPTimerEngine"/> that involves incremental time.
/// </summary>
public class TimerStopwatchAction : TimerAction, IEquatable<TimerStopwatchAction>
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

    protected override void OnTimerUpdated()
    {
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
    protected override void OnTimerPausedEvent()
    {
        return;
    }

    protected override void OnTimerUnpausedEvent(double offset)
    {
        ExecuteTime = AudioSettings.dspTime + offset;
        previousDSPTime = AudioSettings.dspTime + offset;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TimerStopwatchAction);
    }

    public bool Equals(TimerStopwatchAction other)
    {
        return other is not null &&
               base.Equals(other) &&
               MeasureDeltaTime == other.MeasureDeltaTime &&
               maxTimeElapsed == other.maxTimeElapsed;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), MeasureDeltaTime, maxTimeElapsed);
    }
}
