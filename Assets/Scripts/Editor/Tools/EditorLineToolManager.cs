using System;
using System.Collections.Generic;

public class EditorLineToolManager : EditorToolManager
{
    private const int k_CALCULATEEXTERIOR = 0;
    private const int k_GENERATEREGION = 1;
    private const int k_SUBDIVIDELINE = 2;

    protected override void CheckForToolActiveState(ObjectPlaceDeleteType obj, out bool validResult)
    {
        validResult = obj == ObjectPlaceDeleteType.Line;
    }

    protected override void OnButtonPressedEvent(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case k_CALCULATEEXTERIOR:
                GenerateRegion(false);
                break;
            case k_GENERATEREGION:
                GenerateRegion(true);
                break;
            case k_SUBDIVIDELINE:
                SubdivideLine();
                break;
        }
    }

    protected override void OnPositiveNegativeInput(float input)
    {
        return;
    }

    private void GenerateRegion(bool interior)
    {
        AddLinesToRegion(out EditorRegion region, interior);
        region.EvaluateRegion();
    }

    private void SubdivideLine()
    {
        bool result = int.TryParse(editorInstance.InputFields[k_SUBDIVIDELINE].text, out int numberOfBeats);

        if (!result)
        {
            return;
        }

        AddLinesToRegion(out EditorRegion region, true);

        List<EditorPoint> subdivideResult = region.SubdivideLines(numberOfBeats);

        Action placeAction = () =>
        {
            for (int i = 0; i < subdivideResult.Count; i++)
            {
                subdivideResult[i].OnPlace();
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < subdivideResult.Count; i++)
            {
                subdivideResult[i].OnDelete();
            }
        };

        EditorCommand subdivideCommand = new EditorCommand(placeAction, undoAction);

        editorInstance.ExecuteEditorCommand(subdivideCommand);
    }

    private void AddLinesToRegion(out EditorRegion region, bool interior)
    {
        List<EditorDynamicObject> allSelected = editorInstance.CurrentSelectedRenderables;

        region = new(interior);
        for (int i = 0; i < allSelected.Count; i++)
        {
            if (allSelected[i] is not EditorLine line)
            {
                continue;
            }

            region.AddLine(line);
        }
    }
}
