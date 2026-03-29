using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class EditorRenderer : MonoBehaviour
{
    private EditorManager editorManager;

    private List<IRenderable> editorRenderables;
    private void Start()
    {
        editorManager = EditorManager.EditorInstance;

        editorManager.OnRenderRenderable += EditorManager_OnRenderRenderable;
        editorManager.OnUnrenderRenderable += EditorManager_OnUnrenderRenderable;
    }
    private void EditorManager_OnUnrenderRenderable(IRenderable obj)
    {
        editorRenderables.Remove(obj);
        editorManager.UpdateEditorRenderableList(editorRenderables);
    }

    private void EditorManager_OnRenderRenderable(IRenderable obj)
    {
        editorRenderables.Add(obj);
        editorManager.UpdateEditorRenderableList(editorRenderables);
    }


}
