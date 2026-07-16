using Newtonsoft.Json.Linq;

/// <summary>
/// A generic class to describe the chart conversion between two versions. <br></br>
/// Version conversion must be upgrading (from early to latest). Downgrading should not be implemented.
/// </summary>
public abstract class VersionConverter
{
    public abstract string InVersion { get; }
    public abstract string OutVersion { get; }
    /// <summary>
    /// Converts a chart of <see cref="InVersion"/> to another chart of <see cref="OutVersion"/>. <br></br>
    /// Returns true if conversion success, otherwise returns false. If the <paramref name="convertedMetadataJObject"/> version does not match <see cref="InVersion"/> OR the converter is a downgrader, it will return false.
    /// </summary>

    public bool ConvertChart(in JObject chartJObject, in JObject metadataJObject, out JObject convertedChartJObject, out JObject convertedMetadataJObject)
    {
        convertedChartJObject = new(chartJObject);
        convertedMetadataJObject = new(metadataJObject);

        if (MathHelper.CompareGameVersions(InVersion, OutVersion) != -1)
        {
            return false;
        }

        if (!IsChartMetadataValid(in metadataJObject))
        {
            return false;
        }

        if (OnConvertChartEvent(ref convertedChartJObject, ref convertedMetadataJObject))
        {
            convertedMetadataJObject[GameManager.k_METADATABASEDATAKEY][GameManager.k_CHARTVERSIONKEY] = OutVersion;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Whether or not the chart metadata provided is valid for this converter.
    /// </summary>
    /// <param name="metadataJObject"></param>
    /// <returns></returns>
    public bool IsChartMetadataValid(in JObject metadataJObject)
    {
        string version = metadataJObject[GameManager.k_METADATABASEDATAKEY][GameManager.k_CHARTVERSIONKEY]?.ToString();


        return !string.IsNullOrWhiteSpace(version) && version == InVersion;
    }

    /// <summary>
    /// Custom implementation of events to convert a chart of <see cref="InVersion"/> to <see cref="OutVersion"/>. Returns true if conversion is successful, otherwise return false. <br></br>
    /// This is where you should implement the differences between versions. <br></br>
    /// </summary>
    /// <param name="chartJObject"></param>
    /// <param name="metadataJObject"></param>
    /// <returns></returns>
    protected abstract bool OnConvertChartEvent(ref JObject chartJObject, ref JObject metadataJObject);
}

// Add converters inheriting the above parent class here.

public class VersionConverter_1_0_0_to_1_0_1 : VersionConverter
{
    public override string InVersion => "1.0.0";

    public override string OutVersion => "1.0.1";

    protected override bool OnConvertChartEvent(ref JObject chartJObject, ref JObject metadataJObject)
    {
        return true;
    }
}

public class VersionConverter_1_0_1_to_1_1_0 : VersionConverter
{
    public override string InVersion => "1.0.1";

    public override string OutVersion => "1.1.0";

    protected override bool OnConvertChartEvent(ref JObject chartJObject, ref JObject metadataJObject)
    {
        return true;
    }
}

public class VersionConverter_1_1_0_to_1_2_0 : VersionConverter
{
    public override string InVersion => "1.1.0";

    public override string OutVersion => "1.2.0";

    protected override bool OnConvertChartEvent(ref JObject chartJObject, ref JObject metadataJObject)
    {
        JProperty JProperty = new JProperty(GameManager.k_CHARTSTARTPREVIEWTIMEKEY, 0d);
        metadataJObject.Add(JProperty);

        return true;
    }
}