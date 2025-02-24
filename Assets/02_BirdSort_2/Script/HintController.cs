namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class HintController : MonoBehaviour
    {
        public int[] hints;//Chứa thứ tự ấn ID Stand để qua màn

        public void Init(int level)
        {
            if (level == 1)
            {
                hints = new int[] { 0, 1 };
            }
            else if (level == 2)
            {
                hints = new int[] { 0, 1, 2, 0 };
            }
        }


        public StandingBase GetPosHint(int idStand)
        {
            var allStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
            var standing = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[idStand];
            if (standing != null)
            {
                var birdEndCurrent = standing.GetBirdSameEnd();

                if (birdEndCurrent.Count > 0)
                {
                    for (int i = 0; i < allStand.Count; i++)
                    {
                        if (standing.ID == allStand[i].ID)
                            continue;

                        var birdEndTarget = allStand[i].GetBirdSameEnd();
                        if (birdEndTarget.Count <= 0)
                            continue;

                        if (birdEndTarget[0].id == birdEndCurrent[0].id)
                        {
                            return allStand[i];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Lấy ra stand có khả năng mang chim đi sang cành khác
        /// </summary>
        /// <returns></returns>
        public StandingBase GetPosCanHint()
        {
            var allStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;

            for (int i = 0; i < allStand.Count; i++)
            {
                var standMatch = GetPosHint(allStand[i].ID);
                if (standMatch != null)
                {
                    return allStand[i];
                }
            }

            return null;
        }


        public StandingBase GetPosHint2(int idStand)
        {
            var allStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
            var standing = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[idStand];
            if (standing != null)
            {
                var birdEndCurrent = standing.GetBirdSameEnd(isGetLockMove: false);

                if (birdEndCurrent.Count > 0)
                {
                    for (int i = 0; i < allStand.Count; i++)
                    {
                        if (standing.ID == allStand[i].ID)
                            continue;

                        if (allStand[i].birdsOnArray.Length >= (int)allStand[0].data.numSlot)
                            continue;

                        var birdEndTarget = allStand[i].GetBirdSameEnd();
                        if (birdEndTarget.Count <= 0)
                        {
                            return allStand[i];
                        }

                        if (birdEndTarget[0].id == birdEndCurrent[0].id)
                        {
                            return allStand[i];
                        }
                    }
                }
            }

            return null;
        }

        //Lấy ra cành có thể di chuyển chim
        public StandingBase GetStandCanSort()
        {
            var allStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
            // if (standing != null)
            {
                //  var birdEndCurrent = standing.GetBirdSameEnd(isGetLockMove: false);

                //if (birdEndCurrent.Count > 0)
                {

                    for (int z = 0; z < allStand.Count; z++)
                    {
                        var birdEndCurrent = allStand[z].GetBirdSameEnd(isGetLockMove: false);
                        if (birdEndCurrent.Count > 0)
                        {
                            for (int i = 0; i < allStand.Count; i++)
                            {
                                if (allStand[z].ID == allStand[i].ID)
                                    continue;

                                var birdEndTarget = allStand[i].GetBirdSameEnd(isGetLockMove: false);
                                if (birdEndTarget.Count <= 0)
                                {
                                    if (allStand[i].birdsOnArray.Length <= 0)
                                        return allStand[i];
                                }

                                if (birdEndTarget.Count > 0)
                                    if (birdEndTarget[0].id == birdEndCurrent[0].id)
                                    {
                                        return allStand[i];
                                    }
                            }
                        }

                    }
                }
            }

            return null;
        }

        public StandingBase GetStandCanSort(int idStand)
        {
            var allStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
            // if (standing != null)
            {
                //  var birdEndCurrent = standing.GetBirdSameEnd(isGetLockMove: false);

                //if (birdEndCurrent.Count > 0)
                {


                    var birdEndCurrent = allStand[idStand].GetBirdSameEnd(isGetLockMove: false);
                    if (birdEndCurrent.Count > 0)
                    {
                        for (int i = 0; i < allStand.Count; i++)
                        {
                            if (allStand[idStand].ID == allStand[i].ID)
                                continue;

                            var birdEndTarget = allStand[i].GetBirdSameEnd(isGetLockMove: false);
                            if (birdEndTarget.Count <= 0)
                            {
                                if (allStand[i].birdsOnArray.Length <= 0)
                                    return allStand[i];
                            }

                            if (birdEndTarget.Count > 0)
                                if (birdEndTarget[0].id == birdEndCurrent[0].id)
                                {
                                    return allStand[i];
                                }
                        }
                    }
                }
            }

            return null;
        }

        public StandingBase GetStandCanSort2(int idStand, bool isFindStandEmpty = true)
        {
            //var standChoice = GetPosHint2(idStand);
            //if (standChoice != null)
            //    return standChoice;

            bool isFindMeSuccess = false;
            var allStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;
            BirdBase[] birdInGame = GameObject.FindObjectsOfType<BirdBase>();
            //Tìm cho idStand trước
            var birdEndMeCurrent = allStand[idStand].GetBirdSameEnd(isGetLockMove: false);

            if (birdEndMeCurrent.Count > 0)
            {
                bool isTypeBirdLock = false;

                if (birdInGame != null)
                {
                    for (int i = 0; i < birdInGame.Length; i++)
                    {
                        if (birdInGame[i].id == birdEndMeCurrent[0].id && birdInGame[i].isLockMove)
                            return null;
                    }
                }

                if (!isTypeBirdLock)
                {
                    if (!birdEndMeCurrent[0].isLockMove)
                    {
                        for (int i = 0; i < allStand.Count; i++)
                        {
                            if (allStand[idStand].ID == allStand[i].ID)
                                continue;

                            var birdEndTarget = allStand[i].GetBirdSameEnd(isGetLockMove: false);
                            if (birdEndTarget.Count <= 0)
                            {
                                continue;
                                //return allStand[i];
                            }
                            if (allStand[i].birdsOnArray.Length >= (int)allStand[i].data.numSlot)
                                continue;

                            if (birdEndTarget.Count > 0)
                                if (birdEndTarget[0].id == -100 || birdEndTarget[0].id == birdEndMeCurrent[0].id)
                                {
                                    return allStand[i];
                                }
                        }
                    }
                }
            }


            for (int t = 0; t < allStand.Count; t++)
            {
                var birdEndCurrent = allStand[t].GetBirdSameEnd(isGetLockMove: false);

                if (birdEndCurrent.Count > 0)
                {
                    if (birdInGame != null)
                    {
                        for (int i = 0; i < birdInGame.Length; i++)
                        {
                            if (birdInGame[i].id == birdEndCurrent[0].id && birdInGame[i].isLockMove)
                                continue;
                        }
                    }
                    if (birdEndCurrent[0].isLockMove)
                        continue;
                    for (int i = 0; i < allStand.Count; i++)
                    {
                        if (allStand[t].ID == allStand[i].ID)
                            continue;


                        var birdEndTarget = allStand[i].GetBirdSameEnd(isGetLockMove: false);
                        if (birdEndTarget.Count <= 0)
                        {
                            continue;
                        }
                        if (allStand[i].birdsOnArray.Length >= (int)allStand[i].data.numSlot)
                            continue;

                        if (birdEndTarget.Count > 0)
                            if (birdEndTarget[0].id == -100 || birdEndTarget[0].id == birdEndCurrent[0].id)
                            {
                                //if (idStand == allStand[i].ID)
                                return allStand[t];
                                //else
                                //    return allStand[i];
                            }
                    }
                }
            }


            if (isFindStandEmpty)
            {
                for (int t = 0; t < allStand.Count; t++)
                {
                    var birdEndCurrent = allStand[t].GetBirdSameEnd(isGetLockMove: false);

                    if (birdEndCurrent.Count > 0)
                    {
                        if (birdInGame != null)
                        {
                            for (int i = 0; i < birdInGame.Length; i++)
                            {
                                if (birdInGame[i].id == birdEndCurrent[0].id && birdInGame[i].isLockMove)
                                    continue;
                            }
                        }
                        if (birdEndCurrent[0].isLockMove)
                            continue;
                        for (int i = 0; i < allStand.Count; i++)
                        {
                            if (allStand[t].ID == allStand[i].ID)
                                continue;


                            var birdEndTarget = allStand[i].GetBirdSameEnd(isGetLockMove: false);

                            //if (birdEndTarget.Count >= (int)allStand[i].data.numSlot)
                            //    continue;

                            if (allStand[i].birdsOnArray.Length <= 0)
                            {
                                return allStand[i];
                            }

                            if (birdEndTarget.Count > 0)
                                if (birdEndTarget[0].id == -100 || birdEndTarget[0].id == birdEndCurrent[0].id)
                                {
                                    return allStand[i];
                                }
                        }
                    }
                }
            }

            for (int t = 0; t < allStand.Count; t++)
            {
                var birdEndCurrent = allStand[t].GetBirdSameEnd(isGetLockMove: false);

                if (birdEndCurrent.Count > 0)
                {
                    if (birdInGame != null)
                    {
                        for (int i = 0; i < birdInGame.Length; i++)
                        {
                            if (birdInGame[i].id == birdEndCurrent[0].id && birdInGame[i].isLockMove)
                                continue;
                        }
                    }
                    if (birdEndCurrent[0].isLockMove)
                        continue;
                    for (int i = 0; i < allStand.Count; i++)
                    {
                        if (allStand[t].ID == allStand[i].ID)
                            continue;


                        var birdEndTarget = allStand[i].GetBirdSameEnd();

                        //if (birdEndTarget.Count >= (int)allStand[i].data.numSlot)
                        //    continue;

                        if (allStand[i].birdsOnArray.Length <= 0)
                        {
                            // return allStand[i];
                        }

                        if (birdEndTarget.Count > 0)
                            if (birdEndTarget[0].id == -100)
                            {
                                return allStand[i];
                            }
                    }
                }
            }

            for (int t = 0; t < allStand.Count; t++)
            {
                var birdEndCurrent = allStand[t].GetBirdSameEnd(isGetLockMove: false);

                if (birdEndCurrent.Count > 0)
                {
                    if (birdInGame != null)
                    {
                        for (int i = 0; i < birdInGame.Length; i++)
                        {
                            if (birdInGame[i].id == birdEndCurrent[0].id && birdInGame[i].isLockMove)
                                continue;
                        }
                    }
                    if (birdEndCurrent[0].isLockMove)
                        continue;
                    for (int i = 0; i < allStand.Count; i++)
                    {
                        if (allStand[t].ID == allStand[i].ID)
                            continue;


                        var birdEndTarget = allStand[i].GetBirdSameEnd();

                        //if (birdEndTarget.Count >= (int)allStand[i].data.numSlot)
                        //    continue;

                        if (allStand[i].birdsOnArray.Length <= 0)
                        {
                            return allStand[t];
                        }
                    }
                }
            }

            // var standChoice = GetPosHint2(idStand);
            return null;
            // return standChoice;
        }
    }
}