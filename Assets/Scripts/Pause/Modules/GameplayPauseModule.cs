public class GameplayPauseModule : BaseDataModule
{
    private const int k_GAMESCROLLSPEEDGROUPINDEX = 0;
    private const int k_LOOKAHEADGROUPINDEX = 1;
    private const int k_BACKGROUNDENABLEINDEX = 2;
    private const int k_BACKGROUNDBLURINDEX = 3;
    private const int k_BACKGROUNDDARKENINDEX = 4;
    private const int k_BACKGROUNDPULSESTRENGTHINDEX = 5;
    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        moduleDataGroups[k_GAMESCROLLSPEEDGROUPINDEX].SetGroupAction_InputField(x =>
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

            GameManager.GameInstance.GlobalSettings.OnEdit(x => x.GameSettings.GameScrollSpeed, speed);
        }, GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed.ToString("F2"));

        moduleDataGroups[k_LOOKAHEADGROUPINDEX].SetGroupAction_InputField(x =>
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

            GameManager.GameInstance.GlobalSettings.OnEdit(x => x.GameSettings.GameLookaheadTime, time);
        }, GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime.ToString("F2"));

        moduleDataGroups[k_BACKGROUNDENABLEINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.OnEdit(y => y.GameSettings.UseCustomBackground, x),
            GameManager.GameInstance.GlobalSettings.GameSettings.UseCustomBackground);

        moduleDataGroups[k_BACKGROUNDBLURINDEX].SetGroupAction_Slider(x =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(y => y.GameSettings.BackgroundBlurAmount, x);
            moduleDataGroups[k_BACKGROUNDBLURINDEX].SetGroupDisplayText(x.ToString("F2"));
        }, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundBlurAmount);

        moduleDataGroups[k_BACKGROUNDBLURINDEX].SetGroupDisplayText(GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundBlurAmount.ToString("F2"));

        moduleDataGroups[k_BACKGROUNDDARKENINDEX].SetGroupAction_Slider(x =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(y => y.GameSettings.BackgroundDarkenAmount, x);
            moduleDataGroups[k_BACKGROUNDDARKENINDEX].SetGroupDisplayText(x.ToString("F2"));
        }, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundDarkenAmount);

        moduleDataGroups[k_BACKGROUNDDARKENINDEX].SetGroupDisplayText(GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundDarkenAmount.ToString("F2"));

        moduleDataGroups[k_BACKGROUNDPULSESTRENGTHINDEX].SetGroupAction_Slider(x =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(y => y.GameSettings.BackgroundPulseStrength, x);
            moduleDataGroups[k_BACKGROUNDPULSESTRENGTHINDEX].SetGroupDisplayText(x.ToString("F2"));
        }, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundPulseStrength);

        moduleDataGroups[k_BACKGROUNDPULSESTRENGTHINDEX].SetGroupDisplayText(GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundPulseStrength.ToString("F2"));
    }
}
