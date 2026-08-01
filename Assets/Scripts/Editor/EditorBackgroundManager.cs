using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EditorBackgroundManager : EditorUIBehavior
{
    [SerializeField] private RawImage backgroundImage;
    [SerializeField] private AspectRatioFitter backgroundAspectRatio;


    protected override void Start()
    {
        base.Start();
        EditorManager.EditorInstance.OnBackgroundTextureLoaded += EditorInstance_OnBackgroundTextureLoaded;
    }

    private void OnDestroy()
    {
        EditorManager.EditorInstance.OnBackgroundTextureLoaded -= EditorInstance_OnBackgroundTextureLoaded;
    }

    private void EditorInstance_OnBackgroundTextureLoaded(Texture2D obj)
    {
        if (obj == null)
        {
            backgroundImage.texture = null;
            backgroundAspectRatio.aspectRatio = 1f;
            return;
        }

        backgroundImage.texture = obj;
        backgroundAspectRatio.aspectRatio = (float)obj.width / obj.height;
    }

    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)BackgroundOptions.LOAD_IMAGE || index > (int)BackgroundOptions.REMOVE_IMAGE)
        {
            return;
        }

        switch (index)
        {
            case (int)BackgroundOptions.LOAD_IMAGE:
                string[] paths = StandaloneFileBrowser.OpenFilePanel("Load custom BG", "", new ExtensionFilter[1] {
                new ExtensionFilter("Image Files", "png", "jpg")
                }, false);

                if (paths == null || paths.Length < 1)
                {
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Invalid image file");
                    return;
                }

                byte[] imageBytes = File.ReadAllBytes(paths[0]);
                bool imageResult = GamePersistenceManager.GetTexture2DFromBytes(imageBytes, out Texture2D texture);
                if (!imageResult)
                {
                    return;
                }

                EditorManager.EditorInstance.InvokeBackgroundTextureLoadedEvent(texture, imageBytes);
                GameManager.GameInstance.InvokeInformationDisplayNeeded("Loaded background");
                break;
            case (int)BackgroundOptions.REMOVE_IMAGE:
                EditorManager.EditorInstance.InvokeRemoveBackgroundTextureEvent();
                break;
        }
    }

    private enum BackgroundOptions
    {
        LOAD_IMAGE = 0,
        REMOVE_IMAGE = 1,
    }
}
