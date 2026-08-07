using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlPauseModule : BasePauseModule
{
    private const int k_CURSORMOVEMODEINDEX = 0;
    private const int k_MOUSESENSITIVITYINDEX = 1;
    private const int k_CURSORINVERTXINDEX = 2;
    private const int k_CURSORINVERTYINDEX = 3;
    private const int k_REBINDAKEYINDEX = 4;
    private const int k_REBINDBKEYINDEX = 5;

    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        pauseMenuGroups[k_CURSORMOVEMODEINDEX].SetGroupAction_Dropdown(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.CursorMovementType, (CursorMovementTypes)x),
        GameManager.GameInstance.GlobalSettings.CursorMovementType);

        pauseMenuGroups[k_MOUSESENSITIVITYINDEX].SetGroupAction_Slider(x =>
        {
            float scale = math.remap(0f, 1f, 0.1f, 3f, x);
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor, scale);
            pauseMenuGroups[k_MOUSESENSITIVITYINDEX].SetGroupDisplayText(scale.ToString("F2"));
        }, math.remap(0.1f, 3f, 0f, 1f, GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor));

        pauseMenuGroups[k_MOUSESENSITIVITYINDEX].SetGroupDisplayText(GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor.ToString("F2"));

        pauseMenuGroups[k_CURSORINVERTXINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.MouseInvert_XAxis, x), GameManager.GameInstance.GlobalSettings.MouseInvert_XAxis);

        pauseMenuGroups[k_CURSORINVERTYINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.MouseInvert_YAxis, x), GameManager.GameInstance.GlobalSettings.MouseInvert_YAxis);

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
