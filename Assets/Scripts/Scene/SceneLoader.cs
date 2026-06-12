using System;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static Action callbackOnSceneLoad;
    public static void LoadSceneAtIndex(int index, Action callback)
    {
        callbackOnSceneLoad = callback;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }

    private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        callbackOnSceneLoad?.Invoke();
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded; // unsubscribe so we don't keep repeating this, that way this callback occurs only when the scene is loaded
    }
}
