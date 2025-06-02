using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SalePackData", menuName = "Scriptable Objects/SalePackData")]
public class SalePackData : ScriptableObject
{
    // public string packName;
    public SaleIAPPackType packType;
    public int levelUnlock;
    public DayOfWeek[] availableDays;
    public int maxPack;
    public int delayShow;
    public int priority;
    public int lifetime;
}
