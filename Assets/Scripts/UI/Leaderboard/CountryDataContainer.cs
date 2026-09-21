using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CountryDataContainer", menuName = "ScriptableObjects/CountryDataContainer")]
public class CountryDataContainer : ScriptableObject
{
    public List<Country> Countries;
}

// [System.Serializable]
// public class Country
// {
//     public string Name;
//     public Sprite Flag;
// }
