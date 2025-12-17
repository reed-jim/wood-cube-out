using UnityEngine;

public class RemoteConfigBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer woodBackground;
    [SerializeField] private SpriteRenderer gradientBackground;
    [SerializeField] private SpriteAutoScaler gradientBackgroundAutoScaler;

    void Awake()
    {
        RemoteConfigController.remoteConfigReadyEvent += UpdateBackground;

        ChangeBackgroundColor(GamePersistentVariable.gameplayBackgroundConfig);
    }

    void OnDestroy()
    {
        RemoteConfigController.remoteConfigReadyEvent -= UpdateBackground;
    }

    private void ChangeBackgroundColor(GameplayBackgroundConfig gameplayBackgroundConfig)
    {
        if (gameplayBackgroundConfig.isUseWoodBackground)
        {
            woodBackground.gameObject.SetActive(true);
            gradientBackground.gameObject.SetActive(false);
        }
        else
        {
            woodBackground.gameObject.SetActive(false);
            gradientBackground.gameObject.SetActive(true);

            gradientBackground.color = ColorUtil.HexToColor(gameplayBackgroundConfig.backgroundColorHex);
            gradientBackgroundAutoScaler.ScaleSpriteToScreen();
        }
    }

    private void UpdateBackground()
    {
        ChangeBackgroundColor(GamePersistentVariable.gameplayBackgroundConfig);
    }
}
