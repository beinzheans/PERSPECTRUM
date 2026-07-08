using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class GraphicsPauseModule : BasePauseModule
{
    // todo: make this access URP, change:
    // resolution
    // anti-aliasing (off, 2x, 4x, 8x)
    // render scale [0,1]
    // Vsync (on or off)
    // frame cap (overrides Vsync)

    private const int k_RESOLUTIONINDEX = 0;
    private const int k_FULLSCREENINDEX = 1;
    private const int k_ANTIALIASINGINDEX = 2;
    private const int k_RENDERSCALEINDEX = 3;
    private const int k_VSYNCINDEX = 4;
    private const int k_FRAMELIMITINDEX = 5;

    private List<Vector2Int> allPossibleResolutions;
    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        allPossibleResolutions = GetAllScreenResolutions();

        pauseMenuGroups[k_RESOLUTIONINDEX].SetGroupAction_Dropdown(GetStringRepresentationOfScreenResolution(allPossibleResolutions), 
            x => {
                GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GraphicSettings.CurrentResolution, allPossibleResolutions[x]);
                }, 
            GetStringRepresentationOfCurrentScreenResolution());

        pauseMenuGroups[k_FULLSCREENINDEX].SetGroupAction_Toggle(x =>
        {
            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GraphicSettings.IsUseFullScreen, x);
        }, GameManager.GameInstance.GlobalSettings.GraphicSettings.IsUseFullScreen);

        pauseMenuGroups[k_ANTIALIASINGINDEX].SetGroupAction_Dropdown(x =>
        {
            AntiAliasingMSAA antiAliasing;
            switch (x)
            {
                case 0:
                    antiAliasing = AntiAliasingMSAA.Off;
                    break;
                case 1:
                    antiAliasing = AntiAliasingMSAA.Two;
                    break;
                case 2:
                    antiAliasing = AntiAliasingMSAA.Four;
                    break;
                case 3:
                    antiAliasing = AntiAliasingMSAA.Eight;
                    break;
                default:
                    antiAliasing = AntiAliasingMSAA.Off;
                    break;
            }

            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GraphicSettings.AntiAliasingMSAA, antiAliasing);
        }, GameManager.GameInstance.GlobalSettings.GraphicSettings.AntiAliasingMSAA);

        pauseMenuGroups[k_RENDERSCALEINDEX].SetGroupAction_Slider(x =>
        {
            float value = math.remap(0f, 1f, 0.5f, 1f, x);

            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GraphicSettings.RenderScale, value);
        }, math.remap(0.5f, 1f, 0f, 1f, GameManager.GameInstance.GlobalSettings.GraphicSettings.RenderScale));

        pauseMenuGroups[k_VSYNCINDEX].SetGroupAction_Toggle(x => GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GraphicSettings.IsUseVsync, x), GameManager.GameInstance.GlobalSettings.GraphicSettings.IsUseVsync);

        pauseMenuGroups[k_FRAMELIMITINDEX].SetGroupAction_InputField(x =>
        {
            bool parseResult = int.TryParse(x, out int result);

            if (!parseResult)
            {
                return;
            }

            GameManager.GameInstance.GlobalSettings.EditSettings(() => GameManager.GameInstance.GlobalSettings.GraphicSettings.FrameRateLimit, result);
        }, GameManager.GameInstance.GlobalSettings.GraphicSettings.FrameRateLimit.ToString());
    }
    private List<Vector2Int> GetAllScreenResolutions()
    {
        HashSet<Vector2Int> result = new();

        Resolution[] resolutions = Screen.resolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Vector2Int resolution = new Vector2Int(resolutions[i].width, resolutions[i].height);

            if (result.Contains(resolution))
            {
                continue;
            }

            result.Add(resolution);
        }

        return result.ToList();
    }

    private List<string> GetStringRepresentationOfScreenResolution(List<Vector2Int> resolutions)
    {
        List<string> result = new List<string>(resolutions.Count);

        for (int i = 0; i < resolutions.Count; i++)
        {
            result.Add($"{resolutions[i].x} x {resolutions[i].y}");
        }

        return result;
    }

    private string GetStringRepresentationOfCurrentScreenResolution()
    {
        return $"{Screen.width} x {Screen.height}";
    }
}
