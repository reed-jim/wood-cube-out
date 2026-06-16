using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightElectricItem : ItemBase
{
    protected int numIDBirdClear = 2;

    [SerializeField] protected List<AutoFlickLightning> lightings;
    [SerializeField] protected Transform posTop;

    protected override bool ActiveItemHandle()
    {
        List<int> idBirdsRequire = new List<int>();
        var requirements = GamePlayController.Instance.level.requirements;
        foreach (var item in requirements)
        {
            idBirdsRequire.Add(item.Key);
        }

        var birdInLevel = GamePlayController.Instance.level.birdsInLevel;
        int idBirdSellected = 0;
        var birdsHasExtral = GamePlayController.Instance.level.birdsHasExtral;

        bool isLighning = false;
        foreach (var item in birdInLevel)
        {
            bool isBirdInRequire = false;
            for (int i = 0; i < idBirdsRequire.Count; i++)
            {
                if (item.Key == idBirdsRequire[i])
                {
                    isBirdInRequire = true;
                    break;
                }

            }

            if (isBirdInRequire)
                continue;
            
            bool isHasBirdElectric = false;
            if (idBirdSellected < numIDBirdClear)
            {
                int numBirdFound = 0;
                List<BirdBase> birdsHit = new List<BirdBase>();
                for (int i = 0; i < item.Value.Count; i++)
                {
                    if (numBirdFound > 3)
                        break;
                    int index = i;
                    if (!IsBirdCanElectric(item.Value[index]))
                        continue;

                    int idBird = item.Value[index].id;
                    if (birdsHasExtral.ContainsKey(idBird))//Số Bird Extral phải chia hết cho 3
                    {
                        if (birdsHasExtral[idBird].Count % 3 != 0)
                        {
                            int numCount = birdsHasExtral[idBird].Count;
                            int numAdd = 0;
                            for (int t = 0; ; t++)
                            {
                                numCount++;
                                numAdd++;
                                if (numCount % 3 == 0)
                                    break;
                            }

                            for (int t = 0; t < birdInLevel[idBird].Count; t++)
                            {
                                if (IsBirdCanElectric(birdInLevel[idBird][t]))
                                    numAdd--;

                                if (numAdd < 0)//thừa ra 1 con để sét đánh
                                    break;
                            }

                            if (numAdd >= 0)
                                continue;
                        }
                    }

                    birdsHit.Add(item.Value[index]);

                    isHasBirdElectric = true;
                    numBirdFound++;
                }


                if (isHasBirdElectric)
                {
                    if (birdsHit.Count >= 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int index = i;

                            lightings[index].Shoot(new Vector3(
                                birdsHit[index].transform.position.x, posTop.position.y, 0),
                                birdsHit[index].transform,
                                Vector3.zero,
                                Vector3.zero,
                                Time.time, Time.time + 0.35f, item.Value[index]);

                            birdsHit[index].LightingHit();
                        }
                        isLighning = true;
                    }
                    idBirdSellected++;
                }
            }
            else
            {
                //if (isHasBirdElectric)
                //    return true;
                //else
                //    return false;
                return isLighning;
            }
        }

        return isLighning;
    }

    public bool IsBirdCanElectric(BirdBase bird)
    {
        if (bird.isLockMove || bird.isHasExtral || bird.isDie || bird.state == StateBird.InSlot || bird.state == StateBird.Match3)
            return false;

        return true;
    }

    public override void EndItem()
    {

    }

    public override bool IsCanUseItem()
    {
        return true;
    }
}
