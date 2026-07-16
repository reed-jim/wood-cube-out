using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class MagnetAntBoxLink : MonoBehaviour
{
    [SerializeField] private SpriteRenderer linkRenderer;

    [SerializeField] private WashingMachineItem[] antBoxes;

    private bool[] boxCountReachedZero;

    void Awake()
    {
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent += OnAntBoxDone;
        WashingMachineItem.magnetBoxCountZeroEvent += OnMagnetBoxCountZero;
        WashingMachineItem.moveUpEvent += OnAntBoxMovingUp;
    }

    void OnDestroy()
    {
        WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent -= OnAntBoxDone;
        WashingMachineItem.magnetBoxCountZeroEvent -= OnMagnetBoxCountZero;
        WashingMachineItem.moveUpEvent -= OnAntBoxMovingUp;
    }

    public void Setup(WashingMachineItem[] antBoxes)
    {
        linkRenderer.gameObject.SetActive(true);
        linkRenderer.color = Color.white;

        boxCountReachedZero = new bool[antBoxes.Length];

        for (int i = 0; i < antBoxes.Length; i++)
        {
            WashingMachineItem antBox = antBoxes[i];

            antBox.isMagnet = true;
            antBox.numSlotRequired = antBoxes.Length;

            boxCountReachedZero[i] = false;

            // Gán magnetPartner cho nhau
            if (i == 0 && antBoxes.Length > 1)
            {
                antBox.magnetPartner = antBoxes[1];
            }
            else if (i == 1)
            {
                antBox.magnetPartner = antBoxes[0];
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
        if (antBoxes == null || antBoxes.Length < 2)
        {
            return;
        }

        Vector3 a = antBoxes[0].lid.transform.position.AddZ(-0.2f);
        Vector3 b = antBoxes[1].lid.transform.position.AddZ(-0.2f);

        Vector2 dir = b - a;
        float distance = dir.magnitude;
        Vector3 mid = (a + b) * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        linkRenderer.transform.position = mid;
        linkRenderer.transform.rotation = rot;

        Vector2 size = linkRenderer.size;
        size.x = distance;
        linkRenderer.size = size;
    }

    private void HideLink()
    {
        SaferioTween.AlphaHide(linkRenderer, onCompleteAction: () =>
        {
            linkRenderer.color = Color.white;
        });
    }

    private void Unlink()
    {
        // Reset trạng thái magnet cho tất cả box trước khi deactivate
        for (int i = 0; i < antBoxes.Length; i++)
        {
            if (antBoxes[i] != null)
            {
                antBoxes[i].isMagnet = false;
                antBoxes[i].magnetPartner = null;
                antBoxes[i].numSlotRequired = 1;
            }
        }

        antBoxes = new WashingMachineItem[0];
        boxCountReachedZero = new bool[0];

        gameObject.SetActive(false);
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
            StartCoroutine(UpdateLinkPositions());
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

    private void OnMagnetBoxCountZero(WashingMachineItem antBox)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        int foundIndex = -1;
        for (int i = 0; i < antBoxes.Length; i++)
        {
            if (antBoxes[i] == antBox)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex == -1)
        {
            return;
        }

        boxCountReachedZero[foundIndex] = true;
        Tween.Alpha(antBox.numberText, 0f, duration: 0.3f);

        bool allDone = true;
        for (int i = 0; i < boxCountReachedZero.Length; i++)
        {
            if (!boxCountReachedZero[i])
            {
                allDone = false;
                break;
            }
        }

        if (allDone)
        {
            CompleteAllBoxes();
        }
    }

    private void CompleteAllBoxes()
    {
        // Lưu copy vì Unlink sẽ xóa mảng và reset trạng thái
        WashingMachineItem[] boxesToComplete = new WashingMachineItem[antBoxes.Length];
        System.Array.Copy(antBoxes, boxesToComplete, antBoxes.Length);

        HideLink();

        // Unlink ngay lập tức để tránh OnAntBoxDone re-entrant
        Unlink();

        for (int i = 0; i < boxesToComplete.Length; i++)
        {
            WashingMachineItem antBox = boxesToComplete[i];

            SaferioTween.ScaleDownWithPunch(antBox.transform, onCompleteAction: () =>
            {
                if (antBox.washingMachineSlot != null)
                {
                    antBox.washingMachineSlot.washingMachineItem = null;
                }

                antBox.isInSlot = false;

                WashingMachineItem.InvokeAntBoxDoneEvent(antBox);
            });
        }
    }

    private void OnAntBoxDone(WashingMachineItem antBox)
    {
        bool isDone = antBoxes.Contains(antBox);

        if (isDone)
        {
            // Lưu copy trước khi Unlink vì Unlink sẽ xóa mảng
            WashingMachineItem[] boxesCopy = new WashingMachineItem[antBoxes.Length];
            System.Array.Copy(antBoxes, boxesCopy, antBoxes.Length);

            // Unlink trước để tránh re-entrant callback
            Unlink();

            foreach (var box in boxesCopy)
            {
                if (box != null && box != antBox)
                {
                    if (box.number <= 0)
                    {
                        SaferioTween.ScaleDownWithPunch(box.transform, onCompleteAction: () =>
                        {
                            if (box.washingMachineSlot != null)
                            {
                                box.washingMachineSlot.washingMachineItem = null;
                            }
                            box.isInSlot = false;
                            WashingMachineItem.InvokeAntBoxDoneEvent(box);
                        });
                    }
                }
            }
        }
    }
}
