using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Threading;

public static class CSVReader
{
    //static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    //static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    //static char[] TRIM_CHARS = { '\"', ',' };

#if UNITY_EDITOR
    // only use in editor
    public static List<Dictionary<string, object>> ReadCSVInEditor(string filePath)
    {
        // 파일이 존재하는지 확인
        //if (!File.Exists(filePath))
        //{
        //    Debug.LogWarning($"File not found at path: {filePath}");
        //    return null;
        //}

        //// 파일의 모든 텍스트를 읽어 TextAsset으로 변환
        ////string fileContent = File.ReadAllText(filePath);
        //string fileContent = File.ReadAllText(filePath);

        //List<Dictionary<string, object>> value = ReadCSV(fileContent);

        //return value;

        Encoding enc = Encoding.UTF8;
        int retries = 5;
        int delayMs = 120;
        string fileContent = "";

        // Excel 강한 잠금 대책용 retries
        for (int i = 0; i < retries; i++)
        {
            try
            {
                // using을 사용해서 실패해도, file 잠금 되돌아가도록
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete); // 공유 최대 허용
                using var sr = new StreamReader(fs, enc, true);
                fileContent = sr.ReadToEnd();
            }
            catch (IOException)
            {
                if (i == retries - 1) throw; // 마지막 시도에서도 실패면 던짐
                Thread.Sleep(delayMs);
            }
        }

        if (fileContent == "") return null;
        List<Dictionary<string, object>> value = ReadCSV(fileContent);
        return value;

        //return null;
    }
#endif

    public static void ReadByAddressablePath(string path, Action<List<Dictionary<string, object>>> callback)
    {
        Addressables.LoadAssetAsync<TextAsset>(path).Completed += (AsyncOperationHandle<TextAsset> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                TextAsset csvText = handle.Result;
                List<Dictionary<string, object>> value = ReadCSV(csvText.text);

                callback.Invoke(value);
            }
            else
            {
                Debug.LogError("Failed to load CSV file from Addressables.");
            }
        };
    }

    public static async Task<List<Dictionary<string, object>>> ReadByAddressablePathAsync(string path)
    {
        var handle = Addressables.LoadAssetAsync<TextAsset>(path);
        try
        {
            var asset = await handle.Task; // await 후 handle.Result와 동일
            if (handle.Status != AsyncOperationStatus.Succeeded || asset == null)
            {
                Debug.LogError($"Failed to load CSV at {path} (status: {handle.Status})");
                return null; // 필요하면 빈 리스트로 대체
            }

            // 에셋에 대한 의존 끊기 (문자열만 복사)
            string text = asset.text;

            // 여기서 파싱
            var value = ReadCSV(text);
            return value;
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception while loading CSV at {path}\n{e}");
            return null;
        }
        finally
        {
            // 성공/실패 상관없이 해제 (핸들/의존 번들 정리)
            Addressables.Release(handle);
        }
    }

    public static List<Dictionary<string, object>> ReadCSVByTextAsset(TextAsset textAsset) => ReadCSV(textAsset.text);

    public static List<Dictionary<string, object>> ReadCSV(string textData)
    {
        var list = new List<Dictionary<string, object>>();
        using (var reader = new StringReader(textData))
        {
            string headerLine = reader.ReadLine();
            if (string.IsNullOrEmpty(headerLine)) return list;

            string[] headers = headerLine.Split(',');

            string line;
            while ((line = ReadFullLine(reader)) != null)
            {
                string[] fields = ParseCSVLine(line);
                var dict = new Dictionary<string, object>();

                for (int i = 0; i < headers.Length && i < fields.Length; i++)
                {
                    string value = fields[i].Trim();

                    if (int.TryParse(value, out int intVal))
                        dict[headers[i]] = intVal;
                    else
                        dict[headers[i]] = value;
                }

                list.Add(dict);
            }
        }

        return list;
    }

    // 줄바꿈이 포함된 필드를 감지해서 한 줄로 병합
    private static string ReadFullLine(StringReader reader)
    {
        string line = reader.ReadLine();
        if (line == null) return null;

        while (CountQuotes(line) % 2 != 0) // 따옴표가 홀수면 아직 안 닫힘
        {
            string nextLine = reader.ReadLine();
            if (nextLine == null) break;
            line += "\n" + nextLine;
        }

        return line;
    }

    private static int CountQuotes(string s) => s.Count(c => c == '"');

    // CSV 필드 파싱 (쉼표 구분 + 따옴표 제거)
    private static string[] ParseCSVLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var field = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"'); // 따옴표 이스케이프
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }

        result.Add(field.ToString());
        return result.ToArray();
    }
}
