using UnityEngine;

public class ObjectPoolingEverythingInitializer : MonoBehaviour
{
    public static ObjectPoolingEverythingInitializer Instance;

    [Header("CONTAINER")]
    [SerializeField] private Transform screwBoxContainer;

    [SerializeField] private GameObject screwBox;
    [SerializeField] private GameObject screwPortPrefab;
    [SerializeField] private GameObject fakeScrewPrefab;
    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private GameObject loosenScrewSoundPrefab;
    [SerializeField] private GameObject tightenScrewSoundPrefab;
    [SerializeField] private GameObject loosenScrewFailSoundPrefab;
    [SerializeField] private GameObject screwBoxDoneSoundPrefab;
    [SerializeField] private GameObject breakObjectSoundPrefab;
    [SerializeField] private GameObject winSoundPrefab;
    [SerializeField] private GameObject loseSoundPrefab;
    [SerializeField] private GameObject clickSoundPrefab;
    [SerializeField] private GameObject closePopupSoundPrefab;
    [SerializeField] private GameObject unlockAdsScrewBoxSoundPrefab;
    [SerializeField] private GameObject clearScrewPortsSoundPrefab;
    [SerializeField] private GameObject countingTimeSoundPrefab;
    [SerializeField] private GameObject chestAppearSoundPrefab;
    [SerializeField] private GameObject openChestSoundPrefab;
    [SerializeField] private GameObject rewardItemSoundPrefab;
    // [SerializeField] private GameObject destroyObjectPartFxPrefab;
    [SerializeField] private GameObject looseScrewFxPrefab;
    [SerializeField] private GameObject boxCompletedFxPrefab;
    [SerializeField] private GameObject supriseRewardFxPrefab;
    [SerializeField] private GameObject eventItem;

    [SerializeField] private GameObject taskItemUIPrefab;

    private bool _isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            _isInitialized = true;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            InitPool();
        }
    }

    private void InitPool()
    {
        RegisterPool<AudioSource>(GameConstants.LOOSEN_SCREW_SOUND, loosenScrewSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.TIGHTEN_SCREW_SOUND, tightenScrewSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.LOOSEN_SCREW_FAIL_SOUND, loosenScrewFailSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.SCREW_BOX_DONE_SOUND, screwBoxDoneSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.BREAK_OBJECT_SOUND, breakObjectSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.WIN_SOUND, winSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.LOSE_SOUND, loseSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.CLICK_SOUND, clickSoundPrefab, 2);
        RegisterPool<AudioSource>(GameConstants.CLOSE_POPUP_SOUND, closePopupSoundPrefab, 2);
        RegisterPool<AudioSource>(GameConstants.UNLOCK_ADS_SCREW_BOX_SOUND, unlockAdsScrewBoxSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.CLEAR_SCREW_PORTS_SOUND, clearScrewPortsSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.COUNTING_TIME_SOUND, countingTimeSoundPrefab, 1);
        RegisterPool<AudioSource>(GameConstants.CHEST_APPEAR_SOUND, chestAppearSoundPrefab, 1);
        RegisterPool<AudioSource>(GameConstants.OPEN_CHEST_SOUND, openChestSoundPrefab, 1);
        RegisterPool<AudioSource>(GameConstants.REWARD_ITEM_SOUND, rewardItemSoundPrefab, 1);
        RegisterPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX, looseScrewFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX, boxCompletedFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.SUPRISE_REWARD_FX, supriseRewardFxPrefab, 1);
    }

    private void RegisterPool<T>(string key, GameObject prefab, int poolSize)
    {
        ObjectPool pool = new ObjectPool();

        pool.Init<T>(prefab, poolSize);

        ObjectPoolingEverything.AddToPool(key, pool);
    }

    private void RegisterPool<T>(string key, GameObject prefab, Transform parent, int poolSize)
    {
        ObjectPool pool = new ObjectPool();

        pool.Init<T>(prefab, parent, poolSize);

        ObjectPoolingEverything.AddToPool(key, pool);
    }
}
