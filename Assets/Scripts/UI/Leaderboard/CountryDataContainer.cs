using System.Collections.Generic;
using UnityEngine;

public static class CountryDataContainer
{
    public static List<Country> Countries;
}

[System.Serializable]
public class Country
{
    public string Name;
    public Sprite Flag;
}
