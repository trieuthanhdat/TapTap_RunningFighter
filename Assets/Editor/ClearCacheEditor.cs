using UnityEngine;
using System.IO;
using UnityEditor;
public class ClearCacheEditor
{
    [MenuItem("Clear Cache/Clear All")]
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("result cache clear DownloaderData: " + Application.dataPath.Replace("Assets", "DownloadedData") + FileUtil.DeleteFileOrDirectory(Application.dataPath.Replace("Assets", "DownloadedData")));
        Debug.Log("result cache clear Resource: " + FileUtil.DeleteFileOrDirectory("Assets/Resources/GameJSONData"));
        AssetDatabase.Refresh();
        string newDataPath = Path.Combine(Application.persistentDataPath, "new_playerdata.txt");
        if (File.Exists(newDataPath))
        {
            File.Delete(newDataPath);
            Debug.Log($"newDataPath cache clear Resource: {newDataPath}");
        }
        SaveAndLoad<TextAsset>.DeleteFilesInFolder("DataResources");
    }
    [MenuItem("Clear Cache/Clear Resources")]
    public static void ClearCache()
    {
        //PlayerPrefs.DeleteAll ();
        //Debug.Log("result cache clear DownloaderData: "+ Application.dataPath.Replace("Assets", "DownloadedData") +FileUtil.DeleteFileOrDirectory(Application.dataPath.Replace("Assets", "DownloadedData")));
        Debug.Log("result cache clear Resource: " + FileUtil.DeleteFileOrDirectory("Assets/Resources/GameJSONData"));
        AssetDatabase.Refresh();
    }
    [MenuItem("Clear Cache/Clear DownloadedData")]
    public static void ClearDownloadedData()
    {
        //PlayerPrefs.DeleteAll ();
        Debug.Log("result cache clear DownloaderData: " + Application.dataPath.Replace("Assets", "DownloadedData") + FileUtil.DeleteFileOrDirectory(Application.dataPath.Replace("Assets", "DownloadedData")));
        //Debug.Log("result cache clear Resource: " + FileUtil.DeleteFileOrDirectory("Assets/Resources/GameJSONData"));
    }
    [MenuItem("Clear Cache/Clear PlayerRef")]
    public static void ClearPlayerRef()
    {
        PlayerPrefs.DeleteAll();
        //Debug.Log("result cache clear DownloaderData: " + Application.dataPath.Replace("Assets", "DownloadedData") + FileUtil.DeleteFileOrDirectory(Application.dataPath.Replace("Assets", "DownloadedData")));
        //Debug.Log("result cache clear Resource: " + FileUtil.DeleteFileOrDirectory("Assets/Resources/GameJSONData"));
    }
}