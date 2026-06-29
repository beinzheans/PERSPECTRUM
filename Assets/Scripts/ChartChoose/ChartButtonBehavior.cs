using TMPro;
using UnityEngine;

/// <summary>
/// A class to handle the behavior of the chart buttons in the chart choose screen.
/// </summary>
public class ChartButtonBehavior : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text difficultyText;

    public BaseChartMetadata BaseChartMetadata { get; private set; }

    public string AssociatedFullFilePath { get; private set; }
    public void AssignChartButtonValues(BaseChartMetadata baseChartMetadata, string path)
    {
        BaseChartMetadata = baseChartMetadata;
        AssociatedFullFilePath = path;

        buttonText.text = $"{BaseChartMetadata.ChartName} by {BaseChartMetadata.ChartMapper}";
        difficultyText.text = $"Difficulty {BaseChartMetadata.ChartDifficulty}";
    }

    public void UI_OnButtonPressed()
    {
        ChartChooseManager.ChartChooseInstance.InvokeOnChartButtonClickedEvent(this);
    }
}
