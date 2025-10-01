using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
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