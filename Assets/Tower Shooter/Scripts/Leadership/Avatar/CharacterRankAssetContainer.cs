using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterRank
{
    FlightCadet,
    ShipSpecialist,
    EnsignOperator,
    SystemOfficer,
    ChiefEngineer,
    ExecutiveOfficer,
    ShipCommander,
    FlagCaptain,
    Commodore,
    StarMarshal
}

[CreateAssetMenu(fileName = "CharacterRankAssetContainer", menuName = "Idle/Character/CharacterRankAssetContainer")]
public class CharacterRankAssetContainer : ScriptableObject
{
    public List<CharacterRankAssetItem> items;

    public Sprite GetRankSprite(CharacterRank characterRank)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].characterRank == characterRank)
            {
                return items[i].rankSprite;
            }
        }

        return null;
    }

    public string GetRankTitle(CharacterRank rank)
    {
        return rank switch
        {
            CharacterRank.FlightCadet => "Flight Cadet",
            CharacterRank.ShipSpecialist => "Ship Specialist",
            CharacterRank.EnsignOperator => "Ensign Operator",
            CharacterRank.SystemOfficer => "System Officer",
            CharacterRank.ChiefEngineer => "Chief Engineer",
            CharacterRank.ExecutiveOfficer => "Executive Officer",
            CharacterRank.ShipCommander => "Ship Commander",
            CharacterRank.FlagCaptain => "Flag Captain",
            CharacterRank.Commodore => "Commodore",
            CharacterRank.StarMarshal => "Star Marshal",
            _ => "Flight Cadet"
        };
    }
}

[Serializable]
public class CharacterRankAssetItem
{
    public CharacterRank characterRank;
    public Sprite rankSprite;
}
