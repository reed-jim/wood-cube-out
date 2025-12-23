using NamPhuThuy.GamePlay;
using UnityEngine;

public class RemoteConfigBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer woodBackground;
    [SerializeField] private SpriteRenderer gradientBackground;
    [SerializeField] private SpriteAutoScaler gradientBackgroundAutoScaler;

    void Awake()
    {
        RemoteConfigController.remoteConfigReadyEvent += UpdateBackground;
        LevelLoader.startLevelCompletedEvent += UpdateBackground;

        ChangeBackgroundColor(GamePersistentVariable.gameplayBackgroundConfig);
    }

    void OnDestroy()
    {
        RemoteConfigController.remoteConfigReadyEvent -= UpdateBackground;
        LevelLoader.startLevelCompletedEvent -= UpdateBackground;
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

            if (gameplayBackgroundConfig.isUseRandomBackground)
            {
                try
                {
                    string randomHex = gameplayBackgroundConfig.backgroundColorHexPool[Random.Range(0, gameplayBackgroundConfig.backgroundColorHexPool.Length)];

                    gradientBackground.color = ColorUtil.HexToColor(randomHex);
                }
                catch (System.Exception)
                {
                    gradientBackground.color = ColorUtil.HexToColor(gameplayBackgroundConfig.backgroundColorHex);
                }
            }
            else
            {
                gradientBackground.color = ColorUtil.HexToColor(gameplayBackgroundConfig.backgroundColorHex);
            }

            gradientBackgroundAutoScaler.ScaleSpriteToScreen();
        }
    }

    private void UpdateBackground()
    {
        if (GamePersistentVariable.gameplayBackgroundConfig.isAvoidUpdateBackgroundEachLevel)
        {
            return;
        }

        ChangeBackgroundColor(GamePersistentVariable.gameplayBackgroundConfig);
    }
}
