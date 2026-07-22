using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class responsible for handling the background of the gameplay. <br></br>
/// Creates a blurred background image in gameplay, if one exists, otherwise creates a base color background. <br></br>
/// Additionally handles the pulse of the background.
/// </summary>
public class GameplayImageBlurManager : MonoBehaviour
{
    private static readonly int k_SHADER_BLURAMOUNT = Shader.PropertyToID("_Sigma");

    [SerializeField] private Texture2D testTex;
    [SerializeField] private RawImage rawImage;
    private Material blurMaterial;

    [SerializeField] private Image darkPanelImage;

    private GameplayManager gameplayManager;
    [SerializeField] private Color defaultBackgroundColor;
    [SerializeField] private Color pulseBackgroundColor_default;

    /// <summary>
    /// How much we change the alpha of <see cref="darkPanelImage"/> for a pulse. Note this value is signed assumes the value given is 0 - 255.
    /// </summary>
    [SerializeField] private float pulseBackgroundAlpha_custom;


    int metronomeLoopIndex = 0;

    TimerStopwatchAction pulseAction;

    private bool isUsingCustomBackground = false;

    private void Awake()
    {
        blurMaterial = rawImage.material;
    }
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayMetronomeFired += GameplayManager_OnGameplayMetronomeFired;
    }

    private const int k_CAMERABACKGROUNDPULSEBEAT = 4;

    private void GameplayManager_OnGameplayMetronomeFired(double obj)
    {
        if (gameplayManager.CurrentActiveGameplayMarker == null)
        {
            return;
        }

        if (metronomeLoopIndex == 0)
        {
            pulseAction = new TimerStopwatchAction(this, (x) => PulseBackground(x), () => { }, 0d, GetPulseLength(), false);
            DSPTimerEngine.TimerInstance.AddActionToTimer(pulseAction);
        }

        metronomeLoopIndex = (metronomeLoopIndex + 1) % k_CAMERABACKGROUNDPULSEBEAT;
    }

    private void PulseBackground(double timeElapsed)
    {
        double progress = timeElapsed / GetPulseLength();

        if (isUsingCustomBackground)
        {
            float darkenAmount = math.remap(0f, 1f, 0.8f, 0.95f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundDarkenAmount);
            float dAlpha = Mathf.Lerp(pulseBackgroundAlpha_custom, 0f, (float)progress) / 255;
            darkPanelImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(darkenAmount + dAlpha));
        }
        else
        {
            Color pulseColor = Color.Lerp(pulseBackgroundColor_default, defaultBackgroundColor, (float)progress);
            darkPanelImage.color = pulseColor;
        }
    }
    private void GameplayManager_OnGameplayRestarted()
    {
        metronomeLoopIndex = 0;
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        UpdateBackground(testTex);
    }

    private void GameplayManager_OnGameplayStarted()
    {
        UpdateBackground(testTex);
    }

    private void SetupFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = GameplayManager.k_HITPLANEDEPTH;
        RenderSettings.fogEndDistance = gameplayManager.GameplayFarClipPlane;
    }
    private void UpdateBackground(Texture2D texture)
    {
        SetupFog();

        if (texture == null || !GameManager.GameInstance.GlobalSettings.GameSettings.UseCustomBackground)
        {
            isUsingCustomBackground = false;
            rawImage.gameObject.SetActive(false);
            darkPanelImage.color = defaultBackgroundColor;

            RenderSettings.fogColor = defaultBackgroundColor;
            return;
        }

        isUsingCustomBackground = true;

        rawImage.gameObject.SetActive(true);
        rawImage.texture = texture;
        float darkenAmount = math.remap(0f, 1f, 0.8f, 0.95f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundDarkenAmount);
        darkPanelImage.color = new Color(0f, 0f, 0f, darkenAmount);

        float blurAmount = math.remap(0f, 1f, 0.1f, 3f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundBlurAmount);
        blurMaterial.SetFloat(k_SHADER_BLURAMOUNT, blurAmount);

        RenderSettings.fogColor = Color.black;
    }

    private double GetPulseLength()
    {
        return 60d / gameplayManager.CurrentActiveGameplayMarker.BPM * (k_CAMERABACKGROUNDPULSEBEAT / 2);

    }
}
