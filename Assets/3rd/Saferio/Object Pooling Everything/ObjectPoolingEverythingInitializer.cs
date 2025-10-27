using System.Collections;
using NamPhuThuy.GamePlay;
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

    [Header("Sounds")]
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
    [SerializeField] private GameObject chainBreakSoundPrefab;
    [SerializeField] private GameObject rustyBreakSoundPrefab;
    [SerializeField] private GameObject completeTaskSoundPrefab;
    [SerializeField] private GameObject generateTaskSoundPrefab;


    [Header("VFXs")]
    [SerializeField] private GameObject destroyObjectPartFxPrefab;
    [SerializeField] private GameObject looseScrewFxPrefab;
    [SerializeField] private GameObject boxCompletedFxPrefab;
    [SerializeField] private GameObject supriseRewardFxPrefab;
    [SerializeField] private GameObject eventItem;
    [SerializeField] private GameObject lockedScrewBox;

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
        StartCoroutine(DelayInit());
    }

    private IEnumerator DelayInit()
    {
        yield return new WaitForSeconds(6);

        if (!_isInitialized)
        {
            InitPool();
        }
    }

    private void InitPool()
    {
        RegisterPool<ScrewBox>(GameConstants.SCREW_BOX, screwBox, screwBoxContainer, 5);
        RegisterPool<ScrewBoxSlot>(GameConstants.SCREW_PORT_SLOT, screwPortPrefab, 3);
        RegisterPool<FakeScrew>(GameConstants.FAKE_SCREW, fakeScrewPrefab, 3);
        RegisterPool<BoosterHammer>(GameConstants.HAMMER, hammerPrefab, 3);


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
        RegisterPool<AudioSource>(GameConstants.CHAIN_BREAK_SOUND, chainBreakSoundPrefab, 1);
        RegisterPool<AudioSource>(GameConstants.RUSTY_BREAK_SOUND, rustyBreakSoundPrefab, 1);
        RegisterPool<AudioSource>(GameConstants.COMPlETE_TASK_SOUND, completeTaskSoundPrefab, 1);
        RegisterPool<AudioSource>(GameConstants.GENERATE_TASK_SOUND, generateTaskSoundPrefab, 1);


        RegisterPool<TaskItemUI>(GameConstants.TASK_ITEM_UI, taskItemUIPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.DESTROY_OBJECT_PART_FX, destroyObjectPartFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX, looseScrewFxPrefab, 25);
        RegisterPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX, boxCompletedFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.SUPRISE_REWARD_FX, supriseRewardFxPrefab, 1);
        RegisterPool<EventItem>(GameConstants.EVENT_ITEM, eventItem, 5);
        RegisterPool<ScrewBoxLocked>(GameConstants.SCREW_BOX_LOCKED, lockedScrewBox, 2);
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
