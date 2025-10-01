using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class Localizing
{
    // ���� ��� ���� �����
    
    public static string Get(string table, string key, bool isPreloaded = false)
    {
        // ����: preload ���ѳ������� ���
        if (isPreloaded) return LocalizationSettings.StringDatabase.GetLocalizedString(table, key);

        // �񵿱�: Async �� Completed �ڵ� ��� ��. 
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        if (handle.IsDone) return handle.Result;

        // ���� �ȳ������� key �״�� ��ȯ (Ȥ�� �⺻��)
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
