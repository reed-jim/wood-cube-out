using UnityEngine;

public class EngineUIAnimator : MonoBehaviour
{
    public Material engineMaterial;
    public float cycleDuration = 2.0f;
    private float progress;

    void Update()
    {
        progress += Time.deltaTime / cycleDuration;
        if (progress >= 1.0f)
        {
            progress = 0.0f; // Instant snap back for power discharge effect
        }
        
        engineMaterial.SetFloat("_Progress", progress);
    }
}