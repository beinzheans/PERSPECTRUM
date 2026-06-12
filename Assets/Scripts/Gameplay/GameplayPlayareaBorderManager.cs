using UnityEngine;

/// <summary>
/// Manages the hit border logic during gameplay.
/// </summary>
public class GameplayPlayareaBorderManager : MonoBehaviour
{
    private readonly int k_SHADERPULSEPROGRESSID = Shader.PropertyToID("_PulseProgress");
    private MaterialPropertyBlock propertyBlock;
    private MeshRenderer playareaBorderMeshRenderer;
    [SerializeField] private MeshFilter playareaBorderMeshFilter;

    private GameplayManager gameplayManager;
    private const float k_BORDERINSETTHICKNESS = 0.025f;

    private double previousPulseTime;
    private double pulseInterval;

    private bool disableMetronome = false;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        propertyBlock = new MaterialPropertyBlock();
        playareaBorderMeshRenderer = playareaBorderMeshFilter.GetComponent<MeshRenderer>();
        GeneratePlayAreaMesh();

        gameplayManager.OnGameplayMetronomeFired += GameplayManager_OnGameplayMetronomeFired;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        if (gameplayManager.CurrentActiveGameplayMarker == null)
        {
            return;
        }

        if (disableMetronome)
        {
            return;
        }

        playareaBorderMeshRenderer.GetPropertyBlock(propertyBlock);

        double progress = (time - previousPulseTime) / pulseInterval;

        propertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, (float)progress);

        playareaBorderMeshRenderer.SetPropertyBlock(propertyBlock);
    }

    private void GameplayManager_OnGameplayMetronomeFired(double fireTime)
    {
        if (gameplayManager.CurrentActiveGameplayMarker == null)
        {
            return;
        }

        previousPulseTime = fireTime;
        if (MathHelper.IsTwoDoublesEqualWithEpsilion(gameplayManager.CurrentActiveGameplayMarker.BPM, 0d))
        {
            disableMetronome = true;
            return;
        }

        disableMetronome = false;
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

        Vector3 min = gameplayManager.WorldPositionOfPreviewMin - k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);
        Vector3 max = gameplayManager.WorldPositionOfPreviewMax - k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        // outer verts is from 0 to 3, with 0 bottom left and increment clockwise
        // inner verts is from 4 to 7, with 4 bottom left and increment clockwise. we want the inner verts to be where the border is too

        Vector3[] verts = new Vector3[k_NUMBEROFVERTICES];
        verts[0] = min;
        verts[1] = new Vector3(min.x, max.y, GameplayManager.k_HITPLANEDEPTH);
        verts[2] = max;
        verts[3] = new Vector3(max.x, min.y, GameplayManager.k_HITPLANEDEPTH);

        verts[4] = verts[0] + k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);
        verts[5] = verts[1] + k_BORDERINSETTHICKNESS * new Vector3(k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        verts[6] = verts[2] + k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, -k_DiagonalDisplacementComponent, 0f);
        verts[7] = verts[3] + k_BORDERINSETTHICKNESS * new Vector3(-k_DiagonalDisplacementComponent, k_DiagonalDisplacementComponent, 0f);

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

        playareaBorderMeshFilter.mesh = mesh;
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
        Vector3 position = gameplayManager.GameplayCamera.transform.position + displacement;

        playareaBorderMeshFilter.transform.SetPositionAndRotation(position, rotation);
    }
}
