using Newtonsoft.Json.Linq;
using SFB;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ChartChooseManager : MonoBehaviour
{
    public static ChartChooseManager ChartChooseInstance;
    [SerializeField] private Button importChartButton;
    [SerializeField] private Button returnMainMenuButton;

    public event Action<ChartGameplayRecordButtonBehavior> OnChartRecordButtonClicked;
    public event Action<ChartButtonBehaviorContents, int> OnChartButtonClicked;
    public event Action<ChartButtonBehaviorContents> OnChartButtonNeededAdd;

    public event Func<(ChartButtonBehaviorContents, int)> OnRequestCurrentSelectedChartButton;
    public event Action<ChartButtonBehaviorContents> OnChartDeleted;

    public event Action<ChartButtonSortOptions> OnSortOptionSelected;
    public event Action<ChartSortOrder> OnSortOrderChanged;

    public int CurrentSelectChartContentID { get; private set; }

    private void Awake()
    {
        ChartChooseInstance = this;
    }

    private void OnDestroy()
    {
        ChartChooseInstance = null;
    }

    public void InitializeChartButtonsFromFile()
    {
        GamePersistenceManager.ReadEditorChartsInGameStorage(out string[] allPaths);

        for (int i = 0; i < allPaths.Length; i++)
        {
            AddChartButton(allPaths[i]);
        }
    }

    private void AddChartButton(string path)
    {
        GamePersistenceManager.GetMetadataJsonOfEditorChartPath(path, out string metadataJson);

        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            Debug.Log($"Removed chart due to null / empty JSON. Path:\n" +
                      $"{path}");

            File.Delete(path);
            return;
        }

        JObject metadataJObject = JObject.Parse(metadataJson);
        if (!GameVersionConverter.GetBaseDetailsFromMetadataJObject(metadataJObject, out BaseChartMetadata baseChartMetadata))
        {
            Debug.Log($"Removed chart due to unsupported file. Path:\n" +
                      $"{path}");
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Ignored and deleted invalid chart. Check log.", 1d);
            File.Delete(path);
            return;
        }

        ChartButtonBehaviorContents contents = new ChartButtonBehaviorContents(baseChartMetadata, path);

        OnChartButtonNeededAdd?.Invoke(contents);
    }

    public void UI_ImportButtonClicked()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Import Chart", "", GameManager.k_FILEEXTENSION_EDITOR, false);

        if (paths.Length <= 0)
        {
            return;
        }

        if (!GamePersistenceManager.ImportEditorChartToGameStorage(paths[0], out string internalChartPath))
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Failed to import chart", 1d);
            return;
        }

        AddChartButton(internalChartPath);
    }

    public void UI_ReturnMainMenuButton()
    {
        SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_TITLESCREENINDEX, () => Task.CompletedTask);
    }

    public void UI_SettingsButton()
    {
        GameManager.GameInstance.RequestOverrideGamePauseState(true);
    }

    public void RequestRemoveChart()
    {
        var request = OnRequestCurrentSelectedChartButton?.Invoke();
        if (request == null)
        {
            return;
        }

        ChartButtonBehaviorContents contentsToDelete = request.Value.Item1;
        if (contentsToDelete == null)
        {
            return;
        }

        OnChartDeleted?.Invoke(contentsToDelete);
        Debug.Log($"Deleted {contentsToDelete.AssociatedFullFilePath}");
        File.Delete(contentsToDelete.AssociatedFullFilePath);
    }

    public void RequestPlayChart()
    {
        var request = OnRequestCurrentSelectedChartButton?.Invoke();

        if (request == null)
        {
            return;
        }

        ChartButtonBehaviorContents contents = request.Value.Item1;

        if (contents == null)
        {
            return;
        }

        GameManager.GameInstance.RequestPlayChartEvent(contents.AssociatedFullFilePath);
    }

    public void RequestReplayChart(GameplayStatisticRecord record)
    {
        var request = OnRequestCurrentSelectedChartButton?.Invoke();

        if (request == null)
        {
            return;
        }

        ChartButtonBehaviorContents contents = request.Value.Item1;

        if (contents == null)
        {
            return;
        }

        GameManager.GameInstance.RequestReplayChartEvent(contents.AssociatedFullFilePath, record);
    }
    public void InvokeOnChartButtonClickedEvent(ChartButtonBehaviorContents contents, int contentID)
    {
        CurrentSelectChartContentID = contentID;
        OnChartButtonClicked?.Invoke(contents, contentID);
    }
    public void InvokeOnChartRecordButtonClickedEvent(ChartGameplayRecordButtonBehavior recordButton)
    {
        OnChartRecordButtonClicked?.Invoke(recordButton);
    }

    public void InvokeOnChartSortingOptionSelectedEvent(ChartButtonSortOptions sortingOption)
    {
        OnSortOptionSelected?.Invoke(sortingOption);
    }

    public void InvokeOnChartOrderingOptionEvent(ChartSortOrder sortOrder)
    {
        OnSortOrderChanged?.Invoke(sortOrder);
    }
}
