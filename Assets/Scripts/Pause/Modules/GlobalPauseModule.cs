public class GlobalPauseModule : BaseDataModule
{
    private const int k_OFFSETGROUPINDEX = 0;
    private const int k_PREDICTIVEHITSOUNDGROUPINDEX = 1;
    private const int k_SHOWFPSGROUPINDEX = 2;
    private const int k_SONGVOLUMEGROUPINDEX = 3;
    private const int k_HITSOUNDVOLUMEGROUPINDEX = 4;
    private const int k_UIVOLUMEGROUPINDEX = 5;
    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        moduleDataGroups[k_OFFSETGROUPINDEX].SetGroupAction_InputField((x) =>
        {
            if (double.TryParse(x, out double ms))
            {
                GameManager.GameInstance.GlobalSettings.OnEdit(() => GameManager.GameInstance.GlobalSettings.AudioOffsetMs, ms);
            }
        }, GameManager.GameInstance.GlobalSettings.AudioOffsetMs.ToString("F2"));

        moduleDataGroups[k_OFFSETGROUPINDEX].SetGroupAction_Button(() =>
        {
            ConfirmAction action = new ConfirmAction(() =>
            {
                GameManager.GameInstance.RequestOverrideGamePauseState(false);
                SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_CALIBRATIONINDEX, () => System.Threading.Tasks.Task.CompletedTask);
            }, () =>
            {
                GameManager.GameInstance.PauseCanvas.gameObject.SetActive(true);
            }, "Are you sure you want to enter the calibration screen? This will bring you out of the current session.");

            GameManager.GameInstance.PauseCanvas.gameObject.SetActive(false);
            GameManager.GameInstance.InvokeConfirmActionNeeded(action);
        });

        moduleDataGroups[k_PREDICTIVEHITSOUNDGROUPINDEX].SetGroupAction_Toggle((x) =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(() => GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds, x);
        }, GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds);

        moduleDataGroups[k_SHOWFPSGROUPINDEX].SetGroupAction_Toggle((x) => GameManager.GameInstance.GlobalSettings.OnEdit(() => GameManager.GameInstance.GlobalSettings.ShowFPSCounter, x), GameManager.GameInstance.GlobalSettings.ShowFPSCounter);

        moduleDataGroups[k_SONGVOLUMEGROUPINDEX].SetGroupAction_Slider((x) =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(() => GameManager.GameInstance.GlobalSettings.SongVolume, x);
        }, GameManager.GameInstance.GlobalSettings.SongVolume);


        moduleDataGroups[k_HITSOUNDVOLUMEGROUPINDEX].SetGroupAction_Slider((x) =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(() => GameManager.GameInstance.GlobalSettings.HitsoundVolume, x);
        }, GameManager.GameInstance.GlobalSettings.HitsoundVolume);

        moduleDataGroups[k_UIVOLUMEGROUPINDEX].SetGroupAction_Slider(x =>
        {
            GameManager.GameInstance.GlobalSettings.OnEdit(() => GameManager.GameInstance.GlobalSettings.UIVolume, x);
        }, GameManager.GameInstance.GlobalSettings.UIVolume);
    }
}
