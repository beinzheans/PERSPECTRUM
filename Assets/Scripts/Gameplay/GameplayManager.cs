using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
public class GameplayManager : MonoBehaviour
{
    public static GameplayManager GameplayInstance;

    public event Action OnGameplayStarted;
    public event Action OnGameplayEnded;

    /// <summary>
    /// How much leniency we allow before a note is considered a miss <br></br>
    /// Note that it is impossible for notes to be early.
    /// </summary>
    public const double k_LENIENCYTIMEFRAME = 0.1d;

    public GameplayChart CurrentGameplayChart { get; private set; }

    [SerializeField] private AudioSource musicAudioSource;

    public double CurrentGameplayTime { get; private set; }
    private void Awake()
    {
        GameplayInstance = this;
    }


    private void OnDestroy()
    {
        GameplayInstance = null;
    }

    private const double k_TIMEOFFSET = 3d;
    private void StartGameplay()
    {
        if (CurrentGameplayChart == null)
        {
            Debug.LogWarning($"No gameplay chart assigned");
            return;
        }

        Debug.Log($"Starting gameplay");
        double endTime = CurrentGameplayChart.GameplayObjects[CurrentGameplayChart.GameplayObjects.Length - 1].RenderTime; // note it is sorted
        Action<double> timerElaspedAction = (x) => { CurrentGameplayTime += x; };
        Action timerEndAction = () => { InvokeGameplayEndedEvent(); };

        TimerStopwatchAction stopwatchAction = new TimerStopwatchAction(timerElaspedAction, timerEndAction, k_TIMEOFFSET, endTime, true);
        DSPTimerEngine.TimerInstance.AddActionToTimer(stopwatchAction);
    }
    
    public void Test()
    {
        Debug.Log($"Test from gameplay manager!!");
    }
    public async void InvokeGameplayStartedEvent(string path)
    {
        SaveLoadManager.LoadChartFile(path, out string chartJson, out _, out byte[] bytes);

        (bool convertResult, EditorChart editorChart, AudioClip clip) = await SaveLoadManager.ConvertFilesToEditorChart(chartJson, bytes);

        CurrentGameplayChart = MathHelper.ConvertEditorChartToGameplayChart(editorChart, clip);

        musicAudioSource.clip = clip;
        StartGameplay();
        OnGameplayStarted?.Invoke();
    }

    public void InvokeGameplayEndedEvent()
    {
        CurrentGameplayChart = null;
        OnGameplayEnded?.Invoke();
    }
}

public class GameplayChart
{
    public GameplayObject[] GameplayObjects { get; private set; }
    public AudioClip AudioClip { get; private set; }
    public GameplayChart(GameplayObject[] gameplayObjects, AudioClip audioClip)
    {
        GameplayObjects = gameplayObjects;
        AudioClip = audioClip;
    }
}
