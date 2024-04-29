using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SaveAssets
{
    public static void SaveFile(string path, Object obj)
    {
        string directory = path.Split('.').Last();
        directory = $"{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(directory.ToLower())}s";
        List<string> splitPath = path.Split('/').ToList();
        splitPath.Insert(splitPath.Count - 1, directory);
        string completePath = string.Join('/', splitPath);
        splitPath.RemoveAt(splitPath.Count - 1);
        string directoryPath = string.Join('/', splitPath);

        if (!AssetDatabase.IsValidFolder(directoryPath))
        {
            List<string> splitDirectoryPath = directoryPath.Split('/').ToList();
            splitDirectoryPath.RemoveAt(splitDirectoryPath.Count - 1);
            string parentFolder = string.Join("/", splitDirectoryPath);
            AssetDatabase.CreateFolder(parentFolder, directory);
        }
        
        if (obj is Texture2D)
        {
            Texture2D texture = (Texture2D)obj;
            byte[] png = ImageConversion.EncodeToPNG(texture);

            File.WriteAllBytes(completePath, png);

            AssetDatabase.Refresh();
            return;
        }

        AssetDatabase.CreateAsset(obj, completePath);
    }
}
