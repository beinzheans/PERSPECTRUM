using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChartDetailsManager : MonoBehaviour
{
    [SerializeField] private GameObject selectedUI;
    [SerializeField] private GameObject noneSelectedUI;

    [SerializeField] private TMP_Text chartTitleText;
    [SerializeField] private TMP_Text chartMapperText;
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

        chartTitleText.text = obj.associatedMetadata.ChartName;
        chartMapperText.text = obj.associatedMetadata.ChartMapper;
        songCreditText.text = $"{obj.associatedMetadata.SongName} by {obj.associatedMetadata.SongArtist}";

        PlayChartButton.onClick.AddListener(() => GameManager.GameInstance.RequestPlayChartEvent(obj.associatedFullFilePath));
        DeleteChartButton.onClick.AddListener(() => {
            ChartChooseManager.ChartChooseInstance.DeleteChartWithPath(obj.associatedFullFilePath);
            HideSelectedUI();
        });

        ShowSelectedUI();
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
