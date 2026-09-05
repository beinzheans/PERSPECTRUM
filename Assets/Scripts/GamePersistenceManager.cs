using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Debug = UnityEngine.Debug;
/// <summary>
/// A class to handle game persistence logic.
/// </summary>
public static class GamePersistenceManager
{
    public static readonly byte[] zipEncryptionByteKey = Encoding.UTF8.GetBytes("PleaseDontCrackThisKey");
    public static byte[] XorProcesser(byte[] bytes)
    {
        byte[] result = new byte[bytes.Length];

        for (int i = 0; i < bytes.Length; i++)
        {
            result[i] = (byte)(bytes[i] ^ zipEncryptionByteKey[i % zipEncryptionByteKey.Length]);
        }

        return result;
    }

    /// <summary>
    /// Saves a chart file to a file destination given the JSON and audio byte array information. This overrides existing paths.
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="chartJson"></param>
    /// <param name="audioByte"></param>
    public static void SaveAsChartFile(string fullFilePath, string chartJson, string metadataJson, byte[] audioByte, byte[] imageByte)
    {
        MemoryStream memoryStream = new MemoryStream();

        ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create);

        CreateEntry(ref archive, GameManager.k_CHARTFILENAME, chartJson);
        CreateEntry(ref archive, GameManager.k_METADATAFILENAME, metadataJson);
        CreateEntry(ref archive, GameManager.k_AUDIOFILENAME, audioByte);

        bool isValidImage = IsByteArrayValidImageFile(imageByte, out string extension);

        if (isValidImage)
        {
            string imageFilePath = $"{GameManager.k_BACKGROUNDIMAGEFILENAME}.{extension}";

            CreateEntry(ref archive, imageFilePath, imageByte);
        }

        archive.Dispose();

        byte[] archiveBytes = memoryStream.ToArray();

