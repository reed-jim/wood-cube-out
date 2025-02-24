namespace BirdSort2
{
    using System.Collections.Generic;
    using UnityEngine;
    using Newtonsoft.Json;
    using System;
    using Sirenix.OdinInspector;
    using System.IO;
    using System.Linq;
    using UnityEngine.Serialization;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class ToolEditLevelBirdSort : SerializedMonoBehaviour
    {
        public bool isUpdate = false;
        public bool changeID = false;
        [Header("**********INPUT LEVEL*********************************************************************************")]
        public TextAsset text;
        public TypeStand typeStand;
        public int numberBird;

        [FormerlySerializedAs("CurrentID")]
        [FormerlySerializedAs("editId")]
        [HorizontalGroup("Split")]
        public List<int> lsCurrentID;

        [HorizontalGroup("Split")]
        public List<int> lsEditID;

        private JsonConvertTools jsonData; // Đổi JsonConvertTools thành JsonConvertTools
        private Trees lsTrees;


        [Header("**********EDIT EXTRA**********************************************************************************")]
        public LevelConfig levelConfig;

        public int BiR;

        public int BranchCount;

        public float DifficultPoint;

        public string path;

        [Button]
        public void Scan()
        {
            levelConfig = null;
            jsonData = JsonConvert.DeserializeObject<JsonConvertTools>(text.text); // Đọc JsonConvertTools từ JSON
            lsCurrentID = jsonData.standConfig.SelectMany(s => s.idBirds).Distinct().ToList(); // Lấy ID từ standConfig
            lsEditID = new List<int>(lsCurrentID); // Chỉ sao chép danh sách hiện tại
            if (isUpdate)
            {
                lsEditID = jsonData.standConfig.SelectMany(s => s.idBirds).Distinct().ToList();
            }
            else
            {
                lsEditID = lsEditID.Select(id =>
                {
                    var newID = 90 - id * 10;
                    switch (id)
                    {
                        case 0:
                            newID += UnityEngine.Random.Range(0, 4);
                            break;
                        case 1:
                            newID += UnityEngine.Random.Range(0, 6);
                            break;
                        case 2:
                            newID += UnityEngine.Random.Range(0, 6);
                            break;
                        case 3:
                            newID += UnityEngine.Random.Range(0, 4);
                            break;
                        case 4:
                            newID += UnityEngine.Random.Range(0, 6);
                            break;
                        case 5:
                            newID += UnityEngine.Random.Range(0, 6);
                            break;
                        case 6:
                            newID += UnityEngine.Random.Range(0, 6);
                            break;
                        case 7:
                            newID += UnityEngine.Random.Range(0, 6);
                            break;
                        case 8:
                            newID += UnityEngine.Random.Range(0, 7);
                            break;
                        case 9:
                            newID += UnityEngine.Random.Range(0, 8);
                            break;
                        default: break;
                    }
                    return newID;
                }).ToList(); // Thay đổi ID tại đây
            }
            lsCurrentID.Sort();
            lsEditID.Sort();

            typeStand = (TypeStand)jsonData.standConfig.Select(s => s.standData.numSlot)
            .ToList()[0] - 4;
            BiR = (int)typeStand + 4;
            numberBird = jsonData.standConfig.SelectMany(s => s.idBirds).ToList().Count;
        }

        [Button]
        private void SwitchId()
        {
            for (int i = 0; i < jsonData.standConfig.Count; i++)
            {
                for (int j = 0; j < lsCurrentID.Count; j++)
                {
                    for (int k = 0; k < jsonData.standConfig[i].idBirds.Count; k++)
                    {
                        if (jsonData.standConfig[i].idBirds[k] == lsCurrentID[j])
                        {
                            jsonData.standConfig[i].idBirds[k] = lsEditID[j];
                        }
                    }
                }
            }
            Init();
            path = $"Assets/Resources/Levels/Level_{levelConfig.id}.txt";
        }

        [Button]
        private void CalculateDiff()
        {
            path = $"Assets/Resources/Levels/Level_{levelConfig.id}.txt";
            if (levelConfig == null)
            {
                DifficultPoint = -1;
                return;
            }

            numberBird = levelConfig.standConfig.SelectMany(s => s.idBirds).ToList().Count;
            var allIdBirds = levelConfig.standConfig.SelectMany(s => s.idBirds).Distinct().ToList();
            BiR = (int)typeStand + 4;
            var tensCount = allIdBirds
                        .GroupBy(id => id / 10) // Chia cho 10 để lấy hàng chục
                        .Select(group => new
                        {
                            Ten = group.Key,
                            UniqueIds = group.Distinct().Count() // Đếm số loại ID duy nhất
                        })
                        .OrderByDescending(x => x.UniqueIds)
                        .FirstOrDefault();
            // Tính điểm độ khó từ số loại chim
            CalculateDifficulty(allIdBirds.Count, tensCount.UniqueIds, numberBird, BiR);

            foreach (var item in levelConfig.extralsConfig)
            {
                GetExtralTypeScore(item.type);
            }
            lsCurrentID = levelConfig.standConfig.SelectMany(s => s.idBirds).Distinct().ToList();
            lsCurrentID.Sort();
            List<int> Temp = levelConfig.standConfig.SelectMany(s => s.idBirds).ToList();
            Temp.Sort();
            lsEditID = levelConfig.standConfig.SelectMany(s => s.idBirds).Distinct().ToList();
            int count = 0;
            while (count < lsCurrentID.Count)
            {
                int value = 0;
                foreach (var item in Temp)
                {
                    if (item == lsCurrentID[count])
                    {
                        value++;
                    }
                }
                lsEditID[count] = value;
                count++;
            }
        }

        private void GetExtralTypeScore(ExtralPlayType typeName)
        {
            switch (typeName)
            {
                case ExtralPlayType.Boom: DifficultPoint += 20; break;// Điểm cho loại bomb
                case ExtralPlayType.LockBird: DifficultPoint += 20; break; // Điểm cho loại cage
                case ExtralPlayType.EggLock: DifficultPoint += 30; break; // Điểm cho loại egglock
                case ExtralPlayType.SleepBird: DifficultPoint += 40; break; // Điểm cho loại sleepbird
                case ExtralPlayType.LockStand: DifficultPoint += 60; break; // Điểm cho loại lockstand
                default: break; // Trường hợp không xác định
            }
        }

        private void CalculateDifficulty(int birdTypesCount, int stylesCount, int totalBird, int birdInbranch)
        {
            DifficultPoint = 0;
            //DifficultPoint += totalBird * 10;
            switch (birdTypesCount)
            {
                case 1: DifficultPoint += 7.5f; break;
                case 2: DifficultPoint += 15; break;
                case 3: DifficultPoint += 22.5f; break;
                case 4: DifficultPoint += 30; break;
                case 5: DifficultPoint += 37.5f; break;
                case 6: DifficultPoint += 45; break;
                case 7: DifficultPoint += 52.5f; break;
                case 8: DifficultPoint += 60; break;
                case 9: DifficultPoint += 67.5f; break;
                case 10: DifficultPoint += 75; break;
                default: break; // Trường hợp không xác định
            }

            switch (stylesCount)
            {
                case 2: DifficultPoint += 20; break;
                case 3: DifficultPoint += 40; break;
                case 4: DifficultPoint += 60; break;
                case 5: DifficultPoint += 80; break;
                case 6: DifficultPoint += 100; break;
                case 7: DifficultPoint += 120; break;
                default: break; // Trường hợp không xác định
            }

            DifficultPoint += (totalBird * 1.0f / birdTypesCount) * 50f;

            DifficultPoint += birdInbranch * 30f;
        }

        [Button]
        private void Export()
        {
            ExportJson();
        }

        private void Init()
        {
            lsTrees = new Trees();
            for (int j = 0; j < jsonData.standConfig.Count; j++)
            {
                lsTrees.standConfig.Add(new StandConfig { });
            }

            for (int j = 0; j < lsTrees.standConfig.Count; j++)
            {
                for (int o = 0; o < jsonData.standConfig[j].idBirds.Count; o++)
                {
                    lsTrees.standConfig[j].idBirds.Add(0);
                }
            }

            for (int j = 0; j < lsTrees.standConfig.Count; j++)
            {
                for (int o = 0; o < lsTrees.standConfig[j].idBirds.Count; o++)
                {
                    if (jsonData.standConfig.Count > 0)
                    {
                        lsTrees.standConfig[j].idBirds[o] = jsonData.standConfig[j].idBirds[o]; // Ví dụ, lấy ID đầu tiên
                        //jsonData.standConfig[j].idBirds.RemoveAt(0); // Xóa ID đã lấy
                    }
                    else
                    {
                        lsTrees.standConfig[j].idBirds.Clear();
                    }
                }
                lsTrees.standConfig[j].standData = new StandData { };
                lsTrees.standConfig[j].standData.numSlot = BiR;
                lsTrees.standConfig[j].standData.type = typeStand; // Chuyển đổi type

                // Cài đặt side
                if (j % 2 == 0)
                {
                    lsTrees.standConfig[j].side = -1;
                }
                else
                {
                    lsTrees.standConfig[j].side = 1;
                }
            }

            var data = JsonConvert.SerializeObject(lsTrees);
            var temp = JsonConvert.DeserializeObject<LevelConfig>(data);

            BranchCount = lsTrees.standConfig.Count;

            temp.id = jsonData.id == 0 || changeID ? int.Parse(text.name.Split('_')[1]) : jsonData.id;
            temp.idHardMode = jsonData.idHardMode;
            temp.extralsConfig = jsonData.extralsConfig;
            temp.min = jsonData.min == 0 ? 15 : jsonData.min;
            temp.second = jsonData.second == 0 ? 30 : jsonData.second;

            levelConfig = temp;
            string idBirds = "";
            foreach (var item in lsEditID)
            {
                idBirds += item + " |";
            }
            string BranchNull = "";
            foreach (var item in lsTrees.standConfig)
            {
                if (item.idBirds.Count == 0)
                {
                    BranchNull += lsTrees.standConfig.IndexOf(item) + " |";
                }
            }

            Debug.Log($"<color=yellow>Data: ID chim {idBirds}</color>");
            Debug.Log($"<color=yellow>Data: Cành trống {BranchNull}</color>");

            CalculateDiff();
        }

        private void ExportJson()
        {
            string mess = "";
            if (CheckDataBeforeExport(levelConfig, ref mess))
            {
                var data = JsonUtility.ToJson(levelConfig);
                File.WriteAllText(path, data);
            }
            Debug.Log(mess);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        bool CheckDataBeforeExport(LevelConfig _levelConfig, ref string _mes)
        {
            bool checkValid = true;
            foreach (var item in _levelConfig.standConfig)
            {
                foreach (var i1 in item.idBirds)
                {
                    if (!validateID(i1, (int)Math.Floor((decimal)i1 / 10), ref _mes))
                    {
                        checkValid = false;
                    }
                }
            }

            foreach (var item in _levelConfig.extralsConfig)
            {
                if (!validExtral(item, _levelConfig, ref _mes))
                {
                    _mes += $"\n<color=red> Lỗi ở Yếu tố đặc biệt thứ {_levelConfig.extralsConfig.IndexOf(item)} </color>\n";
                    checkValid = false;
                }
            }

            if (checkValid)
            {
                _mes = $"\n<color=#66FF00>Export success</color>\n";
            }
            return checkValid;
        }

        bool validExtral(ExtralPlayData _extralConfig, LevelConfig _levelConfig, ref string _mess)
        {
            bool check = true;
            List<int> lstID = _levelConfig.standConfig.SelectMany(s => s.idBirds).Distinct().ToList();

            List<int> BranchNull = new List<int>();
            foreach (var item in _levelConfig.standConfig)
            {
                if (item.idBirds.Count == 0)
                {
                    BranchNull.Add(_levelConfig.standConfig.IndexOf(item));
                }
            }

            if (BranchNull.Contains(_extralConfig.positionData.indexStand))
            {
                _mess += $"\n<color=red> Yếu tố đặc biệt đang đặt trên cành trống {_extralConfig.positionData.indexStand} </color>\n";
                check = false;
            }

            if (_extralConfig.positionData.indexBird > (int)typeStand + 4)
            {
                _mess += $"\n<color=red> Chim đang đứng ngoài cành cần phải nhỏ hơn {(int)typeStand + 4 - 1} </color>\n";
                check = false;
            }
            int count = _extralConfig.extralValues.Length;
            switch (_extralConfig.type)
            {
                case ExtralPlayType.None:
                    break;
                case ExtralPlayType.Boom:
                    break;
                case ExtralPlayType.LockBird:
                    if (count == 0)
                    {
                        _mess += $"\n<color=red> Chưa có chim để mở khóa </color>\n";
                        check = false;
                    }
                    if (!lstID.Contains((int)_extralConfig.extralValues[0]))
                    {
                        _mess += $"\n<color=red> ID chim {(int)_extralConfig.extralValues[0]} không tồn tại </color>\n";
                        check = false;
                    }
                    break;
                case ExtralPlayType.EggLock:
                    if (count == 0)
                    {
                        _mess += $"\n<color=red> Chưa có chim để mở trứng </color>\n";
                        check = false;
                    }
                    for (var i = 0; i < count; i += 2)
                    {
                        if (!lstID.Contains((int)_extralConfig.extralValues[i]))
                        {
                            _mess += $"\n<color=red> ID chim {(int)_extralConfig.extralValues[0]} không tồn tại </color>\n";
                            check = false;
                        }
                        if ((int)_extralConfig.extralValues[i] == _levelConfig.standConfig[_extralConfig.positionData.indexStand].idBirds[_levelConfig.standConfig[_extralConfig.positionData.indexStand].idBirds.Count - 1 - _extralConfig.positionData.indexBird])
                        {
                            _mess += $"\n<color=red> Chim <color=yellow> BÚA </color> đang trong trứng {(int)_extralConfig.extralValues[i]} </color>\n";
                            check = false;
                        }
                    }
                    break;
                case ExtralPlayType.SleepBird:
                    if (count == 0)
                    {
                        _mess += $"\n<color=red> Chưa có chim để đánh thức </color>\n";
                        check = false;
                    }
                    for (var i = 0; i < count; i += 2)
                    {
                        if ((int)_extralConfig.extralValues[i] > (int)typeStand + 4)
                        {
                            _mess += $"\n<color=red> Chim <color=yellow> ĐỒNG HỒ </color> đang đứng ngoài cành cần phải nhỏ hơn {(int)typeStand + 4 - 1} </color>\n";
                            check = false;
                        }
                        if ((int)_extralConfig.extralValues[i] == _levelConfig.standConfig[_extralConfig.positionData.indexStand].idBirds[_levelConfig.standConfig[_extralConfig.positionData.indexStand].idBirds.Count - 1 - _extralConfig.positionData.indexBird])
                        {
                            _mess += $"\n<color=red> Chim <color=yellow> ĐỒNG HỒ </color> đang ngủ {(int)_extralConfig.extralValues[i]} </color>\n";
                            check = false;
                        }
                        if (BranchNull.Contains((int)_extralConfig.extralValues[i + 1]))
                        {
                            _mess += $"\n<color=red> Yếu tố đặc biệt đang đặt trên cành trống {_extralConfig.positionData.indexStand} </color>\n";
                            check = false;
                        }
                        if ((int)_extralConfig.extralValues[i + 1] > BranchCount)
                        {
                            _mess += $"\n<color=red> Không có cành phù hợp phải nhỏ hơn {BranchCount} </color>\n";
                            check = false;
                        }
                    }
                    break;
                case ExtralPlayType.LockStand:
                    if (count == 0)
                    {
                        _mess += $"\n<color=red> Chưa có chim để mở khóa </color>\n";
                        check = false;
                    }
                    for (var i = 0; i < count; i++)
                    {
                        if (i == 0 || i % 2 == 0)
                        {
                            if ((int)_extralConfig.extralValues[i] > (int)typeStand + 4)
                            {
                                _mess += $"\n<color=red> Chim <color=yellow> KHÓA </color> đang đứng ngoài cành cần phải nhỏ hơn {(int)typeStand + 4 - 1} </color>\n";
                                check = false;
                            }
                        }
                        else
                        {
                            if (BranchNull.Contains((int)_extralConfig.extralValues[i]))
                            {
                                _mess += $"\n<color=red> Yếu tố đặc biệt đang đặt trên cành trống {_extralConfig.positionData.indexStand} </color>\n";
                                check = false;
                            }
                            if ((int)_extralConfig.extralValues[i] > BranchCount)
                            {
                                _mess += $"\n<color=red> Không có cành phù hợp phải nhỏ hơn {BranchCount} </color>\n";
                                check = false;
                            }
                        }
                    }
                    break;
            }

            return check;
        }

        bool validateID(int id, int type, ref string _mess)
        {
            bool check = true;
            int bound = 0;
            switch (type)
            {
                case 0:
                    if (id - type * 10 > 7)
                        check = false;
                    bound = 7;
                    break;
                case 1:
                    if (id - type * 10 > 6)
                        check = false;
                    bound = 6;
                    break;
                case 2:
                    if (id - type * 10 > 5)
                        check = false;
                    bound = 5;
                    break;
                case 3:
                    if (id - type * 10 > 5)
                        check = false;
                    bound = 5;
                    break;
                case 4:
                    if (id - type * 10 > 5)
                        check = false;
                    bound = 5;
                    break;
                case 5:
                    if (id - type * 10 > 5)
                        check = false;
                    bound = 5;
                    break;
                case 6:
                    if (id - type * 10 > 3)
                        check = false;
                    bound = 3;
                    break;
                case 7:
                    if (id - type * 10 > 5)
                        check = false;
                    bound = 5;
                    break;
                case 8:
                    if (id - type * 10 > 5)
                        check = false;
                    bound = 5;
                    break;
                case 9:
                    if (id - type * 10 > 3)
                        check = false;
                    bound = 3;
                    break;
                default:
                    check = false;
                    break;

            }
            if (!check) { _mess += $"\n<color=red>Lỗi ID không có chim phù hợp với {id} chỉ có thể sử dụng ID nhỏ hơn {type}{bound}</color>\n"; }
            return check;
        }
    }

    [Serializable]
    public class Trees
    {
        public List<StandConfig> standConfig = new List<StandConfig>();
    }

    [Serializable]
    public class JsonConvertTools
    {
        public int id { get; set; }
        public int idHardMode { get; set; }
        public List<StandConfig> standConfig { get; set; } = new List<StandConfig>();
        public List<ExtralPlayData> extralsConfig { get; set; } = new List<ExtralPlayData>();
        public int min { get; set; }
        public int second { get; set; }
    }

    [Serializable]
    public class PositionData
    {
        public int posType { get; set; }
        public int indexStand { get; set; }
        public int indexBird { get; set; }
        public LocalPosition localPosition { get; set; }
    }

    [Serializable]
    public class LocalPosition
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    [Serializable]
    public class ParentData
    {
        public int instanceID { get; set; }
    }
}