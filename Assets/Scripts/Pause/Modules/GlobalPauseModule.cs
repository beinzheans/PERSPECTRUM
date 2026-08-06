using Unity.Mathematics;

public class GlobalPauseModule : BasePauseModule
{
    private const int k_OFFSETGROUPINDEX = 0;
    private const int k_CURSORMOVEMODEINDEX = 1;
    private const int k_MOUSESENSITIVITYINDEX = 2;
    private const int k_CURSORINVERTXINDEX = 3;
    private const int k_CURSORINVERTYINDEX = 4;
    private const int k_PREDICTIVEHITSOUNDGROUPINDEX = 5;
    private const int k_SHOWFPSGROUPINDEX = 6;
    private const int k_SONGVOLUMEGROUPINDEX = 7;
    private const int k_HITSOUNDVOLUMEGROUPINDEX = 8;
    private const int k_UIVOLUMEGROUPINDEX = 9;
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
                GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.AudioOffsetMs, ms);
            }
        }, GameManager.GameInstance.GlobalSettings.AudioOffsetMs.ToString("F2"));

        pauseMenuGroups[k_CURSORMOVEMODEINDEX].SetGroupAction_Dropdown(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.CursorMovementType, (CursorMovementTypes)x),
        GameManager.GameInstance.GlobalSettings.CursorMovementType);

        pauseMenuGroups[k_SHOWFPSGROUPINDEX].SetGroupAction_Toggle((x) => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.ShowFPSCounter, x), GameManager.GameInstance.GlobalSettings.ShowFPSCounter);

        pauseMenuGroups[k_MOUSESENSITIVITYINDEX].SetGroupAction_Slider(x =>
        {
            float scale = math.remap(0f, 1f, 0.1f, 3f, x);
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor, scale);
            pauseMenuGroups[k_MOUSESENSITIVITYINDEX].SetGroupDisplayText(scale.ToString("F2"));
        }, math.remap(0.1f, 3f, 0f, 1f, GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor));

        pauseMenuGroups[k_MOUSESENSITIVITYINDEX].SetGroupDisplayText(GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor.ToString("F2"));

        pauseMenuGroups[k_CURSORINVERTXINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.MouseInvert_XAxis, x), GameManager.GameInstance.GlobalSettings.MouseInvert_XAxis);

        pauseMenuGroups[k_CURSORINVERTYINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.MouseInvert_YAxis, x), GameManager.GameInstance.GlobalSettings.MouseInvert_YAxis);

        pauseMenuGroups[k_PREDICTIVEHITSOUNDGROUPINDEX].SetGroupAction_Toggle((x) =>
        {
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds, x);
        }, GameManager.GameInstance.GlobalSettings.UsePrescheduledHitsounds);
        pauseMenuGroups[k_SONGVOLUMEGROUPINDEX].SetGroupAction_Slider((x) =>
        {
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.SongVolume, x);
        }, GameManager.GameInstance.GlobalSettings.SongVolume);


        pauseMenuGroups[k_HITSOUNDVOLUMEGROUPINDEX].SetGroupAction_Slider((x) =>
        {
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.HitsoundVolume, x);
        }, GameManager.GameInstance.GlobalSettings.HitsoundVolume);

        pauseMenuGroups[k_UIVOLUMEGROUPINDEX].SetGroupAction_Slider(x =>
        {
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.UIVolume, x);
        }, GameManager.GameInstance.GlobalSettings.UIVolume);
    }
}
