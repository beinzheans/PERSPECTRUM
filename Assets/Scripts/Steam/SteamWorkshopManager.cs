using UnityEngine;
using Steamworks;

/// <summary>
/// A class to handle Steam workshop logic. Specificaly: <br></br>
/// 1. uploading of custom levels to Workshop, <br></br>
/// 2. subscriptions of custom levels from Workshop, <br></br>
/// 3. fetching Workshop query and rendering options on the game. <br></br>
/// </summary>
public class SteamWorkshopManager : MonoBehaviour
{
    private CallResult<CreateItemResult_t> CreateItemCallback;

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        CreateItemCallback = CallResult<CreateItemResult_t>.Create(OnItemCreated);
    }

    private void OnItemCreated(CreateItemResult_t call, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogWarning("bIOFailure when attempting to create item");
            return;
        }

        if (call.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"Create item result is not OK. Result: {call.m_eResult}");
            return;
        }

        if (call.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            Debug.Log($"Open Workshop User agreement");
            return;
        }

        
    }
}
