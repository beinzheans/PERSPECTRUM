using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Implementation of <see cref="ModuleDataGroupObject"/> with added pointer handlers to display descriptions.
/// </summary>
public class PauseSettingsModuleDataGroup : ModuleDataGroupObject, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrWhiteSpace(ModuleData.GroupDescription))
        {
            GameManager.GameInstance.InvokeGamePauseDescriptionChanged(ModuleData.GroupDescription);
        }
        else
        {
            GameManager.GameInstance.InvokeGamePauseDescriptionChanged(GamePauseManager.k_PAUSEMENUNODESCRIPTIONPROVIDED);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.GameInstance.InvokeGamePauseDescriptionChanged(GamePauseManager.k_PAUSEMENUNODESCRIPTIONPROVIDED);
    }
}
