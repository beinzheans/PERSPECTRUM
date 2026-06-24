using System;
using UnityEngine;

/// <summary>
/// Manages the hit border logic during gameplay.
/// </summary>
public class GameplayPlayareaBorderManager : MonoBehaviour
{
    private readonly int k_SHADERPULSEPROGRESSID = Shader.PropertyToID("_PulseProgress");
    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer playareaBorderMeshRenderer_front;
    private MeshRenderer playareaBorderMeshRenderer_back;

    [SerializeField] private MeshFilter playareaBorderMeshFilter_Front;
    [SerializeField] private MeshFilter playareaBorderMeshFilter_Back;

    private GameplayManager gameplayManager;
    private const float k_BORDERINSETTHICKNESS = 0.025f;

    private double previousPulseTime;
    private double pulseInterval;

    [SerializeField] private GameObject[] perspectiveLineGameObjects = new GameObject[4]; // 0 is bottom-left corner, increment clockwise.
    private Vector3[] localBorderCorners = new Vector3[4]; // 0 is bottom-left corner, increment clockwise

    private const float k_BACKSCALEFROMFARPLANE = 0.9f; // how much we scale the far clip plane to place the back mesh filter
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        propertyBlock = new MaterialPropertyBlock();

        playareaBorderMeshRenderer_front = playareaBorderMeshFilter_Front.GetComponent<MeshRenderer>();
        playareaBorderMeshRenderer_back = playareaBorderMeshFilter_Back.GetComponent<MeshRenderer>();

        playareaBorderMeshFilter_Front.transform.localPosition = new Vector3(0f, 0f, GameplayManager.k_HITPLANEDEPTH);
        playareaBorderMeshFilter_Back.transform.localPosition = new Vector3(0f, 0f, gameplayManager.GameplayCamera.farClipPlane * k_BACKSCALEFROMFARPLANE);

