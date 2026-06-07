using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIBehavior : MonoBehaviour
{
    private const int k_CURSORNONECOLORINDEX = 0;
    private const int k_CURSORACOLORINDEX = 1;
    private const int k_CURSORBCOLORINDEX = 2;
    [SerializeField] private Color[] cursorColors;
    private GameplayManager gameplayManager;

    [SerializeField] private TMP_Text comboText;

    [SerializeField] private RawImage cursorRawImage;
    void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnMouseActiveTypeChanged += GameplayManager_OnMouseActiveTypeChanged;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        comboText.text = gameplayManager.CurrentCombo.ToString();
    }

    private void GameplayManager_OnGameplayEnded()
    {
        cursorRawImage.gameObject.SetActive(false);
    }

    private void GameplayManager_OnMouseActiveTypeChanged(MouseActiveType obj)
    {
        switch (obj)
        {
            case MouseActiveType.NONE:
                cursorRawImage.color = cursorColors[k_CURSORNONECOLORINDEX];
                break;
            case MouseActiveType.A:
                cursorRawImage.color = cursorColors[k_CURSORACOLORINDEX];
                break;
            case MouseActiveType.B:
                cursorRawImage.color = cursorColors[k_CURSORBCOLORINDEX];
                break;
        }
    }

    private void GameplayManager_OnGameplayStarted()
    {
        cursorRawImage.gameObject.SetActive(true);
        comboText.text = "0";
    }

    private void GameplayManager_OnHitboxMiss(int numberOfMisses)
    {
        comboText.text = "0";
    }

    private void GameplayManager_OnHitboxHit(VisualHitbox obj)
    {
        comboText.text = gameplayManager.CurrentCombo.ToString();
    }

    private void Update()
    {
        RectTransform cursorRect = cursorRawImage.rectTransform;
        cursorRect.anchorMin = cursorRect.anchorMax = gameplayManager.GameplayMousePosition;
        cursorRect.anchoredPosition = Vector2.zero;
    }
}
