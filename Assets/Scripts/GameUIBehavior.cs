using UnityEngine;

/// <summary>
/// A class to handle the behavior of the UI on a game-global basis.
/// </summary>
public class GameUIBehavior : MonoBehaviour
{
    public GameUIBehavior GameManagerUI { get; private set; }
    [SerializeField] private AudioClip clickUISound;

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

    private void GameVirtualCursorInstance_OnVirtualCursorClickedUIElement()
    {
        AudioEngine.AudioInstance.PlayAudioClip(clickUISound, 0d, GameManager.GameInstance.GlobalSettings.UIVolume, 1d, 0f);
    }
}
