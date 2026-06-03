public class EditorPlaybackSpeedManager : EditorUIBehavior
{
    private PlaybackSpeed currentPlaybackSpeed = PlaybackSpeed.NORMAL_SPEED;
    protected override void UI_OnButtonPress(int index)
    {
    }
}

public enum PlaybackSpeed
{
    QUARTER_SPEED = 0,
    HALF_SPEED = 1,
    NORMAL_SPEED = 2,
    ONE_HALF_SPEED = 3,
    DOUBLE_SPEED = 4
}
