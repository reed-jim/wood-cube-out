using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;

public class MagnetAntBoxLink : MonoBehaviour
{
    [SerializeField] private Image linkImage;
    [SerializeField] private Sprite[] linkSprites;
    [SerializeField] private float frameRate = 0.1f;

    [SerializeField] private Image blur1;
    [SerializeField] private Image blur2;
    [SerializeField] private Image bubble1;
    [SerializeField] private Image bubble2;
    [SerializeField] private float bubbleRadius = 0.5f;

    [SerializeField] private WashingMachineItem[] antBoxes;

    private bool[] boxCountReachedZero;
    private Coroutine animationCoroutine;
    private AudioSource linkSound;

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

        if (linkSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSoundMagnetLink(linkSound);
            linkSound = null;
        }
    }

    private void OnDisable()
    {
        if (linkSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSoundMagnetLink(linkSound);
            linkSound = null;
        }
    }

    public void Setup(WashingMachineItem[] antBoxes)
    {
        linkImage.gameObject.SetActive(true);
        linkImage.color = Color.white;

        if (blur1 != null) { blur1.gameObject.SetActive(true); blur1.color = Color.white; }
        if (blur2 != null) { blur2.gameObject.SetActive(true); blur2.color = Color.white; }
        if (bubble1 != null) { bubble1.gameObject.SetActive(true); bubble1.color = Color.white; }
        if (bubble2 != null) { bubble2.gameObject.SetActive(true); bubble2.color = Color.white; }

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
        StartCoroutine(AnimateBlurs());
        IEnumerator AnimateBlurs()
        {
            if (blur1 != null)
            {
                Tween.StopAll(blur1.transform);
                blur1.transform.localScale = Vector3.one;
                Tween.Scale(blur1.transform, startValue: Vector3.one * 0.8f, endValue: Vector3.one * 1.2f, duration: 0.4f, cycles: -1, cycleMode: CycleMode.Yoyo);
            }

            yield return new WaitForSeconds(0.2f);

            if (blur2 != null)
            {
                Tween.StopAll(blur2.transform);
                blur2.transform.localScale = Vector3.one;
                Tween.Scale(blur2.transform, startValue: Vector3.one * 0.8f, endValue: Vector3.one * 1.2f, duration: 0.4f, cycles: -1, cycleMode: CycleMode.Yoyo);
            }
        }



        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateLink());
    }

    private IEnumerator AnimateLink()
    {
        if (linkSprites == null || linkSprites.Length == 0)
        {
            yield break;
        }

        int currentFrame = 0;
        while (true)
        {
            if (GamePersistentVariable.isLoadingLevel)
            {
                if (linkSound == null && SoundManager.Instance != null)
                {
                    linkSound = SoundManager.Instance.PlaySoundMagnetLink();

                }
            }
            linkImage.sprite = linkSprites[currentFrame];
            currentFrame = (currentFrame + 1) % linkSprites.Length;
            yield return new WaitForSeconds(frameRate);
        }
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

        Vector3 a = antBoxes[0].lid.transform.position.AddZ(-3f);
        Vector3 b = antBoxes[1].lid.transform.position.AddZ(-3f);

        Vector3 startPos;
        Vector3 endPos;
        float distance;
        Vector3 mid;

        //Nối trực tiếp từ viền bong bóng này sang viền bong bóng kia (dọc theo vector hướng) ---

        Vector3 dirRef = b - a;
        Vector3 dirNormalized = dirRef.normalized;
        float totalDistance = dirRef.magnitude;
        startPos = a + dirNormalized * bubbleRadius;
        endPos = b - dirNormalized * bubbleRadius;
        distance = Mathf.Max(0f, totalDistance - 2f * bubbleRadius);
        mid = (startPos + endPos) * 0.5f;


        //Trừ Y đi một khoảng bằng bán kính (ở dưới bong bóng) ---
        //startPos = a - new Vector3(0, bubbleRadius, 0);
        //endPos = b - new Vector3(0, bubbleRadius, 0);
        //distance = Vector3.Distance(startPos, endPos);
        //mid = (startPos + endPos) * 0.5f;
        // end moi
        Vector3 dir = endPos - startPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        linkImage.rectTransform.position = mid;
        linkImage.rectTransform.rotation = rot;

        Vector2 size = linkImage.rectTransform.sizeDelta;
        size.x = distance;
        linkImage.rectTransform.sizeDelta = size;

        if (blur1 != null)
        {
            blur1.rectTransform.position = startPos;
            blur1.gameObject.SetActive(distance > 0f);
        }
        if (blur2 != null)
        {
            blur2.rectTransform.position = endPos;
            blur2.gameObject.SetActive(distance > 0f);
        }

        if (bubble1 != null)
        {
            bubble1.rectTransform.position = a;
        }
        if (bubble2 != null)
        {
            bubble2.rectTransform.position = b;
        }
    }

    private void HideLink()
    {
        SaferioTween.AlphaHide(linkImage, onCompleteAction: () =>
        {
            linkImage.color = Color.white;
        });
        if (blur1 != null) SaferioTween.AlphaHide(blur1, onCompleteAction: () => blur1.color = Color.white);
        if (blur2 != null) SaferioTween.AlphaHide(blur2, onCompleteAction: () => blur2.color = Color.white);
        if (bubble1 != null) SaferioTween.AlphaHide(bubble1, onCompleteAction: () => bubble1.color = Color.white);
        if (bubble2 != null) SaferioTween.AlphaHide(bubble2, onCompleteAction: () => bubble2.color = Color.white);
    }

    private void Unlink()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (linkSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSoundMagnetLink(linkSound);
            linkSound = null;
        }

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

        if (blur1 != null)
        {
            Tween.StopAll(blur1.transform);
            blur1.gameObject.SetActive(false);
        }
        if (blur2 != null)
        {
            Tween.StopAll(blur2.transform);
            blur2.gameObject.SetActive(false);
        }
        if (bubble1 != null) bubble1.gameObject.SetActive(false);
        if (bubble2 != null) bubble2.gameObject.SetActive(false);

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
        WashingMachineItem[] boxesToComplete = new WashingMachineItem[antBoxes.Length];
        System.Array.Copy(antBoxes, boxesToComplete, antBoxes.Length);

        HideLink();

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
            WashingMachineItem[] boxesCopy = new WashingMachineItem[antBoxes.Length];
            System.Array.Copy(antBoxes, boxesCopy, antBoxes.Length);

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
