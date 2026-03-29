using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class EditorHitboxSelectManager : MonoBehaviour
{
    private EditorManager editorManager;

    private List<EditorHitbox> selectedHitboxes;

    private void Start()
    {
        editorManager = EditorManager.EditorInstance;

        editorManager.OnEditorHitboxSelected += EditorManager_OnEditorHitboxSelected;
    }

    private void EditorManager_OnEditorHitboxSelected(EditorHitbox obj)
    {
        //
    }
}
