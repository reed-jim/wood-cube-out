using UnityEngine;
using UnityEngine.UI;

public class NodeEffectController : MonoBehaviour
{
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

    private double lastPowerValue;
    private double lastCycleSpeedValue;
    private float accumulatedRotation = 0f;

    void Awake()
    {
        CharacterStats.OnStatModified += OnNodeStatModified;

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

    void OnDestroy()
    {
        CharacterStats.OnStatModified -= OnNodeStatModified;
    }

    // void Update()
    // {
    //     // 1. Smoothly interpolate progress and speed if enabled
    //     if (smoothTransitions)
    //     {
    //         progress = Mathf.SmoothDamp(progress, targetProgress, ref progressVelocity, transitionSmoothTime);
    //         vortexSpeed = Mathf.SmoothDamp(vortexSpeed, targetSpeed, ref speedVelocity, transitionSmoothTime);
    //     }
    //     else
    //     {
    //         progress = targetProgress;
    //         vortexSpeed = targetSpeed;
    //     }

    //     float minHighSpeed = 5f;
    //     float maxHighSpeed = 10f;

    //     if (materialInstance != null)
    //     {
    //         // 2. Check if we are in High-Speed mode or Normal mode
    //         if (vortexSpeed > minHighSpeed)
    //         {
    //             // High-speed mode: let the shader handle the warp tunnel animation via _HighSpeedTransition
    //             float transitionVal = Mathf.Clamp01((vortexSpeed - minHighSpeed) / maxHighSpeed);
    //             materialInstance.SetFloat("_HighSpeedTransition", transitionVal);
    //         }
    //         else
    //         {
    //             // Normal mode: turn off high-speed transition and accumulate rotation angle smoothly
    //             materialInstance.SetFloat("_HighSpeedTransition", 0f);

    //             accumulatedRotation += vortexSpeed * Time.deltaTime * Mathf.PI * 2f;
    //             accumulatedRotation %= Mathf.PI * 2000f; // Keep value wrapped to prevent precision issues

    //             materialInstance.SetFloat("_RotationAngle", accumulatedRotation);
    //         }
    //     }
    // }

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

    public void SetLockState(bool isLocked)
    {
        materialInstance.SetFloat("_Locked", isLocked ? 1 : 0);
    }

    public void SetPower(float power)
    {
        materialInstance.SetFloat("_Power", power);
    }

    public void SetSpeed(float newSpeed)
    {
        // Drive the smooth target spin speed
        SetSpinSpeed(newSpeed);
    }

    public void SetSpinSpeed(float newSpeed)
    {
        targetSpeed = newSpeed;
        if (!smoothTransitions) vortexSpeed = targetSpeed;
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

    #region Event
    private void OnNodeStatModified(StatType statType, double value)
    {
        if (statType == StatType.AttackSpeed)
        {
            SetSpeed((float)(value * GameConstants.nodeSpeedMultiplier));
        }
    }
    #endregion
}