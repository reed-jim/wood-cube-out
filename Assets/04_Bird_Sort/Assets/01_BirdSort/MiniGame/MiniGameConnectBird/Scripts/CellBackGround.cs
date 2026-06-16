using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
public class CellBackGround : MonoBehaviour
{
 public void ScaleIn()
    {
        this.transform.localScale = new Vector3(0,0,0);
        this.transform.DOScale(new Vector3( 1,1,1) , 1).OnComplete( delegate { /*this.transform.DOScale(new Vector3(1, 1, 1), 1);*/ });
    }

}
