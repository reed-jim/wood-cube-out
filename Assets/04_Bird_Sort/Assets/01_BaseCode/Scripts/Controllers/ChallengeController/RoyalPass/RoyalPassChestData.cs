using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/RoyalPassChestData", fileName = "RoyalPassChestData.asset")]
public class RoyalPassChestData : ScriptableObject
{
    public List<RoyalPassChestModel> chestsFreeData;
    public List<RoyalPassChestModel> chestsIAPData;

    public RoyalPassChestModel GetChest(int indexRewardInRoyal, int side)
    {
        if (side == 0)
        {
            for (int i = 0; i < chestsFreeData.Count; i++)
            {
                if (indexRewardInRoyal == chestsFreeData[i].indexRewardInRoyal)
                    return chestsFreeData[i];
            }

            return chestsFreeData[0];
        }
        else
        {
            for (int i = 0; i < chestsIAPData.Count; i++)
            {
                if (indexRewardInRoyal == chestsIAPData[i].indexRewardInRoyal)
                    return chestsIAPData[i];
            }

            return chestsIAPData[0];
        }    
    }    
}

[System.Serializable]
public class RoyalPassChestModel
{
    public int indexRewardInRoyal;
    public List<RewardRandom> rewards;
}
