using System;
using System.IO;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class SaferioCloudStorageManager : MonoBehaviour
{
    #region PRIVATE FIELD
    private FirebaseStorage _storage;
    StorageReference _storageRef;
    #endregion

    #region EVENT
    public static event Action cloudStorageOperationCompleteEvent;
    #endregion

    void Awake()
    {
        RemoteConfigController.remoteConfigReadyEvent += Init;
        BackupPopup.uploadGameDataEvent += UploadDataInMemory;
        BackupPopup.downloadGameDataEvent += DownloadFile;
    }

    void OnDestroy()
    {
        RemoteConfigController.remoteConfigReadyEvent -= Init;
        BackupPopup.uploadGameDataEvent -= UploadDataInMemory;
        BackupPopup.downloadGameDataEvent -= DownloadFile;
    }

    private void Init()
    {
        _storage = FirebaseStorage.DefaultInstance;

        CreateReference();
    }

    private void CreateReference()
    {
        _storageRef = _storage.GetReferenceFromUrl("gs://wood-cube-out.firebasestorage.app/wood-cube-out");

        // StorageReference mountainsRef = _storageRef.Child("mountains.jpg");

        // StorageReference mountainImagesRef =
        //     _storageRef.Child("images/mountains.jpg");

        // Assert.AreEqual(mountainsRef.Name, mountainImagesRef.Name);
        // Assert.AreNotEqual(mountainsRef.Path, mountainImagesRef.Path);
    }

    private void UploadDataInMemory()
    {
        string userId = GamePersistentVariable.FirebaseUserId;

        string localFilePath = Application.persistentDataPath + $"/{GameConstants.SAVE_FILE_NAME}.json";
        string cloudFilePath = $"wood-cube-out/{userId}/game-data";

        try
        {
            byte[] fileBytes = File.ReadAllBytes(localFilePath);

            StorageReference gameDataRef = _storageRef.Child(cloudFilePath);

            gameDataRef.PutBytesAsync(fileBytes)
                .ContinueWith((Task<StorageMetadata> task) =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {

                    }
                    else
                    {
                        StorageMetadata metadata = task.Result;

                        string md5Hash = metadata.Md5Hash;

                        GamePersistentVariable.isCloudStorageOperationCompleted = true;

                        cloudStorageOperationCompleteEvent?.Invoke();
                    }
                });
        }
        catch (System.Exception e)
        {
            Debug.Log("SAFERIO " + e.Message);
        }
    }

    private void UploadFromLocalFile()
    {
        string userId = GamePersistentVariable.FirebaseUserId;

        string localFilePath = Application.persistentDataPath + $"/{GameConstants.SAVE_FILE_NAME}.json";
        string cloudFilePath = $"wood-cube-out/{userId}/game-data";

        try
        {
            string data = File.ReadAllText(localFilePath);

            StorageReference gameDataRef = _storageRef.Child(cloudFilePath);

            gameDataRef.PutFileAsync(localFilePath)
                .ContinueWith((Task<StorageMetadata> task) =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {

                    }
                    else
                    {
                        StorageMetadata metadata = task.Result;

                        string md5Hash = metadata.Md5Hash;

                        GamePersistentVariable.isCloudStorageOperationCompleted = true;

                        cloudStorageOperationCompleteEvent?.Invoke();
                    }
                });
        }
        catch (System.Exception e)
        {
            Debug.Log("SAFERIO " + e.Message);
        }
    }

    private void DownloadFile()
    {
        string userId = GamePersistentVariable.FirebaseUserId;

        string localFilePath = Application.persistentDataPath + $"/{GameConstants.SAVE_FILE_NAME}.json";
        string cloudFilePath = $"/wood-cube-out/{userId}/game-data";

        StorageReference pathReference =
            _storage.GetReference(cloudFilePath);

        pathReference.GetFileAsync(localFilePath).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                GamePersistentVariable.isCloudStorageOperationCompleted = true;

                cloudStorageOperationCompleteEvent?.Invoke();
            }
        });
    }
}
