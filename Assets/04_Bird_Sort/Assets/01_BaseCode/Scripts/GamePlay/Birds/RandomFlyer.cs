using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody2D))] // Requires Rigidbody to move around

public class RandomFlyer : MonoBehaviour
{
    [SerializeField] float idleSpeed, turnSpeed, switchSeconds, idleRatio;
    [SerializeField] Vector2 animSpeedMinMax, moveSpeedMinMax, changeAnimEveryFromTo, changeTargetEveryFromTo;
    [SerializeField] Vector3 homeTarget, flyingTarget;
    [SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 yMinMax;
    [SerializeField] public bool returnToBase = false;
    [SerializeField] public float randomBaseOffset = 5, delayStart = 0f;

    private Animator animator;
    [SerializeField] private Transform bodyRotate;
    [System.NonSerialized]
    public float changeTarget = 0f, changeAnim = 0f, timeSinceTarget = 0f, timeSinceAnim = 0f, prevAnim, currentAnim = 0f, prevSpeed, speed, zturn, prevz,
        turnSpeedBackup;
    private Vector3 rotateTarget, position, direction, velocity, randomizedBase;
    private Quaternion lookRotation;
    [System.NonSerialized] public float distanceFromBase, distanceFromTarget;

    private bool isMove;
    private int numGlide;
    private int m_numGlide;

    private UnityAction actionCome;

    public void Init()
    {
        // Inititalize
        animator = GetComponent<Animator>();
        //rb = GetComponent<Rigidbody2D>();
        turnSpeedBackup = turnSpeed;
        direction = Quaternion.Euler(transform.eulerAngles) * (Vector3.forward);
        //if (delayStart < 0f) rb.velocity = idleSpeed * direction;
        isMove = false;
    }

    public void SetMove(Vector3 targetMove, int numGlide, UnityAction actionCome)
    {
        flyingTarget = targetMove;
        this.numGlide = numGlide;
        m_numGlide = 0;
        this.actionCome = actionCome;
        isMove = true;
    }


    void FixedUpdate()
    {
        if (!this.isMove)
            return;
        // Wait if start should be delayed (useful to add small differences in large flocks)
        if (delayStart > 0f)
        {
            delayStart -= Time.fixedDeltaTime;
            return;
        }
        // Calculate distances
        distanceFromBase = Vector2.Distance(randomizedBase, this.position);
        distanceFromTarget = Vector2.Distance(flyingTarget, this.position);
        if (distanceFromTarget < 0.2f)
        {
            m_numGlide++;
            if(m_numGlide >= this.numGlide)
            {
                this.isMove = false;
                this.actionCome?.Invoke();
                return;
            }    
        }    
        // Allow drastic turns close to base to ensure target can be reached
        //if (returnToBase && distanceFromBase < 10f)
        //{
        //    if (turnSpeed != 300f && rb.velocity.magnitude != 0f)
        //    {
        //        turnSpeedBackup = turnSpeed;
        //        turnSpeed = 300f;
        //    }
        //    else if (distanceFromBase <= 2f)
        //    {
        //        rb.velocity = Vector3.zero;
        //        turnSpeed = turnSpeedBackup;
        //        return;
        //    }
        //}
        // Time for a new animation speed
        if (changeAnim < 0f)
        {
            prevAnim = currentAnim;
            currentAnim = ChangeAnim(currentAnim);
            changeAnim = Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
            timeSinceAnim = 0f;
            prevSpeed = speed;
            if (currentAnim == 0) speed = idleSpeed;
            else speed = Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y, (currentAnim - animSpeedMinMax.x) / (animSpeedMinMax.y - animSpeedMinMax.x));
        }
        // Time for a new target position
        if (changeTarget < 0f)
        {
            rotateTarget = ChangeDirection(this.transform.position);
            if (returnToBase) changeTarget = 0.2f; else changeTarget = Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
            timeSinceTarget = 0f;
        }
        // Turn when approaching height limits
        // ToDo: Adjust limit and "exit direction" by object's direction and velocity, instead of the 10f and 1f - this works in my current scenario/scale
        if (this.transform.position.y < yMinMax.x + 10f ||
            this.transform.position.y > yMinMax.y - 10f)
        {
            if (this.transform.position.y < yMinMax.x + 10f) rotateTarget.y = 1f; else rotateTarget.y = -1f;
        }
        //body.transform.Rotate(0f, 0f, -prevz, Space.Self); // If required to make Quaternion.LookRotation work correctly, but it seems to be fine
        zturn = Mathf.Clamp(Vector3.SignedAngle(rotateTarget, direction, Vector3.up), -45f, 45f);
        // Update times
        changeAnim -= Time.fixedDeltaTime;
        changeTarget -= Time.fixedDeltaTime;
        timeSinceTarget += Time.fixedDeltaTime;
        timeSinceAnim += Time.fixedDeltaTime;

        // Rotate towards target
        if (rotateTarget != Vector3.zero) lookRotation = Quaternion.LookRotation(rotateTarget, Vector3.up);
        Vector3 rotation = Quaternion.RotateTowards(bodyRotate.transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        bodyRotate.transform.eulerAngles = new Vector3(0, 0, rotation.z);
        // Rotate on z-axis to tilt body towards turn direction
        float temp = prevz;
        if (prevz < zturn) prevz += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zturn - prevz);
        else if (prevz >= zturn) prevz -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevz - zturn);
        // Min and max rotation on z-axis - can also be parameterized
        prevz = Mathf.Clamp(prevz, -45f, 45f);
        // Remove temp if transform is rotated back earlier in FixedUpdate
        bodyRotate.transform.Rotate(0f, 0f, prevz - temp, Space.Self);
        // Move flyer
        Vector2 _dir = Vector2.zero;
        direction = Quaternion.Euler(bodyRotate.transform.eulerAngles) * Vector2.one;
        Debug.Log("direction " + direction);
        if (returnToBase && distanceFromBase < idleSpeed)
        {
            this.transform.Translate(Mathf.Min(idleSpeed, distanceFromBase) * direction * Time.deltaTime);
        }
        else
            this.transform.Translate(Mathf.Lerp(prevSpeed, speed, Mathf.Clamp(timeSinceAnim / switchSeconds, 0f, 1f)) * direction * Time.deltaTime);
        // Hard-limit the height, in case the limit is breached despite of the turnaround attempt
        if (this.transform.position.y < yMinMax.x || this.transform.position.y > yMinMax.y)
        {
            position = this.transform.position;
            position.y = Mathf.Clamp(position.y, yMinMax.x, yMinMax.y);
            this.transform.position = position;
        }
    }

    // Select a new animation speed randomly
    private float ChangeAnim(float currentAnim)
    {
        float newState = 0;
        //if (Random.Range(0f, 1f) < idleRatio) newState = 0f;
        //else
        //{
        //    newState = Random.Range(animSpeedMinMax.x, animSpeedMinMax.y);
        //}
        //if (newState != currentAnim)
        //{
        //    animator.SetFloat("flySpeed", newState);
        //    if (newState == 0) animator.speed = 1f; else animator.speed = newState;
        //}
        return newState;
    }

    // Select a new direction to fly in randomly
    private Vector3 ChangeDirection(Vector3 currentPosition)
    {
        Vector3 newDir;
        if (returnToBase)
        {
            randomizedBase = homeTarget;
            randomizedBase.y += Random.Range(-randomBaseOffset, randomBaseOffset);
            newDir = randomizedBase - currentPosition;
        }
        else if (distanceFromTarget > radiusMinMax.y)
        {
            newDir = flyingTarget - currentPosition;
        }
        else if (distanceFromTarget < radiusMinMax.x)
        {
            newDir = currentPosition - flyingTarget;
        }
        else
        {
            // 360-degree freedom of choice on the horizontal plane
            float angleXY = Random.Range(-Mathf.PI, Mathf.PI);
            // Limited max steepness of ascent/descent in the vertical direction
            float angleZ = Random.Range(-Mathf.PI / 48f, Mathf.PI / 48f);
            // Calculate direction
           newDir = Mathf.Sin(angleXY) * Vector3.up + Mathf.Cos(angleXY) * Vector3.right + Mathf.Sin(angleZ) * Vector3.forward;
        }
        newDir.z = 0;
        return newDir.normalized;
    }
}