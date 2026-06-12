using Newtonsoft.Json;
using SFB;
using UnityEngine;
public class EditorChartTool : EditorUIBehavior
{
    protected override async void UI_OnButtonPress(int index)
    {
        if (index < (int)ChartOptions.LOAD_AUDIO_FILE || index > (int)ChartOptions.EXITEDITOR)
        {
            return;
        }

        ChartOptions currentOption = (ChartOptions)index;
        JsonSerializerSettings serializer = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        serializer.Converters.Add(new Vector2Serializer());

        switch (currentOption)
        {
            case ChartOptions.LOAD_AUDIO_FILE:
                string[] paths = StandaloneFileBrowser.OpenFilePanel("Load Audio File", "", "mp3", false);
                if (paths.Length <= 0)
                {
                    return;
                }

                (bool success, AudioClip clip, byte[] bytes) = await AudioEngine.AudioInstance.GetAudioClipFromLocalFile(paths[0]);

                if (!success)
                {
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Failed to load audio clip");
                    return;
                }
                else
                {
                    EditorManager.EditorInstance.InvokeAudioClipLoadedEvent(clip, bytes);
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Loaded audio", 0.5d);
                }

                break;
            case ChartOptions.SAVE_EDITOR_CHART:
                EditorManager.EditorInstance.SaveEditorChart();
                break;
            case ChartOptions.LOAD_EDITOR_CHART:
                await EditorManager.EditorInstance.LoadEditorChart();
                break;
            case ChartOptions.EXITEDITOR:
                ConfirmAction action = new(() => SceneLoader.LoadSceneAtIndex(0, () => { }), () => { }, "Are you sure you want to exit?");
                GameManager.GameInstance.InvokeConfirmActionNeeded(action);
                break;
        }
    }


}

public enum ChartOptions
{
    LOAD_AUDIO_FILE = 0,
    SAVE_EDITOR_CHART = 1,
    LOAD_EDITOR_CHART = 2,
    EXITEDITOR = 3
}
