using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ParentOutlineStand : MonoBehaviour
{
    public List<OutlineStand> lsImages;
  
    public void OnOutLine()
    {
         foreach(var item in lsImages)
        {
            item.OnOutLine();
        }    
    }
    public void OffOutLine()
    {
        foreach (var item in lsImages)
        {
            item.OffOutLine();
        }
    }
}
