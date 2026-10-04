public class EditorPauseModule : BaseDataModule
{
    private const int k_LOOKAHEADTIMEINDEX = 0;
    private const int k_SHIFTSCROLLTIMEINTERVAL = 1;

    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        moduleDataGroups[k_LOOKAHEADTIMEINDEX].SetGroupAction_InputField(x =>
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

            GameManager.GameInstance.GlobalSettings.OnEdit(x => x.EditorSettings.EditorLookaheadTime, time);
        }, GameManager.GameInstance.GlobalSettings.EditorSettings.EditorLookaheadTime.ToString("F2"));

        moduleDataGroups[k_SHIFTSCROLLTIMEINTERVAL].SetGroupAction_InputField(x =>
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

            GameManager.GameInstance.GlobalSettings.OnEdit(x => x.EditorSettings.BigScrollTimeInterval, time);
        }, GameManager.GameInstance.GlobalSettings.EditorSettings.BigScrollTimeInterval.ToString("F2"));

    }
}
