namespace BirdSort2
{
    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ShuffleController : MonoBehaviour
    {

        //public List<CloneDataShuffle> lsCloneData;
        Level_BirdSort_2 level;
        public void Init()
        {
            level = GamePlayController_BirdSort.Instance.level;
        }
        public void HandleShuffle()
        {
            
        }
    }
  
}