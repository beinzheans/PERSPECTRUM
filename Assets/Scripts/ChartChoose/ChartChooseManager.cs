using Newtonsoft.Json.Linq;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChartChooseManager : MonoBehaviour
{
    public static ChartChooseManager ChartChooseInstance;
    [SerializeField] private ChartButtonBehavior chartButtonPrefab;
    [SerializeField] private Button importChartButton;
    [SerializeField] private Button returnMainMenuButton;

    [SerializeField] private TMP_Text importedChartsText;

    public event Action<ChartGameplayRecordButtonBehavior> OnChartRecordButtonClicked;
    public event Action<ChartButtonBehaviorContents> OnChartButtonClicked;
    public event Action<ChartButtonBehaviorContents> OnChartButtonNeededAdd;

    public event Func<ChartButtonBehaviorContents> OnRequestCurrentSelectedChartButton;
    public event Action<ChartButtonBehaviorContents> OnChartDeleted;

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
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Import Chart", "", GameManager.k_FILEEXTENSION, false);

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

    public void RequestRemoveChart()
    {
        ChartButtonBehaviorContents contentsToDelete = OnRequestCurrentSelectedChartButton?.Invoke();

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
        ChartButtonBehaviorContents contents = OnRequestCurrentSelectedChartButton?.Invoke();

        if (contents == null)
        {
            return;
        }

        GameManager.GameInstance.RequestPlayChartEvent(contents.AssociatedFullFilePath);
    }

    public void RequestReplayChart(GameplayStatisticRecord record)
    {
        ChartButtonBehaviorContents contents = OnRequestCurrentSelectedChartButton?.Invoke();

        if (contents == null)
        {
            return;
        }

        GameManager.GameInstance.RequestReplayChartEvent(contents.AssociatedFullFilePath, record);
    }
    public void InvokeOnChartButtonClickedEvent(ChartButtonBehaviorContents contents)
    {
        OnChartButtonClicked?.Invoke(contents);
    }
    public void InvokeOnChartRecordButtonClickedEvent(ChartGameplayRecordButtonBehavior recordButton)
    {
        OnChartRecordButtonClicked?.Invoke(recordButton);
    }
}
