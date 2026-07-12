using UnityEngine.InputSystem;

public class GameplayPauseModule : BasePauseModule
{
    private const int k_GAMESCROLLSPEEDGROUPINDEX = 0;
    private const int k_LOOKAHEADGROUPINDEX = 1;
    private const int k_REBINDAKEYINDEX = 2;
    private const int k_REBINDBKEYINDEX = 3;

    protected override void OnModuleAwake()
    {
        return;
    }

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

            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed, speed);
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

            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime, time);
        }, GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime.ToString("F2"));

        pauseMenuGroups[k_REBINDAKEYINDEX].SetGroupAction_Button(() =>
        {
            pauseMenuGroups[k_REBINDAKEYINDEX].SetGroupDisplayText("Press any key...");
            RebindHelper.StartRebindAction(GameManager.GameInstance.InputActions.Gameplay.SwitchAInput,
                () => pauseMenuGroups[k_REBINDAKEYINDEX].SetGroupDisplayText($"Current: {GameManager.GameInstance.InputActions.Gameplay.SwitchAInput.GetBindingDisplayString()}"));
        });

        pauseMenuGroups[k_REBINDAKEYINDEX].SetGroupDisplayText($"Current: {GameManager.GameInstance.InputActions.Gameplay.SwitchAInput.GetBindingDisplayString()}");
        pauseMenuGroups[k_REBINDBKEYINDEX].SetGroupAction_Button(() =>
        {
            pauseMenuGroups[k_REBINDBKEYINDEX].SetGroupDisplayText("Press any key...");
            RebindHelper.StartRebindAction(GameManager.GameInstance.InputActions.Gameplay.SwitchBInput,
                () => pauseMenuGroups[k_REBINDBKEYINDEX].SetGroupDisplayText($"Current: {GameManager.GameInstance.InputActions.Gameplay.SwitchBInput.GetBindingDisplayString()}"));
        });

        pauseMenuGroups[k_REBINDBKEYINDEX].SetGroupDisplayText($"Current: {GameManager.GameInstance.InputActions.Gameplay.SwitchBInput.GetBindingDisplayString()}");

    }
}
