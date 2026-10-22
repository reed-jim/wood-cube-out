using System.Collections;
using AntsFlow;
using NamPhuThuy.GamePlay;
using Spine.Unity;
using UnityEngine;

public class ObjectPoolingEverythingInitializer : MonoBehaviour
{
    [Header("Shooting Idle")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject stackingItemUI;
    [SerializeField] private GameObject nodeGroup;
    [SerializeField] private GameObject node;
    [SerializeField] private GameObject pendingAugmentItemUI;
    [SerializeField] private GameObject nodeDetailAugmentItemUI;
    [SerializeField] private GameObject augmentSetItemUI;
    [SerializeField] private GameObject augmentSetBuffItemUI;
    [SerializeField] private GameObject storedPowerCapsuleUI;
    [SerializeField] private GameObject notificationBadge;
    [SerializeField] private GameObject energyCube;
    [SerializeField] private GameObject applyAugmentToNodeEffect;
    [SerializeField] private GameObject applyAugmentToNodeGroupEffect;
    [SerializeField] private GameObject voyageMilestoneItemUI;
    [SerializeField] private GameObject collectResourceItemUI;
    [SerializeField] private GameObject baseTask;
    [SerializeField] private GameObject upgradeNodeTask;
    [SerializeField] private GameObject taskItemUI;
    [SerializeField] private GameObject rewardItemUI;
    [SerializeField] private GameObject leadershipCharacterUI;
    [SerializeField] private GameObject characterTraitItemUI;
    [SerializeField] private GameObject ingameEventItemUI;
    [SerializeField] private GameObject leadershipPeriodItemUI;
    [SerializeField] private GameObject electionItemUI;
    [SerializeField] private GameObject statItemUI;
    [SerializeField] private GameObject spaceEventItemUI;
    [SerializeField] private GameObject researchItemUI;
    [SerializeField] private GameObject UIRectLine;
    [SerializeField] private GameObject saveSlotItemUI;
    [SerializeField] private GameObject augmentIcon;

    [Header("Chicken Feed")]
    [SerializeField] private GameObject baseChicken;
    [SerializeField] private GameObject grainPile;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject chickenCompleteFx;
    [SerializeField] private GameObject smokeFx;
    [SerializeField] private GameObject smokeAnim;

    public static ObjectPoolingEverythingInitializer Instance;

    [Header("Sounds")]
    [SerializeField] private GameObject clickSoundPrefab;
    [SerializeField] private GameObject errorSoundPrefab;
    [SerializeField] private GameObject cycleCompleteSound;

    [Header("VFXs")]
    [SerializeField] private GameObject looseScrewFxPrefab;
    [SerializeField] private GameObject boxCompletedFxPrefab;
    [SerializeField] private GameObject supriseRewardFxPrefab;

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
        #region Shooting Idle
        RegisterPool<StackingItemUI>(GameConstants.stackingItemUI, stackingItemUI, 20);
        RegisterPool<NodeGroup>(GameConstants.nodeGroup, nodeGroup, 20);
        RegisterPool<BaseCharacter>(GameConstants.node, node, 50);
        RegisterPool<PendingAugmentItemUI>(GameConstants.pendingAugmentItemUI, pendingAugmentItemUI, 10);
        RegisterPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI, nodeDetailAugmentItemUI, 10);
        RegisterPool<AugmentSetItemUI>(GameConstants.augmentSetItemUI, augmentSetItemUI, 10);
        RegisterPool<AugmentSetBuffItemUI>(GameConstants.augmentSetBuffItemUI, augmentSetBuffItemUI, 30);
        RegisterPool<StoredPowerCapsuleUI>(GameConstants.storedPowerCapsuleUI, storedPowerCapsuleUI, 10);
        RegisterPool<NotificationBadge>(GameConstants.notificationBadge, notificationBadge, 10);
        RegisterPool<EnergyCube>(GameConstants.energyCube, energyCube, 100);
        RegisterPool<ApplyAugmentToNodeEffect>(GameConstants.applyAugmentToNodeEffect, applyAugmentToNodeEffect, 5);
        RegisterPool<ApplyAugmentToNodeGroupEffect>(GameConstants.applyAugmentToNodeGroupEffect, applyAugmentToNodeGroupEffect, 5);
        RegisterPool<VoyageMilestoneItemUI>(GameConstants.voyageMilestoneItemUI, voyageMilestoneItemUI, 10);
        RegisterPool<CollectResourceItemUI>(GameConstants.collectResourceItemUI, collectResourceItemUI, 10);

        RegisterPool<BaseTask>(GameConstants.baseTask, baseTask, 5);
        RegisterPool<UpgradeNodeTask>(GameConstants.upgradeNodeTask, upgradeNodeTask, 5);
        RegisterPool<TaskItemUI>(GameConstants.taskItemUI, taskItemUI, 10);
        RegisterPool<RewardItemUI>(GameConstants.rewardItemUI, rewardItemUI, 10);
        RegisterPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI, leadershipCharacterUI, 20);
        RegisterPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI, characterTraitItemUI, 20);
        RegisterPool<IngameEventItemUI>(GameConstants.ingameEventItemUI, ingameEventItemUI, 20);
        RegisterPool<LeadershipPeriodItemUI>(GameConstants.leadershipPeriodItemUI, leadershipPeriodItemUI, 20);
        RegisterPool<ElectionItemUI>(GameConstants.electionItemUI, electionItemUI, 20);
        RegisterPool<StatItemUI>(GameConstants.statItemUI, statItemUI, 20);
        RegisterPool<SpaceEventItemUI>(GameConstants.spaceEventItemUI, spaceEventItemUI, 20);
        RegisterPool<ResearchItemUI>(GameConstants.researchItemUI, researchItemUI, 20);
        RegisterPool<UIRectLine>(GameConstants.UIRectLine, UIRectLine, 20);
        RegisterPool<SaveSlotItemUI>(GameConstants.saveSlotItemUI, saveSlotItemUI, 20);
        RegisterPool<AugmentIcon>(GameConstants.augmentIcon, augmentIcon, 20);
        #endregion

        RegisterPool<AudioSource>(GameConstants.CLICK_SOUND, clickSoundPrefab, 2);
        RegisterPool<AudioSource>(GameConstants.errorSound, errorSoundPrefab, 2);
        RegisterPool<AudioSource>(GameConstants.cycleCompleteSound, cycleCompleteSound, 2);

        RegisterPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX, looseScrewFxPrefab, 25);
        RegisterPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX, boxCompletedFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.SUPRISE_REWARD_FX, supriseRewardFxPrefab, 1);

        RegisterPool<DamageText>(GameConstants.damageText, damageTextPrefab, 10);
    }

    private void RegisterPool<T>(string key, GameObject prefab, int poolSize)
    {
        ObjectPool pool = new ObjectPool();

        pool.Init<T>(prefab, poolSize);

        ObjectPoolingEverything.AddToPool(key, pool);
    }
}
