using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ThemeData", fileName = "ThemeData.asset")]
public class ThemeData : ScriptableObject
{
    public List<ThemeDataModle> data;
}

[System.Serializable]
public class ThemeDataModle
{
    public EventSaleType themeType;
    public Sprite HomeBG;
    public Sprite GamePlayBG;
    public Sprite ItemBG;
    public Sprite BtnBG;
    public Sprite BtnHomeBG;
    public Sprite AvatarBG;
    public Sprite BtnPlayBG;
    public Sprite Top;
    public Sprite Title;
}
