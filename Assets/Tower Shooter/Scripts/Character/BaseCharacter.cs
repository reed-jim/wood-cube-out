using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public CharacterServiceLocator serviceLocator;
    public CharacterStats stats;
    
    public List<StatModifier> upgrades;
    public List<Augment> ownedAugments;

    void Awake()
    {
        upgrades = new List<StatModifier>();
    }
}
