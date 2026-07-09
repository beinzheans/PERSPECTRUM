using Newtonsoft.Json.Linq;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChartChooseManager : MonoBehaviour
{
    public static ChartChooseManager ChartChooseInstance;
    [SerializeField] private RectTransform ChartChooseContentRect;
    [SerializeField] private ChartButtonBehavior chartButtonPrefab;
    [SerializeField] private Button importChartButton;
    [SerializeField] private Button returnMainMenuButton;

    [SerializeField] private TMP_Text importedChartsText;

    public event Action<ChartGameplayRecordButtonBehavior> OnChartRecordButtonClicked;
    public event Action<ChartButtonBehavior> OnChartButtonClicked;
    public event Action OnChartDeleted;
    public ChartButtonBehavior CurrentSelectedChartButton { get; private set; }
    private List<ChartButtonBehavior> spawnedChartButtonBehaviors = new();
    private void Awake()
    {
        ChartChooseInstance = this;
    }

    private void OnDestroy()
    {
        ChartChooseInstance = null;
    }

    private void Start()
    {
        CurrentSelectedChartButton = null;
        CreateChartButtons();
    }
    private void CreateChartButtons()
    {
        GamePersistenceManager.ReadEditorChartsInGameStorage(out string[] allPaths);

        for (int i = 0; i < allPaths.Length; i++)
        {
            AddChartButton(allPaths[i]);
        }

        importedChartsText.text = $"Imported Charts ({spawnedChartButtonBehaviors.Count})";
    }

    private void AddChartButton(string path)
    {
        GamePersistenceManager.GetMetadataJsonOfEditorChartPath(path, out string metadataJson);

        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return;
        }

        JObject metadataJObject = JObject.Parse(metadataJson);
        if (!GameVersionConverter.GetBaseDetailsFromMetadataJObject(metadataJObject, out BaseChartMetadata baseChartMetadata))
        {
            Debug.Log($"Removed chart due to unsupported file. Path:\n" +
                      $"{path}");
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Ignored and deleted old chart. Check log.", 1d);
            File.Delete(path); // the json is not valid to be our chart anymore, we are going to delete it from imported storage.
            return;
        }

        ChartButtonBehavior behavior = Instantiate(chartButtonPrefab, ChartChooseContentRect, false);
        behavior.AssignChartButtonValues(baseChartMetadata, path);

        spawnedChartButtonBehaviors.Add(behavior);
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

        importedChartsText.text = $"Imported Charts ({spawnedChartButtonBehaviors.Count})";
    }

    public void UI_ReturnMainMenuButton()
    {
        SceneLoader.LoadSceneAtIndex(SceneLoader.k_TITLESCREENINDEX, () => { });
    }

    public void DeleteChartWithPath(string path)
    {

        int deleteIndex = -1;
        for (int i = 0; i < spawnedChartButtonBehaviors.Count; i++)
        {
            if (spawnedChartButtonBehaviors[i].AssociatedFullFilePath != path)
            {
                continue;
            }

            deleteIndex = i;
        }

        if (deleteIndex == -1)
        {
            return;
        }

        Destroy(spawnedChartButtonBehaviors[deleteIndex].gameObject);
        spawnedChartButtonBehaviors.RemoveAt(deleteIndex);

        File.Delete(path);

        importedChartsText.text = $"Imported Charts ({spawnedChartButtonBehaviors.Count})";
        OnChartDeleted?.Invoke();
    }

    public void InvokeOnChartButtonClickedEvent(ChartButtonBehavior chartButton)
    {
        CurrentSelectedChartButton = chartButton;
        OnChartButtonClicked?.Invoke(CurrentSelectedChartButton);
    }

    public void InvokeOnChartRecordButtonClickedEvent(ChartGameplayRecordButtonBehavior recordButton)
    {
        OnChartRecordButtonClicked?.Invoke(recordButton);
    }
}
