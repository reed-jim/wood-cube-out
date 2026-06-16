using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public Effect effectCool;
    public Effect effectGreat;
    public Effect effectExcellent;
    public Effect effectPerfect;
    public Effect effectFantastic;
    public Effect effectLengendary;
    public GameObject parent;
    public float postY;
    //public void SpwanEffect(TypeCombo param)
    //{
    //   switch(param)
    //    {
    //        case TypeCombo.Cool:
    //            var effectCombo = SimplePool.Spawn(effectCool, new Vector3(0, -1, 0) , Quaternion.identity);
    //            effectCombo.ShowCombo();
    //            effectCombo.transform.SetParent(parent.transform);
    //            effectCombo.transform.localScale = new Vector3(1,1,1);
    //            break;
    //        case TypeCombo.Great:
    //            var effectComboGreat = SimplePool.Spawn(effectGreat, new Vector3(0, -2.5f, 0), Quaternion.identity);
    //            effectComboGreat.ShowCombo();
    //            effectComboGreat.transform.SetParent(parent.transform);
    //            effectComboGreat.transform.localScale = new Vector3(1, 1, 1);
    //            break;
    //        case TypeCombo.Excellent:
    //            var effectComboExcellent = SimplePool.Spawn(effectExcellent, new Vector3(0, -2, 0), Quaternion.identity);
    //            effectComboExcellent.ShowCombo();
    //            effectComboExcellent.transform.SetParent(parent.transform);
    //            effectComboExcellent.transform.localScale = new Vector3(1, 1, 1);
    //            break;
    //        case TypeCombo.Perfect:
    //            var effectComboPerfect = SimplePool.Spawn(effectPerfect, new Vector3(0, -2, 0), Quaternion.identity);
    //            effectComboPerfect.ShowCombo();
    //            effectComboPerfect.transform.SetParent(parent.transform);
    //            effectComboPerfect.transform.localScale = new Vector3(1, 1, 1);
    //            break;
    //        case TypeCombo.Fantastic:
    //            var effectComboFantastic = SimplePool.Spawn(effectFantastic, new Vector3(0, -2, 0), Quaternion.identity);
    //            effectComboFantastic.ShowCombo();
    //            effectComboFantastic.transform.SetParent(parent.transform);
    //            effectComboFantastic.transform.localScale = new Vector3(1, 1, 1);
    //            break;
    //        case TypeCombo.Lengendary:
    //            var effectComboLengendary = SimplePool.Spawn(effectLengendary, new Vector3(0, -2, 0), Quaternion.identity);
    //            effectComboLengendary.ShowCombo();
    //            effectComboLengendary.transform.SetParent(parent.transform);
    //            effectComboLengendary.transform.localScale = new Vector3(1, 1, 1);
    //            break;

    //    }    

    //}



}


//public enum TypeCombo
//{
//    Cool = 0,
//    Great = 1,
//    Excellent = 2,
//    Perfect = 3,
//    Fantastic = 4,
//    Lengendary = 5
//}