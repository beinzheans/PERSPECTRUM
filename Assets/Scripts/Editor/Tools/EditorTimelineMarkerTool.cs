using System;
using UnityEngine;

// this also has issues with undo, even though I thought this was impossible to undo anyways
// seems like it's because of place delete manager
public class EditorTimelineMarkerTool : EditorToolManager
{
    private const int k_MARKERNAMEINDEX = 0;
    private const int k_MARKERBPMINDEX = 1;
    private const int k_DELETECURRENTMARKERINDEX = 2;

    protected override void Start()
    {
        base.Start();

        editorInstance.OnEditorPlaceEditorObject += EditorInstance_OnEditorPlaceEditorObject;
        editorInstance.OnEditorDeleteEditorObject += EditorInstance_OnEditorDeleteEditorObject;
    }

    private void EditorInstance_OnEditorDeleteEditorObject(EditorDynamicObject obj)
    {
        if (obj is not TimelineMarker marker)
        {
            return;
        }

        editorInstance.CurrentEditorChart.OnDelete(marker);
    }

    private void EditorInstance_OnEditorPlaceEditorObject(EditorDynamicObject obj)
    {
        if (obj is not TimelineMarker marker)
        {
            return;
        }


        string inputFieldLabel = editorInstance.InputFields[k_MARKERNAMEINDEX].text;
        bool bpmParseResult = double.TryParse(editorInstance.InputFields[k_MARKERBPMINDEX].text, out double bpm);
        if (!bpmParseResult)
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Invalid BPM");
            Debug.LogWarning($"Can not parse BPM");
            return;
        }

        marker.AssignMarkerValues(string.IsNullOrWhiteSpace(inputFieldLabel) ? "Unnamed Section" : inputFieldLabel, bpm);

        Action placeAction = () =>
        {
            editorInstance.CurrentEditorChart.OnPlace(marker);
            editorInstance.InvokeEditorPreviewUpdateEvent();
        };

        Action undoAction = () =>
        {
            editorInstance.CurrentEditorChart.OnDelete(marker);
            editorInstance.InvokeEditorPreviewUpdateEvent();
        };

        EditorCommand command = new(placeAction, undoAction);

        editorInstance.ExecuteEditorCommand(command);
    }

    protected override void CheckForToolActiveState(ObjectPlaceDeleteType obj, out bool validResult)
    {
        validResult = obj == ObjectPlaceDeleteType.TimelineMarker;
    }

    protected override void OnButtonPressedEvent(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case k_DELETECURRENTMARKERINDEX:

                TimelineMarker marker = editorInstance.CurrentTimelineMarker;

                Action deleteAction = () =>
                {
                    editorInstance.CurrentEditorChart.OnDelete(marker);
                };

                Action undoAction = () =>
                {
                    editorInstance.CurrentEditorChart.OnPlace(marker);
                };

                EditorCommand command = new(deleteAction, undoAction);
                GameManager.GameInstance.InvokeInformationDisplayNeeded("Deleted Marker");
                editorInstance.ExecuteEditorCommand(command);
                editorInstance.InvokeEditorPreviewUpdateEvent();
                break;
        }
    }

    protected override void OnPositiveNegativeInput(float input)
    {
    }

}
