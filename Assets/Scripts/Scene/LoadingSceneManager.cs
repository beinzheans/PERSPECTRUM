using UnityEngine;

/// <summary>
/// A class to handle the logic inside the loading scene.
/// </summary>
public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private Camera loadingSceneCamera;
    [SerializeField] private AudioListener loadingSceneAudioListener;

    private void Start()
    {
        SceneLoader.SceneLoaderInstance.OnNextSceneStartActivate += SceneLoaderInstance_OnNextSceneReady;
    }

    private void SceneLoaderInstance_OnNextSceneReady()
    {
        loadingSceneCamera.gameObject.SetActive(false);
        loadingSceneAudioListener.gameObject.SetActive(false);
    }


    private void OnDestroy()
    {
        SceneLoader.SceneLoaderInstance.OnNextSceneStartActivate -= SceneLoaderInstance_OnNextSceneReady;

    }
}
