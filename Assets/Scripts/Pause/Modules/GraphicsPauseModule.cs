using UnityEngine;

public class GraphicsPauseModule : BasePauseModule
{
    // todo: make this access URP, change:
    // resolution
    // anti-aliasing (off, 2x, 4x, 8x)
    // render scale [0,1]
    // Vsync (on or off)
    // frame cap (overrides Vsync)
    protected override void OnModuleInitialized()
    {
    }
}
