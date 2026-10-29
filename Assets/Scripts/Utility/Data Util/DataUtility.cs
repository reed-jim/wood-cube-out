using System;
using System.IO;
using System.Threading.Tasks;
using LiteDB;
using NamPhuThuy;
using Newtonsoft.Json;
using UnityEngine;

// Document model stored inside LiteDB collections
public class LiteDBRecord
{
    [BsonId]
    public string Key { get; set; }
    public string SerializedValue { get; set; }
}

public static class DataUtility
{
    private static string GetDatabasePath(string fileName)
    {
        string dir = Application.persistentDataPath;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, $"{fileName}.db");
    }

    public static void Save<T>(string fileName, string key, T data)
    {
        int attempt = 0;
        int maxRetries = 3;

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        string serializedData = JsonConvert.SerializeObject(data, settings: settings);
        string dbPath = GetDatabasePath(fileName);

        while (attempt < maxRetries)
        {
            try
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    var col = db.GetCollection<LiteDBRecord>("key_value_store");
                    
                    var record = new LiteDBRecord
                    {
                        Key = key,
                        SerializedValue = serializedData
                    };

                    // Upsert inserts if new, updates existing document by Key index automatically
                    col.Upsert(record);
                }
                break;
            }
            catch (Exception e)
            {
                attempt++;
                DebugLogger.LogSimple(message: $"{e}");
            }
        }
    }

    public static void Save<T>(string key, T data)
    {
        Save(GameConstants.SAVE_FILE_NAME, key, data);
    }

    public static void SaveToCurrentSaveSlot<T>(string key, T data)
    {
        Save(GameConstants.SAVE_FILE_NAME, SaveSlotManager.Ins.GetSaveKey(key), data);
    }

    public static async Task SaveAsync<T>(string key, T data)
    {
        await Task.Run(() => Save(key, data));
    }

    public static T Load<T>(string fileName, string key, T defaultValue)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        string dbPath = GetDatabasePath(fileName);

        try
        {
            if (File.Exists(dbPath))
            {
                using (var db = new LiteDatabase(dbPath))
                {
                    var col = db.GetCollection<LiteDBRecord>("key_value_store");
                    var record = col.FindById(key);

                    if (record != null && !string.IsNullOrEmpty(record.SerializedValue))
                    {
                        return JsonConvert.DeserializeObject<T>(record.SerializedValue, settings: settings);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{key} {e.Message}");
        }

        return defaultValue;
    }

    public static T Load<T>(string key, T defaultValue)
    {
        return Load(GameConstants.SAVE_FILE_NAME, key, defaultValue);
    }

    public static T LoadCurrentSaveSlot<T>(string key, T defaultValue)
    {
        return Load(GameConstants.SAVE_FILE_NAME, SaveSlotManager.Ins.GetSaveKey(key), defaultValue);
    }

    public static T LoadAsync<T>(string key, T defaultValue)
    {
        return Load(GameConstants.SAVE_FILE_NAME, key, defaultValue);
    }

    public static void RemoveKey(string key)
    {
        string fileName = GameConstants.SAVE_FILE_NAME;
        string dbPath = GetDatabasePath(fileName);
        int attempt = 0;
        int maxRetries = 3;

        while (attempt < maxRetries)
        {
            try
            {
                if (File.Exists(dbPath))
                {
                    using (var db = new LiteDatabase(dbPath))
                    {
                        var col = db.GetCollection<LiteDBRecord>("key_value_store");
                        col.Delete(key);
                    }
                }
                break;
            }
            catch (Exception e)
            {
                attempt++;
                DebugLogger.LogSimple(message: $"{e}");
            }
        }
    }
}