using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    // =========================================================
    // SPACE / BACKGROUND
    // =========================================================

    public static readonly Color DeepSpace     = Hex("#080D1A");
    public static readonly Color SpaceBackground = Hex("#0D1526");
    public static readonly Color Panel         = Hex("#151F35");
    public static readonly Color PanelLight    = Hex("#202C47");
    public static readonly Color Border        = Hex("#354566");

    // =========================================================
    // TEXT
    // =========================================================

    public static readonly Color PrimaryText   = Hex("#E8F1FF");
    public static readonly Color SecondaryText = Hex("#91A4C4");
    public static readonly Color DisabledText  = Hex("#53627A");
    public static readonly Color ActiveText  = Hex("#50FF00");

    // =========================================================
    // MAIN ACCENTS
    // =========================================================

    public static readonly Color Cyan          = Hex("#32D9FF");
    public static readonly Color Purple        = Hex("#9B6CFF");
    public static readonly Color Green         = Hex("#52E38C");
    public static readonly Color Gold          = Hex("#FFC857");
    public static readonly Color Red           = Hex("#FF5C6C");
    public static readonly Color Orange        = Hex("#FF9F43");
    public static readonly Color Pink          = Hex("#FF5FD7");

    // =========================================================
    // BUTTONS - PRIMARY / CYAN
    // =========================================================

    public static readonly Color ButtonPrimary        = Hex("#32D9FF");
    public static readonly Color ButtonPrimaryHover   = Hex("#5DE4FF");
    public static readonly Color ButtonPrimaryPressed = Hex("#18AFCF");

    public static readonly Color ButtonPrimaryBorder       = Hex("#6BE7FF");
    public static readonly Color ButtonPrimaryHoverBorder  = Hex("#9AF0FF");
    public static readonly Color ButtonPrimaryGlow         = Hex("#32D9FF");

    // =========================================================
    // BUTTONS - SECONDARY
    // =========================================================

    public static readonly Color ButtonSecondary        = Hex("#202C47");
    public static readonly Color ButtonSecondaryHover   = Hex("#2B3A5B");
    public static readonly Color ButtonSecondaryPressed = Hex("#151F35");

    public static readonly Color ButtonSecondaryBorder      = Hex("#354566");
    public static readonly Color ButtonSecondaryHoverBorder = Hex("#32D9FF");

    // =========================================================
    // BUTTONS - DISABLED
    // =========================================================

    public static readonly Color ButtonDisabled       = Hex("#151F35");
    public static readonly Color ButtonDisabledText    = Hex("#53627A");
    public static readonly Color ButtonDisabledBorder = Hex("#27334A");

    // =========================================================
    // BUTTONS - SUCCESS / BUY
    // =========================================================

    public static readonly Color ButtonSuccess        = Hex("#52E38C");
    public static readonly Color ButtonSuccessHover   = Hex("#7AF0A8");
    public static readonly Color ButtonSuccessPressed = Hex("#36C873");
    public static readonly Color ButtonSuccessBorder  = Hex("#7AF0A8");
    public static readonly Color ButtonSuccessGlow    = Hex("#52E38C");

    // =========================================================
    // BUTTONS - UPGRADE / RESEARCH
    // =========================================================

    public static readonly Color ButtonUpgrade        = Hex("#9B6CFF");
    public static readonly Color ButtonUpgradeHover   = Hex("#BD9CFF");
    public static readonly Color ButtonUpgradePressed = Hex("#7548D6");
    public static readonly Color ButtonUpgradeBorder  = Hex("#BD9CFF");
    public static readonly Color ButtonUpgradeGlow    = Hex("#9B6CFF");

    // =========================================================
    // BUTTONS - DANGER
    // =========================================================

    public static readonly Color ButtonDanger        = Hex("#FF5C6C");
    public static readonly Color ButtonDangerHover   = Hex("#FF8490");
    public static readonly Color ButtonDangerPressed = Hex("#D93F50");
    public static readonly Color ButtonDangerBorder  = Hex("#FF8490");
    public static readonly Color ButtonDangerGlow    = Hex("#FF5C6C");

    // =========================================================
    // BUTTONS - GOLD / PREMIUM
    // =========================================================

    public static readonly Color ButtonPremium        = Hex("#FFC857");
    public static readonly Color ButtonPremiumHover   = Hex("#FFD979");
    public static readonly Color ButtonPremiumPressed = Hex("#D9A63E");
    public static readonly Color ButtonPremiumBorder  = Hex("#FFE29A");
    public static readonly Color ButtonPremiumGlow    = Hex("#FFC857");

    // =========================================================
    // RESOURCES
    // =========================================================

    public static readonly Color Credits     = Hex("#52E38C");
    public static readonly Color Energy      = Hex("#32D9FF");
    public static readonly Color Minerals    = Hex("#FFC857");
    public static readonly Color Research    = Hex("#9B6CFF");
    public static readonly Color DarkMatter  = Hex("#FF5FD7");
    public static readonly Color Fuel        = Hex("#FF9F43");

    // =========================================================
    // SHIP RARITY
    // =========================================================

    public static readonly Color Common      = Hex("#91A4C4");
    public static readonly Color Uncommon    = Hex("#52E38C");
    public static readonly Color Rare        = Hex("#32D9FF");
    public static readonly Color Epic        = Hex("#9B6CFF");
    public static readonly Color Legendary   = Hex("#FFC857");
    public static readonly Color Mythic      = Hex("#FF5FD7");

    // =========================================================
    // STATUS
    // =========================================================

    public static readonly Color Success    = Hex("#52E38C");
    public static readonly Color Warning    = Hex("#FFC857");
    public static readonly Color Error      = Hex("#FF5C6C");
    public static readonly Color Info       = Hex("#32D9FF");

    // =========================================================
    // EFFECTS
    // =========================================================

    public static readonly Color EnergyGlow     = Hex("#32D9FF");
    public static readonly Color EngineGlow     = Hex("#FF9F43");
    public static readonly Color ShieldGlow     = Hex("#32D9FF");
    public static readonly Color WarpGlow       = Hex("#9B6CFF");
    public static readonly Color RareGlow       = Hex("#FFC857");
    public static readonly Color DarkMatterGlow = Hex("#FF5FD7");

    // =========================================================
    // UNITY SETUP
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // HEX → COLOR
    // =========================================================

    private static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        Debug.LogError($"Invalid color hex: {hex}");

        return Color.magenta;
    }

    // =========================================================
    // PUBLIC HELPERS
    // =========================================================

    public static Color GetRarityColor(ShipRarity rarity)
    {
        return rarity switch
        {
            ShipRarity.Common    => Common,
            ShipRarity.Uncommon  => Uncommon,
            ShipRarity.Rare      => Rare,
            ShipRarity.Epic      => Epic,
            ShipRarity.Legendary => Legendary,
            ShipRarity.Mythic    => Mythic,
            _                    => Common
        };
    }
}


// =============================================================
// SUPPORTING ENUMS
// =============================================================

public enum ShipRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}