        GeneratePlayAreaMesh();

        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnGameplayMetronomeFired += GameplayManager_OnGameplayMetronomeFired;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        previousPulseTime = 0d;
    }

    // we bounce the border when we hit, shrink the border when miss, ignore if hit bomb
    // pulseInterval will be used for the bounce timer so it is dynamic to BPM too

    private readonly Vector3 k_BOUNCEMAXSIZE = new Vector3(1.015f, 1.015f, 1f);
    private readonly Vector3 k_SHRINKMINSIZE = new Vector3(0.985f, 0.985f, 1f);
    private void GameplayManager_OnHitboxMiss(VisualHitbox obj)
    {
        ShrinkBorders();
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        BounceBorders();
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        BounceBorders();
    }

    // optimizations for this is to somehow stop the previous timers from executing? But this should be negligible anyway
    private void BounceBorders()
    {
        Action<double> bounceAction = (x) =>
        {
            Vector3 scale = Vector3.Lerp(k_BOUNCEMAXSIZE, Vector3.one, (float)(x / pulseInterval));
            playareaBorderMeshFilter_Front.transform.localScale = playareaBorderMeshFilter_Back.transform.localScale = scale;
        };


        TimerStopwatchAction bounceTimer = new TimerStopwatchAction(this, bounceAction, () => { }, 0d, pulseInterval, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(bounceTimer);
    }

    private void ShrinkBorders()
    {
        Action<double> shrinkAction = (x) =>
        {
            Vector3 scale = Vector3.Lerp(k_SHRINKMINSIZE, Vector3.one, (float)(x / pulseInterval));
            playareaBorderMeshFilter_Front.transform.localScale = playareaBorderMeshFilter_Back.transform.localScale = scale;
        };


        TimerStopwatchAction shrinkTimer = new TimerStopwatchAction(this, shrinkAction, () => { }, 0d, pulseInterval, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(shrinkTimer);
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        if (gameplayManager.CurrentActiveGameplayMarker == null)
        {
            return;
        }

        if (gameplayManager.IsMetronomeDisabled)
        {
            propertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, 1f);
            playareaBorderMeshRenderer_front.SetPropertyBlock(propertyBlock);
            playareaBorderMeshRenderer_back.SetPropertyBlock(propertyBlock);

            return;
        }

        if (MathHelper.IsTwoDoublesEqualWithEpsilion(pulseInterval, 0d))
        {
            propertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, 1f);
            playareaBorderMeshRenderer_front.SetPropertyBlock(propertyBlock);
            playareaBorderMeshRenderer_back.SetPropertyBlock(propertyBlock);

            return;
        }

        UpdatePlayareaBorderShaders(time);
    }

    private void UpdatePlayareaBorderShaders(double time)
    {
        double progress = (time - previousPulseTime) / pulseInterval;

        propertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, (float)progress);

        playareaBorderMeshRenderer_front.SetPropertyBlock(propertyBlock);
        playareaBorderMeshRenderer_back.SetPropertyBlock(propertyBlock);
    }

    private void GameplayManager_OnGameplayMetronomeFired(double fireTime)
    {
        if (gameplayManager.CurrentActiveGameplayMarker == null)
        {
            return;
        }

        previousPulseTime = fireTime;

        pulseInterval = 60d / gameplayManager.CurrentActiveGameplayMarker.BPM;

    }

    private const int k_NUMBEROFVERTICES = 8;
    private const int k_NUMBEROFTRIANGLES = 8;
    private const float k_DiagonalDisplacementComponent = 0.707106781f; // precomputes the unit vector of (1,1) and stores the component (sqrt(2) / 2)

    /// <summary>
    /// Generates a mesh with a defined inset thickness at the preview borders. Refer to the border schematic to better understand this code.
    /// </summary>
    private void GeneratePlayAreaMesh()
    {
        if (k_BORDERINSETTHICKNESS * 2 > gameplayManager.WorldSizeOfPreview.x || k_BORDERINSETTHICKNESS * 2 > gameplayManager.WorldSizeOfPreview.y) // invalid inset
        {
            return;
        }

        Mesh mesh = new Mesh();

        Vector3 worldMin = new Vector3(gameplayManager.WorldPositionOfPreviewMin.x, gameplayManager.WorldPositionOfPreviewMin.y, 0f);
        Vector3 worldMax = new Vector3(gameplayManager.WorldPositionOfPreviewMax.x, gameplayManager.WorldPositionOfPreviewMax.y, 0f);

        Vector3 min = worldMin - k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);
        Vector3 max = worldMax - k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        // outer verts is from 0 to 3, with 0 bottom left and increment clockwise
        // inner verts is from 4 to 7, with 4 bottom left and increment clockwise. we want the inner verts to be where the border is too, hence min and max has a displacement vector

        Vector3[] verts = new Vector3[k_NUMBEROFVERTICES];
        verts[0] = min;
        verts[1] = new Vector3(min.x, max.y, 0f);
        verts[2] = max;
        verts[3] = new Vector3(max.x, min.y, 0f);

        verts[4] = verts[0] + k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);
        verts[5] = verts[1] + k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        verts[6] = verts[2] + k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        verts[7] = verts[3] + k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);

        localBorderCorners[0] = verts[4];
        localBorderCorners[1] = verts[5];
        localBorderCorners[2] = verts[6];
        localBorderCorners[3] = verts[7];

        int[] tris = new int[k_NUMBEROFTRIANGLES * 3];

        // refer to schematic
        for (int i = 0; i < k_NUMBEROFTRIANGLES / 2; i++)
        {
            int offset = 6 * i; // generate 2 triangles for each cycle

            tris[offset] = i;
            tris[offset + 1] = (i + 1) % 4;
            tris[offset + 2] = (i + 1) % 4 + 4;

            tris[offset + 3] = i;
            tris[offset + 4] = (i + 1) % 4 + 4;
            tris[offset + 5] = i + 4;
        }


        Vector2[] uvs = new Vector2[k_NUMBEROFVERTICES];

        Vector2 thicknessRelative = (Vector2.one * k_BORDERINSETTHICKNESS) / gameplayManager.WorldSizeOfPreview; // how large the inset thickness relative to whole object scale. Note object scale is 16:9 ratio

        uvs[0] = Vector2.zero;
        uvs[1] = new Vector2(0, 1);
        uvs[2] = Vector2.one;
        uvs[3] = new Vector2(1, 0);

        uvs[4] = uvs[0] + thicknessRelative;
        uvs[5] = uvs[1] + new Vector2(thicknessRelative.x, -thicknessRelative.y);
        uvs[6] = uvs[2] + (-1f * thicknessRelative);
        uvs[7] = uvs[3] + new Vector2(-thicknessRelative.x, thicknessRelative.y);

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        playareaBorderMeshFilter_Front.mesh = playareaBorderMeshFilter_Back.mesh = mesh;
    }

    private const float k_PlayareaMaxRotationDegree = 1.5f;
    private const float k_PlayareaMaxDisplacement = 0.1f;
    private void Update()
    {
        Vector2 mousePosition = gameplayManager.GameplayMousePosition;

        Vector2 remappedPosition = 2f * mousePosition - Vector2.one; // x = 2n - 1 where x is [-1, 1], n is normalized value [0, 1].

        Vector3 eulerAngles = new Vector3(remappedPosition.y * k_PlayareaMaxRotationDegree, -remappedPosition.x * k_PlayareaMaxRotationDegree, 0f);
        Quaternion rotation = Quaternion.Euler(eulerAngles);
        Vector3 displacement = new Vector3(remappedPosition.x * k_PlayareaMaxDisplacement, remappedPosition.y * k_PlayareaMaxDisplacement, 0f);


        // Note that the camera is always pointing in +z axis, which means the parent quaternion (camera) is the identity quaternion, so the global rotation is same as the local rotation
        playareaBorderMeshFilter_Front.transform.SetLocalPositionAndRotation(displacement + new Vector3(0f, 0f, playareaBorderMeshFilter_Front.transform.localPosition.z), rotation);
        playareaBorderMeshFilter_Back.transform.SetLocalPositionAndRotation(displacement + new Vector3(0f, 0f, playareaBorderMeshFilter_Back.transform.localPosition.z), rotation);

        GeneratePerspectiveLines();
    }

    private const float k_PERSPECTIVELINETHICKNESS = 0.025f;
    private const float k_PERSPECTIVELINELENGTHFACTOR = 0.5f; // how long we draw the perspective line to the vanishing point
    private void GeneratePerspectiveLines()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 borderWorldPoint = playareaBorderMeshFilter_Front.transform.TransformPoint(localBorderCorners[i]);
            Vector3 vanishWorldPoint = playareaBorderMeshFilter_Back.transform.TransformPoint(localBorderCorners[i]);
            Vector3 position = (borderWorldPoint + vanishWorldPoint) / 2;
            Vector3 fromToVector = vanishWorldPoint - borderWorldPoint;

            Vector3 size = new Vector3(k_PERSPECTIVELINETHICKNESS, k_PERSPECTIVELINETHICKNESS, fromToVector.magnitude);
            Quaternion rotation = Quaternion.LookRotation(fromToVector, Vector3.up);

            perspectiveLineGameObjects[i].transform.localScale = size;
            perspectiveLineGameObjects[i].transform.SetPositionAndRotation(position, rotation);
        }
    }
}
