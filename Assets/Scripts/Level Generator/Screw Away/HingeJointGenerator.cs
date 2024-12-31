#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static GameEnum;

public class HingeJointGenerator : EditorWindow
{
    private GameObject target;
    private GameObject screwPrefab;
    private Material targetMaterial;



    private Vector3 dimensionSize;
    private Vector3 distance;
    private float expectedScale;
    private Vector3 expectedLocalPositionOffset;
    private Vector3 expectedLocalRotationOffset;



    private string prefabPath = "Assets/Prefabs/MyPrefab.prefab";

    [MenuItem("Saferio/Tools/Hinge Joint Generator")]
    public static void ShowWindow()
    {
        GetWindow<HingeJointGenerator>("Hinge Joint Generator");
    }

    private void OnGUI()
    {
        float padding = 20f;

        Rect areaRect = new Rect(padding, padding, position.width - 2 * padding, position.height - 2 * padding);

        GUILayout.BeginArea(areaRect);

        prefabPath = EditorGUILayout.TextField("Prefab Path", prefabPath);

        target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
        screwPrefab = (GameObject)EditorGUILayout.ObjectField("Screw Prefab", screwPrefab, typeof(GameObject), true);
        targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), true);

        dimensionSize = EditorGUILayout.Vector3Field("Dimension Size", dimensionSize);
        distance = EditorGUILayout.Vector3Field("Distance", distance);
        expectedScale = EditorGUILayout.FloatField("Expected Scale", expectedScale);

        expectedLocalPositionOffset = EditorGUILayout.Vector3Field("Expected Local Position Offset", expectedLocalPositionOffset);
        expectedLocalRotationOffset = EditorGUILayout.Vector3Field("Expected Local Rotation Offset", expectedLocalRotationOffset);

        if (GUILayout.Button("Generate"))
        {
            Generate(prefabPath);
        }

        if (GUILayout.Button("Generate All"))
        {
            GenerateHingeJointAll(prefabPath);
        }

        if (GUILayout.Button("Reset Local Rotation"))
        {
            ResetLocalRotation(target.transform);
        }

        if (GUILayout.Button("Flip Horizontally"))
        {
            FlipHorizontally(Selection.activeTransform, prefabPath);
        }

        if (GUILayout.Button("Flip Vertically"))
        {
            FlipVertically(Selection.activeTransform, prefabPath);
        }

        if (GUILayout.Button("Move"))
        {
            Move(Selection.activeTransform, prefabPath);
        }

        if (GUILayout.Button("Auto Assign Screw Faction"))
        {
            AutoAssignScrewFaction(prefabPath);
        }

        if (GUILayout.Button("Spawn Screw"))
        {
            SpawnScrew(prefabPath);
        }

        if (GUILayout.Button("Assign Material"))
        {
            AssignMaterial(prefabPath);
        }

        if (GUILayout.Button("Center Level Pivot"))
        {
            CenterLevelPivot(prefabPath);
        }

        if (GUILayout.Button("Remove All Grandchildren Of"))
        {
            RemoveAllGrandchildrenOf();
        }

        if (GUILayout.Button("Get Screw Number By Phase"))
        {
            GetScrewNumberByPhase(prefabPath);
        }

        GUILayout.EndArea();
    }

    private void CenterLevelPivot(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        Vector3 averagePostition = new Vector3();

        for (int i = 0; i < objectParts.Length; i++)
        {
            averagePostition += objectParts[i].transform.position;
        }

        averagePostition /= objectParts.Length;

        Selection.activeTransform.position = averagePostition;
    }

    private void ResetLocalRotation(Transform target)
    {
        target.localPosition = Vector3.zero;
        target.localRotation = Quaternion.Euler(Vector3.zero);
        target.localScale = new Vector3(1 / target.transform.parent.localScale.x, 1 / target.transform.parent.localScale.y, 1 / target.transform.parent.localScale.z);
    }

    private void FlipHorizontally(Transform target, string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Transform flippedObject = Instantiate(target, levelPrefab.transform);

        flippedObject.name = $"{target.name.Replace("- Generated", "")} - Generated";

        Vector3 position = target.transform.position;

        position.y *= -1;

        Vector3 eulerAngle = target.transform.rotation.eulerAngles;

        eulerAngle.y *= -1;

        flippedObject.transform.position = position;
        flippedObject.transform.rotation = Quaternion.Euler(eulerAngle);

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void FlipVertically(Transform target, string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Transform flippedObject = Instantiate(target, levelPrefab.transform);

        flippedObject.name = $"{target.name.Replace("- Generated", "")} - Generated";

        Vector3 position = target.transform.position;

        position.x *= -1;

        Vector3 eulerAngle = target.transform.rotation.eulerAngles;

        eulerAngle.x *= -1;

        flippedObject.transform.position = position;
        flippedObject.transform.rotation = Quaternion.Euler(eulerAngle);

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void Move(Transform target, string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Transform clonedObject = Instantiate(target, levelPrefab.transform);

        clonedObject.name = $"{target.name.Replace("- Generated", "")} - Generated";

        clonedObject.transform.position += distance.x * clonedObject.transform.right;
        clonedObject.transform.position += distance.y * clonedObject.transform.up;
        clonedObject.transform.position += distance.z * clonedObject.transform.forward;

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void Generate(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        RemoveOldHingeJoints(target);

        for (int i = 0; i < target.transform.childCount; i++)
        {
            BasicScrew screw = target.transform.GetChild(i).GetComponent<BasicScrew>();

            if (screw != null)
            {
                HingeJoint joint = target.AddComponent<HingeJoint>();

                // WARNING: HingeJoint axis gizmos may not update correctly, so be sure to double-check it.

                joint.connectedBody = screw.GetComponent<Rigidbody>();
                joint.anchor = screw.transform.localPosition;

                // CRUCIAL
                joint.axis = screw.transform.localRotation * Vector3.forward;

                screw.Joint = joint;
            }
        }

        EditorUtility.SetDirty(target);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void RemoveOldHingeJoints(GameObject objectPart)
    {
        // REMOVE OLD HINGE JOINTS
        HingeJoint[] hingeJoints = objectPart.GetComponents<HingeJoint>();

        for (int I = 0; I < hingeJoints.Length; I++)
        {
            DestroyImmediate(hingeJoints[I]);
        }
    }

    private void GenerateHingeJointAll(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        for (int i = 0; i < objectParts.Length; i++)
        {
            RemoveOldHingeJoints(objectParts[i].gameObject);

            GenerateHingeJoint(objectParts[i].gameObject);
        }

        // for (int i = 0; i < levelPrefab.transform.childCount; i++)
        // {
        //     GameObject target = levelPrefab.transform.GetChild(i).gameObject;

        //     BasicObjectPart objectPart = target.transform.GetComponent<BasicObjectPart>();

        //     if (objectPart != null)
        //     {
        //         // REMOVE OLD HINGE JOINTS
        //         RemoveOldHingeJoints(objectPart.gameObject);

        //         GenerateHingeJoint(target);
        //     }
        // }

        void GenerateHingeJoint(GameObject target)
        {
            for (int i = 0; i < target.transform.childCount; i++)
            {
                BasicScrew screw = target.transform.GetChild(i).GetComponent<BasicScrew>();

                if (screw != null)
                {
                    HingeJoint joint = target.AddComponent<HingeJoint>();

                    // WARNING: HingeJoint axis gizmos may not update correctly, so be sure to double-check it.

                    joint.connectedBody = screw.GetComponent<Rigidbody>();
                    joint.anchor = screw.transform.localPosition;

                    // CRUCIAL
                    joint.axis = screw.transform.localRotation * Vector3.forward;

                    screw.Joint = joint;
                }
            }
        }

        // for (int i = 0; i < target.transform.childCount; i++)
        // {
        //     BasicScrew screw = target.transform.GetChild(i).GetComponent<BasicScrew>();

        //     if (screw != null)
        //     {
        //         HingeJoint joint = target.AddComponent<HingeJoint>();

        //         // WARNING: HingeJoint axis gizmos may not update correctly, so be sure to double-check it.

        //         joint.connectedBody = screw.GetComponent<Rigidbody>();
        //         joint.anchor = screw.transform.localPosition;

        //         // CRUCIAL
        //         joint.axis = screw.transform.localRotation * Vector3.forward;

        //         screw.Joint = joint;
        //     }
        // }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void AutoAssignScrewFaction(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BaseScrew[] screws = GetComponentsFromAllChildren<BaseScrew>(levelPrefab.transform).ToArray();

        int currentFaction = 0;

        List<GameFaction> remainingFactionForScrews = new List<GameFaction>();

        for (int i = 0; i < screws.Length; i++)
        {
            remainingFactionForScrews.Add(GameConstants.SCREW_FACTION[currentFaction]);

            if (i > 0 && (i + 1) % 3 == 0)
            {
                currentFaction++;

                if (currentFaction >= GameConstants.SCREW_FACTION.Length)
                {
                    currentFaction = 0;
                }
            }
        }

        for (int i = 0; i < screws.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, remainingFactionForScrews.Count);

            screws[i].ScrewId = i;
            screws[i].Faction = remainingFactionForScrews[randomIndex];

            remainingFactionForScrews.RemoveAt(randomIndex);
        }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void SpawnScrew(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        Vector3 position = new Vector3();

        int currentX = 0;
        int currentY = 0;
        int currentZ = 0;

        dimensionSize.x = Mathf.Max(dimensionSize.x, 1);
        dimensionSize.y = Mathf.Max(dimensionSize.y, 1);
        dimensionSize.z = Mathf.Max(dimensionSize.z, 1);

        Transform target = Selection.activeTransform;

        for (int i = 0; i < dimensionSize.x * dimensionSize.y * dimensionSize.z; i++)
        {
            GameObject screw = (GameObject)PrefabUtility.InstantiatePrefab(screwPrefab, target.transform);

            position.x = (-(dimensionSize.x - 1) / 2f + currentX) * distance.x;
            position.y = (-(dimensionSize.y - 1) / 2f + currentY) * distance.y;
            position.z = (-(dimensionSize.z - 1) / 2f + currentZ) * distance.z;

            currentX++;

            if (currentX >= dimensionSize.x)
            {
                currentY++;

                if (currentY >= dimensionSize.y)
                {
                    currentZ++;

                    currentY = 0;
                }

                currentX = 0;
            }

            // for (int j = 0; j < dimensionSize.x; j++)
            // {
            //     position.x = (-(dimensionSize.x - 1) / 2f + currentX) * distance.x + target.transform.position.x;
            // }

            // for (int j = 0; j < dimensionSize.y; j++)
            // {
            //     position.y = (-(dimensionSize.y - 1) / 2f + currentY) * distance.y + target.transform.position.y;
            // }

            // for (int j = 0; j < dimensionSize.z; j++)
            // {
            //     position.z = (-(dimensionSize.z - 1) / 2f + currentZ) * distance.z + target.transform.position.z;
            // }

            screw.transform.localPosition = position + expectedLocalPositionOffset;
            screw.transform.localEulerAngles += expectedLocalRotationOffset;
            screw.transform.localScale = TransformUtil.ComponentWiseDivine(expectedScale * Vector3.one, target.transform.localScale);
        }




        // for (int i = 0; i < dimensionSize.x; i++)
        // {
        //     for (int j = 0; j < dimensionSize.y; j++)
        //     {
        //         GameObject screw = (GameObject)PrefabUtility.InstantiatePrefab(screwPrefab, target.transform);

        //         Vector3 position;

        //         position.x = (-(dimensionSize.x - 1) / 2f + j) * distance.x;
        //         position.y = (-(dimensionSize.y - 1) / 2f + i) * distance.y;
        //         position.z = (-(dimensionSize.y - 1) / 2f + i) * distance.z;

        //         position += target.transform.position;

        //         screw.transform.position = position;
        //         screw.transform.localScale = TransformUtil.ComponentWiseDivine(expectedScale * Vector3.one, target.transform.localScale);
        //     }
        // }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    private void AssignMaterial(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        BasicObjectPart[] objectParts = GetComponentsFromAllChildren<BasicObjectPart>(levelPrefab.transform).ToArray();

        for (int i = 0; i < objectParts.Length; i++)
        {
            MeshRenderer meshRenderer = objectParts[i].GetComponent<MeshRenderer>();

            Debug.Log(meshRenderer);

            List<Material> materials = meshRenderer.materials.ToList();

            materials[0] = targetMaterial;

            meshRenderer.SetMaterials(materials);
        }

        EditorUtility.SetDirty(levelPrefab);

        PrefabUtility.SaveAsPrefabAsset(levelPrefab, path);
    }

    #region REMOVE
    private void RemoveAllGrandchildrenOf()
    {
        List<GameObject> toRemoveList = new List<GameObject>();

        for (int i = 0; i < Selection.activeTransform.childCount; i++)
        {
            for (int j = 0; j < Selection.activeTransform.GetChild(i).childCount; j++)
            {
                toRemoveList.Add(Selection.activeTransform.GetChild(i).GetChild(j).gameObject);
            }
        }

        foreach (var item in toRemoveList)
        {
            DestroyImmediate(item);
        }

        EditorUtility.SetDirty(Selection.activeTransform);
    }
    #endregion

    #region INFO
    private void GetScrewNumberByPhase(string path)
    {
        GameObject levelPrefab = PrefabUtility.LoadPrefabContents(path);

        MultiPhaseScrew[] screws = GetComponentsFromAllChildren<MultiPhaseScrew>(levelPrefab.transform).ToArray();

        Dictionary<int, int> numberScrewByPhase = new Dictionary<int, int>();

        for (int i = 0; i < screws.Length; i++)
        {
            if (numberScrewByPhase.ContainsKey(screws[i].Phase))
            {
                numberScrewByPhase[screws[i].Phase]++;
            }
            else
            {
                numberScrewByPhase.Add(screws[i].Phase, 1);
            }
        }

        foreach (var phase in numberScrewByPhase.Keys)
        {
            Debug.Log($"Phase {phase}: {numberScrewByPhase[phase]}");
        }
    }
    #endregion

    #region UTIL
    public List<T> GetComponentsFromAllChildren<T>(Transform parent) where T : Component
    {
        List<T> components = new List<T>();
        GetComponentsFromAllChildrenRecursive<T>(parent, components);
        return components;
    }

    private void GetComponentsFromAllChildrenRecursive<T>(Transform parent, List<T> components) where T : Component
    {
        T component = parent.GetComponent<T>();
        if (component != null)
        {
            components.Add(component);
        }

        foreach (Transform child in parent)
        {
            GetComponentsFromAllChildrenRecursive<T>(child, components);
        }
    }
    #endregion
}
#endif
