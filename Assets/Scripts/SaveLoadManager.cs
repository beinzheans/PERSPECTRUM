using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;


public static class SaveLoadManager
{
    public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    /// <summary>
    /// Saves a chart file to a file destination given the JSON and audio byte array information 
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="chartJson"></param>
    /// <param name="audioByte"></param>
    public static void SaveAsChartFile(string fullFilePath, string chartJson, string metadataJson, byte[] audioByte)
    {
        FileStream stream = new FileStream(fullFilePath, FileMode.Create); // filemode will automatically override existing file
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create);

        ZipArchiveEntry chartJsonEntry = archive.CreateEntry(GameManager.k_CHARTFILENAME);

        StreamWriter jsonWriter = new StreamWriter(chartJsonEntry.Open());
        jsonWriter.Write(chartJson);
        jsonWriter.Close();

        ZipArchiveEntry metadataJsonEntry = archive.CreateEntry(GameManager.k_METADATAFILENAME);

        StreamWriter metadataWriter = new StreamWriter(metadataJsonEntry.Open());
        metadataWriter.Write(metadataJson);
        metadataWriter.Close();

        ZipArchiveEntry audioEntry = archive.CreateEntry(GameManager.k_AUDIOFILENAME);

        Stream audioWriter = audioEntry.Open();
        audioWriter.Write(audioByte);
        audioWriter.Close();

