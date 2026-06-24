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
    private ChartChooseManager chartChooseManager;

    private List<ChartGameplayRecordButtonBehavior> currentActiveRecordButtonBehaviors = new();
    private void Start()
    {
        chartChooseManager = ChartChooseManager.ChartChooseInstance;
        ResetPlayHistoryUI();
        chartChooseManager.OnChartButtonClicked += ChartChooseManager_OnChartButtonClicked;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;
    }

    private void ChartChooseManager_OnChartDeleted()
    {
        ResetPlayHistoryUI();
    }

    private void ChartChooseManager_OnChartButtonClicked(ChartButtonBehavior obj)
    {
        RemoveAllPlayHistoryButton();

        EditorChartMetadata metadata = obj.associatedMetadata;

        if (!GameManager.GameInstance.ChartMetadataToGameplayRecordMapping.TryGetValue(metadata, out List<GameplayStatisticRecord> records))
        {
            playHistoryLabelText.text = "Play History (0)";
            return;
        }

        playHistoryLabelText.text = $"Play History ({records.Count})";

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
        playHistoryLabelText.text = "Play History (0)";
    }
}
