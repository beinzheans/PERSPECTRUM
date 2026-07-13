using System;
using UnityEngine;

/// <summary>
/// Manages the hit border logic during gameplay.
/// </summary>
public class GameplayPlayareaBorderManager : MonoBehaviour
{
    private readonly int k_SHADERPULSEPROGRESSID = Shader.PropertyToID("_PulseProgress");
    private MaterialPropertyBlock pulsePropertyBlock;

    private MeshRenderer playareaBorderMeshRenderer_front;
    private MeshRenderer playareaBorderMeshRenderer_back;
    private MeshRenderer playareaBorderMeshRenderer_earlyHitPlane;

    [SerializeField] private MeshFilter playareaBorderMeshFilter_Front;
    [SerializeField] private MeshFilter playareaBorderMeshFilter_earlyHitPlane;
    [SerializeField] private MeshFilter playareaBorderMeshFilter_Back;

    private GameplayManager gameplayManager;

    private double previousPulseTime;
    private double pulseInterval;


    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        pulsePropertyBlock = new MaterialPropertyBlock();

        playareaBorderMeshRenderer_front = playareaBorderMeshFilter_Front.GetComponent<MeshRenderer>();
        playareaBorderMeshRenderer_back = playareaBorderMeshFilter_Back.GetComponent<MeshRenderer>();
        playareaBorderMeshRenderer_earlyHitPlane = playareaBorderMeshFilter_earlyHitPlane.GetComponent<MeshRenderer>();

        playareaBorderMeshFilter_Front.transform.localPosition = new Vector3(0f, 0f, GameplayManager.k_HITPLANEDEPTH);
        playareaBorderMeshFilter_earlyHitPlane.transform.localPosition = new Vector3(0f, 0f, (float)(GameplayManager.k_HITPLANEDEPTH + GameplayManager.k_EARLYTIMEFRAME * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed));
        playareaBorderMeshFilter_Back.transform.localPosition = new Vector3(0f, 0f, gameplayManager.GameplayFarClipPlane);

        playareaBorderMeshFilter_Back.sharedMesh = playareaBorderMeshFilter_earlyHitPlane.sharedMesh = playareaBorderMeshFilter_Front.sharedMesh = gameplayManager.PlayAreaBorderMesh;
        
        gameplayManager.AssignGameplayBorderScale(Vector3.one);
        gameplayManager.AssignGameplayDisplacementRotation(Vector3.zero, Quaternion.identity);

