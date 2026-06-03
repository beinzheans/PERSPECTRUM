using Newtonsoft.Json;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChartChooseManager : MonoBehaviour
{
    public static ChartChooseManager ChartChooseInstance;
    [SerializeField] private RectTransform ChartChooseContentRect;
    [SerializeField] private ChartButtonBehavior chartButtonPrefab;
    [SerializeField] private Button importChartButton;

    public event Action<ChartButtonBehavior> OnChartButtonClicked; 
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

    private void DeleteChartButtons()
    {

    }
    private void CreateChartButtons()
    {
        SaveLoadManager.ReadEditorChartsInGameStorage(out string[] allPaths);

        for (int i = 0; i < allPaths.Length; i++)
        {
            AddChartButton(allPaths[i]);
        }
    }

    private void AddChartButton(string path)
    {
        SaveLoadManager.GetMetadataOfEditorChartPath(path, out EditorChartMetadata metadata);

        if (metadata == null)
        {
            return;
        }

        ChartButtonBehavior behavior = Instantiate(chartButtonPrefab, ChartChooseContentRect, false);
        behavior.AssignChartButtonValues(metadata, path);

        spawnedChartButtonBehaviors.Add(behavior);
    }

    public void UI_ImportButtonClicked()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Import Chart", "", GameManager.k_FILEEXTENSION, false);

        if (paths.Length <= 0)
        {
            return;
        }
        
        SaveLoadManager.ImportEditorChartToGameStorage(paths[0], out string internalChartPath);

        AddChartButton(internalChartPath);
    }

    public void DeleteChartWithPath(string path)
    {

        int deleteIndex = -1;
        for (int i = 0; i < spawnedChartButtonBehaviors.Count; i++)
        {
            if (spawnedChartButtonBehaviors[i].associatedFullFilePath != path)
            {
                continue;
            }

            deleteIndex = i;
        }

        if (deleteIndex == -1)
        {
            return;
        }
        Debug.Log($"Going to remove {path} at index {deleteIndex}");

        Destroy(spawnedChartButtonBehaviors[deleteIndex].gameObject);
        spawnedChartButtonBehaviors.RemoveAt(deleteIndex);

        File.Delete(path);
    }

    public void InvokeOnChartButtonClickedEvent(ChartButtonBehavior chartButton)
    {
        CurrentSelectedChartButton = chartButton;
        OnChartButtonClicked?.Invoke(CurrentSelectedChartButton);
    }
}
