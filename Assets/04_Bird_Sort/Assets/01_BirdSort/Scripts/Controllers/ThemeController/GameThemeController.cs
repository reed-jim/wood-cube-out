using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
public class GameThemeController : MonoBehaviour
{
    [HideInInspector] public GameThemeConfig config;
    public ThemeDecorButton themeDecor;
    public List<ThemeDecorConfig> decors;
    public static int VERSION_ACCEPT = 3;
    public ThemeData theme;
    public EventSaleType type;
    public bool hasItemUI;
    GameThemeType themeActive = GameThemeType.None;
    public void Init()
    {
        if(Config.IsActive)
        {
            for (int i = 0; i < decors.Count; i++)
            {
                for (int j = 0; j < decors[i].decorObjects.Count; j++)
                {
                    ThemeDecorButton decor = Instantiate(themeDecor);
                    decor.SetDecor(decors[i].decorType, decors[i].decorObjects[j].transform);
                }
            }
        }
    }
    public GameThemeConfig Config
    {
        get
        {
            if (this.config != null)
                return this.config;
            try
            {
                var config = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
                if (config != null && (config.end - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
                {
                    switch (config.gameTheme)
                    {
                        case EventSaleType.Halloween:
                            themeActive = GameThemeType.Halloween;
                            break;
                        case EventSaleType.Noel:
                            themeActive = GameThemeType.Christmas;
                            break;
                        case EventSaleType.NewYearEve:
                            themeActive = GameThemeType.NewYear;
                            break;
                        default:
                            themeActive = GameThemeType.None;
                            break;
                    }
                }
                else
                {
                    themeActive = GameThemeType.None;
                }
            }
            catch
            {
                themeActive = GameThemeType.None;
            }
            
                var defaultConfig = new GameThemeConfig()
            {
                game_theme_type = themeActive,
                version_accept = 3
            };

            this.config = defaultConfig;
            return this.config;
            //            if (RemoteConfigController.GetJsonConfig<GameThemeConfig>(FirebaseConfig.GAME_THEME_CONFIG, out GameThemeConfig _config))
            //            {
            //                this.config = _config;
            //                return this.config;
            //            }
            //            else
            //            {
            //#if UNITY_EDITOR

            //#else
            //                var defaultConfig = new GameThemeConfig()
            //                {
            //                    game_theme_type = GameThemeType.None,
            //                    version_accept = 3
            //                };
            //                return defaultConfig;
            //#endif
            //            }
        }
    }
}
[Serializable]
public class ThemeDecorConfig
{
    public ThemeDecorType decorType;
    public List<GameObject> decorObjects;
}
public enum GameThemeType
{
    None = 0,
    Halloween = 1,
    Christmas = 2,
    NewYear = 3
}
public enum ThemeDecorType
{
    None = 0,
    // Halloween
    Halloween = 1,
    Xmas = 2,
    NewYear = 3
}

public class GameThemeConfig
{
    public GameThemeType game_theme_type
    {
        get
        {
            return (GameThemeType)game_theme_type_Int;
        }
        set
        {
            game_theme_type_Int = (int)value;
        }    
    }
    public int game_theme_type_Int;
    public int version_accept;
    public bool IsActive
    {
        get
        {
            return (game_theme_type != GameThemeType.None) && (version_accept == GameThemeController.VERSION_ACCEPT);
        }
    }
}