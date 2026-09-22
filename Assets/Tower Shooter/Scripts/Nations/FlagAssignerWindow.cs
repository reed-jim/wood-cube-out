#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FlagAssignerWindow : EditorWindow
{
    private CountryFlagDatabase database;
    private DefaultAsset folderLocation;

    [MenuItem("Tools/Auto-Assign Country Flags")]
    public static void ShowWindow()
    {
        GetWindow<FlagAssignerWindow>("Flag Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto-Assign Flag Sprites", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        database = (CountryFlagDatabase)EditorGUILayout.ObjectField("Database Asset", database, typeof(CountryFlagDatabase), false);
        folderLocation = (DefaultAsset)EditorGUILayout.ObjectField("Flags Folder", folderLocation, typeof(DefaultAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Populate & Assign Flags", GUILayout.Height(30)))
        {
            if (database == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a CountryFlagDatabase asset target.", "OK");
                return;
            }

            AssignFlags();
        }
    }

    private void AssignFlags()
    {
        string folderPath = folderLocation != null 
            ? AssetDatabase.GetAssetPath(folderLocation) 
            : "Assets";

        // Find all Sprites in the specified folder
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
        Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                // Store using lower-case file name without extension
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower().Trim();
                if (!spriteLookup.ContainsKey(fileName))
                {
                    spriteLookup.Add(fileName, sprite);
                }
            }
        }

        Undo.RecordObject(database, "Auto Assign Flags");
        database.flags.Clear();

        Dictionary<string, Country> isoToEnum = GetIsoMapping();
        int assignedCount = 0;

        foreach (KeyValuePair<string, Country> kvp in isoToEnum)
        {
            string isoCode = kvp.Key;
            Country country = kvp.Value;

            if (spriteLookup.TryGetValue(isoCode, out Sprite foundSprite))
            {
                database.flags.Add(new CountryFlagEntry
                {
                    country = country,
                    flagSprite = foundSprite
                });
                assignedCount++;
            }
            else
            {
                Debug.LogWarning($"[Flag Assigner] No sprite found for ISO code: '{isoCode}' ({country})");
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>[Flag Assigner] Successfully matched and assigned {assignedCount} out of {isoToEnum.Count} flags!</color>");
    }

    private Dictionary<string, Country> GetIsoMapping()
    {
        return new Dictionary<string, Country>
        {
            {"ad", Country.Andorra}, {"ae", Country.UnitedArabEmirates}, {"af", Country.Afghanistan}, {"ag", Country.AntiguaAndBarbuda},
            {"ai", Country.Anguilla}, {"al", Country.Albania}, {"am", Country.Armenia}, {"ao", Country.Angola},
            {"aq", Country.Antarctica}, {"ar", Country.Argentina}, {"as", Country.AmericanSamoa}, {"at", Country.Austria},
            {"au", Country.Australia}, {"aw", Country.Aruba}, {"ax", Country.AlandIslands}, {"az", Country.Azerbaijan},
            {"ba", Country.BosniaAndHerzegovina}, {"bb", Country.Barbados}, {"bd", Country.Bangladesh}, {"be", Country.Belgium},
            {"bf", Country.BurkinaFaso}, {"bg", Country.Bulgaria}, {"bh", Country.Bahrain}, {"bi", Country.Burundi},
            {"bj", Country.Benin}, {"bl", Country.SaintBarthelemy}, {"bm", Country.Bermuda}, {"bn", Country.Brunei},
            {"bo", Country.Bolivia}, {"bq", Country.BonaireSintEustatiusAndSaba}, {"br", Country.Brazil}, {"bs", Country.Bahamas},
            {"bt", Country.Bhutan}, {"bv", Country.BouvetIsland}, {"bw", Country.Botswana}, {"by", Country.Belarus},
            {"bz", Country.Belize}, {"ca", Country.Canada}, {"cc", Country.CocosIslands}, {"cd", Country.DemocraticRepublicOfTheCongo},
            {"cf", Country.CentralAfricanRepublic}, {"cg", Country.RepublicOfTheCongo}, {"ch", Country.Switzerland}, {"ci", Country.IvoryCoast},
            {"ck", Country.CookIslands}, {"cl", Country.Chile}, {"cm", Country.Cameroon}, {"cn", Country.China},
            {"co", Country.Colombia}, {"cr", Country.CostaRica}, {"cu", Country.Cuba}, {"cv", Country.CapeVerde},
            {"cw", Country.Curacao}, {"cx", Country.ChristmasIsland}, {"cy", Country.Cyprus}, {"cz", Country.CzechRepublic},
            {"de", Country.Germany}, {"dj", Country.Djibouti}, {"dk", Country.Denmark}, {"dm", Country.Dominica},
            {"do", Country.DominicanRepublic}, {"dz", Country.Algeria}, {"ec", Country.Ecuador}, {"ee", Country.Estonia},
            {"eg", Country.Egypt}, {"eh", Country.WesternSahara}, {"er", Country.Eritrea}, {"es", Country.Spain},
            {"et", Country.Ethiopia}, {"fi", Country.Finland}, {"fj", Country.Fiji}, {"fk", Country.FalklandIslands},
            {"fm", Country.Micronesia}, {"fo", Country.FaroeIslands}, {"fr", Country.France}, {"ga", Country.Gabon},
            {"gb", Country.UnitedKingdom}, {"gd", Country.Grenada}, {"ge", Country.Georgia}, {"gf", Country.FrenchGuiana},
            {"gg", Country.Guernsey}, {"gh", Country.Ghana}, {"gi", Country.Gibraltar}, {"gl", Country.Greenland},
            {"gm", Country.Gambia}, {"gn", Country.Guinea}, {"gp", Country.Guadeloupe}, {"gq", Country.EquatorialGuinea},
            {"gr", Country.Greece}, {"gs", Country.SouthGeorgiaAndSouthSandwichIslands}, {"gt", Country.Guatemala}, {"gu", Country.Guam},
            {"gw", Country.GuineaBissau}, {"gy", Country.Guyana}, {"hk", Country.HongKong}, {"hm", Country.HeardIslandAndMcDonaldIslands},
            {"hn", Country.Honduras}, {"hr", Country.Croatia}, {"ht", Country.Haiti}, {"hu", Country.Hungary},
            {"id", Country.Indonesia}, {"ie", Country.Ireland}, {"il", Country.Israel}, {"im", Country.IsleOfMan},
            {"in", Country.India}, {"io", Country.BritishIndianOceanTerritory}, {"iq", Country.Iraq}, {"ir", Country.Iran},
            {"is", Country.Iceland}, {"it", Country.Italy}, {"je", Country.Jersey}, {"jm", Country.Jamaica},
            {"jo", Country.Jordan}, {"jp", Country.Japan}, {"ke", Country.Kenya}, {"kg", Country.Kyrgyzstan},
            {"kh", Country.Cambodia}, {"ki", Country.Kiribati}, {"km", Country.Comoros}, {"kn", Country.SaintKittsAndNevis},
            {"kp", Country.NorthKorea}, {"kr", Country.SouthKorea}, {"kw", Country.Kuwait}, {"ky", Country.CaymanIslands},
            {"kz", Country.Kazakhstan}, {"la", Country.Laos}, {"lb", Country.Lebanon}, {"lc", Country.SaintLucia},
            {"li", Country.Liechtenstein}, {"lk", Country.SriLanka}, {"lr", Country.Liberia}, {"ls", Country.Lesotho},
            {"lt", Country.Lithuania}, {"lu", Country.Luxembourg}, {"lv", Country.Latvia}, {"ly", Country.Libya},
            {"ma", Country.Morocco}, {"mc", Country.Monaco}, {"md", Country.Moldova}, {"me", Country.Montenegro},
            {"mf", Country.SaintMartin}, {"mg", Country.Madagascar}, {"mh", Country.MarshallIslands}, {"mk", Country.NorthMacedonia},
            {"ml", Country.Mali}, {"mm", Country.Myanmar}, {"mn", Country.Mongolia}, {"mo", Country.Macao},
            {"mp", Country.NorthernMarianaIslands}, {"mq", Country.Martinique}, {"mr", Country.Mauritania}, {"ms", Country.Montserrat},
            {"mt", Country.Malta}, {"mu", Country.Mauritius}, {"mv", Country.Maldives}, {"mw", Country.Malawi},
            {"mx", Country.Mexico}, {"my", Country.Malaysia}, {"mz", Country.Mozambique}, {"na", Country.Namibia},
            {"nc", Country.NewCaledonia}, {"ne", Country.Niger}, {"nf", Country.NorfolkIsland}, {"ng", Country.Nigeria},
            {"ni", Country.Nicaragua}, {"nl", Country.Netherlands}, {"no", Country.Norway}, {"np", Country.Nepal},
            {"nr", Country.Nauru}, {"nu", Country.Niue}, {"nz", Country.NewZealand}, {"om", Country.Oman},
            {"pa", Country.Panama}, {"pe", Country.Peru}, {"pf", Country.FrenchPolynesia}, {"pg", Country.PapuaNewGuinea},
            {"ph", Country.Philippines}, {"pk", Country.Pakistan}, {"pl", Country.Poland}, {"pm", Country.SaintPierreAndMiquelon},
            {"pn", Country.Pitcairn}, {"pr", Country.PuertoRico}, {"ps", Country.Palestine}, {"pt", Country.Portugal},
            {"pw", Country.Palau}, {"py", Country.Paraguay}, {"qa", Country.Qatar}, {"re", Country.Reunion},
            {"ro", Country.Romania}, {"rs", Country.Serbia}, {"ru", Country.Russia}, {"rw", Country.Rwanda},
            {"sa", Country.SaudiArabia}, {"sb", Country.SolomonIslands}, {"sc", Country.Seychelles}, {"sd", Country.Sudan},
            {"se", Country.Sweden}, {"sg", Country.Singapore}, {"sh", Country.SaintHelena}, {"si", Country.Slovenia},
            {"sj", Country.SvalbardAndJanMayen}, {"sk", Country.Slovakia}, {"sl", Country.SierraLeone}, {"sm", Country.SanMarino},
            {"sn", Country.Senegal}, {"so", Country.Somalia}, {"sr", Country.Suriname}, {"ss", Country.SouthSudan},
            {"st", Country.SaoTomeAndPrincipe}, {"sv", Country.ElSalvador}, {"sx", Country.SintMaarten}, {"sy", Country.Syria},
            {"sz", Country.Eswatini}, {"tc", Country.TurksAndCaicosIslands}, {"td", Country.Chad}, {"tf", Country.FrenchSouthernTerritories},
            {"tg", Country.Togo}, {"th", Country.Thailand}, {"tj", Country.Tajikistan}, {"tk", Country.Tokelau},
            {"tl", Country.TimorLeste}, {"tm", Country.Turkmenistan}, {"tn", Country.Tunisia}, {"to", Country.Tonga},
            {"tr", Country.Turkey}, {"tt", Country.TrinidadAndTobago}, {"tv", Country.Tuvalu}, {"tw", Country.Taiwan},
            {"tz", Country.Tanzania}, {"ua", Country.Ukraine}, {"ug", Country.Uganda}, {"um", Country.UnitedStatesOutlyingIslands},
            {"us", Country.UnitedStates}, {"uy", Country.Uruguay}, {"uz", Country.Uzbekistan}, {"va", Country.VaticanCity},
            {"vc", Country.SaintVincentAndTheGrenadines}, {"ve", Country.Venezuela}, {"vg", Country.BritishVirginIslands}, {"vi", Country.USVirginIslands},
            {"vn", Country.Vietnam}, {"vu", Country.Vanuatu}, {"wf", Country.WallisAndFutuna}, {"ws", Country.Samoa},
            {"xk", Country.Kosovo}, {"ye", Country.Yemen}, {"yt", Country.Mayotte}, {"za", Country.SouthAfrica},
            {"zm", Country.Zambia}, {"zw", Country.Zimbabwe}
        };
    }
}
#endif
