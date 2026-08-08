using AirFishLab.ScrollingList;
using AirFishLab.ScrollingList.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ChartChooseButtonBank : BaseListBank
{
    [SerializeField] private CircularScrollingList circularScrollingList;
    [SerializeField] private List<ChartButtonBehaviorContents> behaviorContents = new();
    private ChartChooseManager chartChooseManager;

    private void Start()
    {
        chartChooseManager = ChartChooseManager.ChartChooseInstance;

        chartChooseManager.OnChartButtonNeededAdd += ChartChooseManager_OnChartButtonNeededAdd;
        chartChooseManager.OnRequestCurrentSelectedChartButton += GetCurrentFocusedChartButton;
        chartChooseManager.OnChartDeleted += ChartChooseManager_OnChartDeleted;

        circularScrollingList.Initialize();
        chartChooseManager.InitializeChartButtonsFromFile();
    }

    private void OnDestroy()
    {
        chartChooseManager.OnChartButtonNeededAdd -= ChartChooseManager_OnChartButtonNeededAdd;
        chartChooseManager.OnRequestCurrentSelectedChartButton -= GetCurrentFocusedChartButton;
        chartChooseManager.OnChartDeleted -= ChartChooseManager_OnChartDeleted;
    }

    private ChartButtonBehaviorContents GetCurrentFocusedChartButton()
    {
        int focusedID = circularScrollingList.GetFocusingContentID();

        if (focusedID < 0 || focusedID >= behaviorContents.Count)
        {
            return null;
        }

        return behaviorContents[focusedID];
    }

    private void ChartChooseManager_OnChartDeleted(ChartButtonBehaviorContents obj)
    {
        if (!behaviorContents.Remove(obj))
        {
            return;
        }

        circularScrollingList.Refresh();

        ChartButtonBehaviorContents contents = GetCurrentFocusedChartButton();
        if (contents == null)
        {
            return;
        }

        chartChooseManager.InvokeOnChartButtonClickedEvent(contents);
    }

    private void ChartChooseManager_OnChartButtonNeededAdd(ChartButtonBehaviorContents obj)
    {
        behaviorContents.Add(obj);
        circularScrollingList.Refresh();

        ChartButtonBehaviorContents contents = GetCurrentFocusedChartButton();
        if (contents == null)
        {
            return;
        }

        chartChooseManager.InvokeOnChartButtonClickedEvent(contents);
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

        chartChooseManager.InvokeOnChartButtonClickedEvent(behavior.Contents);
    }

    
    
}

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
