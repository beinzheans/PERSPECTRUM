using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUICursor : MonoBehaviour
{
    [SerializeField] private RawImage cursorRawImage;
    [SerializeField] private RectTransform cursorTrailParticleSystemRectTransform;

    private GameplayManager gameplayManager;

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayStarted += GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded += GameplayManager_OnGameplayEnded;
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayStarted -= GameplayManager_OnGameplayStarted;
        gameplayManager.OnGameplayEnded -= GameplayManager_OnGameplayEnded;
    }


    private void GameplayManager_OnGameplayEnded()
    {
        GameVirtualCursor.GameVirtualCursorInstance.ShowVirtualMouse();
        cursorRawImage.gameObject.SetActive(false);
    }

    private void GameplayManager_OnGameplayStarted()
    {
        GameVirtualCursor.GameVirtualCursorInstance.HideVirtualMouse();
        cursorRawImage.gameObject.SetActive(true);
    }

    private Vector2 previousGameplayMousePosition = Vector2.zero;
    private Vector3 scaleVelocity = Vector3.zero;
    private const float k_CURSORDEADZONERATE = 100f;
    private const float k_MAXXSCALESIZE = 2f;
    private const float k_MAXDISPLACEMENTMAGNTIUDE = 5000f;

    private const float k_CURSORSCALETIME = 0.1f;
    /// <summary>
    /// How much time the cursor needs to be idle before we actually scale it down.
    /// </summary>
    private const double k_CURSORIDLEWAITTIME = 0.5d;

    private TimerIntervalAction cursorIdleShrinkTimer;
    private bool cursorHasStartedShrinking = false;
    private void LateUpdate()
    {
        RectTransform cursorRect = cursorRawImage.rectTransform;
        cursorRect.anchorMin = cursorRect.anchorMax = gameplayManager.GameplayMousePosition;
        cursorRect.anchoredPosition = Vector2.zero;

        cursorTrailParticleSystemRectTransform.anchorMin = cursorTrailParticleSystemRectTransform.anchorMax = gameplayManager.GameplayMousePosition;
        cursorTrailParticleSystemRectTransform.anchoredPosition = Vector2.zero;

        Vector2 mousePixelDisplacement = MathHelper.GetPixelFromToVectorFromNormalizedPoints(gameplayManager.GameplayMousePosition, previousGameplayMousePosition, gameplayManager.GameplayRectTransform) / Time.deltaTime;

        previousGameplayMousePosition = gameplayManager.GameplayMousePosition;

        float deadzoneDisplacementThresholdSqr = k_CURSORDEADZONERATE * k_CURSORDEADZONERATE;
        Vector3 targetScale = Vector3.one;

        if (mousePixelDisplacement.sqrMagnitude >= deadzoneDisplacementThresholdSqr)
        {
            DSPTimerEngine.TimerInstance.RemoveActionFromTimer(cursorIdleShrinkTimer);
            cursorHasStartedShrinking = false;
            float zRadian = MathHelper.IsTwoFloatsEqualWithEpsilion(mousePixelDisplacement.x, 0f) ? Mathf.PI / 2 : Mathf.Atan2(mousePixelDisplacement.y, mousePixelDisplacement.x);

            float zRotation = GetConvertedRotationAngleForSymmetricalUI(zRadian) * Mathf.Rad2Deg;

            Quaternion rotation = Quaternion.Euler(0f, 0f, zRotation);
            cursorRect.rotation = rotation;

            float maxDisplacementThresholdSqr = k_MAXDISPLACEMENTMAGNTIUDE * k_MAXDISPLACEMENTMAGNTIUDE;
            float normalizedX = math.remap(deadzoneDisplacementThresholdSqr, maxDisplacementThresholdSqr, 0f, 1f, mousePixelDisplacement.sqrMagnitude);

            targetScale = new Vector3(MathHelper.EvaluateSigmoidFunction(normalizedX, 1f, k_MAXXSCALESIZE, 5f, 0.5f), 1f, 1f);
        }
        else
        {
            if (!cursorHasStartedShrinking)
            {
                cursorIdleShrinkTimer = new TimerIntervalAction(this, x => { targetScale = Vector3.one; cursorHasStartedShrinking = true; }, () => { }, k_CURSORIDLEWAITTIME, TimerBehavior.TEMPORARY, 0d);
                DSPTimerEngine.TimerInstance.AddActionToTimer(cursorIdleShrinkTimer);
            }
        }

        cursorRect.localScale = Vector3.SmoothDamp(cursorRect.localScale, targetScale, ref scaleVelocity, k_CURSORSCALETIME);
    }

    /// <summary>
    /// Returns the corrected rotation angle (in radians) for symmetrical UI elements given a <paramref name="angle"/> in [-pi, pi] range. <br></br>
    /// That is, f: [-pi, pi] -> [-pi/2, pi/2].
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float GetConvertedRotationAngleForSymmetricalUI(float angle)
    {
        if (angle > -Mathf.PI / 2 && angle < Mathf.PI / 2)
        {
            return angle;
        }
        else if (angle >= Mathf.PI / 2)
        {
            return angle - Mathf.PI;
        }
        else
        {
            return Mathf.PI + angle;
        }
    }

}
