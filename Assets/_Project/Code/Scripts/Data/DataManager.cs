using System;
using System.Collections;
using System.IO;
using MoreMountains.Tools;
using NamPhuThuy.Common;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    /*
ScriptableObjects for:
- Static game configuration
- Level design data
- Item/weapon/character definitions
- Anything that needs Unity asset references

JSON for:
- Player progress/saves
- User settings
- Dynamic content updates
- Anything that needs to change after build
*/

    public class DataManager : Singleton<DataManager>
    {
        #region Serializable Fields

        [SerializeField] private IntVariable currentLevel;
        public IntVariable CurrentLevel => currentLevel;
        [SerializeField] private UserResourcesObserver userResourcesObserver;
        public UserResourcesObserver UserResourcesObserver => userResourcesObserver;

        [Header("IAP")] 
        [SerializeField] private IAPPackageData removeAdsPackIAP;

        public IAPPackageData RemoveAdsPackIAP => removeAdsPackIAP;

        #endregion
        
        #region Private Fields

        

        private Coroutine _saveDebounce;

        
        #endregion

        #region Mutable Datas

        [Header("Mutable Datas")]
        private string _savePlayerDataPath;
        private string _saveSettingsDataPath;
        private string _saveGalleryDataPath;

        private bool _isPlayerDataLoaded;
        private bool _isSettingsDataLoaded;
        private bool _isGalleryDataLoaded;

        [SerializeField] private PlayerData cachedPlayerData;

        public PlayerData PlayerData // Lazy loading - only load when needed
        {
            get
            {
                if (!_isPlayerDataLoaded)
                {
                    _isPlayerDataLoaded = true;
                    LoadPlayerData();
                }

                return cachedPlayerData;
            }
        }


        [SerializeField] private PlayerSettingsData cachedPlayerSettingsData;

        public PlayerSettingsData PlayerSettingsData
        {
            get
            {
                if (!_isSettingsDataLoaded)
                {
                    LoadSettingsData();
                    _isSettingsDataLoaded = true;
                }

                return cachedPlayerSettingsData;
            }
        }


        [SerializeField] private PlayerGalleryData cachedPlayerGalleryData;

        public PlayerGalleryData PlayerGalleryData
        {
            get
            {
                // if (!_isGalleryDataLoaded)
                if (cachedPlayerGalleryData == null)
                {
                    LoadGalleryData();
                }
                
                return cachedPlayerGalleryData;
            }
        }

        #endregion




        #region Immutable Datas

        [Header("Immutable Datas")]
    
        [SerializeField] private PictureDatas pictureDatas;

        public PictureDatas PictureDatas
        {
            get
            {
                if (pictureDatas == null)
                {
                    pictureDatas = Resources.Load<PictureDatas>("PictureDatas_temp");
                }

                return pictureDatas;
            }

            set => pictureDatas = value;
        }

        [SerializeField] private GalleryPackData galleryPackData;

        public GalleryPackData GalleryPackData
        {
            get
            {
                if (galleryPackData == null)
                {
                    galleryPackData = Resources.Load<GalleryPackData>("GalleryPackDatas");
                }

                return galleryPackData;
            }
        }

        [SerializeField] private RewardDatas rewardDatas;

        public RewardDatas RewardDatas
        {
            get
            {
                if (rewardDatas == null)
                {
                    rewardDatas = Resources.Load<RewardDatas>("ServiceDatas");
                }

                return rewardDatas;
            }
        }
        
        [SerializeField] private ResourceData resourceData;

        public ResourceData ResourceData
        {
            get
            {
                if (resourceData == null)
                {
                    resourceData = Resources.Load<ResourceData>("ResourceData");
                }

                return resourceData;
            }
        }
        
        [SerializeField] private ServiceData serviceData;

        public ServiceData ServiceData
        {
            get
            {
                if (serviceData == null)
                {
                    serviceData = Resources.Load<ServiceData>("ServiceData");
                }

                return serviceData;
            }
        }

        #endregion


        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();
            // Debug.Log($"path: {Application.persistentDataPath}");
            _savePlayerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";
            _saveSettingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            _saveGalleryDataPath = $"{Application.persistentDataPath}/gallery.{DataConst.DATA_FILES_EXTENSION}";

            LoadData();
        }

        private void OnEnable()
        {
            MMEventManager.RegisterAllCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregisterAllCurrentEvents(this);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveData();
        }

        private void OnApplicationQuit()
        {
            SaveData();
        }

        #endregion

        #region Private Methods

        public void SavePlayerData()
        {
            _savePlayerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";

            //example: origin = "{"name":"NamTrinh","level":12,"currentExpPoint":31.0}"
            string origin = JsonUtility.ToJson(cachedPlayerData);
            string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);


            File.WriteAllText(_savePlayerDataPath, encrypted);
        }

        public void SaveSettingsData()
        {
            _saveSettingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPlayerSettingsData);
            string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_saveSettingsDataPath, encrypted);
        }

        public void SaveGalleryData()
        {
            // Debug.Log($"DataManager.SaveGalleryData()");
            _saveGalleryDataPath = $"{Application.persistentDataPath}/gallery.{DataConst.DATA_FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPlayerGalleryData);
            string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_saveGalleryDataPath, encrypted);
        }

        private void LoadPlayerData()
        {
            _savePlayerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";
            if (File.Exists(_savePlayerDataPath))
            {
                try
                {
                    /*
                 File.ReadAllText(_savePath) reads from disk
                 Disk operations are significantly slower than memory operations
                 Can cause frame drops if called during gameplay
                 */
                    string data = File.ReadAllText(_savePlayerDataPath);

                    if (!string.IsNullOrEmpty(data))
                    {
                        //Large string operations can be memory and CPU intensive
                        string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                        cachedPlayerData = JsonUtility.FromJson<PlayerData>(decrypted);    
                    }
                    else
                    {
                        if (cachedPlayerData == null)
                            ResetPlayerData();
                    }

                    
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetPlayerData();
                }
            }
            else
                ResetPlayerData();
        }

        private void LoadSettingsData()
        {
            _saveSettingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            if (File.Exists(_saveSettingsDataPath))
            {
                try
                {
                    string data = File.ReadAllText(_saveSettingsDataPath);

                    if (!string.IsNullOrEmpty(data))
                    {
                        string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                        cachedPlayerSettingsData = JsonUtility.FromJson<PlayerSettingsData>(decrypted);
                    }
                    else
                    {
                        if(cachedPlayerSettingsData == null)
                            ResetSettingsData();
                    }
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetSettingsData();
                }
            }
            else
                ResetSettingsData();
        }

        private void LoadGalleryData()
        {
            DebugLogger.Log(message:$"Load Data", context:this);
            _saveGalleryDataPath = $"{Application.persistentDataPath}/gallery.{DataConst.DATA_FILES_EXTENSION}";
            
            
            if (File.Exists(_saveGalleryDataPath))
            {
                
                DebugLogger.Log(message:$"{cachedPlayerGalleryData}", context:this);
                try
                {
                    string data = File.ReadAllText(_saveGalleryDataPath);

                    if (!string.IsNullOrEmpty(data))
                    {
                        string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                        
                        DebugLogger.Log(message:$"data: {decrypted}", context:this);
                        cachedPlayerGalleryData = JsonUtility.FromJson<PlayerGalleryData>(decrypted);
                        _isGalleryDataLoaded = true;
                    }
                    else
                    {
                        if(cachedPlayerGalleryData == null)
                            ResetGalleryData();
                        else
                        {
                            DebugLogger.Log(message:$"Not null", context:this);
                        }
                    }
                }
                catch (Exception e)
                {
                    ResetGalleryData();
                }
            }
            else
                ResetGalleryData();

            
        }

        public void ResetPlayerData()
        {
            DebugLogger.Log(message:$"");
            cachedPlayerData = new PlayerData();
            SavePlayerData();
        }

        public void ResetSettingsData()
        {
            DebugLogger.Log(message:$"");
            cachedPlayerSettingsData = new PlayerSettingsData();
            SaveSettingsData();
        }

        public void ResetGalleryData()
        {
            DebugLogger.Log(message:$"");
            // Debug.Log($"DataManager.LoadResetData()");
            cachedPlayerGalleryData = new PlayerGalleryData();
            _isGalleryDataLoaded = true;

            cachedPlayerGalleryData.AddNewPicture(DataConst.FREE_PICTURE_ID);
            // Debug.Log($"DataManager.LoadResetData() - Add new picture");
            
            SaveGalleryData();
        }

        #endregion


        #region Data Management

        public void MarkDirty()
        {
            if (_saveDebounce != null) StopCoroutine(_saveDebounce);
            _saveDebounce = StartCoroutine(SaveAfterDelay(DataConst.SAVE_INTERVAL));
        }

        private IEnumerator SaveAfterDelay(float delay)
        {
            yield return YieldHelper.GetRealtime(delay);
            SaveData();
            _saveDebounce = null;
        }

        public void ResetData()
        {
            ResetGalleryData();
            ResetPlayerData();
            ResetSettingsData();

            // gameData.dataResourcesInGame = JsonUtility.FromJson<DataResources>(Resources.Load<TextAsset>("Data/data_resources").text).dataResources;
            // SaveDataAsync().ConfigureAwait(false);
        }

        public void SaveData(bool postData = true)
        {
            SavePlayerData();
            SaveSettingsData();
            SaveGalleryData();
        }

        public void LoadData()
        {
            // Debug.Log($"DataManager.LoadData()");
            LoadPlayerData();
            LoadSettingsData();

            cachedPlayerGalleryData = null; //? Why this data is not null before load-process
            LoadGalleryData();
        }

        #endregion

