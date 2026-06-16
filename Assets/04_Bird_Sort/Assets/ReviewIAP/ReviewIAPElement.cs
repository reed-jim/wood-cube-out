using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReviewIAPElement : EnhancedScrollerCellView
{
    [SerializeField] private Text namePack_Txt;
    [SerializeField] private Image iconPack;

    private ReviewIAPController.ReviewData data;

    public void Init(ReviewIAPController.ReviewData data)
    {
        this.data = data;
        if (this.data == null)
            return;

        if (data.iconPack != null)
        {
            iconPack.sprite = data.iconPack;
            //iconPack.SetNativeSize();
        } 
      

        namePack_Txt.text = data.shortID;
    }

   


}
