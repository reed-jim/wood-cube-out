
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemCategory
{
    None = -1,

    Fast_Food = 0,
    Pets = 1,
    Teddys = 2,
    Fruits = 3,
    Drinks = 4,
    Vehicles = 5,
    Bakery = 6,
    Musical = 7,
    Art = 8,
    Audio = 9,
    Theatre = 10,
    Sports = 11,
    Schools = 12,
    Board = 13,
    Gaming = 14,
    Clothes = 15,
    Bow_Girls = 16,
    Number_2 = 17,
    Number_5 = 18,
    Colors = 19,
    Shapes = 20,
    Number_4 = 21,
    Number_20 = 22,
    Shinobi = 23,
    Glasses = 24,
    Candy = 25,
    Veggies = 26,
    Vegan = 27,
    Farming = 28,
    Greens = 29,
    Plants = 30,
    Aliens = 31,
    Friends = 32,
    Weapons = 33,
    Cosmos = 34,
    Robots = 35,
    Number_3 = 36,
    Starships = 37,
    Pirates = 38,
    Ninjas = 39,
    Wizards = 40,
    Detective = 41,
    Royalty = 42,
    Digging = 43,
    Circus = 44,
    Number_8 = 45,
    Marine = 46,
    Diggers = 47,
    Sweets = 48,
    Heroes = 49,
    Notes = 50,
    Playland = 51,
    Homeware = 52,
    Monument = 53,
    Autoparts = 54,
    Number_14 = 55,
    Number_16 = 56,
    Laundry = 57,
    Fishing = 58,
    Tools = 59,
    Cattles = 60,
    Toybox = 61,
    Egypt = 62,
    Winter = 63,
    Summer = 64,
    Disasters = 65,
    Skyview = 66,
    Number_13 = 67,
    Number_18 = 68,
    Rainfalls = 69,
    Writing = 70,
    Mathkit = 71,
    Snacks = 72,
    Galaxy = 73,
    Fitness = 74,
    Caps = 75,
    Number_7 = 76,
    Computers = 77,
    Phones = 78,
    Cakes = 79,
    Firetruck = 80,
    Bridges = 81,
    Boats = 82,
    Pasta = 83,
    Pastries = 84,
    Toiletries = 85,
    Office = 86,
    Bags = 87,
    Casino = 88,
    Knights = 89,
    Armor = 90,
    Treasures = 91,
    Wizardry = 92,
    Castle = 93,
    Mythics = 94,
    Number_17 = 95,
    Desserts = 96,
    ArtTools = 97,
    Sewing = 98,
    Garden = 99,
    Cinema = 100,
    Dinosaurs = 101,
    Safari = 102,
    Ocean = 103,
    Birds = 104,
    Bugs = 105,
    Arctic = 106,
    Number_19 = 107,
    Kitchen = 108,
    Cleaning = 109,
    Clothing = 110,
    Medical = 111,
    Lab_Tools = 112,
    Desert_Life = 113,
    CampGear = 114,
    TrailGear = 115,
    Japan_Food = 116,
    Camera = 117,
    Dog_Breeds = 118,
    Living_Room = 119,
    Number_15 = 120,
    Bags_Shop = 121,
    Shoes = 122,
    Big_Trucks = 123,
    Mexican = 124,
    Beauty = 125,
    Adventure = 126,
    Coffee = 127,
    Bar_Drinks = 128,
    Space = 129,
    Birthday = 130,
    Themepark = 131,
    Seafoods = 132,
    Princess = 133,
    Witch = 134,
    Magic = 135,
    Monsters = 136,
    ToyBattle = 137,
    Number_24 = 138,
    Number_42 = 139,
    Number_30 = 140,
    Mail = 141,
    Breakfast = 142,
    Baby_Items = 143,
    Scrafs_Rabbit = 144,
    Halloween = 145,
    Supermarket = 146,
    Dentistry = 147,
    Optometry = 148,
    Utensils = 149,
    Street_Food = 150,
    Rainforest = 151,
    Air_Travel = 152,
    Train_Travel = 153,
    Road_Trip = 154,
    Beach_Gear = 155,
    Grilling = 156,
    Number_27 = 157,
    Number_72 = 158,
    Barbecue = 159,
    Picnic = 160,
    Rainy_Day = 161,
    Cat_Toys = 162,
    Bedroom = 163,
    Wild_West = 164,
    Computer = 165,
    Mobiles = 166,
    Offices = 167,
    Gamings = 168,
    Media = 169,
    Internet = 170,
    Vintage = 171,
    Number_29 = 172,
    Snow = 173,
    Surfing = 174,
    Movie_Theater = 175,
    Camping = 176,
    Cafe = 177,
    Cars = 178,
    Number_10 = 179,
    Cats = 180,
    Cards = 181,
    Stationery = 182,
    Reptiles = 183,
    Ancient = 184,
    HatCats=185,
    Books=186,
    Green=188,
    Morning =189,
    Snack=190,
    Pizza=191,
    Spices=192,
    Cutters=193,
    Storage=194,
    Appliance=195,
    Clean=196,
    Oils=197,
    Cookware=198,
    Tableware=199,



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
