using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class BasicScrew : BaseScrew
{
    private float _distanceBlocked;

    private void Update()
    {
        if (_isRotating)
        {
            transform.RotateAround(transform.forward, 0.3f);
        }
    }

    public override async void Loose(int screwId, GameEnum.GameFaction faction, ScrewBoxSlot screwBoxSlot)
    {
        if (screwId == this.screwId)
        {
            if (!_isInteractable)
            {
                return;
            }
            else
            {
                _isInteractable = false;
            }

            if (_isDone)
            {
                return;
            }

            CountBlockingObjects();

            // BLOCKED
            if (_numberBlockingObjects > 0)
            {
                FakeScrew fakeScrew = ObjectPoolingEverything.GetFromPool<FakeScrew>(GameConstants.FAKE_SCREW);

                fakeScrew.CloneFromScrew(this);

                fakeScrew.transform.SetParent(transform);

                GetComponent<MeshRenderer>().enabled = false;

                fakeScrew.IsRotating = true;

                SoundManager.Instance.PlaySoundLoosenScrewFail();

                _tweens.Add(Tween.LocalPosition(fakeScrew.transform, 0.1f * (fakeScrew.transform.localRotation * Vector3.forward),
                    cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.5f)
                .OnComplete(() =>
                {
                    fakeScrew.IsRotating = false;

                    ObjectPoolingEverything.ReturnToPool(GameConstants.FAKE_SCREW, fakeScrew.gameObject);
                    // fakeScrew.gameObject.SetActive(false);

                    GetComponent<MeshRenderer>().enabled = true;

                    _isInteractable = true;
                }));

                return;
            }

            joint.breakForce = 0;

            InvokeBreakJointEvent();

            screwBoxSlot.Fill(this);

            _isRotating = true;

            // FOR ROTATING
            await Task.Delay(100);

            _tweens.Add(Tween.Position(transform, transform.position + 2f * transform.forward, duration: 0.3f).OnComplete(() =>
            {
                _isRotating = false;

                _tweens.Add(Tween.Delay(duration: 0.1f)
                .OnComplete(() =>
                {
                    transform.SetParent(screwBoxSlot.transform);

                    gameObject.layer = LayerMask.NameToLayer("UI");

                    float duration = 0.3f;

                    _tweens.Add(Tween.LocalRotation(transform, Quaternion.Euler(new Vector3(0, 180, 0)), duration: duration));
                    _tweens.Add(Tween.LocalPosition(transform, new Vector3(0, 0, -0.3f), duration: duration));
                    _tweens.Add(Tween.Scale(transform, scaleOnScrewBox * Vector3.one, duration: duration)
                    .OnComplete(() =>
                    {
                        SoundManager.Instance.PlaySoundTightenScrew();

                        screwBoxSlot.CompleteFill();

                        _isDone = true;
                        _isInScrewPort = screwBoxSlot.IsScrewPort;

                        InvokeScrewLoosenedEvent();
                    }));
                }));
            }));

            SoundManager.Instance.PlaySoundLoosenScrew();

            // VibrateSlightly();
        }
    }

    public void VibrateSlightly()
    {
#if UNITY_ANDROID
        using (AndroidJavaClass vibratorClass = new AndroidJavaClass("android.os.Vibrator"))
        {
            using (AndroidJavaObject vibrator = vibratorClass.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                // Pattern: start immediately, vibrate for 50ms, then pause for 50ms, and repeat
                long[] pattern = { 0, 20, 20 };  // Vibrate for 50ms, pause for 50ms
                vibrator.Call("vibrate", pattern, -1); // -1 means no repeat
            }
        }
#endif
    }

    public override int CountBlockingObjects()
    {
        Vector3 start = transform.position + transform.forward;

        Collider[] hits = Physics.OverlapCapsule(start, start + 10 * transform.forward, 0.2f);

        int number = 0;

        float minBlockedDistance = float.MaxValue;

        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform == transform.parent)
                {
                    continue;
                }

                if (hits[i].GetComponent<IObjectPart>() != null)
                {
                    float distance = Vector3.Distance(hits[i].ClosestPoint(transform.position), transform.position);

                    if (distance < minBlockedDistance)
                    {
                        minBlockedDistance = distance;
                    }

                    if (distance < 2)
                    {
                        number++;
                    }
                }
            }
        }

        _numberBlockingObjects = number;
        _distanceBlocked = minBlockedDistance;

        return number;
    }
}
