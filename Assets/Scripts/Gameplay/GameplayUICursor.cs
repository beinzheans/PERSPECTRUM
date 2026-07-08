using UnityEngine;
using UnityEngine.UI;

public class GameplayUICursor : MonoBehaviour
{
    private const int k_CURSORNONECOLORINDEX = 0;
    private const int k_CURSORACOLORINDEX = 1;
    private const int k_CURSORBCOLORINDEX = 2;
    [SerializeField] private Color[] cursorColors;
    [SerializeField] private RawImage cursorRawImage;
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
    private void Update()
    {
        RectTransform cursorRect = cursorRawImage.rectTransform;
        cursorRect.anchorMin = cursorRect.anchorMax = gameplayManager.GameplayMousePosition;
        cursorRect.anchoredPosition = Vector2.zero;
    }

}
