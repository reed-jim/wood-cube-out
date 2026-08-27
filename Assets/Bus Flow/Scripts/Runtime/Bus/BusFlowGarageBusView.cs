using Spine.Unity;
using TMPro;
using UnityEngine;

namespace BusFlow
{
    // 1 xe bus trong bãi đỗ (chưa chạy trên map). Có 2 chế độ hình:
    // - Prefab mode: dùng lại prefab Ant Box (washing machine) của Ants Flow làm placeholder theo yêu
    //   cầu. Component WashingMachineItem bị Destroy ngay sau Instantiate (nó gắn chặt vào các singleton
    //   Ants Flow), chỉ giữ lại phần hình: Spine SkeletonAnimation (skin đổi theo faction), SpriteRenderer lid + TMP số.
    // - Primitive mode: fallback cube 3D khi chưa gán prefab.
    // Số lượng xe trong bãi rất nhỏ (vài chục) nên click-pick từng xe là bình thường, khác hành khách.
    public class BusFlowGarageBusView : MonoBehaviour
    {
        public Color faction;
        public int number;
        public int column;
        public int row;

        // Prefab mode (Ant Box): tham chiếu Spine / lid / số lấy được từ WashingMachineItem.
        private SkeletonAnimation externalSpine;
        private SpriteRenderer externalLid;
        private TMP_Text externalNumberText;

        // Primitive mode.
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock materialPropertyBlock;
        private TextMeshPro fallbackNumberLabel;

        public void Setup(Color busFaction, int busNumber, int busColumn, int busRow, Material overrideMaterial)
        {
            faction = busFaction;
            number = busNumber;
            column = busColumn;
            row = busRow;

            if (externalSpine != null || externalLid != null)
            {
                // Prefab mode: đổi skin Spine theo faction, đổi màu lid và text số
                if (externalSpine != null && externalSpine.Skeleton != null)
                {
                    string skinName = FactionMapper.Ins != null ? FactionMapper.Ins.GetAntBoxSkin(faction) : null;
                    if (!string.IsNullOrEmpty(skinName))
                    {
                        var skin = externalSpine.Skeleton.Data.FindSkin(skinName);
                        if (skin != null)
                        {
                            externalSpine.Skeleton.SetSkin(skin);
                            externalSpine.Skeleton.SetSlotsToSetupPose();
                            externalSpine.AnimationState?.Apply(externalSpine.Skeleton);
                        }
                    }
                }

                if (externalLid != null)
                {
                    externalLid.color = faction;
                }

                if (externalNumberText != null)
                {
                    externalNumberText.text = number.ToString();
                }

                return;
            }

            // Primitive mode.
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            BusFlowVisualFactory.ApplyFactionVisual(meshRenderer, faction, overrideMaterial, ref materialPropertyBlock);

            if (fallbackNumberLabel == null)
            {
                GameObject labelGO = new GameObject("Number");
                labelGO.transform.SetParent(transform, false);
                labelGO.transform.localPosition = new Vector3(0f, 0f, -0.55f);

                fallbackNumberLabel = labelGO.AddComponent<TextMeshPro>();
                fallbackNumberLabel.alignment = TextAlignmentOptions.Center;
                fallbackNumberLabel.fontSize = 4f;
                fallbackNumberLabel.color = Color.white;
            }

            fallbackNumberLabel.text = number.ToString();
        }

        private void OnMouseDown()
        {
            if (BusFlowGameManager.Ins == null)
            {
                return;
            }

            if (BusFlowGameManager.Ins.IsBooster1ModeActive)
            {
                BusGarageManager.Ins.SolveGarageBus(this);

                return;
            }

            BusGarageManager.Ins.TryPickBus(column);
        }

        // Fallback khi chưa gán prefab Ant Box: cube 3D + collider để click.
        public static BusFlowGarageBusView CreatePlaceholder(Transform parent)
        {
            GameObject go = BusFlowVisualFactory.CreatePlaceholder(
                PrimitiveType.Cube,
                parent,
                new Vector3(0.9f, 0.5f, 1.4f),
                keepCollider: true);

            go.name = "GarageBus";

            return go.AddComponent<BusFlowGarageBusView>();
        }

        // Dùng lại prefab Ant Box (washing machine) của Ants Flow làm hình xe bus.
        public static BusFlowGarageBusView CreateFromPrefab(GameObject prefab, Transform parent)
        {
            GameObject go = Instantiate(prefab, parent);
            go.name = "GarageBus";
            go.transform.localScale = Vector3.one * 1.5f;

            SkeletonAnimation spine = go.GetComponentInChildren<SkeletonAnimation>();
            SpriteRenderer lid = null;
            TMP_Text numberText = null;

            WashingMachineItem washingMachineItem = go.GetComponent<WashingMachineItem>();

            if (washingMachineItem != null)
            {
                lid = washingMachineItem.lid;
                numberText = washingMachineItem.numberText;
                if (spine == null)
                {
                    spine = washingMachineItem.skeletonAnimation;
                }

                Destroy(washingMachineItem);
            }

            BusFlowGarageBusView view = go.AddComponent<BusFlowGarageBusView>();
            view.externalSpine = spine;
            view.externalLid = lid;
            view.externalNumberText = numberText;

            return view;
        }
    }
}
