using System.Collections.Generic;
using UnityEngine;

public class EditorPointVisualManager : MonoBehaviour
{
    private EditorManager editorManager;
    [SerializeField] private EditorPointSelectableUI editorPointPrefab;
    [SerializeField] private Canvas editorPointLineCanvas;

    private Queue<EditorPointSelectableUI> selectableUIPool = new();
    private Dictionary<EditorPoint, EditorPointSelectableUI> activeEditorPointToSelectableUIMapping = new();
    private const int k_MAXIMUMPOOLOBJECTS = 249;
    private void Start()
    {
        editorManager = EditorManager.EditorInstance;

    }

    // I don't want to make the same code again
    // Can I try to use IRenderable for everything...
    private void InstantiateObjectPool()
    {
        for (int i = 0; i < k_MAXIMUMPOOLOBJECTS; i++)
        {
            EditorPointSelectableUI selectableUI = Instantiate(editorPointPrefab, editorPointLineCanvas.transform, false);
            selectableUI.gameObject.SetActive(false);
            selectableUIPool.Enqueue(selectableUI);
        }
    }
}
