using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeThemeController : MonoBehaviour
{
    [SerializeField] private Image BG;
    [SerializeField] private Image AvatarBG;
    [SerializeField] private Image PlayBG;
    [SerializeField] private Image Top;
    [SerializeField] private List<Image> BtnBG;
    [SerializeField] private ThemeData theme;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Init()
    {
        try
        {
            var config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
            if (config != null && (config.end - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
            {
                var themeType = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive().gameTheme;
                ThemeDataModle themeData = new ThemeDataModle();
                for (int i = 0; i < theme.data.Count; i++)
                {
                    if (theme.data[i].themeType == themeType)
                    {
                        themeData = theme.data[i];
                        setTheme(themeData);
                        break;
                    }
                }
            }
        }
        catch
        {

            throw;
        }
       
    }

    private void setTheme(ThemeDataModle themeData)
    {
        //BG.gameObject.SetActive(true);
        BG.sprite = themeData.HomeBG;
        BG.SetNativeSize();
        Top.gameObject.SetActive(true);
        Top.sprite = themeData.Top;
        AvatarBG.gameObject.SetActive(true);
        AvatarBG.sprite = themeData.AvatarBG;
        PlayBG.gameObject.SetActive(true);
        PlayBG.sprite = themeData.BtnPlayBG;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
