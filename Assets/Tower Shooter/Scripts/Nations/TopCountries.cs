using UnityEngine;

public static class TopCountries
{
    // Top 50 countries by population mapped directly to your Country enum
    public static readonly Country[] Top50 = new Country[]
    {
        Country.India, Country.China, Country.UnitedStates, Country.Indonesia, Country.Pakistan,
        Country.Nigeria, Country.Brazil, Country.Bangladesh, Country.Russia, Country.Mexico,
        Country.Ethiopia, Country.Japan, Country.Philippines, Country.Egypt, Country.DemocraticRepublicOfTheCongo,
        Country.Vietnam, Country.Iran, Country.Turkey, Country.Germany, Country.Thailand,
        Country.Tanzania, Country.UnitedKingdom, Country.France, Country.SouthAfrica, Country.Italy,
        Country.Kenya, Country.Myanmar, Country.Colombia, Country.SouthKorea, Country.Uganda,
        Country.Sudan, Country.Spain, Country.Argentina, Country.Algeria, Country.Iraq,
        Country.Afghanistan, Country.Poland, Country.Canada, Country.Morocco, Country.SaudiArabia,
        Country.Ukraine, Country.Angola, Country.Uzbekistan, Country.Yemen, Country.Peru,
        Country.Malaysia, Country.Ghana, Country.Mozambique, Country.Nepal, Country.Madagascar
    };

    /// <summary>
    /// Returns a random country from the top 50 populated countries.
    /// </summary>
    public static Country GetRandomTop50Country()
    {
        int index = Random.Range(0, Top50.Length); // Range(int minInclusive, int maxExclusive)
        return Top50[index];
    }
}
