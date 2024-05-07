using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SaveAssets
{
    /// <summary>
    /// Saves combined stuff into it's appropriate file
    /// </summary>
    /// <param name="path">the path to the combined folder</param>
    /// <param name="obj">the object to save into a file</param>
    internal static void SaveFile(string path, Object obj)
    {
        string[] paths = Rearrange(path);
        string directory = paths[0];
        string completePath = paths[1];
        string directoryPath = paths[2];
        
        if (!AssetDatabase.IsValidFolder(directoryPath))
        {
            List<string> splitDirectoryPath = directoryPath.Split('/').ToList();
            splitDirectoryPath.RemoveAt(splitDirectoryPath.Count - 1);
            string parentFolder = string.Join("/", splitDirectoryPath);
            AssetDatabase.CreateFolder(parentFolder, directory);
        }
        
        if (obj is GameObject)
        {
            GameObject gameObject = (GameObject)obj;

            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, completePath, InteractionMode.UserAction);

            return;
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

    /// <summary>
    /// re arranges a path so that a files parent folder is it's extention name
    /// </summary>
    /// <param name="path"></param>
    /// <returns>3 strings, 0: new parent folder name, 1: complete rearranged path, 2: path to parent folder</returns>
    public static string[] Rearrange(string path)
    {
        string directory = path.Split('.').Last();
        directory = $"{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(directory.ToLower())}s";
        List<string> splitPath = path.Split('/').ToList();
        splitPath.Insert(splitPath.Count - 1, directory);
        string completePath = string.Join('/', splitPath);
        splitPath.RemoveAt(splitPath.Count - 1);
        string directoryPath = string.Join('/', splitPath);

        return new string[3] { directory, completePath, directoryPath };
    }
}
