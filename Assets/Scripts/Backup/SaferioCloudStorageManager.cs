using System;
using UnityEngine;

public class SaferioCloudStorageManager : MonoBehaviour
{
    public static event Action cloudStorageOperationCompleteEvent;

    void Awake()
    {
    }

    void OnDestroy()
    {
    }
}
