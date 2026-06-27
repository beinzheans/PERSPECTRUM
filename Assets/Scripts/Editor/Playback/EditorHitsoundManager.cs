using UnityEngine;

/// <summary>
/// A class to handle hitsounds during Editor playback
/// </summary>
public class EditorHitsoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip hitsound_A;
    [SerializeField] private AudioClip hitsound_B;
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

        PlayHitsound(hitbox.HitboxType, 0d);
    }

    private void PlayHitsound(HitboxType hitboxType, double offset)
    {
        if (hitboxType == HitboxType.A)
        {
            AudioEngine.AudioInstance.PlayAudioClip(hitsound_A, offset, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
        }
        else
        {
            AudioEngine.AudioInstance.PlayAudioClip(hitsound_B, offset, GameManager.GameInstance.GlobalSettings.HitsoundVolume, 1d);
        }
    }
}
