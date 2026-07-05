public class GlobalPauseModule : BasePauseModule
{
    private const int k_OFFSETGROUPINDEX = 0;
    private const int k_PREDICTIVEHITSOUNDGROUPINDEX = 1;
    private const int k_SONGVOLUMEGROUPINDEX = 2;
    private const int k_HITSOUNDVOLUMEGROUPINDEX = 3;

    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        pauseMenuGroups[k_OFFSETGROUPINDEX].SetGroupAction_InputField((x) =>
        {
            if (double.TryParse(x, out double ms))
            {
                GameManager.GameInstance.GlobalSettings.AudioOffsetMs = ms;
            }
        }, GameManager.GameInstance.GlobalSettings.AudioOffsetMs.ToString("F2"));
        pauseMenuGroups[k_PREDICTIVEHITSOUNDGROUPINDEX].SetGroupAction_Toggle((x) => {
            GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds = x;
            GameManager.GameInstance.InvokeGameSettingsChanged(); 
        }, GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds);
        pauseMenuGroups[k_SONGVOLUMEGROUPINDEX].SetGroupAction_Slider((x) =>
        {
            GameManager.GameInstance.GlobalSettings.SongVolume = x;
            GameManager.GameInstance.InvokeGameSettingsChanged();
        }, GameManager.GameInstance.GlobalSettings.SongVolume);
        pauseMenuGroups[k_HITSOUNDVOLUMEGROUPINDEX].SetGroupAction_Slider((x) => {
            GameManager.GameInstance.GlobalSettings.HitsoundVolume = x;
            GameManager.GameInstance.InvokeGameSettingsChanged();
        }, GameManager.GameInstance.GlobalSettings.HitsoundVolume);
    }
}
