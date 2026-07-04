public class GameplayPauseModule : BasePauseModule
{
    private const int k_GAMESCROLLSPEEDGROUPINDEX = 0;
    private const int k_LOOKAHEADGROUPINDEX = 1;
    protected override void OnModuleInitialized()
    {
        pauseMenuGroups[k_GAMESCROLLSPEEDGROUPINDEX].SetGroupAction_InputField(x =>
        {
            bool parseResult = double.TryParse(x, out double speed);
            if (!parseResult)
            {
                return;
            }

            if (speed < 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(speed, 0d))
            {
                return;
            }

            GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed = speed;
        }, GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed.ToString("F2"));

        pauseMenuGroups[k_LOOKAHEADGROUPINDEX].SetGroupAction_InputField(x =>
        {
            bool parseResult = double.TryParse(x, out double time);
            if (!parseResult)
            {
                return;
            }

            if (time < 0d || MathHelper.IsTwoDoublesEqualWithEpsilion(time, 0d))
            {
                return;
            }

            GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime = time;
        }, GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime.ToString("F2"));

    }
}
