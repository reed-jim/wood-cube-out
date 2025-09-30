using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NamPhuThuy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class DataUtility
{
    // use to save interface or abstract class
    private static JsonSerializerSettings settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public static void SaveWithReferenceLoopHandling<T>(string fileName, string key, T data)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        string fileText = "{}";

        if (File.Exists(filePath))
        {
            fileText = File.ReadAllText(filePath);
        }

        try
        {
            JObject json = JObject.Parse(fileText);

            if (json.ContainsKey(key))
            {
                json[key] = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                // json[key] = JsonUtility.ToJson(data);
            }
            else
            {
                json.Add(key, JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
                // json.Add(key, JsonUtility.ToJson(data));
            }

            File.WriteAllText(filePath, json.ToString());
        }
        catch (Exception e)
        {
            // Debug.Log("SAFERIO " + e.Message);
        }
    }

    public static void SaveWithReferenceLoopHandling<T>(string key, T data)
    {
        SaveWithReferenceLoopHandling(GameConstants.SAVE_FILE_NAME, key, data);
    }

    public static void Save<T>(string fileName, string key, T data)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        string fileText = "{}";

        int attempt = 0;
        int maxRetries = 3;

        while (attempt < maxRetries)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    fileText = File.ReadAllText(filePath);
                }

                JObject json = JObject.Parse(fileText);

                if (json.ContainsKey(key))
                {
                    json[key] = JsonConvert.SerializeObject(data);
                    // json[key] = JsonUtility.ToJson(data);
                }
                else
                {
                    json.Add(key, JsonConvert.SerializeObject(data));
                    // json.Add(key, JsonUtility.ToJson(data));
                }

                File.WriteAllText(filePath, json.ToString());

                break;
            }
            catch (Exception e)
            {
                attempt++;
                DebugLogger.LogSimple(message:$"{e}");
            }
        }
    }

    public static void Save<T>(string key, T data)
    {
        Save(GameConstants.SAVE_FILE_NAME, key, data);
    }

    public async static Task SaveAsync<T>(string fileName, string key, T data)
    {
        Save(fileName, key, data);

        // string filePath = Application.persistentDataPath + $"/{fileName}.json";

        // string fileText = "{}";

        // if (File.Exists(filePath))
        // {
        //     fileText = await File.ReadAllTextAsync(filePath);
        // }

        // JObject json = JObject.Parse(fileText);

        // if (json.ContainsKey(key))
        // {
        //     json[key] = JsonConvert.SerializeObject(data, settings);
        // }
        // else
        // {
        //     json.Add(key, JsonConvert.SerializeObject(data, settings));
        // }

        // await File.WriteAllTextAsync(filePath, json.ToString());
    }

    public static async Task SaveAsync<T>(string key, T data)
    {
        Save(key, data);

        // await SaveAsync(GameConstants.SAVE_FILE_NAME, key, data);
    }

    public static T Load<T>(string fileName, string key, T defaultValue)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        try
        {
            if (File.Exists(filePath))
            {
                string data = File.ReadAllText(filePath);

                if (data != "")
                {
                    JObject json = JObject.Parse(data);

                    if (json.ContainsKey(key))
                    {
                        return JsonConvert.DeserializeObject<T>(json[key].ToString());
                    }
                    else
                    {
                        return defaultValue;
                    }
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
        catch (Exception e)
        {
            
        }

        return defaultValue;
    }

    public static T Load<T>(string key, T defaultValue)
    {
        return Load(GameConstants.SAVE_FILE_NAME, key, defaultValue);
    }

    public static async Task<T> LoadAsync<T>(string fileName, string key, T defaultValue)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        if (File.Exists(filePath))
        {
            string data = await File.ReadAllTextAsync(filePath);

            if (data != "")
            {
                JObject json = JObject.Parse(data);

                if (json.ContainsKey(key))
                {
                    return JsonConvert.DeserializeObject<T>(json[key].ToString());
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }
        else
        {
            return defaultValue;
        }
    }

    public static T LoadAsync<T>(string key, T defaultValue)
    {
        return Load(GameConstants.SAVE_FILE_NAME, key, defaultValue);
    }

    public static void RemoveKey(string key)
    {
        string filePath = Application.persistentDataPath + $"/{GameConstants.SAVE_FILE_NAME}.json";

        string fileText = "{}";

        int attempt = 0;
        int maxRetries = 3;

        while (attempt < maxRetries)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    fileText = File.ReadAllText(filePath);
                }

                JObject json = JObject.Parse(fileText);

                if (json.ContainsKey(key))
                {
                    json.Remove(key);
                }

                File.WriteAllText(filePath, json.ToString());

                break;
            }
            catch (Exception e)
            {
                attempt++;
            }
        }
    }
}
