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
        if (!GameManager.GameInstance.GlobalSettings.GameEvents.HasAdjustedOffset)
        {
            ConfirmAction confirmAction = new ConfirmAction(() => SceneLoader.LoadSceneAtIndex(SceneLoader.k_CALIBRATIONINDEX, () => { }), () => SceneLoader.LoadSceneAtIndex(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => { }), "The audio offset has not been adjusted.\n" +
                                                                                                                                                                                                                                "Do you want go to the audio offset calibration screen?");
            GameManager.GameInstance.InvokeConfirmActionNeeded(confirmAction);
        }
        else
        {
            SceneLoader.LoadSceneAtIndex(SceneLoader.k_CHARTCHOOSESCREENINDEX, () => { });
        }
    }

    public void UI_OnCalibrationButtonPressed()
    {
        SceneLoader.LoadSceneAtIndex(SceneLoader.k_CALIBRATIONINDEX, () => { });
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
