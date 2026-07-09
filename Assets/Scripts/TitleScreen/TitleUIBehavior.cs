using System.IO;
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
            ConfirmAction loadConfirmAction = new ConfirmAction(() => SceneLoader.LoadSceneAtIndex(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => { }), () => { }, "It is recommended to play the tutorial first.\n" +
                                                                                                                                                                "Do you still want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(loadConfirmAction);
            return;
        }
        else
        {
            SceneLoader.LoadSceneAtIndex(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => { });
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
            ConfirmAction loadConfirmAction = new ConfirmAction(() => SceneLoader.LoadSceneAtIndex(SceneLoader.k_CALIBRATIONINDEX, () => { }), () => { }, "It is recommended to play the tutorial first, the offset screen contains gameplay elements.\n" +
                                                                                                                                                          "Do you want to continue?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(loadConfirmAction);
            return;
        }
        else
        {
            SceneLoader.LoadSceneAtIndex(SceneLoader.k_CALIBRATIONINDEX, () => { });
        }

    }

    public void UI_OnEditorButtonPressed()
    {
        SceneLoader.LoadSceneAtIndex(SceneLoader.k_EDITORINDEX, () => { });
    }

    public void UI_OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