        GameManager.GameInstance.OnGameSettingsChanged += GameInstance_OnGameSettingsChanged;
        gameplayManager.OnHitboxMatchedHit += GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit += GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxMiss += GameplayManager_OnHitboxMiss;
        gameplayManager.OnGameplayMetronomeFired += GameplayManager_OnGameplayMetronomeFired;
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
    }

    private void OnDestroy()
    {
        GameManager.GameInstance.OnGameSettingsChanged -= GameInstance_OnGameSettingsChanged;
        gameplayManager.OnHitboxMatchedHit -= GameplayManager_OnHitboxMatchedHit;
        gameplayManager.OnHitboxMismatchedHit -= GameplayManager_OnHitboxMismatchedHit;
        gameplayManager.OnHitboxMiss -= GameplayManager_OnHitboxMiss;
        gameplayManager.OnGameplayMetronomeFired -= GameplayManager_OnGameplayMetronomeFired;
        gameplayManager.OnGameplayTimeUpdated -= GameplayManager_OnGameplayTimeUpdated;
        gameplayManager.OnGameplayRestarted -= GameplayManager_OnGameplayRestarted;

    }
    private void GameInstance_OnGameSettingsChanged()
    {
        playareaBorderMeshFilter_earlyHitPlane.transform.localPosition = new Vector3(0f, 0f, (float)(GameplayManager.k_HITPLANEDEPTH + GameplayManager.k_EARLYTIMEFRAME * GameManager.GameInstance.GlobalSettings.GameSettings.GameScrollSpeed));
        playareaBorderMeshFilter_Back.transform.localPosition = new Vector3(0f, 0f, gameplayManager.GameplayFarClipPlane);

        playareaBorderMeshFilter_Back.sharedMesh = playareaBorderMeshFilter_earlyHitPlane.sharedMesh = playareaBorderMeshFilter_Front.sharedMesh = gameplayManager.PlayAreaBorderMesh;
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        previousPulseTime = 0d;
        pulseInterval = 0d;
        pulsePropertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, 1f);

        playareaBorderMeshRenderer_front.SetPropertyBlock(pulsePropertyBlock);
        playareaBorderMeshRenderer_earlyHitPlane.SetPropertyBlock(pulsePropertyBlock);
        playareaBorderMeshRenderer_back.SetPropertyBlock(pulsePropertyBlock);

        return;
    }

    // we bounce the border when we hit, shrink the border when miss, ignore if hit bomb
    // pulseInterval will be used for the bounce timer so it is dynamic to BPM too

    private readonly Vector3 k_BOUNCEMAXSIZE = new Vector3(1.020f, 1.020f, 1f);
    private readonly Vector3 k_SHRINKMINSIZE = new Vector3(0.980f, 0.980f, 1f);
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
            gameplayManager.AssignGameplayBorderScale(scale);
            playareaBorderMeshFilter_Front.transform.localScale = playareaBorderMeshFilter_earlyHitPlane.transform.localScale = playareaBorderMeshFilter_Back.transform.localScale = scale;
        };


        TimerStopwatchAction bounceTimer = new TimerStopwatchAction(this, bounceAction, () => { }, 0d, pulseInterval, false);
        DSPTimerEngine.TimerInstance.AddActionToTimer(bounceTimer);
    }

    private void ShrinkBorders()
    {
        Action<double> shrinkAction = (x) =>
        {
            Vector3 scale = Vector3.Lerp(k_SHRINKMINSIZE, Vector3.one, (float)(x / pulseInterval));
            gameplayManager.AssignGameplayBorderScale(scale);
            playareaBorderMeshFilter_Front.transform.localScale = playareaBorderMeshFilter_earlyHitPlane.transform.localScale = playareaBorderMeshFilter_Back.transform.localScale = scale;
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
            pulsePropertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, 1f);

            playareaBorderMeshRenderer_front.SetPropertyBlock(pulsePropertyBlock);
            playareaBorderMeshRenderer_earlyHitPlane.SetPropertyBlock(pulsePropertyBlock);
            playareaBorderMeshRenderer_back.SetPropertyBlock(pulsePropertyBlock);

            return;
        }

        if (MathHelper.IsTwoDoublesEqualWithEpsilion(pulseInterval, 0d))
        {
            pulsePropertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, 1f);

            playareaBorderMeshRenderer_front.SetPropertyBlock(pulsePropertyBlock);
            playareaBorderMeshRenderer_earlyHitPlane.SetPropertyBlock(pulsePropertyBlock);
            playareaBorderMeshRenderer_back.SetPropertyBlock(pulsePropertyBlock);

            return;
        }

        UpdatePlayareaBorderShaders(time);
    }

    private void UpdatePlayareaBorderShaders(double time)
    {
        double pulseProgress = (time - previousPulseTime) / pulseInterval;

        pulsePropertyBlock.SetFloat(k_SHADERPULSEPROGRESSID, (float)pulseProgress);
        playareaBorderMeshRenderer_front.SetPropertyBlock(pulsePropertyBlock);
        playareaBorderMeshRenderer_earlyHitPlane.SetPropertyBlock(pulsePropertyBlock);
        playareaBorderMeshRenderer_back.SetPropertyBlock(pulsePropertyBlock);

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

    private const float k_PlayareaMaxRotationDegree = 1.5f;
    private const float k_PlayareaMaxDisplacement = 0.075f;
    private void Update()
    {
        Vector2 mousePosition = gameplayManager.GameplayMousePosition;

        Vector2 remappedPosition = 2f * mousePosition - Vector2.one; // x = 2n - 1 where x is [-1, 1], n is normalized value [0, 1].

        Vector3 eulerAngles = new Vector3(remappedPosition.y * k_PlayareaMaxRotationDegree, -remappedPosition.x * k_PlayareaMaxRotationDegree, 0f);
        Quaternion rotation = Quaternion.Euler(eulerAngles);
        Vector3 displacement = new Vector3(remappedPosition.x * k_PlayareaMaxDisplacement, remappedPosition.y * k_PlayareaMaxDisplacement, 0f);

        gameplayManager.AssignGameplayDisplacementRotation(displacement, rotation);
        // Note that the camera is always pointing in +z axis, which means the parent quaternion (camera) is the identity quaternion, so the global rotation is same as the local rotation
        playareaBorderMeshFilter_Front.transform.SetLocalPositionAndRotation(displacement + new Vector3(0f, 0f, playareaBorderMeshFilter_Front.transform.localPosition.z), rotation);
        playareaBorderMeshFilter_earlyHitPlane.transform.SetLocalPositionAndRotation(displacement + new Vector3(0f, 0f, playareaBorderMeshFilter_earlyHitPlane.transform.localPosition.z), rotation);
        playareaBorderMeshFilter_Back.transform.SetLocalPositionAndRotation(displacement + new Vector3(0f, 0f, playareaBorderMeshFilter_Back.transform.localPosition.z), rotation);
    }
}
