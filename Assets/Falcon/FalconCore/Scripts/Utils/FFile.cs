using System;
using System.IO;
using System.Text;
using Falcon.FalconCore.Scripts.Logs;
using FalconNetSdk.Scripts.Bigdata.Encrypts.Aes;
using UnityEngine;
using Object = System.Object;

namespace Falcon.FalconCore.Scripts.Utils
{
    /// <summary>
    ///     Saves, loads and deletes all data in the game
    /// </summary>
    
    public class FFile
    {
        private byte[] encryptKey = null;
        private static string _persistentDataPath;

        public static string PersistentDataPath
        {
            get
            {
                if (_persistentDataPath == null)
                {
                    OnStart();
                }
        
                return _persistentDataPath;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnStart()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            _persistentDataPath = Application.streamingAssetsPath ?? "";
#else
            _persistentDataPath = Application.persistentDataPath ?? "";
#endif
            CoreLogger.Instance.Info("FFile init complete");
        }
        
        private readonly string fileName;

        public FFile(string fileName)
        {
            this.fileName = fileName;
        }

        public bool Exists()
        {
            return File.Exists(GetFilePath());
        }

        /// <summary>
        ///     Save data to a file (overwrite completely)
        /// </summary>
        public void Save(object data)
        {
            Save(JsonUtil.ToJson(data));
        }

        public void Save(String data)
        {
            var filePath = GetFilePath();
            try
            {
                using (var writer = File.Create(filePath))
                {
                    writer.Write( Encrypt(data));
                }
            }
            catch (Exception e)
            {
                // write out error here
                CoreLogger.Instance.Error("Failed to save data to: " + filePath);
                CoreLogger.Instance.Error(e);
            }
        }
        
        public void Append(Object data)
        {
            Append(JsonUtil.ToJson(data));
        }

        public void Append(String data)
        {
            var filePath = GetFilePath();
            try
            {
                String concat = Load() + data;
                Save(concat);
            }
            catch (Exception e)
            {
                // write out error here
                CoreLogger.Instance.Error("Failed to save data to: " + filePath);
                CoreLogger.Instance.Error(e);
            }
        }

        /// <summary>
        ///     Load all data at a specified file and folder location
        /// </summary>
        /// <returns></returns>
        public T Load<T>()
        {
            try
            {
                return JsonUtil.FromJson<T>(Load());
            }
            catch (Exception e)
            {
                CoreLogger.Instance.Warning("Failed to load file from: " + GetFilePath() + ". If this is the first time you run game with SDK, ignore this exception. If this is not the first time, something is definitely wrong (._.|||)");
                CoreLogger.Instance.Warning(e);
                return default(T);
            }
        }
        
        /// <summary>
        ///     Load all data at a specified file and folder location
        /// </summary>
        /// <returns></returns>
        public String Load()
        {
            try
            {
                return Decrypt(File.ReadAllBytes(GetFilePath()));
            }
            catch (Exception e)
            {
                CoreLogger.Instance.Warning("Failed to load file from: " + GetFilePath());
                CoreLogger.Instance.Warning(e);
                return null;
            }
        }
        /// <summary>
        ///     Load all data at a specified file and folder location
        /// </summary>
        /// <returns></returns>
        public String LoadRaw()
        {
            try
            {
                return File.ReadAllText(GetFilePath());
            }
            catch (Exception e)
            {
                CoreLogger.Instance.Warning("Failed to load file from: " + GetFilePath());
                CoreLogger.Instance.Warning(e);
                return null;
            }
        }
        
        /// <summary>
        ///     Create file path for where a file is stored on the specific platform given a folder name and file name
        /// </summary>
        /// <returns></returns>
        private string GetFilePath()
        {
            var result = fileName.EndsWith(".txt") ? Path.Combine(PersistentDataPath, "data", fileName) : Path.Combine(PersistentDataPath, "data", fileName + ".txt");
            string directory = Path.GetDirectoryName(result);
            if (!Directory.Exists(directory) && directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            return result;
        }
        
        public void Delete()
        {
            File.Delete(GetFilePath());
        }

        private byte[] Encrypt(String data)
        {
            encryptKey ??= AesService.GenerateAesKey();
            var encrypted = AesService.Encrypt(encryptKey, Encoding.ASCII.GetBytes(data)).Concat();
            byte[] keyLength = BitConverter.GetBytes(encryptKey.Length);
            return Concat(Concat(keyLength, encryptKey), encrypted);
        }
        
        private String Decrypt(byte[] data)
        {
            int keyLength = BitConverter.ToInt32(SubArray(data, 0, 4));
            encryptKey = SubArray(data, 4, keyLength);
            return Encoding.ASCII.GetString(AesService.Decrypt(encryptKey, new AesEncrypted(SubArray(data, 4+keyLength, data.Length -4-keyLength))));
        }

        private static T[] SubArray<T>(T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        
        private static T[] Concat<T>(T[] x, T[] y)
        {
            var z = new T[x.Length + y.Length];
            x.CopyTo(z, 0);
            y.CopyTo(z, x.Length);
            return z;
        }
    }
}