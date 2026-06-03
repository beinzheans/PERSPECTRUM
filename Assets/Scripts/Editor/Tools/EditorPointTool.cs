using System;
using System.Collections.Generic;
using UnityEngine;

// Please add undo for point tool!!!!!! I forgot to add it ??!?!?!?!
public class EditorPointTool : EditorToolManager
{
    private const int k_RADIUS = 0;
    private const int k_INITIALANGLE = 1;
    private const int k_FINALANGLE = 2;
    private const int k_NUMBEROFPOINTS = 3;
    private const int k_TIMEOFFSET = 4;
    private const int k_COMPUTEPOINTS = 5;
    private const int k_CONVERTPOINTSTOHITBOX = 6;
    private float radius = 0f;
    private float initialAngle = 0f;
    private float finalAngle = 0f;
    private int numberOfPoints = 0;
    private double timeOffset = 0d;
    protected override void OnButtonPressedEvent(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case k_COMPUTEPOINTS:
                ComputeGuidePoints();
                break;
            case k_CONVERTPOINTSTOHITBOX:
                ConvertPointsToHitbox();
                break;
            default:
                break;
        }
    }

    protected override void OnPositiveNegativeInput(float input)
    {
        return;
    }

    private void ConvertPointsToHitbox()
    {
        List<EditorDynamicObject> objects = new List<EditorDynamicObject>(editorInstance.CurrentSelectedRenderables);

        int count = 0;
        List<EditorPoint> points = new();
        List<EditorHitbox> hitboxes = new();

        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is not EditorPoint point)
            {
                continue;
            }

            count++;
            EditorHitbox hitbox = new EditorHitbox(point.NormalizedPosition, editorInstance.EditorPlaceDeleteSize, HitboxType.A, point.RenderTime);
            points.Add(point);
            hitboxes.Add(hitbox);
        }

        Action convertAction = () =>
        {
            for (int i = 0; i < count; i++)
            {
                hitboxes[i].OnPlace();
                points[i].OnDelete();
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < count; i++)
            {
                points[i].OnPlace();
                hitboxes[i].OnDelete();
            }
        };

        EditorCommand command = new(convertAction, undoAction);
        editorInstance.ExecuteEditorCommand(command);
    }

    private void ParseInputFields()
    {
        bool parseResult;
        for (int i = 0; i < toolLabels.Length; i++)
        {
            switch (i)
            {
                case k_RADIUS:
                    parseResult = float.TryParse(editorInstance.InputFields[k_RADIUS].text, out float radiusResult);
                    if (!parseResult)
                    {
                        continue;
                    }

                    radius = radiusResult;
                    break;
                case k_INITIALANGLE:
                    parseResult = float.TryParse(editorInstance.InputFields[k_INITIALANGLE].text, out float initialAngleResult);
                    if (!parseResult)
                    {
                        continue;
                    }

                    initialAngle = initialAngleResult;
                    break;
                case k_FINALANGLE:
                    parseResult = float.TryParse(editorInstance.InputFields[k_FINALANGLE].text, out float finalAngleResult);
                    if (!parseResult)
                    {
                        continue;
                    }

                    finalAngle = finalAngleResult;
                    break;
                case k_NUMBEROFPOINTS:
                    parseResult = int.TryParse(editorInstance.InputFields[k_NUMBEROFPOINTS].text, out int numberOfPointsResult);
                    if (!parseResult)
                    {
                        continue;
                    }

                    numberOfPoints = numberOfPointsResult;
                    break;
                case k_TIMEOFFSET:
                    parseResult = double.TryParse(editorInstance.InputFields[k_TIMEOFFSET].text, out double timeOffsetResult);
                    if (!parseResult)
                    {
                        continue;
                    }

                    timeOffset = timeOffsetResult;
                    break;
                default:
                    continue;
            }
        }
    }

    private void ComputeGuidePoints()
    {
        ParseInputFields();

        if (numberOfPoints < 1)
        {
            return;
        }

        List<EditorDynamicObject> objects = new List<EditorDynamicObject>(editorInstance.CurrentSelectedRenderables);
        List<EditorPoint> guidePoints = new List<EditorPoint>();
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is not EditorPoint point)
            {
                continue;
            }

            CreateGuidePoints(point, ref guidePoints);
        }

        Action executeAction = () =>
        {
            for (int i = 0; i < guidePoints.Count; i++)
            {
                guidePoints[i].OnPlace();
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < guidePoints.Count; i++)
            {
                guidePoints[i].OnDelete();
            }
        };

        EditorCommand command = new EditorCommand(executeAction, undoAction);
        editorInstance.ExecuteEditorCommand(command);
    }

    private void CreateGuidePoints(EditorPoint point, ref List<EditorPoint> appendList)
    {
        float angle_i = initialAngle * Mathf.Deg2Rad;
        float angle_f = finalAngle * Mathf.Deg2Rad;

        if (numberOfPoints == 1)
        {
            Vector2 displacement = radius * new Vector2(Mathf.Cos(angle_i), Mathf.Sin(angle_i)) * GameManager.aspectRatioConversionScale;
            appendList.Add(new EditorPoint(point.NormalizedPosition + displacement, point.RenderTime));
            return;
        }

        if ((finalAngle - initialAngle) % 360f == 0f) // exactly integer number of revolutions, calculate start and end caps differently.
        {
            float dAngle = (angle_f - angle_i) / numberOfPoints;

            for (int i = 1; i < numberOfPoints + 1; i++)
            {
                float angle = angle_i + dAngle * i;

                Vector2 displacement = radius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * GameManager.aspectRatioConversionScale;
                appendList.Add(new EditorPoint(point.NormalizedPosition + displacement, point.RenderTime + timeOffset * (i - 1)));
            }
        }
        else
        {
            float dAngle = (angle_f - angle_i) / (numberOfPoints - 1);

            for (int i = 0; i < numberOfPoints; i++)
            {
                float angle = angle_i + dAngle * i;

                Vector2 displacement = radius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * GameManager.aspectRatioConversionScale;
                appendList.Add(new EditorPoint(point.NormalizedPosition + displacement, point.RenderTime + timeOffset * i));
            }
        }
    }

    protected override void CheckForToolActiveState(ObjectPlaceDeleteType obj, out bool validResult)
    {
        validResult = obj == ObjectPlaceDeleteType.Point;
    }
}
