using System.Collections;
using AntsFlow;
using NamPhuThuy.GamePlay;
using UnityEngine;

public class ObjectPoolingEverythingInitializer : MonoBehaviour
{
    [Header("Chicken Feed")]
    [SerializeField] private GameObject baseChicken;
    [SerializeField] private GameObject waitingChickenSlot;
    [SerializeField] private GameObject grainPile;
    
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
    [SerializeField] private GameObject coinSoundPrefab;
    [SerializeField] private GameObject antPopSoundPrefab;
    [SerializeField] private GameObject boxTapSoundPrefab;
    [SerializeField] private GameObject cubeEatSoundPrefab;
    [SerializeField] private GameObject cubeLandSoundPrefab;
    [SerializeField] private GameObject slotDoneSoundPrefab;
    [SerializeField] private GameObject tileHitGroundSoundPrefab;
    [SerializeField] private GameObject startFreezeSoundPrefab;
    [SerializeField] private GameObject iceBreakSoundPrefab;
    [SerializeField] private GameObject boosterAppearSoundPrefab;

    [Header("VFXs")]
    [SerializeField] private GameObject destroyObjectPartFxPrefab;
    [SerializeField] private GameObject looseScrewFxPrefab;
    [SerializeField] private GameObject boxCompletedFxPrefab;
    [SerializeField] private GameObject supriseRewardFxPrefab;
    [SerializeField] private GameObject eventItem;
    [SerializeField] private GameObject lockedScrewBox;

    [SerializeField] private GameObject taskItemUIPrefab;

    [Header("Ant Out")]
    [SerializeField] private GameObject baseTile;
    [SerializeField] private GameObject fakeTilePrefab;
    [SerializeField] private GameObject tilemapPrefab;

    [Header("Washing Machine")]
    public GameObject washingMachineItemPrefab;
    public GameObject clothingPiecePrefab;
    public GameObject pathSlotPrefab;
    public GameObject antPrefab;
    public GameObject warriorAntPrefab;
    public GameObject snakeAntPrefab;
    public GameObject antDissapearFxPrefab;
    public GameObject antBoxLink;
    public GameObject hiddenAntBoxPrefab;
    public GameObject freezedAntBoxPrefab;
    public GameObject warriorAntBoxPrefab;
    public GameObject snakeAntBoxPrefab;
    public GameObject tileRemainingPrefab;
    public GameObject missilePrefab;
    public GameObject damageTextPrefab;

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
        // StartCoroutine(DelayInit());
        if (!_isInitialized)
        {
            InitPool();
        }
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
        RegisterPool<BaseChicken>(GameConstants.chicken, baseChicken, 500);
        RegisterPool<WaitingChickenSlot>(GameConstants.waitingChickenSlot, waitingChickenSlot, 50);
        RegisterPool<GrainPile>(GameConstants.grainPile, grainPile, 20);



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
        RegisterPool<AudioSource>(GameConstants.collectCoinSound, coinSoundPrefab, 24);
        RegisterPool<AudioSource>(GameConstants.antPopSound, antPopSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.boxTapSound, boxTapSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.cubeEatSound, cubeEatSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.cubeLandSound, cubeLandSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.slotDoneSound, slotDoneSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.tileHitGroundSound, tileHitGroundSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.startFreezeSound, startFreezeSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.iceBreakSound, iceBreakSoundPrefab, 8);
        RegisterPool<AudioSource>(GameConstants.boosterAppearSound, boosterAppearSoundPrefab, 8);

        RegisterPool<TaskItemUI>(GameConstants.TASK_ITEM_UI, taskItemUIPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.DESTROY_OBJECT_PART_FX, destroyObjectPartFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX, looseScrewFxPrefab, 25);
        RegisterPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX, boxCompletedFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.SUPRISE_REWARD_FX, supriseRewardFxPrefab, 1);
        RegisterPool<EventItem>(GameConstants.EVENT_ITEM, eventItem, 5);
        RegisterPool<ScrewBoxLocked>(GameConstants.SCREW_BOX_LOCKED, lockedScrewBox, 2);

        // RegisterPool<RescueStory.BaseTile>(GameConstants.tile, baseTile, 100);
        // RegisterPool<AntsFlow.BaseTile>(GameConstants.tile, baseTile, 1600);

        RegisterPool<WashingMachineItem>(GameConstants.washingMachine, washingMachineItemPrefab, 50);
        RegisterPool<ClothingPiece>(GameConstants.clothingPiece, clothingPiecePrefab, 50);
        RegisterPool<PathSlot>(GameConstants.pathSlot, pathSlotPrefab, 50);
        RegisterPool<BaseAnt>(GameConstants.ant, antPrefab, 200);
        RegisterPool<BaseAnt>(GameConstants.warriorAnt, warriorAntPrefab, 50);
        RegisterPool<BaseAnt>(GameConstants.snakeAnt, snakeAntPrefab, 50);
        RegisterPool<ParticleSystem>(GameConstants.antDissapearFxPrefab, antDissapearFxPrefab, 5);
        RegisterPool<AntBoxLink>(GameConstants.antBoxLink, antBoxLink, 5);
        RegisterPool<HiddenAntBox>(GameConstants.hiddenAntBox, hiddenAntBoxPrefab, 20);
        RegisterPool<FreezedAntBox>(GameConstants.freezedAntBox, freezedAntBoxPrefab, 20);
        RegisterPool<WarriorAntBox>(GameConstants.warriorAntBox, warriorAntBoxPrefab, 5);
        RegisterPool<SnakeAntBox>(GameConstants.snakeAntBox, snakeAntBoxPrefab, 5);
        RegisterPool<TileRemaining>(GameConstants.tileRemaining, tileRemainingPrefab, 30);
        RegisterPool<FakeTile>(GameConstants.fakeTile, fakeTilePrefab, 200);
        RegisterPool<Transform>(GameConstants.tileMap, tilemapPrefab, 40);
        RegisterPool<Transform>(GameConstants.missile, missilePrefab, 50);
        RegisterPool<DamageText>(GameConstants.damageText, damageTextPrefab, 10);
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
