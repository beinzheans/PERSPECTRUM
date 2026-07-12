using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader SceneLoaderInstance;

    public const string k_TITLESCREENINDEX = "TitleScreen";
    public const string k_CHARTCHOOSESCREENINDEX = "ChartChooseScene";
    public const string k_EDITORINDEX = "EditorScene";
    public const string k_GAMEPLAYINDEX = "GameplayScene";
    public const string k_CALIBRATIONINDEX = "AudioCalibrationScene";
    public const string k_LOADINGSCENEINDEX = "LoadingScreen";
    /// <summary>
    /// An action for when the next scene is current loading. Accepts a float for the progress from [0, 1].
    /// </summary>
    public event Action<float> OnNextSceneLoading;
    /// <summary>
    /// An action for when the next scene is starting to be activated.
    /// </summary>
    public event Action OnNextSceneStartActivate;

    public event Action<double> OnTransitionToBlack;
    public event Action<double> OnTransitionToNextScene;

    public const double k_LOADINGMINTRANSITIONTIME = 0.5d;

    TimerStopwatchAction transitionToBlackTimer;
    TimerStopwatchAction transitionToNewSceneTimer;
    private void Awake()
    {
        if (SceneLoaderInstance == null)
        {
            SceneLoaderInstance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Loads the next scene by a name with an additional callback when the next scene is loaded.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="callback"></param>
    public void LoadSceneByName(string sceneName, Func<Task> callback)
    {
        GameManager.GameInstance.InputActions.Gameplay.EscapeMenuInput.Disable(); // disallow pausing if loading next scene
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(transitionToBlackTimer);
        transitionToBlackTimer = new TimerStopwatchAction(this, x => OnTransitionToBlack?.Invoke(x), () => StartCoroutine(LoadSceneAtIndexAsync(sceneName, callback)), 0d, k_LOADINGMINTRANSITIONTIME, false); // transition to black before doing anything
        DSPTimerEngine.TimerInstance.AddActionToTimer(transitionToBlackTimer);
    }


    private const float k_MAXLOADPROGRESS = 0.9f; // 0.9 is max progress while loading, anything above is to actually activiate
    private IEnumerator LoadSceneAtIndexAsync(string sceneName, Func<Task> callback)
    {
        yield return LoadIntermediateLoadingSceneAsync();

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (loadOperation == null)
        {
            Debug.LogWarning($"Failed to load scene {sceneName}.");
            GameManager.GameInstance.InputActions.Gameplay.EscapeMenuInput.Enable();
            yield break;
        }

        loadOperation.allowSceneActivation = false;
        while (loadOperation.progress < 0.9f)
        {
            OnNextSceneLoading?.Invoke(loadOperation.progress / k_MAXLOADPROGRESS);
            yield return null;
        }

        loadOperation.allowSceneActivation = true;
        OnNextSceneStartActivate?.Invoke(); 

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // here we have completely loaded the next scene
        // ready to unload the loading scene.

        yield return UnloadIntermediateLoadingSceneAsync();

        yield return new WaitForSceneCallbackComplete(callback?.Invoke());

        // completely loaded 
        DSPTimerEngine.TimerInstance.RemoveActionFromTimer(transitionToNewSceneTimer);
        transitionToNewSceneTimer = new TimerStopwatchAction(this, x => OnTransitionToNextScene?.Invoke(x), () => GameManager.GameInstance.InputActions.Gameplay.EscapeMenuInput.Enable(), 0d, k_LOADINGMINTRANSITIONTIME, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(transitionToNewSceneTimer);
    }

    private IEnumerator LoadIntermediateLoadingSceneAsync()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(k_LOADINGSCENEINDEX, LoadSceneMode.Single);

        if (asyncOperation == null)
        {
            Debug.LogWarning("Failed to load the intermediate loading scene.");
            GameManager.GameInstance.InputActions.Gameplay.EscapeMenuInput.Enable();
            yield break;
        }

        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= k_MAXLOADPROGRESS)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private IEnumerator UnloadIntermediateLoadingSceneAsync()
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(k_LOADINGSCENEINDEX);

        if (asyncOperation == null)
        {
            Debug.LogWarning($"Failed to unload the intermediate loading scene.");
            GameManager.GameInstance.InputActions.Gameplay.EscapeMenuInput.Enable();
            yield break;
        }

        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}

public class WaitForSceneCallbackComplete : CustomYieldInstruction
{
    private readonly Task task;

    public WaitForSceneCallbackComplete(Task task)
    {
        this.task = task;
    }

    public override bool keepWaiting => !task.IsCompleted;


}