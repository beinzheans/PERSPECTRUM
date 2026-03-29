using UnityEngine;

public class EditorPointPlaceDeleteManager : MonoBehaviour
{
    private EditorManager editorManager;

    private void Start()
    {
        editorManager = EditorManager.EditorInstance;
    }
}
