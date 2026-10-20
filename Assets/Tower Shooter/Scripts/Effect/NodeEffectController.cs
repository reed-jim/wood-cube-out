using UnityEngine;
using UnityEngine.UI;

public class NodeEffectController : MonoBehaviour
{
    [SerializeField] private CharacterServiceLocator serviceLocator;

    [Header("UI")]
    [SerializeField] private Image uiImage;

    [Header("Live Drive Parameters")]
    [Range(0f, 1f)]
    [SerializeField] private float progress = 0.0f;
    [SerializeField] private float vortexSpeed = 15.0f;

    [Header("Smoothing & Dynamics")]
    [Tooltip("If enabled, progress and speed will smoothly glide to target values instead of snapping.")]
    public bool smoothTransitions = true;
    public float transitionSmoothTime = 0.2f;

    [Header("Node States (0 = Locked, 1 = Unlocked)")]
    public bool centerUnlocked = true;
    public bool topNodeUnlocked = false;
    public bool bottomNodeUnlocked = false;
    public bool leftNodeUnlocked = false;
    public bool rightNodeUnlocked = false;
    public bool topLeftNodeUnlocked = false;
    public bool topRightNodeUnlocked = false;
    public bool bottomLeftNodeUnlocked = false;
    public bool bottomRightNodeUnlocked = false;

    private Material materialInstance;

    // Internal smoothing velocities
    private float progressVelocity;
    private float speedVelocity;
    private float targetProgress;
    private float targetSpeed;

    // Cache for dirty checking (prevents redundant SetFloat calls)
    private float lastAppliedProgress = -1f;
    private float lastAppliedSpeed = -999f;
    private readonly float[] lastAppliedNodes = new float[9];
    private bool nodesDirty = true;

    void Awake()
    {
        if (uiImage != null && uiImage.material != null)
        {
            materialInstance = new Material(uiImage.material);
            uiImage.material = materialInstance;
        }
        else
        {
            Debug.LogWarning("AdvancedMultiverseController: No material assigned to UI Image!", this);
        }

        targetProgress = progress;
        targetSpeed = vortexSpeed;

        // Initialize node cache array with invalid values to force initial sync
        for (int i = 0; i < 9; i++) lastAppliedNodes[i] = -1f;
    }

    void Update()
    {
        SetSpeed(serviceLocator.stats.GetStat(StatType.AttackSpeed).Value);

        // if (materialInstance == null) return;

        // // 1. Handle Smooth Transitions
        // if (smoothTransitions)
        // {
        //     progress = Mathf.SmoothDamp(progress, targetProgress, ref progressVelocity, transitionSmoothTime);
        //     vortexSpeed = Mathf.SmoothDamp(vortexSpeed, targetSpeed, ref speedVelocity, transitionSmoothTime);
        // }
        // else
        // {
        //     progress = targetProgress;
        //     vortexSpeed = targetSpeed;
        // }

        // // 2. Only update Progress if it changed (using an epsilon threshold to avoid float jitter)
        // if (Mathf.Abs(progress - lastAppliedProgress) > 0.0001f)
        // {
        //     materialInstance.SetFloat("_Progress", progress);
        //     lastAppliedProgress = progress;
        // }

        // // 3. Only update Spin Speed if it changed
        // if (Mathf.Abs(vortexSpeed - lastAppliedSpeed) > 0.001f)
        // {
        //     materialInstance.SetFloat("_VortexSpeed", vortexSpeed);
        //     lastAppliedSpeed = vortexSpeed;
        // }

        // // 4. Only update Node States if a node has been modified
        // if (nodesDirty)
        // {
        //     ApplyNodeStates();
        //     nodesDirty = false;
        // }
    }

    private void ApplyNodeStates()
    {
        float[] currentNodes = new float[9]
        {
            topLeftNodeUnlocked ? 1f : 0f,     // 0: Top-Left (0,0)
            topNodeUnlocked ? 1f : 0f,         // 1: Top (1,0)
            topRightNodeUnlocked ? 1f : 0f,    // 2: Top-Right (2,0)
            leftNodeUnlocked ? 1f : 0f,        // 3: Left (0,1)
            centerUnlocked ? 1f : 0f,          // 4: Center (1,1)
            rightNodeUnlocked ? 1f : 0f,       // 5: Right (2,1)
            bottomLeftNodeUnlocked ? 1f : 0f,  // 6: Bottom-Left (0,2)
            bottomNodeUnlocked ? 1f : 0f,      // 7: Bottom (1,2)
            bottomRightNodeUnlocked ? 1f : 0f  // 8: Bottom-Right (2,2)
        };

        string[] propertyNames = new string[9]
        {
            "_Node_0_0", "_Node_1_0", "_Node_2_0",
            "_Node_0_1", "_Node_1_1", "_Node_2_1",
            "_Node_0_2", "_Node_1_2", "_Node_2_2"
        };

        for (int i = 0; i < 9; i++)
        {
            if (currentNodes[i] != lastAppliedNodes[i])
            {
                materialInstance.SetFloat(propertyNames[i], currentNodes[i]);
                lastAppliedNodes[i] = currentNodes[i];
            }
        }
    }

    // ==========================================
    // PUBLIC CONTROL FUNCTIONS
    // ==========================================

    public void SetProgress(float newProgress)
    {
        targetProgress = Mathf.Clamp01(newProgress);
        if (!smoothTransitions) progress = targetProgress;
    }

    public void SetPower(float power)
    {
        materialInstance.SetFloat("_Power", power);
    }

    public void SetSpeed(float newSpeed)
    {
        materialInstance.SetFloat("_CycleSpeed", newSpeed);
    }

    public void SetSpinSpeed(float newSpeed)
    {
        targetSpeed = newSpeed;
        if (!smoothTransitions) vortexSpeed = targetSpeed;
    }

    public void SetEngineParameters(float newProgress, float newSpeed)
    {
        SetProgress(newProgress);
        SetSpinSpeed(newSpeed);
    }

    public void SetNodeState(int x, int y, bool unlocked)
    {
        if (x == 1 && y == 1) centerUnlocked = unlocked;
        else if (x == 1 && y == 0) topNodeUnlocked = unlocked;
        else if (x == 1 && y == 2) bottomNodeUnlocked = unlocked;
        else if (x == 0 && y == 1) leftNodeUnlocked = unlocked;
        else if (x == 2 && y == 1) rightNodeUnlocked = unlocked;
        else if (x == 0 && y == 0) topLeftNodeUnlocked = unlocked;
        else if (x == 2 && y == 0) topRightNodeUnlocked = unlocked;
        else if (x == 0 && y == 2) bottomLeftNodeUnlocked = unlocked;
        else if (x == 2 && y == 2) bottomRightNodeUnlocked = unlocked;

        nodesDirty = true;
    }

    public void UnlockAllNodes(bool unlocked)
    {
        topNodeUnlocked = unlocked;
        bottomNodeUnlocked = unlocked;
        leftNodeUnlocked = unlocked;
        rightNodeUnlocked = unlocked;
        topLeftNodeUnlocked = unlocked;
        topRightNodeUnlocked = unlocked;
        bottomLeftNodeUnlocked = unlocked;
        bottomRightNodeUnlocked = unlocked;

        nodesDirty = true;
    }
}