using System;
using System.Collections.Generic;
using UnityEngine;
public class EditorRegion
{
    private List<EditorLine> boundaryLines = new();

    private List<Vector2> regionPoints = new();
    private bool calculateInterior = true;
    public EditorRegion(bool calculateInterior)
    {
        this.calculateInterior = calculateInterior;
    }

    public void AddLine(EditorLine line)
    {
        boundaryLines.Add(line);
    }

    private double GetMinTimeOfBoundaryLines()
    {
        double result = double.MaxValue;
        for (int i = 0; i < boundaryLines.Count; i++)
        {
            if (boundaryLines[i].RenderTime < result)
            {
                result = boundaryLines[i].RenderTime;
            }
        }

        return result;
    }
    public void EvaluateRegion()
    {
        if (boundaryLines.Count <= 2)
        {
            return;
        }

        double minTime = GetMinTimeOfBoundaryLines();
        GenerateAABB(out float minX, out float minY, out float maxX, out float maxY);

        List<EditorHitbox> hitboxToPlace = new();

        for (int i = 0; i < EditorManager.k_SCREENGRIDSIZE * EditorManager.k_SCREENGRIDSIZE; i++)
        {
            bool interiorResult = IsNormalizedPointInsideInterior(EditorManager.EditorInstance.RegionGridSizePositions[i], minX, maxX, minY, maxY);
            EditorHitbox hitbox;
            if (!interiorResult)
            {
                if (calculateInterior)
                {
                    continue;
                }

                hitbox = new EditorHitbox(EditorManager.EditorInstance.RegionGridSizePositions[i], EditorManager.k_SCREENGRIDSIZE_CELL, HitboxType.BOMB, minTime);
            }
            else
            {
                if (!calculateInterior)
                {
                    continue;
                }

                hitbox = new EditorHitbox(EditorManager.EditorInstance.RegionGridSizePositions[i], EditorManager.k_SCREENGRIDSIZE_CELL, HitboxType.BOMB, minTime);
            }

            hitboxToPlace.Add(hitbox);
        }

        Action placeAction = () =>
        {
            for (int i = 0; i < hitboxToPlace.Count; i++)
            {
                hitboxToPlace[i].OnPlace();
            }
        };

        Action deleteAction = () =>
        {
            for (int i = 0; i < hitboxToPlace.Count; i++)
            {
                hitboxToPlace[i].OnDelete();
            }
        };

        EditorCommand placeCommand = new(placeAction, deleteAction);
        EditorManager.EditorInstance.ExecuteEditorCommand(placeCommand);
    }

    public List<EditorPoint> SubdivideLines(int numberOfPointsPerSegment)
    {
        List<EditorPoint> result = new();
        if (boundaryLines.Count <= 0)
        {
            return result;
        }

        if (boundaryLines.Count == 1)
        {
            boundaryLines[0].SubdivideLineSegmentWithEndpoints(numberOfPointsPerSegment, false, ref result);
            return result;
        }

        for (int i = 0; i < boundaryLines.Count; i++)
        {
            if (i >= boundaryLines.Count - 1) // end of line region
            {
                boundaryLines[i].SubdivideLineSegmentWithEndpoints(numberOfPointsPerSegment, false, ref result);
                continue;
            }

            EditorLine current = boundaryLines[i];
            EditorLine next = boundaryLines[i + 1];

            current.SubdivideLineSegmentWithEndpoints(numberOfPointsPerSegment, current.ToNormalizedPosition == next.FromNormalizedPosition, ref result);
        }

        return result;
    }

    private bool IsNormalizedPointInsideInterior(Vector2 normalizedPoint, float minX, float maxX, float minY, float maxY)
    {
        if (normalizedPoint.x < minX || normalizedPoint.x > maxX || normalizedPoint.y < minY || normalizedPoint.y > maxY)
        {
            return false; // outside AABB, not interior
        }

        return GetNumberOfIntersections(normalizedPoint, minX) % 2 != 0;
    }

    private void GenerateAABB(out float minX, out float minY, out float maxX, out float maxY)
    {
        minX = maxX = boundaryLines[0].FromNormalizedPosition.x;
        minY = maxY = boundaryLines[0].FromNormalizedPosition.y; // init value

        for (int i = 1; i < boundaryLines.Count; i++)
        {
            Vector2 p1 = boundaryLines[i].FromNormalizedPosition;
            Vector2 p2 = boundaryLines[i].ToNormalizedPosition;

            Vector2 min = Vector2.Min(p1, p2);
            Vector2 max = Vector2.Max(p1, p2);

            if (min.x < minX)
            {
                minX = min.x;
            }

            if (max.x > maxX)
            {
                maxX = max.x;
            }

            if (min.y < minY)
            {
                minY = min.y;
            }

            if (max.y > maxY)
            {
                maxY = max.y;
            }
        }

        return;
    }

    private int GetNumberOfIntersections(Vector2 normalizedPosition, float minX)
    {
        Vector2 rayPosition = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(normalizedPosition.x, normalizedPosition.y), EditorManager.EditorInstance.PreviewUIContainer);
        Vector2 rayVector = MathHelper.GetPixelFromToVectorFromNormalizedPoints(new Vector2(normalizedPosition.x, normalizedPosition.y), new Vector2(1f, normalizedPosition.y), EditorManager.EditorInstance.PreviewUIContainer);

        int count = 0;
        for (int i = 0; i < boundaryLines.Count; i++)
        {
            Vector2 linePosition = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(boundaryLines[i].FromNormalizedPosition, EditorManager.EditorInstance.PreviewUIContainer);
            Vector2 lineVector = MathHelper.GetPixelFromToVectorFromNormalizedPoints(boundaryLines[i].FromNormalizedPosition, boundaryLines[i].ToNormalizedPosition, EditorManager.EditorInstance.PreviewUIContainer);

            bool result = MathHelper.IsTwoVectorsIntersect(rayPosition, linePosition, rayVector, lineVector, out _, out _); // note in 2D space, lines intersect only once.

            if (!result)
            {
                continue;
            }

            count++;
        }

        return count;
    }
}

