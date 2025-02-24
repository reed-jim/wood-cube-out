namespace  BirdSort2
{ 
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoldenBrandVFX : MonoBehaviour
{
    public Image thumbnail; 
    public List<Sprite> sprites;
    public RectTransform RectTransform;
    public Image smoke;
     public void Init()
   {
       
       
       switch (GamePlayController_BirdSort.Instance.level.typeStandInLevel)
       {
           
           case TypeStand.Slot_4:
               thumbnail.sprite = sprites[0];
             
               break;
           case TypeStand.Slot_5:
               thumbnail.sprite = sprites[1];
               thumbnail.SetNativeSize();
               break;
           case TypeStand.Slot_6:
               thumbnail.sprite = sprites[2];
          
               break;
           case TypeStand.Slot_7:
               thumbnail.sprite = sprites[3];
               break;
           case TypeStand.Slot_8:
               thumbnail.sprite = sprites[4];
               break;
       }
       
       
       
     
    }
     
     
     
}
}