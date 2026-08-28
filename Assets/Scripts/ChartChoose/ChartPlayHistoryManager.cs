using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A class to manage the chart play history panels. <br></br>
/// A potential optimization is to use object pooling for the button behaviors, or to use async to spawn the buttons.
/// </summary>
public class ChartPlayHistoryManager : MonoBehaviour
{
    [SerializeField] private RectTransform behaviorParentRectTransform;
    [SerializeField] private ChartGameplayRecordButtonBehavior behaviorPrefab;
    [SerializeField] private TMP_Text playHistoryLabelText;
    [SerializeField] private TMP_Text playRecordsLoadingText;
    private ChartChooseManager chartChooseManager;

    private List<ChartGameplayRecordButtonBehavior> currentActiveRecordButtonBehaviors = new();
    private BaseChartMetadata currentBaseMetadata;
    private void Start()
    {
        chartChooseManager = ChartChooseManager.ChartChooseInstance;
        ResetPlayHistoryUI();
        chartChooseManager.OnChartButtonClicked += ChartChooseManager_OnChartButtonClicked;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;

        if (GameManager.GameInstance.IsChartRecordsFinishedLoading)
        {
            playRecordsLoadingText.gameObject.SetActive(false);
            return;
        }
        else
        {
            playRecordsLoadingText.gameObject.SetActive(true);
            GameManager.GameInstance.OnChartRecordsLoadProgressUpdated += GameInstance_OnChartRecordsLoadProgressUpdated;
            GameManager.GameInstance.OnChartRecordsFinishedLoading += GameInstance_OnChartRecordsFinishedLoading;
        }
    }

    private void GameInstance_OnChartRecordsFinishedLoading()
    {
        playRecordsLoadingText.gameObject.SetActive(false);

        ResetPlayHistoryUI();
        UpdateHistoryUIFromBaseMetadata();
    }

    private void GameInstance_OnChartRecordsLoadProgressUpdated(float obj)
    {
        playRecordsLoadingText.text = $"Loading Records...({obj * 100f:F0}%)";
    }

    private void OnDestroy()
    {
        chartChooseManager.OnChartButtonClicked -= ChartChooseManager_OnChartButtonClicked;
        chartChooseManager.OnChartDeleted -= ChartChooseManager_OnChartDeleted;

        GameManager.GameInstance.OnChartRecordsLoadProgressUpdated -= GameInstance_OnChartRecordsLoadProgressUpdated;
        GameManager.GameInstance.OnChartRecordsFinishedLoading -= GameInstance_OnChartRecordsFinishedLoading;

    }
    private void ChartChooseManager_OnChartDeleted(ChartButtonBehaviorContents contents)
    {
        currentBaseMetadata = new();
        ResetPlayHistoryUI();
    }

    private void ChartChooseManager_OnChartButtonClicked(ChartButtonBehaviorContents obj, int id)
    {
        RemoveAllPlayHistoryButton();

        if (obj == null || id == -1)
        {
            currentBaseMetadata = new();
            playHistoryLabelText.text = "History (-)";
            return;
        }

        currentBaseMetadata = obj.BaseChartMetadata;

        if (!GameManager.GameInstance.IsChartRecordsFinishedLoading)
        {
            playHistoryLabelText.text = "History (-)";
            return;
        }

        UpdateHistoryUIFromBaseMetadata();
    }

    private void UpdateHistoryUIFromBaseMetadata()
    {
        if (!GameManager.GameInstance.ChartMetadataGUIDToGameplayRecordMapping.TryGetValue(currentBaseMetadata, out List<GameplayStatisticRecord> records))
        {
            playHistoryLabelText.text = "History (0)";
            return;
        }

        playHistoryLabelText.text = $"History ({records.Count})";

        for (int i = 0; i < records.Count; i++)
        {
            ChartGameplayRecordButtonBehavior behavior = Instantiate(behaviorPrefab, behaviorParentRectTransform);
            behavior.AssignStatisticRecordToBehavior(records[i]);
            currentActiveRecordButtonBehaviors.Add(behavior);
        }
    }

    private void RemoveAllPlayHistoryButton()
    {
        for (int i = 0; i < currentActiveRecordButtonBehaviors.Count; i++)
        {
            ChartGameplayRecordButtonBehavior behavior = currentActiveRecordButtonBehaviors[i];
            Destroy(behavior.gameObject);
        }

        currentActiveRecordButtonBehaviors.Clear();
    }

    private void ResetPlayHistoryUI()
    {
        RemoveAllPlayHistoryButton();
        playHistoryLabelText.text = "History (-)";
    }
}
