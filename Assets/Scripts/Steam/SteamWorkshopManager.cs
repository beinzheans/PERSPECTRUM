using UnityEngine;
using Steamworks;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// A class to handle Steam workshop logic. Specifically: <br></br>
/// 1. uploading of custom levels to Workshop, <br></br>
/// 2. subscriptions of custom levels from Workshop.
/// </summary>
public class SteamWorkshopManager : MonoBehaviour
{
    private Callback<DownloadItemResult_t> OnDownloadItemCallback;
    private Callback<ItemInstalled_t> OnItemInstalledCallback;
    private const string k_STEAM_WORKSHOP_FIXEDURL = @"steam://url/CommunityFilePage/";
    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        SteamManager.SteamInstance.OnRequestPublishToSteamWorkshop += SteamInstance_OnRequestPublishToSteamWorkshop;
        SteamManager.SteamInstance.OnRequestUploadFiles += SteamInstance_OnRequestUploadFiles;
        SteamManager.SteamInstance.OnRequestChartsInLocalSteamStorage += SteamInstance_OnRequestChartsInLocalSteamStorage;

        OnDownloadItemCallback = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
        OnItemInstalledCallback = Callback<ItemInstalled_t>.Create(OnItemInstalled);
    }


    private string[] SteamInstance_OnRequestChartsInLocalSteamStorage()
    {
        uint numberOfSubscribedItems = SteamUGC.GetNumSubscribedItems();
        PublishedFileId_t[] subscribedIDs = new PublishedFileId_t[numberOfSubscribedItems];
        string[] result = new string[numberOfSubscribedItems];
        SteamUGC.GetSubscribedItems(subscribedIDs, numberOfSubscribedItems);

        for (int i = 0; i < numberOfSubscribedItems; i++)
        {
            bool isValid = IsSteamItemValid(subscribedIDs[i], out string path);

            if (!isValid)
            {
                Debug.LogWarning($"Subscribed item is not installed, needs update, or is actively downloading! Sending download request...");
                SteamUGC.DownloadItem(subscribedIDs[i], true); // we will tell Steam to download it first, which moves it to the Callback case.
                result[i] = "";
                continue;
            }

            Debug.Log($"Found file at FOLDER path {path} in local Steam storage");
            result[i] = path;
        }

        return result;
    }

    /// <summary>
    /// Gets if a Steam item is valid by <see cref="PublishedFileId_t"/>. <br></br>
    /// Returns true if it is valid (ie., the <paramref name="path"/> exists locally), otherwise returns false (ie., requires installation, update or is actively updating). <br></br>
    /// Note it is possible for Steam to think the item is valid despite the <paramref name="path"/> not existing locally. This case is handled by returning false.
    /// </summary>
    /// <param name="publisherFileID_t"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool IsSteamItemValid(PublishedFileId_t publisherFileID_t, out string path)
    {
        uint itemState = SteamUGC.GetItemState(publisherFileID_t);

        bool isInstalled = (itemState & (uint)EItemState.k_EItemStateInstalled) != 0;
        bool isNeedUpdate = (itemState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0;
        bool isDownloading = (itemState & (uint)EItemState.k_EItemStateDownloading) != 0;
        if (!isInstalled || isNeedUpdate || isDownloading)
        {
            path = "";
            return false;
        }

        SteamUGC.GetItemInstallInfo(publisherFileID_t, out _, out path, k_STEAM_FOLDERCHARLIMIT, out _);

        bool isValidPathInFile = Directory.Exists(path) && Directory.GetFiles(path).Length > 0;

        if (!isValidPathInFile)
        {
            return false;
        }

        return true;
    }

    private void OnItemDownloaded(DownloadItemResult_t downloadResult)
    {
        if (downloadResult.m_unAppID != SteamUtils.GetAppID())
        {
            Debug.Log("Ignoring download event due to incorrect AppID");
            return;
        }

        if (downloadResult.m_eResult != EResult.k_EResultOK)
        {
            Debug.Log($"Download event is not OK! Status: {downloadResult.m_eResult}");
            return;
        }

        bool isValid = IsSteamItemValid(downloadResult.m_nPublishedFileId, out string path);

        if (!isValid)
        {
            Debug.LogWarning($"Item is not downloaded in a valid way, and thus it is not safe to access it locally!");
            return;
        }

        Debug.Log($"Item {downloadResult.m_nPublishedFileId} downloaded and installed! Invoking event to listeners...");
        SteamManager.SteamInstance.InvokeChartInstalledInSteamStorage(path);
    }

    private const uint k_STEAM_FOLDERCHARLIMIT = 1024;
    private void OnItemInstalled(ItemInstalled_t item)
    {
        if (item.m_unAppID != SteamUtils.GetAppID())
        {
            Debug.Log("Ignoring install event due to incorrect AppID");
            return;
        }

        bool isInstalled = SteamUGC.GetItemInstallInfo(item.m_nPublishedFileId, out _, out string folderPath, k_STEAM_FOLDERCHARLIMIT, out _);

        if (!isInstalled)
        {
            Debug.LogWarning($"Item is not installed, and thus it is not safe to access it locally!\n" +
                             $"This is a contradiction, since this Item has been invoked by Steam's ItemInstalled_t callback!");
            return;
        }

        Debug.Log($"Item {item.m_nPublishedFileId} installed! Invoking event to listeners...");
        SteamManager.SteamInstance.InvokeChartInstalledInSteamStorage(folderPath);
    }

    private async Task<(bool, SteamUGCDetails_t)> GetSteamUGCQueryDetails(ulong publisherFileID)
    {
        UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { new PublishedFileId_t(publisherFileID) }, 1);
        SteamAPICall_t queryCall = SteamUGC.SendQueryUGCRequest(queryHandle);
        SteamUGCQueryCompleted_t queryResult = await SteamHelper.CreateAwaitableFromSteamAPICall<SteamUGCQueryCompleted_t>(queryCall);

        if (queryResult.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"SteamUGCQuery Failed! Status: {queryResult.m_eResult}");
            SteamUGC.ReleaseQueryUGCRequest(queryResult.m_handle);
            return (false, new());
        }

        bool isSuccess = SteamUGC.GetQueryUGCResult(queryResult.m_handle, 0, out SteamUGCDetails_t details);
        SteamUGC.ReleaseQueryUGCRequest(queryResult.m_handle);

        if (!isSuccess)
        {
            Debug.LogWarning($"GetQueryUGCResult Failed! Status: {queryResult.m_eResult}");
            return (false, new());
        }

        return (true, details);
    }


    private const string k_PREVIEWIMAGENAME = "preview";
    private async void SteamInstance_OnRequestUploadFiles(string file, ulong publisherFileID, ulong previousPublisherFileID)
    {
        GamePersistenceManager.LoadChartFile(file, out _, out string metadataJson, out _, out byte[] imageByte);

        GamePersistenceManager.GetMetadataOfEditorChartFromJson(metadataJson, out EditorChartMetadata metadata);

        UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate((AppId_t)480, new PublishedFileId_t(publisherFileID));

        string title = $"{metadata.BaseMetadata.ChartName} [{metadata.BaseMetadata.ChartDifficulty}]";

        SteamUGC.SetItemTitle(handle, title);
        SteamUGC.SetItemMetadata(handle, metadata.BaseMetadata.GUID);

        Debug.Log($"Set item title: {title}");
        Debug.Log($"Set item metadata: {metadata.BaseMetadata.GUID}");

        string description = $"Charted by {metadata.BaseMetadata.ChartMapper}\n" +
                             $"Song: {metadata.BaseMetadata.SongName} [by {metadata.BaseMetadata.SongArtist}]";


        if (previousPublisherFileID != 0) // this means that this is derivative work!
        {
            string originalAuthor = await GetAuthorOfItemByPublisherFileID(publisherFileID);
            description += $"\nA derivative work of {originalAuthor}";
        }

        SteamUGC.SetItemDescription(handle, description);
        Debug.Log($"Set item description: \n" +
                  $"{description}");

        if (GamePersistenceManager.IsByteArrayValidImageFile(imageByte, out string extension))
        {
            string imageFilePath = Path.Combine(Application.temporaryCachePath, SteamManager.k_STEAM_WORKSHOP_STAGINGFOLDER, $"{k_PREVIEWIMAGENAME}.{extension}");

            try
            {
                File.WriteAllBytes(imageFilePath, imageByte);

                SteamUGC.SetItemPreview(handle, imageFilePath);

                Debug.Log($"Set item preview image!");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to copy image bytes onto the Staging area. No preview image is assigned! Exception: \n" +
                                 $"{e.Message}");
            }
        }
        else
        {
            Debug.Log($"No item preview");
        }

        SteamUGC.SetItemContent(handle, Path.Combine(Application.temporaryCachePath, SteamManager.k_STEAM_WORKSHOP_STAGINGFOLDER));

        SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);

        Debug.Log($"Submitting item update request");
        SteamAPICall_t call = SteamUGC.SubmitItemUpdate(handle, null);
        
        SubmitItemUpdateResult_t result = await SteamHelper.CreateAwaitableFromSteamAPICall<SubmitItemUpdateResult_t>(call);

        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"Failed to submit item update! Status: {result.m_eResult}");
        }
        else
        {
            Debug.Log($"Item Update Result is OK");
        }


        if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            string url = $"{k_STEAM_WORKSHOP_FIXEDURL}{result.m_nPublishedFileId}";
            SteamFriends.ActivateGameOverlayToWebPage(url);
            Debug.LogWarning("User hasn't accepted legal agreement!");
        }
        SteamManager.SteamInstance.RemoveAllFilesInStagingArea(); // we remove everything in staging area after the submit result!
    }

    private const string k_FAILEDFETCHNAME = "[unknown]";
    private async Task<string> GetAuthorOfItemByPublisherFileID(ulong publisherFileID)
    {
        (bool hasFetchedQueryDetails, SteamUGCDetails_t details) = await GetSteamUGCQueryDetails(publisherFileID);

        if (!hasFetchedQueryDetails)
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
                await SteamHelper.CreateAwaitableFromCallback<PersonaStateChange_t>(x => x.m_ulSteamID == details.m_ulSteamIDOwner);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PersonaStateChange_t callback returned exception! Exception: \n" +
                                 $"{e.Message}");
                return k_FAILEDFETCHNAME;
            }
        }

        return SteamFriends.GetFriendPersonaName(id);
    }

    private async Task<ulong> SteamInstance_OnRequestPublishToSteamWorkshop()
    {
        SteamAPICall_t handle = SteamUGC.CreateItem((AppId_t)480, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
        CreateItemResult_t result = await SteamHelper.CreateAwaitableFromSteamAPICall<CreateItemResult_t>(handle);

        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogWarning($"Failed to create item! Result: {result.m_eResult}");
            return 0;
        }

        if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            Debug.LogWarning($"User hasn't accepted legal agreement!");
            string url = $"{k_STEAM_WORKSHOP_FIXEDURL}{result.m_nPublishedFileId}";
            SteamFriends.ActivateGameOverlayToWebPage(url);
        }

        Debug.Log($"Steam returned publisher file ID {result.m_nPublishedFileId.m_PublishedFileId}");
        return result.m_nPublishedFileId.m_PublishedFileId;
    }

    private void OnDisable()
    {
        SteamManager.SteamInstance.OnRequestPublishToSteamWorkshop -= SteamInstance_OnRequestPublishToSteamWorkshop;
        SteamManager.SteamInstance.OnRequestUploadFiles -= SteamInstance_OnRequestUploadFiles;
        SteamManager.SteamInstance.OnRequestChartsInLocalSteamStorage -= SteamInstance_OnRequestChartsInLocalSteamStorage;

        OnDownloadItemCallback?.Dispose();
        OnItemInstalledCallback?.Dispose();
    }
}
