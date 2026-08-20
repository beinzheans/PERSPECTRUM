using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// A class to handle the behavior of the chart buttons in the chart choose screen.
/// </summary>
public class ChartButtonBehavior : ListBox
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private GameObject startTextButton;
    [SerializeField] private Image image;

    public ChartButtonBehaviorContents Contents { get; private set; }

    private const float k_DEFAULTALPHA = 0.2f;

    private int previousContentID = 0;

    private bool isSelected = false;
    private void Start()
    {
        ChartChooseManager.ChartChooseInstance.OnChartButtonClicked += ChartChooseInstance_OnChartButtonClicked;

        previousContentID = ContentID;
    }

    private void ChartChooseInstance_OnChartButtonClicked(ChartButtonBehaviorContents obj, int id)
    {
        if (obj == null || id == -1)
        {
            isSelected = false;
            HideSelectedVisuals();
            return;
        }

        if (ContentID != id)
        {
            isSelected = false;
            HideSelectedVisuals();
            return;
        }

        if (isSelected) // this means we have double clicked on the instance!
        {
            ChartChooseManager.ChartChooseInstance.RequestPlayChart();
            return;
        }

        isSelected = true;
        ShowSelectedVisuals();
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

    private void Update()
    {
        if (ContentID == previousContentID)
        {
            return;
        }

        previousContentID = ContentID;

        if (ContentID != ChartChooseManager.ChartChooseInstance.CurrentSelectChartContentID)
        {
            isSelected = false;
            HideSelectedVisuals();
            return;
        }

        isSelected = true;
        ShowSelectedVisuals();
    }

    private void ShowSelectedVisuals()
    {
        image.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, k_DEFAULTALPHA);
        startTextButton.SetActive(true);
    }

    private void HideSelectedVisuals()
    {
        image.color = new Color(Color.white.r, Color.white.g, Color.white.b, k_DEFAULTALPHA);
        startTextButton.SetActive(false);
    }
}