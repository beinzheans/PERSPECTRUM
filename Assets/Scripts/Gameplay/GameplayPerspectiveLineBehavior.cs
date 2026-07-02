using UnityEngine;

/// <summary>
/// A class to handle the perspective lines as a batch at once.
/// </summary>
public class GameplayPerspectiveLineBehavior : MonoBehaviour
{
    private readonly int k_PERSPECTIVEJUDGEMENTTYPE = Shader.PropertyToID("_JudgementType_float");
    private readonly int k_PERSPECTIVENORMALIZEDPROGRESS = Shader.PropertyToID("_NormalizedProgress");

    private MaterialPropertyBlock propertyBlock;
    [SerializeField] private Transform frontHitPlane;
    [SerializeField] private Transform backHitPlane;

    private GameplayManager gameplayManager;

    [SerializeField] private GameObject[] perspectiveLineGameObjects = new GameObject[4];
    private MeshRenderer[] perspectiveLineMeshRenderer = new MeshRenderer[4];

    private const double k_PERSPECTIVELINEPULSETIME = 0.25d;
    private double previousPulseTime = 0d;
    void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        propertyBlock = new();

        for (int i = 0; i < 4; i++)
        {
            perspectiveLineMeshRenderer[i] = perspectiveLineGameObjects[i].GetComponent<MeshRenderer>();
        }

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnGameplayTimeUpdated -= GameplayManager_OnGameplayTimeUpdated;
    }

    private void GameplayManager_OnHitboxMismatchedHit(VisualHitbox obj)
    {
        propertyBlock.SetFloat(k_PERSPECTIVEJUDGEMENTTYPE, 0.5f);
        propertyBlock.SetFloat(k_PERSPECTIVENORMALIZEDPROGRESS, 0f);
        previousPulseTime = gameplayManager.CurrentGameplayTime;

        SetMaterialPropertyBlock();
    }

    private void GameplayManager_OnGameplayTimeUpdated(double obj)
    {
        double progress = (obj - previousPulseTime) / k_PERSPECTIVELINEPULSETIME;

        propertyBlock.SetFloat(k_PERSPECTIVENORMALIZEDPROGRESS, Mathf.Clamp01((float)progress));
        SetMaterialPropertyBlock();
    }

    private void GameplayManager_OnHitboxMatchedHit(VisualHitbox obj)
    {
        propertyBlock.SetFloat(k_PERSPECTIVEJUDGEMENTTYPE, 0f);
        propertyBlock.SetFloat(k_PERSPECTIVENORMALIZEDPROGRESS, 0f);
        previousPulseTime = gameplayManager.CurrentGameplayTime;

        SetMaterialPropertyBlock();
    }

    private void GameplayManager_OnGameplayStarted()
    {
        previousPulseTime = 0d;
        propertyBlock.SetFloat(k_PERSPECTIVEJUDGEMENTTYPE, 1f); // set to default color
        SetMaterialPropertyBlock();
    }

    private void SetMaterialPropertyBlock()
    {
        for (int i = 0; i < 4; i++)
        {
            perspectiveLineMeshRenderer[i].SetPropertyBlock(propertyBlock);
        }
    }

    private const float k_PERSPECTIVELINETHICKNESS = 0.015f;

    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 borderWorldPoint = frontHitPlane.TransformPoint(gameplayManager.LocalBorderCorners[i]);
            Vector3 vanishWorldPoint = backHitPlane.TransformPoint(gameplayManager.LocalBorderCorners[i]);
            Vector3 position = (borderWorldPoint + vanishWorldPoint) / 2;
            Vector3 fromToVector = vanishWorldPoint - borderWorldPoint;

            Vector3 size = new Vector3(k_PERSPECTIVELINETHICKNESS, k_PERSPECTIVELINETHICKNESS, fromToVector.magnitude);
            Quaternion rotation = Quaternion.LookRotation(fromToVector, Vector3.up);

            perspectiveLineGameObjects[i].transform.localScale = size;
            perspectiveLineGameObjects[i].transform.SetPositionAndRotation(position, rotation);
        }

    }
}
