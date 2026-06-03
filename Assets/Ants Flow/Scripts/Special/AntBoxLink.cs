using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class AntBoxLink : MonoBehaviour
{
    [SerializeField] private LineRenderer link;

    [SerializeField] private WashingMachineItem[] antBoxes;

    void Awake()
    {
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent += OnAntBoxDone;
    }

    void OnDestroy()
    {
        WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent -= OnAntBoxDone;
    }

    public void Setup(WashingMachineItem[] antBoxes)
    {
        for (int i = 0; i < antBoxes.Length; i++)
        {
            WashingMachineItem antBox = antBoxes[i];

            antBox.numSlotRequired = antBoxes.Length;
        }

        this.antBoxes = antBoxes;

        link.positionCount = antBoxes.Length;

        for (int i = 0; i < antBoxes.Length; i++)
        {
            link.SetPosition(i, antBoxes[i].transform.position);
        }
    }

    private IEnumerator UpdateLinkPositions()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        float startTime = Time.time;

        while (Time.time - startTime < 3)
        {
            link.positionCount = antBoxes.Length;

            for (int i = 0; i < antBoxes.Length; i++)
            {
                link.SetPosition(i, antBoxes[i].transform.position);
            }

            yield return waitForSeconds;
        }
    }

    private void Unlink()
    {
        SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
        {
            transform.localScale = Vector3.one;
        });
    }

    private void OnAntBoxSelected(WashingMachineItem antBox)
    {
        WashingMachineItem selectedAntBox = null;

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

    private void OnAntBoxDone(WashingMachineItem antBox)
    {
        bool isDone = antBoxes.Any(item => item == antBox);

        if (isDone)
        {
            Unlink();
        }
    }
}
