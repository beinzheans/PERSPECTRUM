using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChartChooseButtonBank : BaseListBank
{
    [SerializeField] private CircularScrollingList circularScrollingList;
    [SerializeField] private List<ChartButtonBehaviorContents> behaviorContents = new();

    [SerializeField] private TMP_Text importedChartsText;

    private ChartChooseManager chartChooseManager;

    private void Start()
    {
        chartChooseManager = ChartChooseManager.ChartChooseInstance;

        chartChooseManager.OnChartButtonNeededAdd += ChartChooseManager_OnChartButtonNeededAdd;
        chartChooseManager.OnRequestCurrentSelectedChartButton += GetCurrentFocusedChartButton;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;

        importedChartsText.text = $"Imported Charts (0)"; // case where no charts present at start

        circularScrollingList.Initialize();
        chartChooseManager.InitializeChartButtonsFromFile();
    }

    private void OnDestroy()
    {
        chartChooseManager.OnChartButtonNeededAdd -= ChartChooseManager_OnChartButtonNeededAdd;
        chartChooseManager.OnRequestCurrentSelectedChartButton -= GetCurrentFocusedChartButton;
        chartChooseManager.OnChartDeleted -= ChartChooseManager_OnChartDeleted;
    }

    private (ChartButtonBehaviorContents, int) GetCurrentFocusedChartButton()
    {
        int focusedID = circularScrollingList.GetFocusingContentID();

        if (focusedID < 0 || focusedID >= behaviorContents.Count)
        {
            return (null, -1);
        }

        return (behaviorContents[focusedID], focusedID);
    }

    private void ChartChooseManager_OnChartDeleted(ChartButtonBehaviorContents obj)
    {
        if (!behaviorContents.Remove(obj))
        {
            return;
        }

        circularScrollingList.Refresh();
        importedChartsText.text = $"Imported Charts ({GetContentCount()})";

        (ChartButtonBehaviorContents contents, int id) = GetCurrentFocusedChartButton();
        if (contents == null)
        {
            return;
        }

        chartChooseManager.InvokeOnChartButtonClickedEvent(contents, id);
    }

    private void ChartChooseManager_OnChartButtonNeededAdd(ChartButtonBehaviorContents obj)
    {
        behaviorContents.Add(obj);
        circularScrollingList.Refresh();

        importedChartsText.text = $"Imported Charts ({GetContentCount()})";
        (ChartButtonBehaviorContents contents, int id) = GetCurrentFocusedChartButton();
        if (contents == null)
        {
            return;
        }

        chartChooseManager.InvokeOnChartButtonClickedEvent(contents, id);
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
        
        chartChooseManager.InvokeOnChartButtonClickedEvent(behavior.Contents, box.ContentID);
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
