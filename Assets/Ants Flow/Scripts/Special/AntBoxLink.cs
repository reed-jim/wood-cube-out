using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class AntBoxLink : MonoBehaviour
{
    [SerializeField] private SpriteRenderer leftHalf;
    [SerializeField] private SpriteRenderer rightHalf;

    [SerializeField] private WashingMachineItem[] antBoxes;

    private MaterialPropertyBlock materialPropertyBlock;

    void Awake()
    {
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent += OnAntBoxDone;
        WashingMachineItem.moveUpEvent += OnAntBoxMovingUp;

        materialPropertyBlock = new MaterialPropertyBlock();
    }

    void OnDestroy()
    {
        WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent -= OnAntBoxDone;
        WashingMachineItem.moveUpEvent -= OnAntBoxMovingUp;
    }

    public void Setup(WashingMachineItem[] antBoxes)
    {
        leftHalf.gameObject.SetActive(true);
        rightHalf.gameObject.SetActive(true);
        leftHalf.color = Color.white;
        rightHalf.color = Color.white;

        for (int i = 0; i < antBoxes.Length; i++)
        {
            WashingMachineItem antBox = antBoxes[i];

            antBox.isDouble = true;
            antBox.numSlotRequired = antBoxes.Length;
            antBox.skeletonAnimation.AnimationState.SetAnimation(0, antBox.doubleBoxAnimation, false);

            if (i == 0)
            {
                leftHalf.sprite = FactionMapper.Ins.GetDoubleAntLinkSprite(antBox.faction);
            }
            else
            {
                rightHalf.sprite = FactionMapper.Ins.GetDoubleAntLinkSprite(antBox.faction);
            }
        }

        this.antBoxes = antBoxes;

        PlaceTheLink();
    }

    private IEnumerator UpdateLinkPositions()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        float startTime = Time.time;

        while (Time.time - startTime < 3 && antBoxes.Length > 0)
        {
            PlaceTheLink();

            yield return waitForSeconds;
        }
    }

    private void PlaceTheLink()
    {
        Vector3 a = Vector3.zero;
        Vector3 b = Vector3.zero;
        if (WashingMachineManager.Ins.isLevelContainingNewSpecialElements)
        {
             a = antBoxes[0].lid.transform.position.AddZ(-1f);
             b = antBoxes[1].lid.transform.position.AddZ(-1f);
        }
        else
        {
             // Giữ nguyên hành vi nhánh ants-flow: link ở z = -0.2
             a = antBoxes[0].lid.transform.position.AddZ(-0.2f);
             b = antBoxes[1].lid.transform.position.AddZ(-0.2f);
        }
        // if (a.x > b.x)
        // {
        //     Vector3 temp = a;
        //     a = b;
        //     b = temp;

        //     Sprite tempSprite = leftHalf.sprite;
        //     leftHalf.sprite = rightHalf.sprite;
        //     rightHalf.sprite = tempSprite;
        // }

        Vector2 dir = b - a;
        float distance = dir.magnitude;
        Vector3 mid = (a + b) * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        float halfLength = distance * 0.5f;

        // Position each half halfway between its endpoint and the center
        leftHalf.transform.position = (a + mid) * 0.5f;
        rightHalf.transform.position = (b + mid) * 0.5f;

        leftHalf.transform.rotation = rot;
        rightHalf.transform.rotation = rot;

        Vector2 size = leftHalf.size;
        size.x = halfLength;

        leftHalf.size = size;
        rightHalf.size = size;

        for (int i = 0; i < antBoxes.Length; i++)
        {
            WashingMachineItem antBox = antBoxes[i];

            if (antBox.isInSlot)
            {
                antBox.doubleAntBoxHole.gameObject.SetActive(true);
                antBox.doubleAntBoxHole.sprite = FactionMapper.Ins.GetDoubleAntHoleSprite(antBox.faction);
            }
        }
    }

    private void HideLink()
    {
        SaferioTween.AlphaHide(leftHalf, onCompleteAction: () =>
        {
            leftHalf.color = Color.white;
        });
        SaferioTween.AlphaHide(rightHalf, onCompleteAction: () =>
        {
            rightHalf.color = Color.white;
        });
    }

    private void Unlink()
    {
        // SaferioTween.AlphaHide(leftHalf, onCompleteAction: () =>
        // {
        //     leftHalf.color = Color.white;
        // });
        // SaferioTween.AlphaHide(rightHalf, onCompleteAction: () =>
        // {
        //     rightHalf.color = Color.white;

        //     gameObject.SetActive(false);
        // });

        gameObject.SetActive(false);

        antBoxes = new WashingMachineItem[0];
    }

    private void OnAntBoxSelected(WashingMachineItem antBox)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        WashingMachineItem selectedAntBox = null;

        if (antBoxes.Length == 0)
        {
            return;
        }

        for (int i = 0; i < antBoxes.Length; i++)
        {
            if (antBoxes[i] == antBox)
            {
                selectedAntBox = antBox;
            }
        }

        bool isNeededToUpdateLinkPosition = false;

        if (selectedAntBox != null)
        {
            List<WashingMachineItem> otherAntBoxes = new List<WashingMachineItem>();

            for (int i = 0; i < antBoxes.Length; i++)
            {
                if (antBoxes[i].washingMachineSlot != null)
                {
                    continue;
                }

                if (antBoxes[i] != selectedAntBox)
                {
                    otherAntBoxes.Add(antBoxes[i]);
                }
            }

            for (int i = 0; i < otherAntBoxes.Count; i++)
            {
                otherAntBoxes[i].ForceSelect();

                isNeededToUpdateLinkPosition = true;
            }
        }

        if (isNeededToUpdateLinkPosition)
        {
            HideLink();
            // StartCoroutine(UpdateLinkPositions());
        }
    }

    private void OnAntBoxMovingUp(WashingMachineItem antBox)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (antBoxes.Contains(antBox))
        {
            StartCoroutine(UpdateLinkPositions());
        }
    }

    private void OnAntBoxDone(WashingMachineItem antBox)
    {
        bool isDone = antBoxes.Any(item => item == antBox);

        if (isDone)
        {
            for (int i = 0; i < antBoxes.Length; i++)
            {
                antBoxes[i].numSlotRequired = 1;
            }

            Unlink();
        }
    }
}
