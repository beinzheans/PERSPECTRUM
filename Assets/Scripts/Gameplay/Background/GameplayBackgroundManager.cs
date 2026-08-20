using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class responsible for handling the background of the gameplay. <br></br>
/// Creates a blurred background image in gameplay, if one exists, otherwise creates a base color background. <br></br>
/// Additionally handles the pulse of the background.
/// </summary>
public class GameplayBackgroundManager : MonoBehaviour
{
    private static readonly int k_SHADER_BLURAMOUNT = Shader.PropertyToID("_Sigma");
    private static readonly int k_SHADER_GRADIENTCOLORAKEY = Shader.PropertyToID("_ColorA");
    private static readonly int k_SHADER_GRADIENTCOLORBKEY = Shader.PropertyToID("_ColorB");

    private Texture2D textureCache;

    [SerializeField] private RawImage rawImage;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    [SerializeField] private Material blurMaterial;
    [SerializeField] private Material gradientMaterial;

    [SerializeField] private Color gradientAColor_Default;
    [SerializeField] private Color gradientBColor_Default;

    [SerializeField] private Color gradientAColor_Pulse;
    [SerializeField] private Color gradientBColor_Pulse;

    [SerializeField] private Image darkPanelImage;

    private GameplayManager gameplayManager;

    int metronomeLoopIndex = 0;

    TimerStopwatchAction pulseAction;

    private bool isUsingCustomBackground = false;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        gameplayManager.OnGameplayChartLoaded += GameplayManager_OnGameplayChartLoaded;
        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        gameplayManager.OnGameplayMetronomeFired += GameplayManager_OnGameplayMetronomeFired;

    }

    private void OnDestroy()
    {
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
    }

    private void GameplayManager_OnGameplayChartLoaded(AudioClip clip, Texture2D texture, EditorChartMetadata metadata)
    {
        if (texture == null)
        {
            textureCache = null;
            return;
        }

        textureCache = texture;
        aspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
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
            float darkenAmount = GetDarkenAmountBasedOnSettings();

            if (isUsingCustomBackground)
            {
                float pulseAmount = -math.remap(0f, 1f, 0f, 0.2f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundPulseStrength);
                darkPanelImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(darkenAmount + pulseAmount));
            }
            else
            {
                darkPanelImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(darkenAmount));
            }

            pulseAction = new TimerStopwatchAction(this, (x) => PulseBackground(x), () => { }, 0d, GetPulseLength(), false);
            DSPTimerEngine.TimerInstance.AddActionToTimer(pulseAction);
        }

        metronomeLoopIndex = (metronomeLoopIndex + 1) % k_CAMERABACKGROUNDPULSEBEAT;
    }

    private void PulseBackground(double timeElapsed)
    {
        double progress = timeElapsed / GetPulseLength();

        float darkenAmount = GetDarkenAmountBasedOnSettings();
        if (isUsingCustomBackground)
        {
            float pulseAmount = -math.remap(0f, 1f, 0f, 0.2f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundPulseStrength);

            float dAlpha = Mathf.Lerp(pulseAmount, 0f, (float)progress);
            darkPanelImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(darkenAmount + dAlpha));
        }
        else
        {
            Color colorA_pulse = Color.Lerp(gradientAColor_Default, gradientAColor_Pulse, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundPulseStrength);
            Color colorB_pulse = Color.Lerp(gradientBColor_Default, gradientBColor_Pulse, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundPulseStrength);

            Color colorA = Color.Lerp(colorA_pulse, gradientAColor_Default, (float)progress);
            Color colorB = Color.Lerp(colorB_pulse,gradientBColor_Default, (float)progress);
            gradientMaterial.SetColor(k_SHADER_GRADIENTCOLORAKEY, colorA);
            gradientMaterial.SetColor(k_SHADER_GRADIENTCOLORBKEY, colorB);
        }
    }
    private void GameplayManager_OnGameplayRestarted()
    {
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(pulseAction);
        metronomeLoopIndex = 0;
    }

    private void GameInstance_OnGameSettingsChanged()
    {
        UpdateBackground(textureCache);
    }

    private void GameplayManager_OnGameplayStarted()
    {
        UpdateBackground(textureCache);
    }

    private void SetupFog()
    {
        RenderSettings.fogColor = Color.black;
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
            rawImage.texture = null;
            rawImage.material = gradientMaterial;

            gradientMaterial.SetColor(k_SHADER_GRADIENTCOLORAKEY, gradientAColor_Default);
            gradientMaterial.SetColor(k_SHADER_GRADIENTCOLORBKEY, gradientBColor_Default);
        }
        else
        {
            isUsingCustomBackground = true;
            rawImage.texture = texture;
            rawImage.material = blurMaterial;


            float blurAmount = math.remap(0f, 1f, 0.1f, 3f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundBlurAmount);
            blurMaterial.SetFloat(k_SHADER_BLURAMOUNT, blurAmount);
        }

        float darkenAmount = GetDarkenAmountBasedOnSettings();
        darkPanelImage.color = new Color(0f, 0f, 0f, darkenAmount);
    }

    private double GetPulseLength()
    {
        return 60d / gameplayManager.CurrentActiveGameplayMarker.BPM * (k_CAMERABACKGROUNDPULSEBEAT / 2);

    }

    /// <summary>
    /// Gets the alpha of <see cref="darkPanelImage"/> based on the settings and usage of custom backgrounds. <br></br>
    /// Note the mapping is vastly different.
    /// </summary>
    /// <returns></returns>
    private float GetDarkenAmountBasedOnSettings()
    {
        if (isUsingCustomBackground)
        {
            return math.remap(0f, 1f, 0.8f, 0.99f, GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundDarkenAmount);
        }
        else
        {
            return GameManager.GameInstance.GlobalSettings.GameSettings.BackgroundDarkenAmount;
        }
    }
}
