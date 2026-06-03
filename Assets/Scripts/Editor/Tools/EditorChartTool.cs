using Newtonsoft.Json;
using SFB;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
public class EditorChartTool : EditorUIBehavior
{
    protected override async void UI_OnButtonPress(int index)
    {
        if (index < (int)ChartOptions.LOAD_AUDIO_FILE || index > (int)ChartOptions.CREATE_NEW_CHART)
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
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Loaded audio");
                }

                break;
            case ChartOptions.REMOVE_AUDIO_FILE:
                break;
            case ChartOptions.SAVE_EDITOR_CHART:
                EditorManager.EditorInstance.SaveEditorChart();
                break;
            case ChartOptions.LOAD_EDITOR_CHART:
                await EditorManager.EditorInstance.LoadEditorChart();
                break;
            case ChartOptions.EXPORT_EDITOR_CHART:
                break;
            case ChartOptions.CREATE_NEW_CHART:
                break;
        }
    }


}

public enum ChartOptions
{
    LOAD_AUDIO_FILE = 0,
    REMOVE_AUDIO_FILE = 1,
    SAVE_EDITOR_CHART = 2,
    LOAD_EDITOR_CHART = 3,
    EXPORT_EDITOR_CHART = 4,
    CREATE_NEW_CHART = 5
}
