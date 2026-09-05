// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.12

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
#if !DISABLESTEAMWORKS
using System.Collections;
using Steamworks;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
#endif

//
// The SteamManager provides a base implementation of Steamworks.NET on which you can build upon.
// It handles the basics of starting up and shutting down the SteamAPI for use.
//
[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    protected static bool s_EverInitialized = false;

    protected static SteamManager s_instance;
    public static SteamManager SteamInstance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("SteamManager").AddComponent<SteamManager>();
            }
            else
            {
                return s_instance;
            }
        }
    }

    protected bool m_bInitialized = false;
    public static bool Initialized
    {
        get
        {
            return SteamInstance.m_bInitialized;
        }
    }

    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

#if UNITY_2019_3_OR_NEWER
    // In case of disabled Domain Reload, reset static members before entering Play Mode.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnPlayMode()
    {
        s_EverInitialized = false;
        s_instance = null;
    }
#endif

    protected virtual void Awake()
    {
        // Only one instance of SteamManager at a time!
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        if (s_EverInitialized)
        {
            // This is almost always an error.
            // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
            // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
            // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        // We want our SteamManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        { // We catch this exception here, as it will be the first occurrence of it.
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        // Initializes the Steamworks API.
        // If this returns false then this indicates one of the following conditions:
        // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
        // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
        // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
        // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
        // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
        // Valve's documentation for this is located here:
        // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
        m_bInitialized = SteamAPI.Init();
        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            return;
        }

        s_EverInitialized = true;
    }

    // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
    protected virtual void OnEnable()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        if (!m_bInitialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            // Set up our callback to receive warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to receive warnings.
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    // OnApplicationQuit gets called too early to shutdown the SteamAPI.
    // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
    protected virtual void OnDestroy()
    {
        if (s_instance != this)
        {
            return;
        }

        s_instance = null;

        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    protected virtual void Update()
    {
        if (!m_bInitialized)
        {
            return;
        }

        // Run Steam client callbacks
        SteamAPI.RunCallbacks();
    }

    public event Func<Task<ulong>> OnRequestPublishToSteamWorkshop;

    public async Task<ulong> InvokePublishWorkshopEvent()
    {
        ulong? result = await OnRequestPublishToSteamWorkshop.Invoke();

        return result == null ? 0 : (ulong)result;
    }

    /// <summary>
    /// The folder used when staging a Steam Workshop upload. This should a subdirectory inside <see cref="Application.temporaryCachePath"/>. <br></br>
    /// </summary>
    public const string k_STEAM_WORKSHOP_STAGINGFOLDER = "Workshop_Staging_Folder";

    /// <summary>
    /// This event fires when the file has been placed into the Staging area and ready to upload.
    /// </summary>
    public event Action<string, ulong, ulong> OnRequestUploadFiles;

    /// <summary>
    /// Adds a file with extension <see cref="GameManager.k_FILEEXTENSION"/> into the staging area for the Steam Workshop. <br></br>
    /// Returns false if the file to add is invalid, or if adding fails.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool AddFileToStagingArea(string filePath, ulong publisherFileID, ulong previousPublisherFileID)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Attempted to add a non-existent file to the staging area!");
            return false;
        }

        if (Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant() != GameManager.k_FILEEXTENSION)
        {
            Debug.LogWarning($"Attempted to add an invalid file to the staging area!");
            return false;
        }

        string stagingFolder = Path.Combine(Application.temporaryCachePath, k_STEAM_WORKSHOP_STAGINGFOLDER);

        if (!Directory.Exists(stagingFolder))
        {
            Directory.CreateDirectory(stagingFolder);
        }

        try
        {
            string originalfileNameWithExtension = Path.GetFileName(filePath);
            string destinationFilePath = Path.Combine(stagingFolder, originalfileNameWithExtension);
            File.Copy(filePath, destinationFilePath, true);
            Debug.Log($"Added {destinationFilePath}");

            OnRequestUploadFiles?.Invoke(destinationFilePath, publisherFileID, previousPublisherFileID);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to add file to the staging area! Exception: \n" +
                             $"{e.Message}");

            return false;
        }
    }

    /// <summary>
    /// Returns true if all of the files inside the staging area has been deleted. <br></br>
    /// Returns false if the staging area is invalid, or if no files are in the staging area, or if at least one file has failed to be deleted.
    /// </summary>
    /// <returns></returns>
    public bool RemoveAllFilesInStagingArea()
    {
        string stagingFolder = Path.Combine(Application.temporaryCachePath, k_STEAM_WORKSHOP_STAGINGFOLDER);
        
        if (!Directory.Exists(stagingFolder))
        {
            Debug.LogWarning($"Staging area does not exist, it has not been created, and thus nothing to remove.");
            return false;
        }

        string[] files = Directory.GetFiles(stagingFolder);
        if (files.Length <= 0)
        {
            Debug.Log($"Nothing to remove in staging area.");
            return false;
        }

        bool oneHasFailed = false;

        for (int i = files.Length - 1; i >= 0; i--)
        {
            try
            {
                File.Delete(files[i]);
            }
            catch (Exception e)
            {
                oneHasFailed = true;
                Debug.LogWarning($"Failed to delete file at {files[i]}. Exception: \n" +
                                 $"{e.Message}");
            }
        }

        return !oneHasFailed;
    }

    /// <summary>
    /// This event fires when we have installed a chart in Steam's local storage. <br></br>
    /// Note since Steam report folder paths, you should search through the result for the files you want. <br></br>
    /// Passes the folder path that Steam reports. If somehow the subscribed item is not installed locally, empty string is passed.
    /// </summary>
    public event Action<string> OnChartInstalledInSteamStorage;

    public void InvokeChartInstalledInSteamStorage(string folderPath_STEAM)
    {
        OnChartInstalledInSteamStorage?.Invoke(folderPath_STEAM);
    }

    /// <summary>
    /// This event fires when we have unsubscribed from a Steam Workshop item. <br></br>
    /// Passes the folder path that the unsubscribed item is in.
    /// </summary>
    public event Action<string> OnSteamWorkshopUnsubscribed;

    public void InvokeWorkshopItemUnsubscribed(string folderPath_STEAM)
    {
        OnSteamWorkshopUnsubscribed?.Invoke(folderPath_STEAM);
    }

    public event Func<string[]> OnRequestChartsInLocalSteamStorage;

    /// <summary>
    /// Gets all charts that are installed in Steam's local storage. This should be done at start-up. <br></br>
    /// Note since Steam report folder paths, you should search through the result for the files you want. <br></br>
    /// Returns an array with the length <see cref="SteamUGC.GetNumDownloadedItems"/> that contains the folder paths. <br></br>
    /// If the subscribed item is not installed locally, empty string is returned.
    /// </summary>
    /// <returns></returns>
    public string[] RequestChartsInLocalSteamStorage()
    {
        string[] allChartsInstalled = OnRequestChartsInLocalSteamStorage?.Invoke();
        return allChartsInstalled;
    }
#else
	public static bool Initialized {
		get {
			return false;
		}
	}
#endif // !DISABLESTEAMWORKS
}