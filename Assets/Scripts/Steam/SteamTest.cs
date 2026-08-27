using Steamworks;
using UnityEngine;

public class SteamTest : MonoBehaviour
{
    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        Debug.Log($"{SteamFriends.GetPersonaName()}");
        Callback<GameOverlayActivated_t>.Create(x => OnOverlayActivated(x));
    }

    private void OnOverlayActivated(GameOverlayActivated_t gameOverlayActivated)
    {
        if (gameOverlayActivated.m_bActive == 0)
        {
            Debug.Log($"Inactive");
        }
        else
        {
            Debug.Log($"Active");
        }
    }
}
