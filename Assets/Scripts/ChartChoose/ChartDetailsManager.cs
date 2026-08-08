using UnityEngine;
using UnityEngine.UI;

public class ChartDetailsManager : MonoBehaviour
{
    [SerializeField] private GameObject selectedUI;
    [SerializeField] private GameObject noneSelectedUI;

    [SerializeField] private UIElasticText chartTitleText;
    [SerializeField] private UIElasticText chartMapperText;
    [SerializeField] private UIElasticText chartDifficultyText;
    [SerializeField] private UIElasticText songCreditText;
    [SerializeField] private Button PlayChartButton;
    [SerializeField] private Button DeleteChartButton;

    private void Start()
    {
        HideSelectedUI();
        ChartChooseManager.ChartChooseInstance.OnChartButtonClicked += ChartChooseInstance_OnChartButtonClicked;
        ChartChooseManager.ChartChooseInstance.OnChartDeleted += ChartChooseInstance_OnChartDeleted;
    }

    private void ChartChooseInstance_OnChartDeleted(ChartButtonBehaviorContents obj)
    {
        HideSelectedUI();
    }

    private readonly Vector2 k_DETAILSCALE = new Vector2(0.9f, 1.1f);
    private const double k_DETAILSCALETIME = 0.1d;
    private void ChartChooseInstance_OnChartButtonClicked(ChartButtonBehaviorContents obj, int id)
    {
        PlayChartButton.onClick.RemoveAllListeners();
        DeleteChartButton.onClick.RemoveAllListeners();

        ShowSelectedUI();

        chartTitleText.SetText(obj.BaseChartMetadata.ChartName, k_DETAILSCALE, k_DETAILSCALETIME);
        chartMapperText.SetText($"Charted by {obj.BaseChartMetadata.ChartMapper}", k_DETAILSCALE, k_DETAILSCALETIME);
        songCreditText.SetText($"{obj.BaseChartMetadata.SongName} by {obj.BaseChartMetadata.SongArtist}", k_DETAILSCALE, k_DETAILSCALETIME);
        chartDifficultyText.SetText($"Difficulty {obj.BaseChartMetadata.ChartDifficulty}", k_DETAILSCALE, k_DETAILSCALETIME);
        PlayChartButton.onClick.AddListener(() => ChartChooseManager.ChartChooseInstance.RequestPlayChart());
        DeleteChartButton.onClick.AddListener(() => ChartChooseManager.ChartChooseInstance.RequestRemoveChart());
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
