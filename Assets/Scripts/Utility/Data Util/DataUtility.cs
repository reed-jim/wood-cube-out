using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
    // Cache database connections to prevent opening/closing the file repeatedly
    private static readonly Dictionary<string, LiteDatabase> _dbCache = new Dictionary<string, LiteDatabase>();
    
    // Background write queue structures to batch and throttle rapid saves
    private struct PendingSave
    {
        public string FileName;
        public string Key;
        public string SerializedData;
    }

    private static readonly ConcurrentQueue<PendingSave> _writeQueue = new ConcurrentQueue<PendingSave>();
    private static readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
    private static CancellationTokenSource _cts;
    private static Task _backgroundWriterTask;

    static DataUtility()
    {
        StartBackgroundWriter();
    }

    private static void StartBackgroundWriter()
    {
        _cts = new CancellationTokenSource();
        _backgroundWriterTask = Task.Run(() => ProcessQueueAsync(_cts.Token));
    }

    private static async Task ProcessQueueAsync(CancellationToken token)
    {
        // Use a dictionary to batch/collapse multiple rapid saves for the same file/key into the latest value
        var batch = new Dictionary<(string fileName, string key), string>();

        while (!token.IsCancellationRequested)
        {
            try
            {
                // Wait until there's at least one item or timeout to flush periodically
                await _signal.WaitAsync(100, token);

                // Drain the queue into our batch dictionary
                while (_writeQueue.TryDequeue(out var item))
                {
                    batch[(item.FileName, item.Key)] = item.SerializedData;
                }

                if (batch.Count > 0)
                {
                    // Group by file name to minimize database session switches
                    var fileGroups = new Dictionary<string, List<KeyValuePair<string, string>>>();
                    foreach (var kvp in batch)
                    {
                        if (!fileGroups.TryGetValue(kvp.Key.fileName, out var list))
                        {
                            list = new List<KeyValuePair<string, string>>();
                            fileGroups[kvp.Key.fileName] = list;
                        }
                        list.Add(new KeyValuePair<string, string>(kvp.Key.key, kvp.Value));
                    }

                    batch.Clear();

                    foreach (var group in fileGroups)
                    {
                        WriteBatchToDatabase(group.Key, group.Value);
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                DebugLogger.LogSimple(message: $"Background writer error: {e}");
            }
        }
    }

    private static void WriteBatchToDatabase(string fileName, List<KeyValuePair<string, string>> records)
    {
        int attempt = 0;
        int maxRetries = 3;

        while (attempt < maxRetries)
        {
            try
            {
                var db = GetDatabase(fileName);
                var col = db.GetCollection<LiteDBRecord>("key_value_store");

                // Perform upserts inside a transaction or loop efficiently
                foreach (var rec in records)
                {
                    var record = new LiteDBRecord
                    {
                        Key = rec.Key,
                        SerializedValue = rec.Value
                    };
                    col.Upsert(record);
                }
                break;
            }
            catch (Exception e)
            {
                attempt++;
                DebugLogger.LogSimple(message: $"{e}");
                Thread.Sleep(10); // brief backoff before retry
            }
        }
    }

    private static string GetPhysicalFilePath(string fileName)
    {
        string dir = Application.persistentDataPath;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return Path.Combine(dir, $"{fileName}.db");
    }

    private static LiteDatabase GetDatabase(string fileName)
    {
        lock (_dbCache)
        {
            if (_dbCache.TryGetValue(fileName, out var existingDb) && existingDb != null)
            {
                return existingDb;
            }

            string path = GetPhysicalFilePath(fileName);
            string connectionString = $"Filename={path};Connection=Shared;";
            
            var db = new LiteDatabase(connectionString);
            _dbCache[fileName] = db;
            return db;
        }
    }

    public static void Save<T>(string fileName, string key, T data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        // Serialize off the main thread if possible, or quickly serialize and queue
        string serializedData = JsonConvert.SerializeObject(data, settings: settings);

        _writeQueue.Enqueue(new PendingSave
        {
            FileName = fileName,
            Key = key,
            SerializedData = serializedData
        });

        _signal.Release();
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
        // Fire-and-queue with background thread serialization option or wrap existing
        await Task.Run(() => Save(key, data));
    }

    public static T Load<T>(string fileName, string key, T defaultValue)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        string physicalPath = GetPhysicalFilePath(fileName);

        try
        {
            if (File.Exists(physicalPath))
            {
                var db = GetDatabase(fileName);
                var col = db.GetCollection<LiteDBRecord>("key_value_store");
                var record = col.FindById(key);

                if (record != null && !string.IsNullOrEmpty(record.SerializedValue))
                {
                    return JsonConvert.DeserializeObject<T>(record.SerializedValue, settings: settings);
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
        string physicalPath = GetPhysicalFilePath(fileName);
        int attempt = 0;
        int maxRetries = 3;

        while (attempt < maxRetries)
        {
            try
            {
                if (File.Exists(physicalPath))
                {
                    var db = GetDatabase(fileName);
                    var col = db.GetCollection<LiteDBRecord>("key_value_store");
                    col.Delete(key);
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        try
        {
            _cts?.Cancel();
            // Wait briefly for background writer to flush if needed
            _backgroundWriterTask?.Wait(500);
        }
        catch { }

        lock (_dbCache)
        {
            foreach (var db in _dbCache.Values)
            {
                db?.Dispose();
            }
            _dbCache.Clear();
        }
    }
}