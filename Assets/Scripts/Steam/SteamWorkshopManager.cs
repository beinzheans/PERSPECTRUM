using UnityEngine;
using Steamworks;
using System.Threading.Tasks;
using System.IO;
using System;

/// <summary>
/// A class to handle Steam workshop logic. Specificaly: <br></br>
/// 1. uploading of custom levels to Workshop, <br></br>
/// 2. subscriptions of custom levels from Workshop, <br></br>
/// 3. fetching Workshop query and rendering options on the game. <br></br>
/// </summary>
public class SteamWorkshopManager : MonoBehaviour
{
    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        SteamManager.SteamInstance.OnRequestPublishToSteamWorkshop += SteamInstance_OnRequestPublishToSteamWorkshop;
        SteamManager.SteamInstance.OnRequestUploadFiles += SteamInstance_OnRequestUploadFiles;
    }

    private const string k_PREVIEWIMAGENAME = "preview";
    private async void SteamInstance_OnRequestUploadFiles(string file, ulong previousPublisherID)
    {
        GamePersistenceManager.LoadChartFile(file, out _, out string metadataJson, out _, out byte[] imageByte);

        GamePersistenceManager.GetMetadataOfEditorChartFromJson(metadataJson, out EditorChartMetadata metadata);

        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(AppId_t.Invalid, PublishedFileId_t.Invalid);

        string title = $"{metadata.BaseMetadata.ChartName} [{metadata.BaseMetadata.ChartDifficulty}]";

        SteamUGC.SetItemTitle(handle, title);
        SteamUGC.SetItemMetadata(handle, metadata.BaseMetadata.GUID);
        string description = $"Charted by {metadata.BaseMetadata.ChartMapper}\n" +
                             $"Song: {metadata.BaseMetadata.SongName} [by {metadata.BaseMetadata.SongArtist}]";
        if (previousPublisherID != 0) // this indicates it is a derivative work! We must credit the original in the derivate work.
        {
            string originalAuthor = await GetAuthorOfItemByPublisherFileID(previousPublisherID);
            description += $"\nA derivative work of {originalAuthor}";
        }

        SteamUGC.SetItemDescription(handle, description);

        if (GamePersistenceManager.IsByteArrayValidImageFile(imageByte, out string extension))
        {
            string imageFilePath = Path.Combine(Application.temporaryCachePath, SteamManager.k_STEAM_WORKSHOP_STAGINGFOLDER, $"{k_PREVIEWIMAGENAME}.{extension}");

            try
            {
                File.WriteAllBytes(imageFilePath, imageByte);

                SteamUGC.SetItemPreview(handle, imageFilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to copy image bytes onto the Staging area. No preview image is assigned! Exception: \n" +
                                 $"{e.Message}");
            }
        }
        SteamUGC.SetItemContent(handle, Path.Combine(Application.temporaryCachePath, SteamManager.k_STEAM_WORKSHOP_STAGINGFOLDER));
        SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);

        SteamAPICall_t call = SteamUGC.SubmitItemUpdate(handle, null);

        SubmitItemUpdateResult_t result = await SteamHelper.CreateAwaitableFromSteamAPICall<SubmitItemUpdateResult_t>(call);


        if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            Debug.LogWarning("User hasn't accepted legal agreement!");
        }

        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"Failed to submit item update! Status: {result.m_eResult}");
        }
        else
        {
            Debug.Log($"Item Update Result is OK");
        }

        SteamManager.SteamInstance.RemoveAllFilesInStagingArea(); // we remove everything in staging area after the submit result!
    }

    private const string k_FAILEDFETCHNAME = "[unknown]";
    private async Task<string> GetAuthorOfItemByPublisherFileID(ulong publisherFileID)
    {
        UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { new PublishedFileId_t(publisherFileID) }, 1);
        SteamAPICall_t queryCall = SteamUGC.SendQueryUGCRequest(queryHandle);
        SteamUGCQueryCompleted_t queryResult = await SteamHelper.CreateAwaitableFromSteamAPICall<SteamUGCQueryCompleted_t>(queryCall);

        SteamUGC.ReleaseQueryUGCRequest(queryHandle);
        if (queryResult.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"Failed to query item with ID {publisherFileID}! Result: {queryResult.m_eResult}");
            return k_FAILEDFETCHNAME;
        }

        bool isQueryResultSuccess = SteamUGC.GetQueryUGCResult(queryResult.m_handle, 0, out SteamUGCDetails_t details);
        SteamUGC.ReleaseQueryUGCRequest(queryResult.m_handle);

        if (!isQueryResultSuccess)
        {
            Debug.LogWarning($"Failed to fetch UGC details from query result.");
            return k_FAILEDFETCHNAME;
        }

        CSteamID id = new CSteamID(details.m_ulSteamIDOwner);
        bool isCached = SteamFriends.RequestUserInformation(id, true);
        if (!isCached)
        {
            try
            {
                await SteamHelper.CreateAwaitableFromCallback<PersonaStateChange_t>(x => x.m_ulSteamID == publisherFileID);
            }
            catch
            {
                return k_FAILEDFETCHNAME;
            }
        }

        return SteamFriends.GetFriendPersonaName(id);
    }

    private async Task<ulong> SteamInstance_OnRequestPublishToSteamWorkshop()
    {
        SteamAPICall_t handle = SteamUGC.CreateItem(AppId_t.Invalid, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
        CreateItemResult_t result = await SteamHelper.CreateAwaitableFromSteamAPICall<CreateItemResult_t>(handle);

        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"Failed to create item! Result: {result.m_eResult}");
            return 0;
        }

        if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            Debug.LogWarning($"User hasn't accepted legal agreement!");
            return 0;
        }

        return result.m_nPublishedFileId.m_PublishedFileId;
    }

    private void OnDestroy()
    {
        SteamManager.SteamInstance.OnRequestPublishToSteamWorkshop -= SteamInstance_OnRequestPublishToSteamWorkshop;
        SteamManager.SteamInstance.OnRequestUploadFiles -= SteamInstance_OnRequestUploadFiles;
    }
}
