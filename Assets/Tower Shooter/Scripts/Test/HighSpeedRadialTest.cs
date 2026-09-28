using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HighSpeedRadialTest : MonoBehaviour
{
    [SerializeField] private Image image;

    [Header("RPM Parameters")]
    [SerializeField] private float maxRPM = 10000f;
    [SerializeField] private float acceleration = 2000f;

    [Header("High Speed Transition Thresholds")]
    [SerializeField] private float highSpeedStartRPM = 1000f;
    [SerializeField] private float highSpeedFullRPM = 5000f;

    [Header("Shader Properties")]
    [SerializeField] private float streakCount = 16f;
    [SerializeField] private float streakLength = 0.35f;
    [SerializeField] private float intensity = 1.5f;

    [Header("Radial Fill Control")]
    [Range(0f, 1f)]
    [SerializeField] private float fillAmount = 1.0f;

    private Material material;
    private float rpm;
    private float rotation;

    private static readonly int HighSpeedProperty = Shader.PropertyToID("_HighSpeed");
    private static readonly int RotationProperty = Shader.PropertyToID("_Rotation");
    private static readonly int FillAmountProperty = Shader.PropertyToID("_FillAmount");

    private void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        // Ensure Image Type is set to Simple when using the integrated custom shader fill
        image.type = Image.Type.Simple;

        // Create an instance so we don't modify the shared material asset
        material = Instantiate(image.material);
        image.material = material;

        // Set static properties
        material.SetFloat("_StreakCount", streakCount);
        material.SetFloat("_StreakLength", streakLength);
        material.SetFloat("_Intensity", intensity);
    }

    private void Update()
    {
        // 2. Convert current RPM to Radians per Second
        float angularVelocity = rpm * (2f * Mathf.PI / 60f);

        // 3. Integrate rotation & wrap around 2*PI cleanly to avoid floating-point drift
        rotation += angularVelocity * Time.deltaTime;
        rotation = Mathf.Repeat(rotation, 2f * Mathf.PI);

        // 4. Calculate HighSpeed normalized factor (0..1)
        float highSpeed = Mathf.InverseLerp(
            highSpeedStartRPM,
            highSpeedFullRPM,
            rpm
        );

        // 5. Update Shader Properties using cached Property IDs for better performance
        material.SetFloat(HighSpeedProperty, highSpeed);
        material.SetFloat(RotationProperty, rotation);
        material.SetFloat(FillAmountProperty, fillAmount);
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            Destroy(material);
        }
    }
}
