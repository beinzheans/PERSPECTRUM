using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class EditorPointTool : EditorToolManager
{
    private const int k_RADIUS = 0;
    private const int k_INITIALANGLE = 1;
    private const int k_FINALANGLE = 2;
    private const int k_NUMBEROFPOINTS = 3;
    private const int k_TIMEOFFSET = 4;
    private const int k_COMPUTECIRCLEPOINTS = 5;
    private const int k_COMPUTEARCPOINTS = 6;
    private const int k_CONVERTPOINTSTOHITBOX = 7;
    private float radius = 0f;
    private float initialAngle = 0f;
    private float finalAngle = 0f;
    private int numberOfPoints = 0;
    private double timeOffset = 0d;
    protected override void OnButtonPressedEvent(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case k_COMPUTECIRCLEPOINTS:
                ComputeCirclePoints();
                break;
            case k_COMPUTEARCPOINTS:
                ComputeArcPoints();
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

    private void ComputeCirclePoints()
    {
        ParseInputFields();

        if (numberOfPoints < 1)
        {
            return;
        }

        List<EditorDynamicObject> objects = new List<EditorDynamicObject>(editorInstance.CurrentSelectedRenderables);
        List<EditorPoint> appendPoints = new List<EditorPoint>();
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is not EditorPoint point)
            {
                continue;
            }

            CreateCirclePoints(point, ref appendPoints);
        }

        Action executeAction = () =>
        {
            for (int i = 0; i < appendPoints.Count; i++)
            {
                appendPoints[i].OnPlace();
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < appendPoints.Count; i++)
            {
                appendPoints[i].OnDelete();
            }
        };

        EditorCommand command = new EditorCommand(executeAction, undoAction);
        editorInstance.ExecuteEditorCommand(command);
    }

    private void ComputeArcPoints()
    {
        ParseInputFields();

        if (numberOfPoints < 2)
        {
            return;
        }

        List<EditorDynamicObject> objects = new List<EditorDynamicObject>(editorInstance.CurrentSelectedRenderables);
        List<EditorPoint> appendPoints = new List<EditorPoint>();
        // keep track of the order we select lines

        Queue<EditorLine> lineQueue = new();
        Queue<EditorPoint> pointQueue = new();
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is EditorLine line)
            {
                lineQueue.Enqueue(line);
                continue;
            }

            if (objects[i] is EditorPoint point)
            {
                pointQueue.Enqueue(point);
                continue;
            }
        }

        while (lineQueue.Count > 0 && pointQueue.Count > 0)
        {
            EditorLine constructLine = lineQueue.Dequeue();
            EditorPoint constructPoint = pointQueue.Dequeue();

            Spline spline = MathHelper.GenerateSplineFromConstructionLineAndVertexPoint(constructLine, constructPoint);
            CreateArcPoints(spline, constructLine, ref appendPoints);

        }

        Action executeAction = () =>
        {
            for (int i = 0; i < appendPoints.Count; i++)
            {
                appendPoints[i].OnPlace();
            }
        };

        Action undoAction = () =>
        {
            for (int i = 0; i < appendPoints.Count; i++)
            {
                appendPoints[i].OnDelete();
            }
        };

        EditorCommand command = new EditorCommand(executeAction, undoAction);
        editorInstance.ExecuteEditorCommand(command);
    }

    private void CreateArcPoints(Spline spline, EditorLine line, ref List<EditorPoint> appendList)
    {
        if (numberOfPoints < 2)
        {
            return;
        }

        float dt = 1f / (numberOfPoints - 1);

        for (int i = 1; i < numberOfPoints - 1; i++)
        {
            float t = dt * (float)i;

            float3 position = spline.EvaluatePosition(t);
            double time = line.EvaluateTimeAtProgress(t);
            EditorPoint point = new EditorPoint(new Vector2(position.x, position.y), time);

            appendList.Add(point);
        }
    }

    private void CreateCirclePoints(EditorPoint point, ref List<EditorPoint> appendList)
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
