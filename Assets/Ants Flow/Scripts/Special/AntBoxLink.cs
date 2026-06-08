using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class AntBoxLink : MonoBehaviour
{
    [SerializeField] private SpriteRenderer link;

    [SerializeField] private WashingMachineItem[] antBoxes;

    private MaterialPropertyBlock materialPropertyBlock;

    void Awake()
    {
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        WashingMachineItem.antBoxDoneEvent += OnAntBoxDone;

        materialPropertyBlock = new MaterialPropertyBlock();
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

        materialPropertyBlock.SetColor("_ColorLeft", antBoxes[0].faction);
        materialPropertyBlock.SetColor("_ColorRight", antBoxes[1].faction);

        link.SetPropertyBlock(materialPropertyBlock);

        PlaceTheLink();
    }

    private IEnumerator UpdateLinkPositions()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        float startTime = Time.time;

        while (Time.time - startTime < 3)
        {
            PlaceTheLink();

            yield return waitForSeconds;
        }
    }

    private void PlaceTheLink()
    {
        Debug.Log("Place " + antBoxes[0].lid.transform.position);
        Vector3 a = antBoxes[0].lid.transform.position.AddZ(-0.2f);
        Vector3 b = antBoxes[1].lid.transform.position.AddZ(-0.2f);

        // Position sprite in the middle
        transform.position = (a + b) * 0.5f;

        // Rotate to face pointB
        Vector3 dir = b - a;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Set width to distance between points
        float distance = dir.magnitude;

        Vector2 size = link.size;
        size.x = distance;
        link.size = size;
    }

    private void Unlink()
    {
        SaferioTween.AlphaHide(link, onCompleteAction: () =>
        {
            link.color = Color.white;
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
