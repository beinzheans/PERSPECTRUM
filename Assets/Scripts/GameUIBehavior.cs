using UnityEngine;
using Random = System.Random;
/// <summary>
/// A class to handle the behavior of the UI on a game-global basis.
/// </summary>
public class GameUIBehavior : MonoBehaviour
{
    public GameUIBehavior GameManagerUI { get; private set; }
    [SerializeField] private AudioClip clickUISound;

    Random random = new Random(0);

    private void Awake()
    {
        if (GameManagerUI == null)
        {
            GameManagerUI = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        GameVirtualCursor.GameVirtualCursorInstance.OnVirtualCursorClickedUIElement += GameVirtualCursorInstance_OnVirtualCursorClickedUIElement;
    }

    private void OnDestroy()
    {
        GameVirtualCursor.GameVirtualCursorInstance.OnVirtualCursorClickedUIElement -= GameVirtualCursorInstance_OnVirtualCursorClickedUIElement;
    }

    private const double k_MINSPEED = 0.9f;
    private const double k_MAXSPEED = 1.1f;
    private void GameVirtualCursorInstance_OnVirtualCursorClickedUIElement(GameObject gameObject)
    {
        double speed = k_MINSPEED + random.NextDouble() * (k_MAXSPEED - k_MINSPEED);
        AudioEngine.AudioInstance.PlayAudioClip(clickUISound, 0d, GameManager.GameInstance.GlobalSettings.UIVolume, speed, 0f);
    }
}
