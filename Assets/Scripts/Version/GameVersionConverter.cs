using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to hold all <see cref="VersionConverter"/> and attempts to convert charts to the current version if possible.
/// </summary>
public static class GameVersionConverter
{
    private static readonly List<VersionConverter> allVersionConverters = new List<VersionConverter>()
    {
        new VersionConvert_1_0_0_to_1_1_0(),
    };

    /// <summary>
    /// Attempts to convert a chart to the current game version. <br></br>
    /// Returns true if the chart is successfully upgraded. Returns false if the game is outdated for the chart OR if the chart has no valid <see cref="VersionConverter"/> path upgrade.
    /// </summary>
    /// <param name="chartJObject"></param>
    /// <param name="metadataJObject"></param>
    /// <param name="convertedChartJObject"></param>
    /// <param name="convertedmetadataJObject"></param>
    /// <returns></returns>
    public static bool ConvertChartVersionToCurrentGameVersion(in JObject chartJObject, in JObject metadataJObject, out JObject convertedChartJObject, out JObject convertedmetadataJObject)
    {
        if (!GetBaseDetailsFromMetadataJObject(in metadataJObject, out BaseChartMetadata baseChartMetadata))
        {
            convertedChartJObject = chartJObject;
            convertedmetadataJObject = metadataJObject;
            return false;
        }

        string version = baseChartMetadata.Version;

        if (string.IsNullOrWhiteSpace(version))
        {
            convertedChartJObject = chartJObject;
            convertedmetadataJObject = metadataJObject;
            return false; // version cant be read somehow, just return false
        }

        int compareResult = MathHelper.CompareGameVersions(version, GameManager.GameInstance.CurrentVersion);
        if (compareResult == 1)
        {
            convertedChartJObject = chartJObject;
            convertedmetadataJObject = metadataJObject;
            return false; // game is outdated, can not convert.
        }
        else if (compareResult == 0)
        {
            convertedChartJObject = chartJObject;
            convertedmetadataJObject = metadataJObject;
            return true; // nothing to convert, it is already in the correct version
        }

        // chart is outdated, check upgrade paths to current version

        for (int i = 0; i < allVersionConverters.Count; i++)
        {
            if (!allVersionConverters[i].IsChartMetadataValid(in metadataJObject))
            {
                continue;
            }

            if (!allVersionConverters[i].ConvertChart(in chartJObject, in metadataJObject, out JObject tempChartJObject, out JObject tempMetadataJObject))
            {
                Debug.LogWarning($"Failed to convert {allVersionConverters[i].InVersion} to {allVersionConverters[i].OutVersion} despite having an existing converter.");
                convertedChartJObject = chartJObject;
                convertedmetadataJObject = metadataJObject;
                return false;
            }

            return ConvertChartVersionToCurrentGameVersion(in tempChartJObject, in tempMetadataJObject, out convertedChartJObject, out convertedmetadataJObject);
        }

        Debug.Log($"Can not convert to current game version.");
        convertedChartJObject = chartJObject;
        convertedmetadataJObject = metadataJObject;
        return false;
    }

    public static bool IsChartMetadataUpToDate(in JObject metadataJObject)
    {
        if (!GetBaseDetailsFromMetadataJObject(metadataJObject, out BaseChartMetadata baseChartMetadata))
        {
            return false;
        }

        int compareResult = MathHelper.CompareGameVersions(baseChartMetadata.Version, GameManager.GameInstance.CurrentVersion);

        return compareResult == 0;
    }

    /// <summary>
    /// Gets <see cref="BaseChartMetadata"/> from <paramref name="metadataJObject"/>. <br></br>
    /// Returns false if no base metadata is found, which indicates <paramref name="baseChartMetadata"/> is invalid.
    /// </summary>
    /// <param name="metadataJObject"></param>
    public static bool GetBaseDetailsFromMetadataJObject(in JObject metadataJObject, out BaseChartMetadata baseChartMetadata)
    {
        bool getResult = metadataJObject.TryGetValue(GameManager.k_METADATABASEDATAKEY, System.StringComparison.Ordinal, out JToken token);

        if (!getResult)
        {
            baseChartMetadata = new();
            return false;
        }

        baseChartMetadata = token.ToObject<BaseChartMetadata>();
        return true;
    }
}
