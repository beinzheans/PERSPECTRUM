using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TitleUIBehavior : MonoBehaviour
{
    [SerializeField] private TMP_Text gameVersionText;

    private void Start()
    {
        gameVersionText.text = $"Version {GameManager.GameInstance.CurrentVersion}";
    }
    public void UI_OnPlayButtonPressed()
    {
        if (!GameManager.GameInstance.GlobalSettings.GameEvents.HasPlayedTutorial)
        {
            ConfirmAction loadConfirmAction = new ConfirmAction(() => SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => Task.CompletedTask), () => { }, "It is recommended to play the tutorial first.\n" +
                                                                                                                                                                "Do you still want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(loadConfirmAction);
            return;
        }
        else
        {
            SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => Task.CompletedTask);
        }
    }

    public void UI_OnTutorialButtonPressed()
    {
        GameManager.GameInstance.RequestPlayChartEvent(GameManager.GameInstance.k_TUTORIALFILEPATHSTRING);
    }

    public void UI_OnCalibrationButtonPressed()
    {
        if (!GameManager.GameInstance.GlobalSettings.GameEvents.HasPlayedTutorial)
        {
            ConfirmAction loadConfirmAction = new ConfirmAction(() => SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_CALIBRATIONINDEX, () => Task.CompletedTask), () => { }, "It is recommended to play the tutorial first, the offset screen contains gameplay elements.\n" +
                                                                                                                                                          "Do you want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(loadConfirmAction);
            return;
        }
        else
        {
            SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_CALIBRATIONINDEX, () => Task.CompletedTask);
        }

    }

    public void UI_OnEditorButtonPressed()
    {
        SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_EDITORINDEX, () => Task.CompletedTask);
    }

    public void UI_OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
