using UnityEngine;
using Steamworks;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// A class to handle Steam workshop logic. Specifically: <br></br>
/// 1. uploading of custom levels to Workshop, <br></br>
/// 2. subscriptions of custom levels from Workshop.
/// </summary>
public class SteamWorkshopManager : MonoBehaviour
{
    private Callback<RemoteStoragePublishedFileSubscribed_t> OnItemSubscribedCallback;
    private Callback<RemoteStoragePublishedFileUnsubscribed_t> OnItemUnsubscribedCallback;

    private Callback<DownloadItemResult_t> OnDownloadItemCallback;

    public const string k_STEAM_WORKSHOP_FIXEDURL = @"https://steamcommunity.com/app/480/workshop";
    public const string k_STEAM_USER_FIXEDURL = @"https://steamcommunity.com/profiles/";
    public const string k_STEAM_WORKSHOPITEM_FIXEDURL = @"https://steamcommunity.com/sharedfiles/filedetails/?id=";
    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            return;
        }

        SteamManager.SteamInstance.OnRequestPublishToSteamWorkshop += SteamInstance_OnRequestPublishToSteamWorkshop;
        SteamManager.SteamInstance.OnRequestUploadFiles += SteamInstance_OnRequestUploadFiles;
        SteamManager.SteamInstance.OnRequestChartsInLocalSteamStorage += SteamInstance_OnRequestChartsInLocalSteamStorage;

        OnItemSubscribedCallback = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(OnItemSubscribed);
        OnItemUnsubscribedCallback = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnItemUnsubscribed);

        OnDownloadItemCallback = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
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
                Debug.LogWarning($"Subscribed item {subscribedIDs[i]} is not installed, needs update, or is actively downloading! Sending download request...");
                SteamUGC.DownloadItem(subscribedIDs[i], true); // we will tell Steam to download it first, which moves it to the Callback case.
                result[i] = "";
                continue;
            }

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

        if (!Directory.Exists(path))
        {
            return false;
        }

        bool hasValidFile = Directory.EnumerateFiles(path).Any(x => Path.GetExtension(x).TrimStart('.').ToLowerInvariant() == GameManager.k_FILEEXTENSION);

        if (!hasValidFile)
        {
            return false;
        }

        return true;
    }

    private void OnItemSubscribed(RemoteStoragePublishedFileSubscribed_t subscribedItem)
    {
        if (subscribedItem.m_nAppID != SteamUtils.GetAppID())
        {
            Debug.Log("Ignoring subscribe event due to incorrect AppID");
            return;
        }

        bool isValid = IsSteamItemValid(subscribedItem.m_nPublishedFileId, out string path);

        if (isValid)
        {
            Debug.LogWarning($"Contradiction! The Workshop item was just subscribed yet it was found to be valid! We ignore this request.");
            return;
        }

        SteamUGC.DownloadItem(subscribedItem.m_nPublishedFileId, true);
    }

    private void OnItemUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t unsubscribedItem)
    {
        if (unsubscribedItem.m_nAppID != SteamUtils.GetAppID())
        {
            Debug.Log("Ignoring download event due to incorrect AppID");
            return;
        }

        bool isValid = IsSteamItemValid(unsubscribedItem.m_nPublishedFileId, out string path);

        if (!isValid)
        {
            Debug.LogWarning($"Contradiction! The Workshop item was just unsubscribed yet it was found to be NOT valid, not invoking unsubscribed event.");
            return;
        }

        SteamManager.SteamInstance.InvokeWorkshopItemUnsubscribed(path);
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
            Debug.LogWarning($"Item is downloaded but the path is still not valid, and thus it is not safe to access it locally!");
            return;
        }

        Debug.Log($"Item {downloadResult.m_nPublishedFileId} downloaded and installed! Invoking event to listeners...");
        SteamManager.SteamInstance.InvokeChartInstalledInSteamStorage(path);
    }

    private const uint k_STEAM_FOLDERCHARLIMIT = 1024;

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


        if (previousPublisherFileID != 0 && previousPublisherFileID != publisherFileID) // this means that this is derivative work!
        {
            string itemLink = $"{k_STEAM_WORKSHOPITEM_FIXEDURL}{previousPublisherFileID}";

            (bool authorResult, CSteamID id, string originalAuthor) = await GetAuthorOfItemByPublisherFileID(previousPublisherFileID);
            if (!authorResult)
            {
                description += $"\nA [url={itemLink}]derivative work[/url]. Can not identify who created it!";
            }
            else
            {
                string userLink = $"{k_STEAM_USER_FIXEDURL}{id.m_SteamID}";
                description += $"\nA [url={itemLink}]derivative work[/url] of [url={userLink}]{originalAuthor}[/url] (name when the item was updated)";
            }
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
            string url = $"{k_STEAM_WORKSHOPITEM_FIXEDURL}{result.m_nPublishedFileId.m_PublishedFileId}";
            SteamFriends.ActivateGameOverlayToWebPage(url);
            Debug.LogWarning("User hasn't accepted legal agreement!");
        }
        SteamManager.SteamInstance.RemoveAllFilesInStagingArea(); // we remove everything in staging area after the submit result!
    }

    private const string k_FAILEDFETCHNAME = "[unknown]";
    private async Task<(bool, CSteamID, string)> GetAuthorOfItemByPublisherFileID(ulong publisherFileID)
    {
        (bool hasFetchedQueryDetails, SteamUGCDetails_t details) = await GetSteamUGCQueryDetails(publisherFileID);

        if (!hasFetchedQueryDetails)
        {
            Debug.LogWarning($"Failed to fetch UGC details from query result.");
            return (false, new CSteamID(), k_FAILEDFETCHNAME);
        }

        CSteamID id = new CSteamID(details.m_ulSteamIDOwner);
        bool needsQuery = SteamFriends.RequestUserInformation(id, true);
        if (needsQuery)
        {
            try
            {
                await SteamHelper.CreateAwaitableFromCallback<PersonaStateChange_t>(x => x.m_ulSteamID == details.m_ulSteamIDOwner);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PersonaStateChange_t callback returned exception! Exception: \n" +
                                 $"{e.Message}");
                return (false, id, k_FAILEDFETCHNAME);
            }
        }

        return (true, id, SteamFriends.GetFriendPersonaName(id));
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
            string url = $"{k_STEAM_WORKSHOP_FIXEDURL}{result.m_nPublishedFileId.m_PublishedFileId}";
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
        OnItemSubscribedCallback?.Dispose();
        OnItemUnsubscribedCallback?.Dispose();
    }
}
