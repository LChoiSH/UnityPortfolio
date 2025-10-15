using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Localization;

public static class LocalizationFontCharExtractor
{
    public const string LOCAL_PATH = "Assets/Localization";
    public const string EXPORT_PATH = "Assets/Editor/ExportCharacters";

    [MenuItem("Tools/Localization/Export Used Characters (Per Locale)")]
    public static void ExportCharacters()
    {
        Dictionary<string, HashSet<char>> localCharDic = new Dictionary<string, HashSet<char>>();

        string[] guids = AssetDatabase.FindAssets("t:StringTableCollection", new[] { LOCAL_PATH });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var tableCollection = AssetDatabase.LoadAssetAtPath<StringTableCollection>(path);
            if (tableCollection == null) continue;

            foreach (StringTable table in tableCollection.StringTables)
            {
                if (table == null) continue;

                // en, ko
                string localeCode = table.LocaleIdentifier.Code;
                if (string.IsNullOrEmpty(localeCode)) localeCode = "unknown";

                localCharDic[localeCode] = new HashSet<char>();

                foreach (StringTableEntry entry in table.Values)
                {
                    if (entry == null) continue;
                    string value = entry.LocalizedValue;
                    if (string.IsNullOrEmpty(value)) continue;

                    foreach (char c in value)
                    {
                        localCharDic[localeCode].Add(c);
                    }
                }
            }
        }

        foreach (string local in localCharDic.Keys)
        {
            string outPath = Path.Combine(EXPORT_PATH, $"UsedFontCharacters_{local}.txt");
            char[] exportedChars = localCharDic[local].ToArray();
            File.WriteAllText(outPath, new string(exportedChars));
            Debug.Log($"{local}: {exportedChars.Length} char exported");
        }

        AssetDatabase.Refresh();
        Debug.Log("Locale Export Complete!");
    }
}
