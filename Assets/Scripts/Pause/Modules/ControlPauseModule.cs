using Unity.Mathematics;
using UnityEngine.InputSystem;

public class ControlPauseModule : BaseDataModule
{
    private const int k_CURSORMOVEMODEINDEX = 0;
    private const int k_MOUSESENSITIVITYINDEX = 1;
    private const int k_CURSORINVERTXINDEX = 2;
    private const int k_CURSORINVERTYINDEX = 3;
    private const int k_REBINDAKEYINDEX = 4;
    private const int k_REBINDBKEYINDEX = 5;
    private const int k_REBINDRESTARTKEYINDEX = 6;
    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        moduleDataGroups[k_CURSORMOVEMODEINDEX].SetGroupAction_Dropdown(x => GameManager.GameInstance.GlobalSettings.OnEdit(x => x.CursorMovementType, (CursorMovementTypes)x),
        GameManager.GameInstance.GlobalSettings.CursorMovementType);

        moduleDataGroups[k_MOUSESENSITIVITYINDEX].SetGroupAction_Slider(x =>
        {
            float scale = math.remap(0f, 1f, 0.1f, 3f, x);
            GameManager.GameInstance.GlobalSettings.OnEdit(x => x.MouseSensitivityScaleFactor, scale);
            moduleDataGroups[k_MOUSESENSITIVITYINDEX].SetGroupDisplayText(scale.ToString("F2"));
        }, math.remap(0.1f, 3f, 0f, 1f, GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor));

        moduleDataGroups[k_MOUSESENSITIVITYINDEX].SetGroupDisplayText(GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor.ToString("F2"));

        moduleDataGroups[k_CURSORINVERTXINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.OnEdit(y => y.MouseInvert_XAxis, x), GameManager.GameInstance.GlobalSettings.MouseInvert_XAxis);

        moduleDataGroups[k_CURSORINVERTYINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.OnEdit(y => y.MouseInvert_YAxis, x), GameManager.GameInstance.GlobalSettings.MouseInvert_YAxis);

        InitializeRebindAction_Button(k_REBINDAKEYINDEX, GameManager.GameInstance.InputActions.Gameplay.SwitchAInput);
        InitializeRebindAction_Button(k_REBINDBKEYINDEX, GameManager.GameInstance.InputActions.Gameplay.SwitchBInput);
        InitializeRebindAction_Button(k_REBINDRESTARTKEYINDEX, GameManager.GameInstance.InputActions.Gameplay.RestartInput);
    }

    private void InitializeRebindAction_Button(int index, InputAction action)
    {
        moduleDataGroups[index].SetGroupAction_Button(() =>
        {
            moduleDataGroups[index].SetGroupDisplayText("Press any key...");
            RebindHelper.StartRebindAction(action,
                () => moduleDataGroups[index].SetGroupDisplayText($"Current: {action.GetBindingDisplayString()}"));
        });

        moduleDataGroups[index].SetGroupDisplayText($"Current: {action.GetBindingDisplayString()}");
    }
}
