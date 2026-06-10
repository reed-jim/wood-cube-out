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
    Musical_Instruments,
    Art_Painting,
    Audio_Listening,
    Theatre_Performance,
    Sports,
    School_Supplies,
    Board_Games,
    Video_Games,
    Clothes,
    Girl_with_Neck,
    Number_2,
    Number_5,
    Colors,
    Shapes,
    Number_4,
    Number_20,
    Ninja_weapons,
    People_with_glasses,
    Candy,
    Vegetables,
    Vegetarian_Dishes,
    Farming_Tools,
    Green_Items,
    Garden_Plants,
    Purple_Alien,
    Purple_teddy,
    Sci_Fi_Weapons,
    Space_Objects,
    Robots,
    Number_3,
    Star_Ships,
    Pirates,
    Ninjas,
    Wizards,
    Detectives,
    Royalty,
    Archaeology,
    Circus,
    Number_8,
    Sea_Animals,
    Construction_Vehicles,
    Ice_Creams,
    Superheroes_Characters,
    Musical_Notes,
    Tech_Devices,
    Home_Furniture,
    World_Landmarks,
    Car_Parts,
    Number_14,
    Number_16,
    Hotel_Amenities,
    Spa_Wellness,
    Hand_Tools,
    Farm_Animals,
    Childhood_Toys,
    Ancient_Egypt,
    Cold_Weather_Gear,
    Hot_Weather_Gear,
    Natural_Disasters,
    Sky_Phenomena,
    Number_13,
    Number_18,
    Precipitation_Types,
    Writing_Stationery,
    Geometry_Tools,
    Deep_Space,
    Gym_Fitness,
    Deep_Sea_Animals,
    Number_7,
    Computer_Hardware,
    Baking_Ingredients,
    Forest_Birds,
    Famous_Landmarks,
    Autumn_Items,
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
