using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChartDetailsManager : MonoBehaviour
{
    [SerializeField] private GameObject selectedUI;
    [SerializeField] private GameObject noneSelectedUI;

    [SerializeField] private TMP_Text chartTitleText;
    [SerializeField] private TMP_Text chartMapperText;
    [SerializeField] private TMP_Text chartDifficultyText;
    [SerializeField] private TMP_Text songCreditText;
    [SerializeField] private Button PlayChartButton;
    [SerializeField] private Button DeleteChartButton;

    private void Start()
    {
        HideSelectedUI();
        ChartChooseManager.ChartChooseInstance.OnChartButtonClicked += ChartChooseInstance_OnChartButtonClicked;
    }

    private void ChartChooseInstance_OnChartButtonClicked(ChartButtonBehavior obj)
    {
        PlayChartButton.onClick.RemoveAllListeners();
        DeleteChartButton.onClick.RemoveAllListeners();

        chartTitleText.text = obj.BaseChartMetadata.ChartName;
        chartMapperText.text = $"Charted by {obj.BaseChartMetadata.ChartMapper}";
        songCreditText.text = $"{obj.BaseChartMetadata.SongName} by {obj.BaseChartMetadata.SongArtist}";
        chartDifficultyText.text = $"Difficulty {obj.BaseChartMetadata.ChartDifficulty}";
        PlayChartButton.onClick.AddListener(() => GameManager.GameInstance.RequestPlayChartEvent(obj.AssociatedFullFilePath));
        DeleteChartButton.onClick.AddListener(() =>
        {
            ChartChooseManager.ChartChooseInstance.DeleteChartWithPath(obj.AssociatedFullFilePath);
            HideSelectedUI();
        });

        ShowSelectedUI();
    }

    private void OnDestroy()
    {
        PlayChartButton.onClick.RemoveAllListeners();
        DeleteChartButton.onClick.RemoveAllListeners();
    }

    private void ShowSelectedUI()
    {
        noneSelectedUI.SetActive(false);
        selectedUI.SetActive(true);
    }

    private void HideSelectedUI()
    {
        selectedUI.SetActive(false);
        noneSelectedUI.SetActive(true);
    }
}
