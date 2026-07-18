using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUICursor : MonoBehaviour
{
    private const int k_CURSORNONECOLORINDEX = 0;
    private const int k_CURSORACOLORINDEX = 1;
    private const int k_CURSORBCOLORINDEX = 2;
    [SerializeField] private Color[] cursorColors;
    [SerializeField] private RawImage cursorRawImage;
    [SerializeField] private RectTransform cursorTrailParticleSystemRectTransform;
    [SerializeField] private ParticleSystem cursorTrailParticleSystem;

    private GameplayManager gameplayManager;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnMouseActiveTypeChanged += GameplayManager_OnMouseActiveTypeChanged;
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded -= GameplayManager_OnGameplayEnded;
        gameplayManager.OnMouseActiveTypeChanged -= GameplayManager_OnMouseActiveTypeChanged;
    }

    private void GameplayManager_OnMouseActiveTypeChanged(MouseActiveType obj)
    {
        Color newColor;
        switch (obj)
        {
            case MouseActiveType.NONE:
                newColor = cursorColors[k_CURSORNONECOLORINDEX];
                break;
            case MouseActiveType.A:
                newColor = cursorColors[k_CURSORACOLORINDEX];
                break;
            case MouseActiveType.B:
                newColor = cursorColors[k_CURSORBCOLORINDEX];
                break;
            default:
                newColor = cursorColors[k_CURSORNONECOLORINDEX];
                break;
        }


        cursorRawImage.color = newColor;
        ParticleSystem.MainModule mainModule = cursorTrailParticleSystem.main;

        mainModule.startColor = newColor;
    }

    private void GameplayManager_OnGameplayEnded()
    {
        GameVirtualCursor.GameVirtualCursorInstance.ShowVirtualMouse();
        cursorRawImage.gameObject.SetActive(false);
    }

    private void GameplayManager_OnGameplayStarted()
    {
        GameVirtualCursor.GameVirtualCursorInstance.HideVirtualMouse();
        cursorRawImage.gameObject.SetActive(true);
    }

    private const float k_MOUSEROTATIONDEADZONEMAGNTIUDE = 5f;
    private const float k_MAXXSCALESIZE = 5f;
    private const float k_MAXSPEEDMAGNTIUDE = 25f;

    private const float k_DEADZONENORMALSCALESPEED = 10f;
    private void Update()
    {
        RectTransform cursorRect = cursorRawImage.rectTransform;
        cursorRect.anchorMin = cursorRect.anchorMax = gameplayManager.GameplayMousePosition;
        cursorRect.anchoredPosition = Vector2.zero;

        cursorTrailParticleSystemRectTransform.anchorMin = cursorTrailParticleSystemRectTransform.anchorMax = gameplayManager.GameplayMousePosition;
        cursorTrailParticleSystemRectTransform.anchoredPosition = Vector2.zero;

        Vector2 mousePixelDisplacement = GameVirtualCursor.GameVirtualCursorInstance.MouseDisplacement;

        if (mousePixelDisplacement.sqrMagnitude < (k_MOUSEROTATIONDEADZONEMAGNTIUDE * k_MOUSEROTATIONDEADZONEMAGNTIUDE))
        {
            cursorRect.localScale = Vector3.Slerp(cursorRect.localScale, Vector3.one, Time.deltaTime * k_DEADZONENORMALSCALESPEED);
            return;
        }

        float zRadian = MathHelper.IsTwoFloatsEqualWithEpsilion(mousePixelDisplacement.x, 0f) ? Mathf.PI / 2 : Mathf.Atan2(mousePixelDisplacement.y, mousePixelDisplacement.x);

        float zRotation = GetConvertedRotationAngleForSymmetricalUI(zRadian) * Mathf.Rad2Deg;

        cursorRect.rotation = Quaternion.Euler(0f, 0f, zRotation);

        float normalizedX = math.remap(k_MOUSEROTATIONDEADZONEMAGNTIUDE * k_MOUSEROTATIONDEADZONEMAGNTIUDE, k_MAXSPEEDMAGNTIUDE * k_MAXSPEEDMAGNTIUDE, 0f, 1f, mousePixelDisplacement.sqrMagnitude);

        cursorRect.localScale = new Vector3(MathHelper.EvaluateSigmoidFunction(normalizedX, 1f, k_MAXXSCALESIZE, 5f, 0.5f), 1f, 1f);
    }

    /// <summary>
    /// Returns the corrected rotation angle (in radians) for symmertical UI elements given a <paramref name="angle"/> in [-pi, pi] range.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float GetConvertedRotationAngleForSymmetricalUI(float angle)
    {
        if (angle > -Mathf.PI / 2 && angle < Mathf.PI / 2)
        {
            return angle;
        }
        else if (angle >= Mathf.PI / 2)
        {
            return angle - Mathf.PI;
        }
        else
        {
            return Mathf.PI + angle;
        }
    }

}
