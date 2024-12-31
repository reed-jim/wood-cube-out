using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class DataUtility
{
    // use to save interface or abstract class
    private static JsonSerializerSettings settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public static void Save<T>(string fileName, string key, T data)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        string fileText = "{}";

        if (File.Exists(filePath))
        {
            fileText = File.ReadAllText(filePath);
        }

        JObject json = JObject.Parse(fileText);

        if (json.ContainsKey(key))
        {
            json[key] = JsonConvert.SerializeObject(data, settings);
            // json[key] = JsonUtility.ToJson(data);
        }
        else
        {
            json.Add(key, JsonConvert.SerializeObject(data, settings));
            // json.Add(key, JsonUtility.ToJson(data));
        }

        File.WriteAllText(filePath, json.ToString());
    }

    public async static Task SaveAsync<T>(string fileName, string key, T data)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        string fileText = "{}";

        if (File.Exists(filePath))
        {
            fileText = await File.ReadAllTextAsync(filePath);
        }

        JObject json = JObject.Parse(fileText);

        if (json.ContainsKey(key))
        {
            json[key] = JsonConvert.SerializeObject(data, settings);
        }
        else
        {
            json.Add(key, JsonConvert.SerializeObject(data, settings));
        }

        await File.WriteAllTextAsync(filePath, json.ToString());
    }

    public static async void SaveAsync<T>(string key, T data)
    {
        await SaveAsync(GameConstants.SAVE_FILE_NAME, key, data);
    }

    public static T Load<T>(string fileName, string key, T defaultValue)
    {
        string filePath = Application.persistentDataPath + $"/{fileName}.json";

        if (File.Exists(filePath))
        {
            string data = File.ReadAllText(filePath);

            if (data != "")
            {
                JObject json = JObject.Parse(data);

                if (json.ContainsKey(key))
                {
                    return JsonConvert.DeserializeObject<T>(json[key].ToString(), settings);
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

    public static T Load<T>(string key, T defaultValue)
    {
        return Load(GameConstants.SAVE_FILE_NAME, key, defaultValue);
    }
}
