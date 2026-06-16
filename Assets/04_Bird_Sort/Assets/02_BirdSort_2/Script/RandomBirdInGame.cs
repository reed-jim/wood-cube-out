namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class RandomBirdInGame : MonoBehaviour
    {
        bool swapBranch = false;
        [SerializeField] private List<int> TypeCount;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public LevelConfig DoRandomList(LevelConfig sample)
        {
            int RandomNum = Random.Range(0, TypeCount.Count);
            //int rand = Random.Range(0, 2);
            Dictionary<int, List<int>> availableUnits = new Dictionary<int, List<int>>()
{
    { 0,  GetUniqueRandoms(TypeCount[0]) },
    { 10, GetUniqueRandoms(TypeCount[1]) },
    { 20, GetUniqueRandoms(TypeCount[2]) },
    { 30, GetUniqueRandoms(TypeCount[3]) },
    { 40, GetUniqueRandoms(TypeCount[4]) },
    { 50, GetUniqueRandoms(TypeCount[5]) },
    { 60, GetUniqueRandoms(TypeCount[6]) },
    { 70, GetUniqueRandoms(TypeCount[7]) },
    { 80, GetUniqueRandoms(TypeCount[8]) },
    { 90, GetUniqueRandoms(TypeCount[9]) },
};


            Dictionary<int, int> idMap = new Dictionary<int, int>(); // oldId -> newId

            foreach (var stand in sample.standConfig)
            {
                for (int i = 0; i < stand.idBirds.Count; i++)
                {
                    int oldId = stand.idBirds[i];

                    if (idMap.ContainsKey(oldId))
                    {
                        // Nếu đã map rồi thì gán thẳng
                        stand.idBirds[i] = idMap[oldId];
                    }
                    else
                    {
                        int tens = (oldId / 10) * 10;   // lấy hàng chục
                        int num = oldId % 10;// lấy hàng đơn vị
                        int tensNew = tens + RandomNum * 10;
                        if (tensNew >= 100)
                        {
                            tensNew -= 100;
                        }

                        if (!availableUnits.ContainsKey(tensNew) || availableUnits[tensNew].Count == 0)
                        {
                            // fallback nếu nhóm hết số
                            tensNew = availableUnits.Keys.First(k => availableUnits[k].Count > 0);
                        }

                        // chọn đơn vị từ nhóm tensNew
                        // ⚠️ Nếu bạn muốn đảm bảo đơn vị giữ nguyên (num), thì check xem có trong kho không
                        int newUnit = getAvailableNum(ref availableUnits, tensNew, num);
                        int newId = tensNew + newUnit;
                        idMap[oldId] = newId;        // lưu map lại
                        stand.idBirds[i] = newId;    // gán vào
                    }
                }
                //if (rand == 1)
                //{
                //    stand.side *= -1;
                //}
            }

            foreach (var extral in sample.extralsConfig)
            {
                if (extral.type == ExtralPlayType.LockBird)
                {
                    for (int i = 0; i < extral.extralValues.Length; i++)
                    {
                        int oldId = (int)extral.extralValues[i];
                        if (idMap.ContainsKey(oldId))
                        {
                            // Nếu đã map rồi thì gán thẳng
                            extral.extralValues[i] = idMap[oldId];
                        }
                    }
                }
                if (extral.type == ExtralPlayType.EggLock)
                {
                    for (int i = 0; i < extral.extralValues.Length; i += 2)
                    {
                        int oldId = (int)extral.extralValues[i];
                        if (idMap.ContainsKey(oldId))
                        {
                            // Nếu đã map rồi thì gán thẳng
                            extral.extralValues[i] = idMap[oldId];
                        }
                    }
                }

            }


            return sample;
        }

        int getAvailableNum(ref Dictionary<int, List<int>> lst, int category, int number)
        {
            int result = 0;
            if (lst[category].Count > number && lst[category][number] != -1)
            {
                result = lst[category][number];
                lst[category][number] = -1;
                return result;
            }
            else
            {
                // Nếu đơn vị cũ không hợp lệ thì random đơn vị khác trong nhóm
                int idx = Random.Range(0, lst[category].Count);
                return getAvailableNum(ref lst, category, idx);
            }
        }

        List<int> GetUniqueRandoms(int count)
        {
            // Tạo list từ 0..9
            List<int> pool = Enumerable.Range(0, count).ToList();

            // Xáo trộn
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int randIndex = Random.Range(0, i + 1);
                int temp = pool[i];
                pool[i] = pool[randIndex];
                pool[randIndex] = temp;
            }

            // Trả về đúng số lượng cần
            return pool.Take(count).ToList();
        }
    }
}
