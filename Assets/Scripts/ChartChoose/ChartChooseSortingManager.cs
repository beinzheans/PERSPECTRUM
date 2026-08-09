using System;
using UnityEngine;

public class ChartChooseSortingManager : EditorUIBehavior
{
    protected override void InitializeButtons()
    {
        base.InitializeButtons();
        UI_OnButtonPress(0);
    }
    protected override void UI_OnButtonPress(int enumIndex)
    {
        if (enumIndex < 0 || enumIndex > (int)ChartButtonSortOptions.SCORE_BEST)
        {
            return;
        }

        for (int i = 0; i < enumIndices.Length; i++)
        {
            SetButtonState(enumIndices[i], enumIndex == enumIndices[i]);
        }
        
        ChartChooseManager.ChartChooseInstance.InvokeOnChartSortingOptionSelectedEvent((ChartButtonSortOptions)enumIndex);
    }
}

/// <summary>
/// The options provided to sort the Chart buttons in the selection screen. <br></br>
/// This will by default choose the "smallest" one at the top and the "largest" one at the bottom. Reversal of this order should be provided in another control.
/// </summary>
public enum ChartButtonSortOptions
{
    ALPHABETICAL = 0,
    DIFFICULTY = 1,
    DATE_IMPORTED = 2,
    SCORE_BEST = 3,
}

