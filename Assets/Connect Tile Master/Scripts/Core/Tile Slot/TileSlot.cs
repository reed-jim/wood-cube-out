using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemCategory
{
    Fast_Food,
    Pets,
    Teddy_Bears,
    Fruits,
    Drinks,
    Vehicles,
    Bakery,
    Musical,
    Art,
    Audio,
    Theatre,
    Sports,
    School_Supplies,
    Board_Games,
    Video_Games,
    Clothes,
    Bow_girls,
    Number_2,
    Number_5,
    Colors,
    Shapes,
    Number_4,
    Number_20,
    Ninja_weapons,
    Glasses_wearers,
    Candy,
    Vegetables,
    Vegan,
    Farmtools,
    Greens,
    Plants,
    Aliens,
    Teddy_Friends,
    Tech_Weapons,
    Cosmos,
    Robots,
    Number_3,
    Starships,
    Pirates,
    Ninjas,
    Wizards,
    Detectives,
    Royalty,
    Archaeology,
    Circus,
    Number_8,
    Sea_Animals,
    Diggers,
    Sweets,
    Heroes,
    Notes,
    Playgrounds,
    Furnitures,
    Landmarks,
    Autoparts,
    Number_14,
    Number_16,
    Amenities,
    Wellness,
    Tools,
    Livestocks,
    Toys,
    Egypt,
    Winterwears,
    Summerwears,
    Disasters,
    Sky_sights,
    Number_13,
    Number_18,
    Rainfalls,
    Stationery,
    Math_tools,
    Food_Combo,
    Deep_Space,
    Fitness,
    Caps,
    Number_7,
    Hardware,
    Gadgets,
    Baking,
    Firehouse,
    Famous,
    Autumn,
    Italian_Cuisine,
    Bakery_Pastry,
    Bathroom_Items,
    Office_Supplies,
    Creepy_Insects,
    Casino_Card_Games,
    Knight_Weapons,
    Knight_Armor,
    Royal_Treasures,
    Wizard_Magic_Items,
    Medieval_Buildings,
    Mythical_Creatures,
    Number_17,
    Ice_Cream_Desserts,
    Fine_Art_Supplies,
    Sewing_Knitting,
    Gardening_Hobby,
    Movie_Cinema,
    Food_Fractions,
    Safari_Mammals,
    Ocean_Creatures,
    Polar_Zone_Animals,
    Reptiles_Amphibian,
    Prehistoric_Animals,
    Number_19,
    Kitchen_Electronics,
    Laundry_Cleaning,
    Personal_Clothing,
    Medical_Tools,
    Laboratory_Glass,
    Desert_Mammals,
    Camping_Gear,
    Hiking_Apparel,
    Japanese_Cuisine,
    Photography_Gear,
    Dog_Breeds,
    Living_Room,
    Number_15,
    Bags_Luggage,
    Footwear_Fashion,
    Big_Trucks,
    Mexican_Food,
    Makeup_Beauty,
    Pirate_Adventure,
    Café_Drinks,
    Bar_Drinks,
    Sky_Space,
    Birthday_Party,
    Amusement_Park,
    Seafood_Dishes,
    Fairytale_Princess,
    Wicked_Witch,
    Wizard_School,
    Fairytale_Monsters,
    Toy_Kingdom,
    Number_24,
    Number_42,
    Number_30,
    Breakfast_Foods,
    Baby_Items,
    Car_Components,
    Halloween_Party,
    Supermarket,
    Dentistry_Care,
    Optometry_Care,
    Kitchen_Utensils,
    Asian_Street_Food,
    Rainforest_Animals,
    Air_Travel,
    Train_Travel,
    Road_Trip,
    Beach_Packing,
    Picnic_Day,
    Number_27,
    Number_72,
    Barbecue_Party,
    Rainy_Day,
    Cat_Toys_Items,
    Bedroom_Items,
    Wild_West_Cowboy,
    Computer_Setup,
    Mobile_Gadgets,
    Office_Desk,
    Gaming_Room,
    TV_Streaming,
    Internet_World,
    Old_Tech_Items,
    Number_29,
    Winter_Wonderland,
    Water_Sports,
    Movie_Theater,
    Camping_Trip,
    Coffee_Shop,
    None

}

public class TileSlot : MonoBehaviour
{
    [SerializeField] private BaseTile tile;
    public TileSlotContainer tileSlotContainer;
    public RectTransform rectTransform;
    public Image containerImage;

    public bool isShelf;
    public bool isLocked;

    public BaseTile Tile
    {
        get => tile;
        set => tile = value;
    }

    public Vector2 Size => rectTransform.sizeDelta;

    public void AddTile(BaseTile tile)
    {
        this.tile = tile;

        if (!isShelf)
        {
            tile.IsOnValidSlot = true;
        }

        tile.IsOnSlot = true;
    }

    public void ClearTile()
    {
        if (tile != null)
        {
            if (!isShelf)
            {
                tile.gameObject.SetActive(false);
                tile.IsDone = true;
            }

            tile = null;
        }
    }
}
