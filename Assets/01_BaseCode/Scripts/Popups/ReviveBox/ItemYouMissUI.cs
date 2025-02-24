using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemYouMissUI : MonoBehaviour
{
    public Image iconItemImg;
    public Text valueItemTxt;

    public void Init(Sprite icon, int numItem)
    {
        iconItemImg.sprite = icon;
        if(numItem < 0)
        {
            valueItemTxt.text = "";
        }    
        else
        {
            valueItemTxt.text = numItem.ToString();
        }    
    }    
}