        archive.Dispose();
        stream.Close();
    }

    /// <summary>
    /// Converts a chart file to JSON and audio byte array information if possible. <br></br>
    /// Returns false and empty chart information if no JSON nor audio byte array is valid.
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="chartJson"></param>
    /// <param name="audioByte"></param>
    /// <returns></returns>
    public static void LoadChartFile(string fullFilePath, out string chartJson, out string metadataJson, out byte[] audioByte)
    {
        FileStream stream = new FileStream(fullFilePath, FileMode.Open);
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry jsonEntry = archive.GetEntry(GameManager.k_CHARTFILENAME);

        if (jsonEntry == null)
        {
            chartJson = "";
        }
        else
        {
            StreamReader jsonReader = new StreamReader(jsonEntry.Open());

            chartJson = jsonReader.ReadToEnd();
            jsonReader.Close();
        }

        ZipArchiveEntry metadataEntry = archive.GetEntry(GameManager.k_METADATAFILENAME);

        if (metadataEntry == null)
        {
            metadataJson = "";
        }
        else
        {
            StreamReader metadataReader = new StreamReader(metadataEntry.Open());

            metadataJson = metadataReader.ReadToEnd();
            metadataReader.Close();
        }

        ZipArchiveEntry audioEntry = archive.GetEntry(GameManager.k_AUDIOFILENAME);

        if (audioEntry == null)
        {
            audioByte = new byte[0];
        }
        else
        {
            Stream audioReader = audioEntry.Open();
            MemoryStream memoryStream = new MemoryStream();

            audioReader.CopyTo(memoryStream);

            audioByte = memoryStream.ToArray();

            memoryStream.Close();
            audioReader.Close();
        }

        archive.Dispose();
        stream.Close();
    }

    private const string k_TEMPORARYFILENAME = "temporary_cache.mp3";
    /// <summary>
    /// Gets an audio clip from the bytes by using temporary file cache.
    /// </summary>
    /// <param name="audioByte"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public static async Task<(bool, AudioClip, byte[])> GetAudioClipFromByteArray(byte[] audioByte)
    {
        if (audioByte == null || audioByte.Length <= 0)
        {
            return (false, null, new byte[0]);
        }

        string tempFilePath = Path.Combine(Application.temporaryCachePath, k_TEMPORARYFILENAME);

        try
        {
            File.WriteAllBytes(tempFilePath, audioByte);
            return await AudioEngine.AudioInstance.GetAudioClipFromLocalFile(tempFilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to create temporary cache file. Exception:\n" +
                             $"{e}");

            return (false, null, new byte[0]);
        }
        finally
        {
            if (File.Exists(tempFilePath)) // delete the cache after we're done
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public static async Task<(bool, EditorChart, AudioClip)>ConvertFilesToEditorChart(string chartJson, byte[] audioBytes)
    {
        try
        {
            EditorChart editorChart = JsonConvert.DeserializeObject<EditorChart>(chartJson, JsonSerializerSettings);

            (bool clipResult, AudioClip clip, byte[] _) = await GetAudioClipFromByteArray(audioBytes);

            if (!clipResult)
            {
                return (true, editorChart, null);
            }

            return (true, editorChart, clip);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert files to editor chart. Exception: \n" +
                             $"{e}");
            return (false, null, null);
        }
    }

    public const string k_GameChartStorageFolderName = "Loaded_Charts";
    public static void ImportEditorChartToGameStorage(string editorChartPath, out string internalChartPath)
    {
        if (!File.Exists(editorChartPath))
        {
            internalChartPath = "";
            return;
        }

        if (Path.GetExtension(editorChartPath).TrimStart('.') != GameManager.k_FILEEXTENSION)
        {
            internalChartPath = "";
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(editorChartPath);

        string gamePath = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName, $"{fileName}.{GameManager.k_FILEEXTENSION}");

        int copyIndex = 0;
        while (File.Exists(gamePath))
        {
            copyIndex++;
            gamePath = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName, $"{fileName}_{copyIndex}.{GameManager.k_FILEEXTENSION}");
        }

        // gamePath does not conflict anymore

        internalChartPath = gamePath;
        File.Copy(editorChartPath, gamePath);
    }

    public static void ReadEditorChartsInGameStorage(out string[] editorChartPaths)
    {
        string path = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName);
        if (!Directory.Exists(path)) // create directory if it doesn't exist
        {
            Directory.CreateDirectory(path);
        }

        editorChartPaths = Directory.EnumerateFiles(path).Where(x => Path.GetExtension(x).TrimStart('.').ToLowerInvariant() == GameManager.k_FILEEXTENSION).OrderBy(x => x).ToArray(); // only get files with our extension and sort in ascending order
    }

    public static void GetMetadataOfEditorChartPath(string fullFilePath, out EditorChartMetadata metadata)
    {
        FileStream stream = new FileStream(fullFilePath, FileMode.Open);
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry metadataEntry = archive.GetEntry(GameManager.k_METADATAFILENAME);

        string metadataJson = "";
        if (metadataEntry == null)
        {
            metadata = null;

            archive.Dispose();
            stream.Close();
            return;
        }
        else
        {
            StreamReader metadataReader = new StreamReader(metadataEntry.Open());

            metadataJson = metadataReader.ReadToEnd();
            metadataReader.Close();
        }

        metadata = JsonConvert.DeserializeObject<EditorChartMetadata>(metadataJson, JsonSerializerSettings);

        archive.Dispose();
        stream.Close();
    }

    public static void SaveGlobalSettingsToFile(GlobalSettings settings)
    {
        string path = Path.Combine(Application.persistentDataPath, GameManager.k_PLAYERSETTINGSFILENAME);

        string json = JsonConvert.SerializeObject(settings);

        File.WriteAllText(path, json);
    }

    public static bool LoadGlobalSettingsFromFile(out GlobalSettings settings)
    {
        string path = Path.Combine(Application.persistentDataPath, GameManager.k_PLAYERSETTINGSFILENAME);

        if (!File.Exists(path))
        {
            settings = GameManager.DefaultGlobalSettings;
            return false;
        }

        string json = File.ReadAllText(path);

        try
        {
            settings = JsonConvert.DeserializeObject<GlobalSettings>(json);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load player settings. Exception: \n" +
                             $"{e}");

            settings = GameManager.DefaultGlobalSettings;
            return false;
        }
    }
}
