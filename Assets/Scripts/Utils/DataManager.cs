using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

public class DataManager : MonoBehaviour
{
    // ========== Synchronous Methods (Legacy) ==========

    public static void SaveToFile(string filePath, string data)
    {
        // 데이터 암호화
        string encryptedData = FileIO.EncryptString(data);

        File.WriteAllText(Application.persistentDataPath + filePath, encryptedData);
    }

    public static void SaveToFile<T>(string filePath, T data)
    {
        string jsonData = JsonConvert.SerializeObject(data);

        // 데이터 암호화
        string encryptedData = FileIO.EncryptString(jsonData);

        File.WriteAllText(Application.persistentDataPath + filePath, encryptedData);
    }

    public static T LoadFromFile<T>(string filePath)
    {
        filePath = Application.persistentDataPath + filePath;

        if (File.Exists(filePath))
        {
            // 데이터 복호화
            string encryptedData = File.ReadAllText(filePath);
            string decryptedData = FileIO.DecryptString(encryptedData);

            T data = JsonConvert.DeserializeObject<T>(decryptedData);

            return data;
        }

        Debug.Log("File doesnt exist: " + filePath);

        return default(T); // 또는 적절한 기본값 반환
    }

    // ========== Asynchronous Methods (Recommended) ==========

    /// <summary>
    /// 비동기로 데이터를 파일에 저장합니다.
    /// CPU Bound 작업(직렬화, 암호화)과 I/O Bound 작업(파일 쓰기)을 모두 백그라운드 스레드에서 처리합니다.
    /// </summary>
    public static async UniTask SaveToFileAsync<T>(string filePath, T data)
    {
        string fullPath = Application.persistentDataPath + filePath;

        // CPU Bound(직렬화, 암호화) + I/O Bound(파일 쓰기)를 백그라운드 스레드로 이동
        await UniTask.RunOnThreadPool(() =>
        {
            // JSON 직렬화 (CPU Bound)
            string jsonData = JsonConvert.SerializeObject(data);

            // AES 암호화 (CPU Bound)
            string encryptedData = FileIO.EncryptString(jsonData);

            // 파일 쓰기 (I/O Bound)
            File.WriteAllText(fullPath, encryptedData);
        });
    }

    /// <summary>
    /// 비동기로 파일에서 데이터를 로드합니다.
    /// I/O Bound 작업(파일 읽기)과 CPU Bound 작업(복호화, 역직렬화)을 백그라운드 스레드에서 처리합니다.
    /// </summary>
    public static async UniTask<T> LoadFromFileAsync<T>(string filePath)
    {
        string fullPath = Application.persistentDataPath + filePath;

        if (!File.Exists(fullPath))
        {
            Debug.Log("File doesnt exist: " + fullPath);
            return default(T);
        }

        // I/O Bound(파일 읽기) + CPU Bound(복호화, 역직렬화)를 백그라운드 스레드로 이동
        return await UniTask.RunOnThreadPool(() =>
        {
            // 파일 읽기 (I/O Bound)
            string encryptedData = File.ReadAllText(fullPath);

            // AES 복호화 (CPU Bound)
            string decryptedData = FileIO.DecryptString(encryptedData);

            // JSON 역직렬화 (CPU Bound)
            return JsonConvert.DeserializeObject<T>(decryptedData);
        });
    }

    public static void RemoveFile(string filePath)
    {
        filePath = Application.persistentDataPath + filePath;

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log(filePath + " removed");
        }
        else
        {
            Debug.Log(filePath + " doesn't exist");
        }
    }
}