using TMPro;
using UnityEngine;
using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using UnityEngine.UI;
/// <summary>
/// A class to handle the behavior of the chart buttons in the chart choose screen.
/// </summary>
public class ChartButtonBehavior : ListBox
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private Image image;
    public ChartButtonBehaviorContents Contents { get; private set; }

    private void Start()
    {
        ChartChooseManager.ChartChooseInstance.OnChartButtonClicked += ChartChooseInstance_OnChartButtonClicked;
    }

    private const float k_DEFAULTALPHA = 0.2f;
    private void ChartChooseInstance_OnChartButtonClicked(ChartButtonBehaviorContents obj)
    {
        if (obj != Contents)
        {
            image.color = new Color(Color.white.r, Color.white.g, Color.white.b, k_DEFAULTALPHA);
            return;
        }

        image.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, k_DEFAULTALPHA);
    }

    protected override void UpdateDisplayContent(IListContent content)
    {
        if (content is not ChartButtonBehaviorContents chartContents)
        {
            return;
        }

        Contents = chartContents;

        buttonText.text = $"{Contents.BaseChartMetadata.ChartName} by {Contents.BaseChartMetadata.ChartMapper}";
        difficultyText.text = $"Difficulty {Contents.BaseChartMetadata.ChartDifficulty}";
    }
}