#if UNITY_EDITOR
        public bool LoadStaticDatas()
        {
            pictureDatas = Resources.Load<PictureDatas>("PictureDatas");
            galleryPackData = Resources.Load<GalleryPackData>("GalleryPackData");
            rewardDatas = Resources.Load<RewardDatas>("RewardDatas");
            resourceData = Resources.Load<ResourceData>("ResourceData");

            /*// Print all Resources folders to help debug
        Object[] resourcesFolders = Resources.FindObjectsOfTypeAll<DefaultAsset>();
        foreach (Object folder in resourcesFolders) {
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folder))) {
                Debug.Log("Resources Folder: " + AssetDatabase.GetAssetPath(folder));
            }
        }*/

            return true;
        }
#endif

        #region Event Listen

        #endregion


    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataManager), true)]
    public class DataManagerInspector : Editor
    {
        private DataManager dataManager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            dataManager = (DataManager)target;

            //Interact with dynamic datas
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Data")) dataManager.SaveData();
            if (GUILayout.Button("Load Data")) dataManager.LoadData();
            if (GUILayout.Button("Reset Data")) dataManager.ResetData();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Player Data")) dataManager.SavePlayerData();
            if (GUILayout.Button("Save Settings Data")) dataManager.SaveSettingsData();
            if (GUILayout.Button("Save Gallery Data")) dataManager.SaveGalleryData();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Player Data")) dataManager.ResetPlayerData();
            if (GUILayout.Button("Reset Settings Data")) dataManager.ResetSettingsData();
            if (GUILayout.Button("Reset Gallery Data")) dataManager.ResetGalleryData();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Static Datas"))
            {
                if (!dataManager.LoadStaticDatas())
                    Debug.LogError($"Load statics datas failed");
                else
                    Debug.LogError($"Load statics successfully");
            }

            GUILayout.EndHorizontal();
        }
    }
#endif

    public enum ResourceType
    {
        COIN = 0,
        BOOSTER = 1,
        PICTURE = 2,
        NO_ADS = 5,
        BOOSTER_UNDO = 6,
        BOOSTER_MAGIC_PICK = 7,
        BOOSTER_SHUFFLE = 8,
    }

    public enum BoosterType
    {
        UNDO = 0,
        MAGIC_PICK = 1,
        SHUFFLE = 2,
        DOUBLE_WIN_BOOSTER = 3,

        NONE = 99,
    }

    public enum CurrencyType
    {
        COIN = 0,
        ADS = 1
    }

    public enum ServiceType
    {
        DOWNLOAD_IMAGE = 0,
        DOWNLOAD_VIDEO = 1
    }
}