using UnityEngine;

/// <summary>
/// A class to handle hitsounds during Editor playback
/// </summary>
public class EditorHitsoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip tick;

    private EditorManager editorInstance;

    private void Start()
    {
        editorInstance = EditorManager.EditorInstance;

        editorInstance.OnUnrenderRenderable += EditorInstance_OnUnrenderRenderable;
        //editorInstance.OnRenderRenderable += EditorInstance_OnRenderRenderable;
    }

    private void EditorInstance_OnUnrenderRenderable(EditorObject obj)
    {
        if (!editorInstance.IsEditorInPlaybackState)
        {
            return;
        }

        if (obj is not EditorHitbox hitbox)
        {
            return;
        }

        if (hitbox.HitboxType == HitboxType.BOMB)
        {
            return;
        }

        PlayHitsound(0d);
    }

    private const float k_HITSOUNDVOLUME = 0.7f;
    private void PlayHitsound(double offset)
    {
        AudioEngine.AudioInstance.PlayAudioClip(tick, offset, k_HITSOUNDVOLUME, 1d);
    }
}
