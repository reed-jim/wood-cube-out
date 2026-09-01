using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    public CharacterServiceLocator serviceLocator;
    public CharacterStats stats;

    public List<Augment> ownedAugments;
}
