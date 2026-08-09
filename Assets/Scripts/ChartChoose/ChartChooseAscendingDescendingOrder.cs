using UnityEngine;

public class ChartChooseAscendingDescendingOrder : EditorUIBehavior
{
    protected override void InitializeButtons()
    {
        base.InitializeButtons();
        UI_OnButtonPress(0);
    }

    protected override void UI_OnButtonPress(int enumIndex)
    {
        if (enumIndex < 0 || enumIndex > (int)ChartSortOrder.DESCENDING)
        {
            return;
        }

        for (int i = 0; i < enumIndices.Length; i++)
        {
            SetButtonState(enumIndices[i], enumIndex == enumIndices[i]);
        }

        ChartChooseManager.ChartChooseInstance.InvokeOnChartOrderingOptionEvent((ChartSortOrder)enumIndex);


    }
}

public enum ChartSortOrder
{
    ASCENDING = 0,
    DESCENDING = 1
}