using UnityEngine;

public class EditorGridManager : EditorUIBehavior
{
    private MouseSnapState currentState = MouseSnapState.NO_SNAP;

    protected override void Start()
    {
        base.Start();

        UI_OnButtonPress(0);
    }
    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)MouseSnapState.NO_SNAP || index > (int)MouseSnapState.SNAP_TO_GRID)
        {
            return;
        }


        buttons[index].image.color = Color.yellow;
        buttons[(int)currentState].image.color = Color.white;

        currentState = (MouseSnapState)index;

        switch (index)
        {
            case (int)MouseSnapState.NO_SNAP:
                SetButtonState(0, true);
                EditorManager.EditorInstance.InvokeEditorSnapMouseToGrid(false);
                break;
            case (int)MouseSnapState.SNAP_TO_GRID:
                EditorManager.EditorInstance.InvokeEditorSnapMouseToGrid(true);
                break;
        }


    }
}

public enum MouseSnapState
{
    NO_SNAP = 0,
    SNAP_TO_GRID = 1
}
