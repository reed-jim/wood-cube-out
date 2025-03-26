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
    [SerializeField] private GameObject destroyObjectPartFxPrefab;
    [SerializeField] private GameObject looseScrewFxPrefab;

    [SerializeField] private GameObject taskItemUIPrefab;

    private bool _isInitialized;

    public void Init()
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
        RegisterPool<ScrewBox>(GameConstants.SCREW_BOX, screwBox, screwBoxContainer, 5);
        RegisterPool<ScrewBoxSlot>(GameConstants.SCREW_PORT_SLOT, screwPortPrefab, 3);
        RegisterPool<FakeScrew>(GameConstants.FAKE_SCREW, fakeScrewPrefab, 3);
        RegisterPool<BoosterHammer>(GameConstants.HAMMER, hammerPrefab, 3);
        RegisterPool<TaskItemUI>(GameConstants.TASK_ITEM_UI, taskItemUIPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.DESTROY_OBJECT_PART_FX, destroyObjectPartFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX, looseScrewFxPrefab, 5);
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
