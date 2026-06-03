using UnityEngine;

public class EditorCommandManager : MonoBehaviour
{
    private EditorManager editorManager;


    private void Start()
    {
        editorManager = EditorManager.EditorInstance;
    }
}