        File.WriteAllBytes(fullFilePath, XorProcesser(archiveBytes));
        memoryStream.Close();
    }
    
    /// <summary>
    /// Creates a new entry with a name inside a zip archive.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entryName"></param>
    /// <param name="entryBytes"></param>
    private static void CreateEntry(ref ZipArchive archive, string entryName, byte[] entryBytes)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName);

        Stream stream = entry.Open();
        stream.Write(entryBytes);
        stream.Close();
    }

    /// <summary>
    /// Creates a new entry with a name inside a zip archive.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entryName"></param>
    /// <param name="entryString"></param>
    private static void CreateEntry(ref ZipArchive archive, string entryName, string entryString)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName);

        StreamWriter stream = new StreamWriter(entry.Open());
        stream.Write(entryString);
        stream.Close();
    }

    /// <summary>
    /// Converts a chart file to JSON and audio byte array information if possible. <br></br>
    /// Returns empty chart information if no JSON nor audio byte array is valid.
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="chartJson"></param>
    /// <param name="audioByte"></param>
    /// <returns></returns>
    public static void LoadChartFile(string fullFilePath, out string chartJson, out string metadataJson, out byte[] audioByte, out byte[] imageByte)
    {
        bool isValid = GameArchiveValidator.GetArchiveFileBytes(fullFilePath, GameManager.k_FILEEXTENSION, out byte[] archiveBytes);

        if (!isValid)
        {
            chartJson = "";
            metadataJson = "";
            audioByte = new byte[0];
            imageByte = new byte[0];
            return;
        }

        MemoryStream stream = new MemoryStream(archiveBytes);
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        LoadEntry(ref archive, GameManager.k_CHARTFILENAME, out chartJson);
        LoadEntry(ref archive, GameManager.k_METADATAFILENAME, out metadataJson);
        LoadEntry(ref archive, GameManager.k_AUDIOFILENAME, out audioByte);

        // we only match the name, since it can be .png or .jpg
        ZipArchiveEntry imageEntry = archive.Entries.FirstOrDefault(x => string.Equals(Path.GetFileNameWithoutExtension(x.Name), GameManager.k_BACKGROUNDIMAGEFILENAME));

        LoadEntry(ref archive, imageEntry, out imageByte);

        archive.Dispose();
        stream.Close();
    }

    /// <summary>
    /// Loads an entry with a name inside an zip archive.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entryName"></param>
    /// <param name="bytes"></param>
    private static void LoadEntry(ref ZipArchive archive, string entryName, out byte[] bytes)
    {
        ZipArchiveEntry entry = archive.GetEntry(entryName);

        LoadEntry(ref archive, entry, out bytes);
    }

    /// <summary>
    /// Loads an entry with a name inside an zip archive.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entryName"></param>
    /// <param name="bytes"></param>
    private static void LoadEntry(ref ZipArchive archive, string entryName, out string stringContent)
    {
        ZipArchiveEntry entry = archive.GetEntry(entryName);

        LoadEntry(ref archive, entry, out stringContent);
    }

    /// <summary>
    /// Loads a given entry inside an zip archive.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entryName"></param>
    /// <param name="bytes"></param>
    private static void LoadEntry(ref ZipArchive archive, ZipArchiveEntry entry, out byte[] bytes)
    {
        if (entry == null)
        {
            bytes = new byte[0];
            return;
        }

        Stream reader = entry.Open();
        MemoryStream memoryStream = new MemoryStream();

        reader.CopyTo(memoryStream);
        bytes = memoryStream.ToArray();

        memoryStream.Close();
        reader.Close();
    }

    /// <summary>
    /// Loads a given entry inside an zip archive.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entryName"></param>
    /// <param name="bytes"></param>
    private static void LoadEntry(ref ZipArchive archive, ZipArchiveEntry entry, out string stringContent)
    {
        if (entry == null)
        {
            stringContent = "";
            return;
        }

        StreamReader stream = new StreamReader(entry.Open());
        stringContent = stream.ReadToEnd();
        stream.Close();
    }

    private const string k_TEMPORARYCACHE_AUDIOFILENAME = "temporary_cache.mp3";
    /// <summary>
    /// Gets an audio clip from the bytes by using temporary file cache.
    /// </summary>
    /// <param name="audioByte"></param>
    /// <returns></returns>
    public static async Task<(bool, AudioClip)> GetAudioClipFromByteArray(byte[] audioByte, bool isStreamingAudio)
    {
        if (audioByte == null || audioByte.Length <= 0)
        {
            return (false, null);
        }

        string tempFilePath = Path.Combine(Application.temporaryCachePath, k_TEMPORARYCACHE_AUDIOFILENAME);

        try
        {
            File.WriteAllBytes(tempFilePath, audioByte);
            if (isStreamingAudio)
            {
                return await AudioEngine.AudioInstance.GetAudioClipFromLocalFileStreaming(tempFilePath);
            }
            else
            {
                return await AudioEngine.AudioInstance.GetAudioClipFromLocalFile(tempFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to create temporary cache file. Exception:\n" +
                             $"{e}");

            return (false, null);
        }
        finally
        {
            if (File.Exists(tempFilePath)) // delete the cache after we're done
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public static async Task<(bool, EditorChart, AudioClip)> ConvertFilesToEditorChart(string chartJson, byte[] audioBytes)
    {
        try
        {
            EditorChart editorChart = JsonConvert.DeserializeObject<EditorChart>(chartJson, GameManager.GameInstance.JsonSerializerSettings);

            (bool clipResult, AudioClip clip) = await GetAudioClipFromByteArray(audioBytes, false);

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
    public static bool ImportEditorChartToGameStorage(string editorChartPath, out string internalChartPath)
    {
        if (!File.Exists(editorChartPath))
        {
            internalChartPath = "";
            return false;
        }

        if (Path.GetExtension(editorChartPath).TrimStart('.') != GameManager.k_FILEEXTENSION)
        {
            internalChartPath = "";
            return false;
        }

        string fileName = Path.GetFileNameWithoutExtension(editorChartPath);

        string gamePath = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName, $"{fileName}.{GameManager.k_FILEEXTENSION}");

        int copyIndex = 0;
        while (File.Exists(gamePath))
        {
            copyIndex++;
            gamePath = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName, $"{fileName}_{copyIndex}.{GameManager.k_FILEEXTENSION}");
        }

        // gamePath does not conflict anymore. we do this because it's possible different charts share the same name.

        internalChartPath = gamePath;
        File.Copy(editorChartPath, gamePath);

        return true;
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

    public static void GetMetadataOfEditorChartFromJson(string metadataJson, out EditorChartMetadata metadata)
    {
        metadata = JsonConvert.DeserializeObject<EditorChartMetadata>(metadataJson, GameManager.GameInstance.JsonSerializerSettings);
    }

    public static void GetMetadataJsonOfEditorChartPath(string fullFilePath, out string metadataJson)
    {
        bool isValid = GameArchiveValidator.GetArchiveFileBytes(fullFilePath, GameManager.k_FILEEXTENSION, out byte[] archiveBytes);

        if (!isValid)
        {
            metadataJson = "";
            return;
        }

        MemoryStream stream = new MemoryStream(archiveBytes);
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry metadataEntry = archive.GetEntry(GameManager.k_METADATAFILENAME);

        if (metadataEntry == null)
        {
            metadataJson = "";

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
            settings = JsonConvert.DeserializeObject<GlobalSettings>(json, GameManager.GameInstance.JsonSerializerSettings);

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

    public const string k_GAMEPLAYRECORDSDIRECTORY = "Play_Records";
    public static void SaveGameplayStatisticRecordToFile(GameplayStatisticRecord gameplay)
    {
        string directory = Path.Combine(Application.persistentDataPath, k_GAMEPLAYRECORDSDIRECTORY);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string fileName = Path.Combine(directory, $"{gameplay.BaseChartMetadata.ChartMapper}-{gameplay.BaseChartMetadata.ChartName}-{gameplay.RecordTimestamp}.json"); // timestamp should ensure that no file collision, unless if someone wants to fuck around

        string jsonString = JsonConvert.SerializeObject(gameplay, GameManager.GameInstance.JsonSerializerSettings);

        File.WriteAllText(fileName, jsonString);
    }

    /// <summary>
    /// Loads all gameplay records into the game as a list.
    /// </summary>
    /// <returns></returns>
    public static async Task<List<GameplayStatisticRecord>> LoadAllGameplayStatisticRecordFile(IProgress<float> numberOfProcessedRecords)
    {
        string directory = Path.Combine(Application.persistentDataPath, k_GAMEPLAYRECORDSDIRECTORY);

        GetAllGameplayStatisticRecordFilePaths(out string[] files);

        List<GameplayStatisticRecord> allRecords = new List<GameplayStatisticRecord>(files.Length);

        Stopwatch watch = new();

        watch.Start();

        await Task.Run(async () =>
        {
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    GameplayStatisticRecord record = await LoadSpecificGameplayStatisticRecordFile(files[i]);
                    allRecords.Add(record);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load record at {files[i]}. Exception: \n" +
                                     $"{e.Message}");

                }

                numberOfProcessedRecords?.Report((float)(i + 1) / files.Length);
            }
        });

        watch.Stop();

        Debug.Log($"Loading {files.Length} records took {watch.ElapsedMilliseconds} ms");
        return allRecords;
    }

    public static async Task<GameplayStatisticRecord> LoadSpecificGameplayStatisticRecordFile(string path)
    {
        GameplayStatisticRecord specificRecord;
        try
        {
            string json = await File.ReadAllTextAsync(path);

            specificRecord = JsonConvert.DeserializeObject<GameplayStatisticRecord>(json, GameManager.GameInstance.JsonSerializerSettings);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load record at {path}. Exception: \n" +
                             $"{e.Message}");
            specificRecord = new();
        }

        return specificRecord;
    }

    public static void GetAllGameplayStatisticRecordFilePaths(out string[] paths)
    {
        Stopwatch watch = new();

        watch.Start();
        string directory = Path.Combine(Application.persistentDataPath, k_GAMEPLAYRECORDSDIRECTORY);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        paths = Directory.EnumerateFiles(directory).Where(x => Path.GetExtension(x).TrimStart('.').ToLowerInvariant() == "json").ToArray(); // note we store our gameplay records as json

        watch.Stop();

        Debug.Log($"Getting statistic records took {watch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Creates a mapping f: Base Metadata -> set of records. This should be done at the beginning of the game load. <br></br>
    /// Note that this will by default create the mapping in descending order of scores.
    /// </summary>
    public static async Task<Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>>> CreateMetadataToRecordsMapping(IProgress<float> numberOfProcessedRecords)
    {
        Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>> mapping = new Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>>();
        List<GameplayStatisticRecord> records = await LoadAllGameplayStatisticRecordFile(numberOfProcessedRecords);

        for (int i = 0; i < records.Count; i++)
        {
            UpdateMetadataToRecordsMapping(records[i], mapping);
        }

        return mapping;
    }

    /// <summary>
    /// Updates the mapping f: Base Metadata -> set of records while keeping the descending order for final scores.
    /// </summary>
    /// <param name="record"></param>
    /// <param name="mapping"></param>
    public static void UpdateMetadataToRecordsMapping(GameplayStatisticRecord record, Dictionary<BaseChartMetadata, List<GameplayStatisticRecord>> mapping)
    {
        if (mapping == null)
        {
            Debug.LogWarning($"Record mapping has not been loaded yet, ignoring request to update the mapping!");
            return;
        }

        BaseChartMetadata metadata = record.BaseChartMetadata;

        if (!mapping.TryGetValue(metadata, out List<GameplayStatisticRecord> recordsList))
        {
            recordsList = new List<GameplayStatisticRecord>() { record };
            mapping.Add(metadata, recordsList);
            return;
        }

        recordsList.Add(record);
        recordsList.Sort((x, y) => SortRecordsComparator(y, x));
    }

    /// <summary>
    /// A comparator to sort records by final score. The comparator assumes ascending order, swap the operands for descending order.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static int SortRecordsComparator(GameplayStatisticRecord x, GameplayStatisticRecord y)
    {
        if (x.FinalScore > y.FinalScore)
        {
            return 1;
        }
        else if (x.FinalScore < y.FinalScore)
        {
            return -1;
        }
        else return 0;
    }

    /// <summary>
    /// Attempts to get a <see cref="Texture2D"/> from a byte array. <br></br>
    /// Returns false if converting the file fails.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="texture"></param>
    /// <returns></returns>
    public static bool GetTexture2DFromBytes(byte[] bytes, out Texture2D texture)
    {
        if (!IsByteArrayValidImageFile(bytes, out _))
        {
            texture = null;
            return false;
        }

        try
        {
            texture = new Texture2D(2, 2);

            texture.LoadImage(bytes, false);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert bytes to Texture2D. Exception:\n" +
                             $"{e.Message}");
            texture = null;
            return false;

        }
    }
    /// <summary>
    /// Checks if a provided byte array is a valid image file (.jpg or .png) and returns the extension. <br></br>
    /// Returns false if the byte array is not .jpg nor .png.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static bool IsByteArrayValidImageFile(byte[] bytes, out string extension)
    {
        if (bytes == null || bytes.Length < 4)
        {
            extension = "";
            return false;
        }

        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            extension = "jpg";
            return true;
        }

        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            extension = "png";
            return true;
        }

        extension = "";
        return false;
    }
}
