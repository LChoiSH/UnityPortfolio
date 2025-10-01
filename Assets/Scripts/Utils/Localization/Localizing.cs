using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class Localizing
{
    // 현재 모두 동기 사용중
    
    public static string Get(string table, string key, bool isPreloaded = false)
    {
        // 동기: preload 시켜놓았으면 사용
        if (isPreloaded) return LocalizationSettings.StringDatabase.GetLocalizedString(table, key);

        // 비동기: Async → Completed 핸들 써야 함. 
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        if (handle.IsDone) return handle.Result;

        // 아직 안끝났으면 key 그대로 반환 (혹은 기본값)
        return key;
    }

    public static string Get(string table, string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
    }

    public static string GetFormat(string table, string key, EffectArgs args)
    {
        string template = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
        return string.Format(template, (args.AllValue ?? Array.Empty<string>()).Cast<object>().ToArray());
    }

    public static string GetFormat(string table, string key, string[] args)
    {
        string template = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
        return string.Format(template, args);
    }

    public static string GetFormat(string table, string key, params object[] args)
    {
        string template = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);
        return string.Format(template, args);
    }
}
