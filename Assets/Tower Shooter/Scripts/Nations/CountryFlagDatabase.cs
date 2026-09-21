using System;
using System.Collections.Generic;
using UnityEngine;

// Full country names formatted for valid C# enum syntax
public enum Country
{
    Andorra, UnitedArabEmirates, Afghanistan, AntiguaAndBarbuda, Anguilla, Albania, Armenia, Angola, 
    Antarctica, Argentina, AmericanSamoa, Austria, Australia, Aruba, AlandIslands, Azerbaijan, 
    BosniaAndHerzegovina, Barbados, Bangladesh, Belgium, BurkinaFaso, Bulgaria, Bahrain, Burundi, 
    Benin, SaintBarthelemy, Bermuda, Brunei, Bolivia, BonaireSintEustatiusAndSaba, Brazil, Bahamas, 
    Bhutan, BouvetIsland, Botswana, Belarus, Belize, Canada, CocosIslands, DemocraticRepublicOfTheCongo, 
    CentralAfricanRepublic, RepublicOfTheCongo, Switzerland, IvoryCoast, CookIslands, Chile, Cameroon, 
    China, Colombia, CostaRica, Cuba, CapeVerde, Curacao, ChristmasIsland, Cyprus, CzechRepublic, 
    Germany, Djibouti, Denmark, Dominica, DominicanRepublic, Algeria, Ecuador, Estonia, Egypt, 
    WesternSahara, Eritrea, Spain, Ethiopia, Finland, Fiji, FalklandIslands, Micronesia, FaroeIslands, 
    France, Gabon, UnitedKingdom, Grenada, Georgia, FrenchGuiana, Guernsey, Ghana, Gibraltar, Greenland, 
    Gambia, Guinea, Guadeloupe, EquatorialGuinea, Greece, SouthGeorgiaAndSouthSandwichIslands, Guatemala, 
    Guam, GuineaBissau, Guyana, HongKong, HeardIslandAndMcDonaldIslands, Honduras, Croatia, Haiti, 
    Hungary, Indonesia, Ireland, Israel, IsleOfMan, India, BritishIndianOceanTerritory, Iraq, Iran, 
    Iceland, Italy, Jersey, Jamaica, Jordan, Japan, Kenya, Kyrgyzstan, Cambodia, Kiribati, Comoros, 
    SaintKittsAndNevis, NorthKorea, SouthKorea, Kuwait, CaymanIslands, Kazakhstan, Laos, Lebanon, 
    SaintLucia, Liechtenstein, SriLanka, Liberia, Lesotho, Lithuania, Luxembourg, Latvia, Libya, 
    Morocco, Monaco, Moldova, Montenegro, SaintMartin, Madagascar, MarshallIslands, NorthMacedonia, 
    Mali, Myanmar, Mongolia, Macao, NorthernMarianaIslands, Martinique, Mauritania, Montserrat, Malta, 
    Mauritius, Maldives, Malawi, Mexico, Malaysia, Mozambique, Namibia, NewCaledonia, Niger, NorfolkIsland, 
    Nigeria, Nicaragua, Netherlands, Norway, Nepal, Nauru, Niue, NewZealand, Oman, Panama, Peru, 
    FrenchPolynesia, PapuaNewGuinea, Philippines, Pakistan, Poland, SaintPierreAndMiquelon, Pitcairn, 
    PuertoRico, Palestine, Portugal, Palau, Paraguay, Qatar, Reunion, Romania, Serbia, Russia, Rwanda, 
    SaudiArabia, SolomonIslands, Seychelles, Sudan, Sweden, Singapore, SaintHelena, Slovenia, SvalbardAndJanMayen, 
    Slovakia, SierraLeone, SanMarino, Senegal, Somalia, Suriname, SouthSudan, SaoTomeAndPrincipe, ElSalvador, 
    SintMaarten, Syria, Eswatini, TurksAndCaicosIslands, Chad, FrenchSouthernTerritories, Togo, Thailand, 
    Tajikistan, Tokelau, TimorLeste, Turkmenistan, Tunisia, Tonga, Turkey, TrinidadAndTobago, Tuvalu, 
    Taiwan, Tanzania, Ukraine, Uganda, UnitedStatesOutlyingIslands, UnitedStates, Uruguay, Uzbekistan, 
    VaticanCity, SaintVincentAndTheGrenadines, Venezuela, BritishVirginIslands, USVirginIslands, Vietnam, 
    Vanuatu, WallisAndFutuna, Samoa, Kosovo, Yemen, Mayotte, SouthAfrica, Zambia, Zimbabwe
}

[Serializable]
public struct CountryFlagEntry
{
    public Country country;
    public Sprite flagSprite;
}

[CreateAssetMenu(fileName = "CountryFlagDatabase", menuName = "Flags/Country Flag Database")]
public class CountryFlagDatabase : ScriptableObject
{
    public List<CountryFlagEntry> flags = new List<CountryFlagEntry>();

    public Sprite GetFlag(Country country)
    {
        var entry = flags.Find(f => f.country == country);
        return entry.flagSprite;
    }
}
