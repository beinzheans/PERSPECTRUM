using TMPro;
using UnityEngine;

/// <summary>
/// A class to handle the behavior of a button in the chart choose screen.
/// </summary>
public class ChartButtonBehavior : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    public EditorChartMetadata associatedMetadata;
    public string associatedFullFilePath { get; private set; }

    public void AssignChartButtonValues(EditorChartMetadata chartMetadata, string path)
    {
        associatedMetadata = chartMetadata;
        associatedFullFilePath = path;

        buttonText.text = $"{chartMetadata.ChartName} by {chartMetadata.ChartMapper}";
    }

    public void UI_OnButtonPressed()
    {
        ChartChooseManager.ChartChooseInstance.InvokeOnChartButtonClickedEvent(this);
    }
}
