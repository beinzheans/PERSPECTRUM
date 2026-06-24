using System.IO;

public static class GameVersionConverter
{
    public static readonly byte[] ValidArchiveMagicBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
    public static bool IsValidArchiveFile(string fullFilePath)
    {
        FileStream fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);

        if (fileStream.Length < 4)
        {
            return false;
        }

        byte[] magicBytes = new byte[4];

        fileStream.Read(magicBytes, 0, 4);

        fileStream.Close();

        for (int i = 0; i < 4; i++)
        {
            if (magicBytes[i] != ValidArchiveMagicBytes[i])
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidArchiveFile(byte[] bytes)
    {
        if (bytes.Length < 4)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (bytes[i] != ValidArchiveMagicBytes[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets a byte array from a file extension <see cref="GameManager.k_FILEEXTENSION"/>. Returns true if it is valid, otherwise returns false. <br></br>
    /// This allows
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool GetArchiveFileBytes(string fullFilePath, out byte[] result)
    {
        if (!File.Exists(fullFilePath))
        {
            result = new byte[0];
            return false;
        }

        if (Path.GetExtension(fullFilePath).TrimStart('.').ToLowerInvariant() != GameManager.k_FILEEXTENSION)
        {
            result = new byte[0];
            return false;
        }

        byte[] archiveBytes = File.ReadAllBytes(fullFilePath);

        if (IsValidArchiveFile(archiveBytes))
        {
            result = archiveBytes;
            return true;
        }
        else
        {
            byte[] cipherBytes = SaveLoadManager.XorProcesser(archiveBytes);

            if (!IsValidArchiveFile(cipherBytes))
            {
                result = new byte[0];
                return false;
            }

            result = cipherBytes;
            return true;
        }
    }

}

