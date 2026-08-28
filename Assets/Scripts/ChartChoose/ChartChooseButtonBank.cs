using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class ChartChooseButtonBank : BaseListBank
{
    [SerializeField] private CircularScrollingList circularScrollingList;
    [SerializeField] private List<ChartButtonBehaviorContents> behaviorContents = new();

    [SerializeField] private TMP_Text importedChartsText;

    private ChartChooseManager chartChooseManager;

    private ChartButtonSortOptions sortOptions = ChartButtonSortOptions.ALPHABETICAL;
    private ChartSortOrder sortOrder = ChartSortOrder.ASCENDING;

    private int currentSelectedContentID = -1;
    private void Start()
    {
        chartChooseManager = ChartChooseManager.ChartChooseInstance;

        chartChooseManager.OnChartButtonNeededAdd += ChartChooseManager_OnChartButtonNeededAdd;
        chartChooseManager.OnRequestCurrentSelectedChartButton += GetCurrentFocusedChartButton;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;
        chartChooseManager.OnSortOptionSelected += ChartChooseManager_OnSortOptionSelected;
        chartChooseManager.OnSortOrderChanged += ChartChooseManager_OnSortOrderChanged;

        importedChartsText.text = "0"; // case where no charts present at start

        circularScrollingList.Initialize();
        chartChooseManager.InitializeChartButtonsFromFile();
        SortScrollingList();
    }

    private void SortScrollingList()
    {
        SortChartButtons();
        circularScrollingList.Refresh();
        SelectNewContentID(-1);
    }

    /// <summary>
    /// Selects a new chart based on content ID as the link. <br></br>
    /// Pass in "-1" to reset, indicating none selected.
    /// </summary>
    /// <param name="contentID"></param>
    private void SelectNewContentID(int contentID)
    {
        if (contentID == -1)
        {
            currentSelectedContentID = contentID;
            chartChooseManager.InvokeOnChartButtonClickedEvent(null, contentID);
            return;
        }

        circularScrollingList.SelectContentID(contentID);
        currentSelectedContentID = contentID;
        chartChooseManager.InvokeOnChartButtonClickedEvent((ChartButtonBehaviorContents)GetListContent(contentID), contentID);
    }
    private void ChartChooseManager_OnSortOrderChanged(ChartSortOrder obj)
    {
        if (sortOrder == obj)
        {
            return;
        }

        sortOrder = obj;

        SortScrollingList();
    }

    private void ChartChooseManager_OnSortOptionSelected(ChartButtonSortOptions obj)
    {
        if (sortOptions == obj)
        {
            return;
        }

        sortOptions = obj;

        SortScrollingList();
    }

    private void OnDestroy()
    {
        chartChooseManager.OnChartButtonNeededAdd -= ChartChooseManager_OnChartButtonNeededAdd;
        chartChooseManager.OnRequestCurrentSelectedChartButton -= GetCurrentFocusedChartButton;
        chartChooseManager.OnChartDeleted -= ChartChooseManager_OnChartDeleted;
    }

    private (ChartButtonBehaviorContents, int) GetCurrentFocusedChartButton()
    {
        return ((ChartButtonBehaviorContents)GetListContent(currentSelectedContentID), currentSelectedContentID);
    }

    private void ChartChooseManager_OnChartDeleted(ChartButtonBehaviorContents obj)
    {
        if (!behaviorContents.Remove(obj))
        {
            return;
        }

        circularScrollingList.Refresh();
        importedChartsText.text = GetContentCount().ToString();

        SelectNewContentID(-1);
    }

    private void ChartChooseManager_OnChartButtonNeededAdd(ChartButtonBehaviorContents obj)
    {
        behaviorContents.Add(obj);
        SortChartButtons();
        circularScrollingList.Refresh();

        importedChartsText.text = GetContentCount().ToString();
    }

    public override int GetContentCount()
    {
        return behaviorContents.Count;
    }

    public override IListContent GetListContent(int index)
    {
        return behaviorContents[index];
    }

    public void UI_OnBoxSelected(ListBox box)
    {
        if (box is not ChartButtonBehavior behavior)
        {
            return;
        }

        SelectNewContentID(box.ContentID);
    }

    private void SortChartButtons()
    {
        List<ChartButtonBehaviorContents> temp = new List<ChartButtonBehaviorContents>(behaviorContents); // create a new copy

        switch (sortOptions)
        {
            case ChartButtonSortOptions.ALPHABETICAL:
            default:
                behaviorContents = temp.OrderBy(x => x.BaseChartMetadata.ChartName).ToList();
                break;
            case ChartButtonSortOptions.DIFFICULTY:
                behaviorContents = temp.OrderBy(x => x.BaseChartMetadata.ChartDifficulty).ToList();
                break;
            case ChartButtonSortOptions.DATE_IMPORTED:
                behaviorContents = temp.OrderBy(x =>
                {
                    DateTime dateTime = File.GetCreationTime(x.AssociatedFullFilePath);
                    return dateTime;
                }).ToList();
                break;
            case ChartButtonSortOptions.SCORE_BEST:

                if (!GameManager.GameInstance.IsChartRecordsFinishedLoading)
                {
                    break;
                }

                behaviorContents = temp.OrderBy(x =>
                {
                    bool result = GameManager.GameInstance.ChartMetadataGUIDToGameplayRecordMapping.TryGetValue(x.BaseChartMetadata, out List<GameplayStatisticRecord> gameplayReplays);

                    if (!result)
                    {
                        return float.MinValue;
                    }

                    return gameplayReplays.Max(x => x.FinalScore);
                }).ToList();
                break;
        }

        if (sortOrder == ChartSortOrder.DESCENDING)
        {
            behaviorContents.Reverse();
        }
    }
}

[Serializable]
public class ChartButtonBehaviorContents : IListContent, IEquatable<ChartButtonBehaviorContents>
{
    public ChartButtonBehaviorContents(BaseChartMetadata baseChartMetadata, string associatedFullFilePath)
    {
        BaseChartMetadata = baseChartMetadata;
        AssociatedFullFilePath = associatedFullFilePath;
    }

    public BaseChartMetadata BaseChartMetadata { get; private set; }
    public string AssociatedFullFilePath { get; private set; }

    public override bool Equals(object obj)
    {
        return Equals(obj as ChartButtonBehaviorContents);
    }

    public bool Equals(ChartButtonBehaviorContents other)
    {
        return other is not null &&
               BaseChartMetadata.Equals(other.BaseChartMetadata) &&
               AssociatedFullFilePath == other.AssociatedFullFilePath;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaseChartMetadata, AssociatedFullFilePath);
    }
}