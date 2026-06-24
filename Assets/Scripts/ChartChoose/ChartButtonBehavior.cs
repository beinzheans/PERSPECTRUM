using TMPro;
using UnityEngine;

/// <summary>
/// A class to handle the behavior of the chart buttons in the chart choose screen.
/// </summary>
public class ChartButtonBehavior : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    public EditorChartMetadata associatedMetadata;
    public string AssociatedFullFilePath { get; private set; }

    public void AssignChartButtonValues(EditorChartMetadata chartMetadata, string path)
    {
        associatedMetadata = chartMetadata;
        AssociatedFullFilePath = path;

        buttonText.text = $"{chartMetadata.ChartName} by {chartMetadata.ChartMapper}";
    }

    public void UI_OnButtonPressed()
    {
        ChartChooseManager.ChartChooseInstance.InvokeOnChartButtonClickedEvent(this);
    }
}
