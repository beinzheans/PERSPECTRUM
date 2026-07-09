using System;
using TMPro;
using UnityEngine;

/// <summary>
/// A class to handle the playback logic.
/// </summary>
public class EditorPlaybackManager : EditorUIBehavior
{
    [SerializeField] private TMP_InputField playbackSpeedInputField;

    private double playbackSpeed = 1d;
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
        playbackSpeedInputField.onValueChanged.AddListener((x) =>
        {
            bool parseResult = double.TryParse(x, out double speed);

            if (!parseResult || speed <= 0d)
            {
                GameManager.GameInstance.InvokeInformationDisplayNeeded("Invalid playback speed");
                playbackSpeed = 1d;
                return;
            }

            GameManager.GameInstance.InvokeInformationDisplayNeeded("Changed playback speed");
            playbackSpeed = speed;
        }
);

        inputActions.Editor.EditorStartPlayback.performed += EditorStartPlayback_performed;
        editorManager.OnTimelineMarkerActive += EditorManager_OnTimelineMarkerActive;
    }

    private void EditorManager_OnTimelineMarkerActive(TimelineMarker obj)
    {
        if (!playbackState)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(obj.DisplayMessage) || obj.DisplayTime <= 0d)
        {
            return;
        }

        GameManager.GameInstance.InvokeInformationDisplayNeeded(obj.DisplayMessage, obj.DisplayTime);
    }

    private void OnDestroy()
    {
        inputActions.Editor.EditorStartPlayback.performed -= EditorStartPlayback_performed;
        editorManager.OnTimelineMarkerActive += EditorManager_OnTimelineMarkerActive;
    }
    private void EditorStartPlayback_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.GameInstance.IsCorrectKeyboardModifierForInputAction(obj.action))
        {
            return;
        }

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

        if (playbackSpeed < 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(playbackSpeed, 0d))
        {
            return;
        }

        Action<double> executeAction = (x) => editorManager.UpdateEditorPreviewTimeByDelta(x * playbackSpeed, false); // local variable so we can't change it while we're playback
        playbackAction = new TimerStopwatchAction(this, executeAction, () => { }, 0d, double.MaxValue, true);

        editorManager.InvokeEditorStartPlayback(playbackSpeed);
        DSPTimerEngine.TimerInstance.AddActionToTimer(playbackAction);
    }

    private void OnPlaybackStop()
    {
        if (playbackSpeed < 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(playbackSpeed, 0d))
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