using UnityEngine;
using UnityEngine.UI;

public class EditorGridManager : EditorUIBehavior
{
    private MouseSnapState currentState = MouseSnapState.NO_SNAP;
    [SerializeField] private Image gridLinePrefab;
    [SerializeField] private Transform gridLineParent;
    protected override void Start()
    {
        base.Start();
        SpawnGridLines();
        UI_OnButtonPress(0);
    }

    private const float k_GRIDLINEWIDTH = 3f;
    private void SpawnGridLines()
    {
        for (int x = 1; x < EditorManager.k_SCREENGRIDSIZE; x++)
        {
            float xPosition = x * EditorManager.k_SCREENGRIDSIZE_CELL;
            Image verticalGridLine = Instantiate(gridLinePrefab, gridLineParent);

            verticalGridLine.rectTransform.anchorMin = new Vector2(xPosition, 0f);
            verticalGridLine.rectTransform.anchorMax = new Vector2(xPosition, 1f);
            verticalGridLine.rectTransform.anchoredPosition = Vector2.zero;
            verticalGridLine.rectTransform.sizeDelta = new Vector2(k_GRIDLINEWIDTH, 0f);

            if (MathHelper.IsTwoFloatsEqualWithEpsilion(xPosition, 0.5f))
            {
                verticalGridLine.color = Color.yellow;
            }
        }

        for (int y = 1; y < EditorManager.k_SCREENGRIDSIZE; y++)
        {
            float yPosition = y * EditorManager.k_SCREENGRIDSIZE_CELL;

            Image horizontalGridLine = Instantiate(gridLinePrefab, gridLineParent);

            horizontalGridLine.rectTransform.anchorMin = new Vector2(0f, yPosition);
            horizontalGridLine.rectTransform.anchorMax = new Vector2(1f, yPosition);
            horizontalGridLine.rectTransform.anchoredPosition = Vector2.zero;
            horizontalGridLine.rectTransform.sizeDelta = new Vector2(0f, k_GRIDLINEWIDTH);

            if (MathHelper.IsTwoFloatsEqualWithEpsilion(yPosition, 0.5f))
            {
                horizontalGridLine.color = Color.yellow;
            }
        }

        gridLineParent.gameObject.SetActive(false);
    }
    protected override void UI_OnButtonPress(int index)
    {
        if (index < (int)MouseSnapState.NO_SNAP || index > (int)MouseSnapState.SNAP_TO_GRID)
        {
            return;
        }

        currentState = (MouseSnapState)index;

        switch (index)
        {
            case (int)MouseSnapState.NO_SNAP:
                SetButtonState(0, true);
                SetButtonState(1, false);
                gridLineParent.gameObject.SetActive(false);
                EditorManager.EditorInstance.InvokeEditorSnapMouseToGrid(false);
                break;
            case (int)MouseSnapState.SNAP_TO_GRID:
                SetButtonState(0, false);
                SetButtonState(1, true);
                gridLineParent.gameObject.SetActive(true);
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
