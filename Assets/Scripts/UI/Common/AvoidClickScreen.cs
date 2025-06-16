using UnityEngine;

public class AvoidClickScreen : MonoBehaviour
{
    public static AvoidClickScreen Instance;

    [SerializeField] private GameObject avoidClickScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        avoidClickScreen.SetActive(false);
    }

    public void Show()
    {
        avoidClickScreen.SetActive(true);
    }

    public void Hide()
    {
        avoidClickScreen.SetActive(false);
    }
}
