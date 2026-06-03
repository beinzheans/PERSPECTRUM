using System;
using UnityEngine;

/// <summary>
/// A class to handle the playback logic.
/// </summary>
public class EditorPlaybackManager : EditorUIBehavior
{
    private EditorManager editorManager;
    private PlayerInputActions inputActions;
    private bool playbackState;

    private PlaybackType currentPlaybackType = PlaybackType.PLAYBACK_FROM_START;

    private TimerStopwatchAction playbackAction;
    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)PlaybackType.PLAYBACK_FROM_START || index > (int)PlaybackType.PLAYBACK_FROM_SECTION)
        {
            return;
        }

        buttons[(int)currentPlaybackType].image.color = Color.white;
        buttons[index].image.color = Color.yellow;
        currentPlaybackType = (PlaybackType)index;
    }

    protected override void Start()
    {
        base.Start();
        UI_OnButtonPress(0);
        editorManager = EditorManager.EditorInstance;
        inputActions = GameManager.GameInstance.InputActions;

        inputActions.Editor.EditorStartPlayback.performed += EditorStartPlayback_performed;
    }

    private void EditorStartPlayback_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        playbackState = !playbackState;

        if (playbackState)
        {
            OnPlaybackStart();
        }
        else
        {
            OnPlaybackStop();
        }
    }

    private void OnPlaybackStart()
    {
        switch (currentPlaybackType)
        {
            case PlaybackType.PLAYBACK_FROM_START:
                editorManager.UpdateEditorPreviewTime(0d, true);
                break;
            case PlaybackType.PLAYBACK_FROM_CURRENT:
                break;
            case PlaybackType.PLAYBACK_FROM_SECTION:
                editorManager.UpdateEditorPreviewTime(editorManager.CurrentTimelineMarker.RenderTime, true);
                break;
        }

        if (editorManager.PlaybackSpeed < 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(editorManager.PlaybackSpeed, 0d))
        {
            return;
        }

        double playbackSpeed = editorManager.PlaybackSpeed;
        Action<double> executeAction = (x) => editorManager.UpdateEditorPreviewTimeByDelta(x * playbackSpeed, false); // local variable so we can't change it while we're playback
        Action endAction = () => { };
        playbackAction = new TimerStopwatchAction(executeAction, endAction, 0d, double.MaxValue, true);

        editorManager.InvokeEditorStartPlayback();
        DSPTimerEngine.TimerInstance.AddActionToTimer(playbackAction);
    }

    private void OnPlaybackStop()
    {
        if (editorManager.PlaybackSpeed < 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(editorManager.PlaybackSpeed, 0d))
        {
            return;
        }

        GameManager.GameInstance.InvokeInformationDisplayNeeded("Stop Playback");
        editorManager.InvokeEditorStopPlayback();
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(playbackAction);
    }
}

public enum PlaybackType
{
    PLAYBACK_FROM_START = 0,
    PLAYBACK_FROM_CURRENT = 1,
    PLAYBACK_FROM_SECTION = 2
}