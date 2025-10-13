using UnityEditor;
using UnityEngine;
using CurrencySystem;
using System.IO;

public static class CurrencyDatabaseCreator
{
    [MenuItem("Tools/Currency/Create Database Asset")]
    public static void CreateDatabase()
    {
        string path = "Assets/Game/Data/Currency";

        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        var asset = ScriptableObject.CreateInstance<CurrencyDatabaseSO>();
        var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "CurrencyDatabase.asset"));

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"Created CurrencyDatabase at: {assetPath}");
    }
}