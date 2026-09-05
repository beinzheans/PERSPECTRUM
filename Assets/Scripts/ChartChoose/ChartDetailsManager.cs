using Steamworks;
using System;
using System.IO;
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

    private void ChartChooseInstance_OnChartDeleted(string path)
    {
        HideSelectedUI();
    }

    private readonly Vector2 k_DETAILSCALE = new Vector2(0.9f, 1.1f);
    private const double k_DETAILSCALETIME = 0.1d;
    private void ChartChooseInstance_OnChartButtonClicked(ChartButtonBehaviorContents obj, int id)
    {
        if (obj == null || id == -1)
        {
            HideSelectedUI();
            return;
        }

        PlayChartButton.onClick.RemoveAllListeners();
        DeleteChartButton.onClick.RemoveAllListeners();

        ShowSelectedUI();

        chartTitleText.SetText(obj.BaseChartMetadata.ChartName, k_DETAILSCALE, k_DETAILSCALETIME);
        chartMapperText.SetText($"Charted by {obj.BaseChartMetadata.ChartMapper}", k_DETAILSCALE, k_DETAILSCALETIME);
        songCreditText.SetText($"{obj.BaseChartMetadata.SongName} by {obj.BaseChartMetadata.SongArtist}", k_DETAILSCALE, k_DETAILSCALETIME);
        chartDifficultyText.SetText($"Difficulty {obj.BaseChartMetadata.ChartDifficulty}", k_DETAILSCALE, k_DETAILSCALETIME);
        PlayChartButton.onClick.AddListener(() => ChartChooseManager.ChartChooseInstance.RequestPlayChart());
        DeleteChartButton.onClick.AddListener(() =>
        {
            try
            {
                GamePersistenceManager.GetMetadataJsonOfEditorChartPath(obj.AssociatedFullFilePath, out string metadataJson);
                GamePersistenceManager.GetMetadataOfEditorChartFromJson(metadataJson, out EditorChartMetadata metadata);

                if (metadata.STEAM_PublisherFileID != 0)
                {
                    SteamUGC.UnsubscribeItem(new PublishedFileId_t(metadata.STEAM_PublisherFileID));
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Unsubscibed from Steam Workshop", 1d);
                }
                else
                {
                    ChartChooseManager.ChartChooseInstance.RequestRemoveChart();
                    GameManager.GameInstance.InvokeInformationDisplayNeeded("Deleted from loaded storage", 1d);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Getting metadata from file failed, thus can not determine if the chart is a Workshop item! Exception:\n" +
                                 $"{e.Message}");
            }
        });
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
