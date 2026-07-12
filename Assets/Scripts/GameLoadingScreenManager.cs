using UnityEngine;

public class GameLoadingScreenManager : MonoBehaviour
{
    [SerializeField] private RectTransform blackPanelRectTransform;

    private void Start()
    {
        SceneLoader.SceneLoaderInstance.OnTransitionToBlack += SceneLoaderInstance_OnTransitionToBlack;
        SceneLoader.SceneLoaderInstance.OnTransitionToNextScene += SceneLoaderInstance_OnTransitionToNextScene;
    }

    private void SceneLoaderInstance_OnTransitionToNextScene(double obj)
    {
        blackPanelRectTransform.anchorMin = new Vector2(Mathf.SmoothStep(0f, 1f, (float)(obj / SceneLoader.k_LOADINGMINTRANSITIONTIME)), 0f);
        blackPanelRectTransform.anchorMax = new Vector2(Mathf.SmoothStep(1f, 2f, (float)(obj / SceneLoader.k_LOADINGMINTRANSITIONTIME)), 1f);

        blackPanelRectTransform.anchoredPosition = Vector2.zero;

    }

    private void SceneLoaderInstance_OnTransitionToBlack(double obj)
    {
        blackPanelRectTransform.anchorMin = new Vector2(Mathf.SmoothStep(-1f, 0f, (float)(obj / SceneLoader.k_LOADINGMINTRANSITIONTIME)), 0f);
        blackPanelRectTransform.anchorMax = new Vector2(Mathf.SmoothStep(0f, 1f, (float)(obj / SceneLoader.k_LOADINGMINTRANSITIONTIME)), 1f);

        blackPanelRectTransform.anchoredPosition = Vector2.zero;
    }
}
