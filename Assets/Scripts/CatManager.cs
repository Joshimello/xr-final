using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using NUnit.Framework;
public enum CatState
{
    Idle,
    Follow,
    Guide,
    Wait ,
}
public class CatManager : MonoBehaviour
{
    public CatState currentState = CatState.Idle;
    public GameObject player  ;
    public NavMeshAgent navMeshAgent;
    public Transform guideTarget;
    public Animator catAnimator;
    public TrailRenderer catTrail;
    public float followThreshold = 2.0f;

    public float guideThreshold = 2.0f; 
    private Vector3 followOffset;             // Smooth offset from player
    private float nextOffsetUpdateTime = 0f;  // Timer for offset change
    private float offsetUpdateInterval = 4f;  // How often to choose new offset
    //private float followDistance = 4f;
    private float waitTimer = 0f;
    public float maxWaitTime = 5f;
    
    [Header("Chat UI")]
    public Canvas chatCanvas;
    public TextMeshProUGUI chatText;
    public float chatDuration = 0.5f;

    private Coroutine chatCoroutine;
    private Camera mainCamera;
    void Start()
    {
        guideTarget = null;
        if (chatCanvas) chatCanvas.enabled = false;
        mainCamera = Camera.main;
        navMeshAgent.updateRotation = false;

    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        catTrail.emitting = (currentState == CatState.Guide);
        switch (currentState)
        {
            case CatState.Idle:
                // Idle behavior
                UpdateIdleBehavior(distanceToPlayer);
                break;
            case CatState.Follow:
                // Follow behavior

                UpdateFollowBehavior(distanceToPlayer);
                break;
            case CatState.Guide:
                UpdateGuideBehavior(distanceToPlayer);
                break;
            case CatState.Wait:
                UpdateWaitBehavior(distanceToPlayer);
                break;
        }
        UpdateAnimation();
    }
    void SetState(CatState newState)
    {
        currentState = newState;
        Debug.Log("Cat State changed to: " + currentState.ToString());
    }
    public void SetGuideTarget(Transform target)
    {
        guideTarget = target;
        SetState(CatState.Guide);
        navMeshAgent.SetDestination(guideTarget.position);

    }
    public void SetFollowState()
    {
        SetState(CatState.Follow);
        navMeshAgent.isStopped = false;
    }
    public void SetWaitState()
    {
        SetState(CatState.Wait);
        ShowChat("Follow me!", 1);
        navMeshAgent.isStopped = true;
        //LookAtPlayer();
    }
    void UpdateIdleBehavior(float distanceToPlayer)
    {
        // If player walks away, start following
        if (distanceToPlayer > followThreshold )
        {
            SetFollowState();
            return;
        }

        // Stop moving and face the player
        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
        //LookAtPlayer();

        // Optionally add subtle idle animation blending
        catAnimator.SetFloat("Forward", 0f);
        catAnimator.SetFloat("Turn", 0f);
    }

    void UpdateGuideBehavior(float distanceToPlayer)
    {
        if (guideTarget == null) return;

        // If player is too far, wait
        if (distanceToPlayer > guideThreshold)
        {
            SetWaitState();
            return;
        }

        // If cat reached destination
        float distanceToTarget = Vector3.Distance(transform.position, guideTarget.position);
        if (distanceToTarget <= 1)
        {
            navMeshAgent.isStopped = true;
            LookAtPlayer();
            ShowChat("Hide here!", 2f); // 👈 show message when arrived
            SetState(CatState.Wait);
            guideTarget = null;    // optional: switch to Idle after arriving
            return;
        }

        // Otherwise keep guiding
        if (!navMeshAgent.pathPending && navMeshAgent.isStopped == false)
            navMeshAgent.SetDestination(guideTarget.position);
    }
    void UpdateFollowBehavior(float distanceToPlayer)
    {
        // Smooth offset refresh for natural wandering
        if (Time.time > nextOffsetUpdateTime)
        {
            nextOffsetUpdateTime = Time.time + offsetUpdateInterval;
            followOffset = new Vector3(
                Random.Range(-followThreshold, followThreshold),
                0,
                Random.Range(-followThreshold, followThreshold)
            );
        }

        // Compute intended target (around player)
        Vector3 desiredTarget = player.transform.position + player.transform.forward * followThreshold + followOffset;

        // Direction from cat to target
        Vector3 dirToTarget = (desiredTarget - transform.position).normalized;

        // --- Clamp target to cat's forward hemisphere ---
        float dot = Vector3.Dot(transform.forward, dirToTarget);

        // Maximum angle allowed (in degrees) — you can adjust this
        float maxTurnAngle = 100f;
        float minDot = Mathf.Cos(maxTurnAngle * Mathf.Deg2Rad);

        if (dot < minDot)
        {
            // Clamp direction to the maxTurnAngle boundary in front of the cat
            Vector3 clampedDir = Vector3.RotateTowards(
                transform.forward,
                dirToTarget,
                maxTurnAngle * Mathf.Deg2Rad,
                0f
            );
            dirToTarget = clampedDir.normalized;

            desiredTarget = transform.position + dirToTarget * followThreshold;
        }

        // Apply movement
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(desiredTarget);
    }

    void UpdateWaitBehavior(float distanceToPlayer)
    {
        // increase timer while waiting
        waitTimer += Time.deltaTime;

        // If waited too long
        if (waitTimer > maxWaitTime)
        {
            ShowChat((guideTarget!=null)?"Follow me! Hurry up!":"Hide here!", 2f);
            waitTimer = 0f; // reset timer so it doesn’t spam
        }
        
        // If player comes close, resume guiding
        if (distanceToPlayer < guideThreshold/2 && guideTarget != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(guideTarget.position);
            SetState(CatState.Guide);
            LookAtTarget();
        }
        else
        {
            LookAtPlayer();
        }
    }
    void UpdateAnimation()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(navMeshAgent.velocity);
        float forward = localVelocity.z;
        float turn = Mathf.Atan2(localVelocity.x, Mathf.Abs(localVelocity.z));

        // Ignore small jitters
        if (Mathf.Abs(forward) < 0.1f) forward = 0f;
        if (Mathf.Abs(turn) < 0.1f) turn = 0f;

        // Smooth blending
        // float smoothForward = Mathf.Lerp(catAnimator.GetFloat("Forward"), forward, Time.deltaTime * 10f);
        // float smoothTurn = Mathf.Lerp(catAnimator.GetFloat("Turn"), turn, Time.deltaTime * 10f);
        float smoothForward = Mathf.Clamp(forward, 0f, 2f);
        float smoothTurn = Mathf.Clamp(turn, -1.6f, 1.6f);
        float idle = Random.Range(0f, 1f);
        switch (currentState)
        {
            case CatState.Idle:
                smoothForward = 0f;
                smoothTurn = 0f;
                break;
            case CatState.Wait:
                smoothForward = 0f;
                smoothTurn = 0f;
                break;
        }
        catAnimator.SetInteger("State", (int)currentState);
        catAnimator.SetFloat("Forward", smoothForward);
        catAnimator.SetFloat("Turn", smoothTurn);
        catAnimator.SetFloat("Idle", idle);
    }


    void LookAtPlayer()
    {
        Vector3 lookDir = player.transform.position - transform.position;
        lookDir.y = 0;
        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
        }
        //let the canva face the camera
        if (chatCanvas)
        {
            chatCanvas.transform.rotation = Quaternion.LookRotation(chatCanvas.transform.position - mainCamera.transform.position);
        }
    }
    void LookAtTarget()
    {
        Vector3 lookDir = guideTarget.transform.position - transform.position;
        lookDir.y = 0;
        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
        }
    }
    
    public void ShowChat(string message,float duration)
    {
        if (chatCoroutine != null)
            StopCoroutine(chatCoroutine);
        chatDuration = duration;
        chatCoroutine = StartCoroutine(ShowChatRoutine(message));
    }

    private IEnumerator ShowChatRoutine(string message)
    {
        chatCanvas.enabled = true;
        chatText.text = message;
        chatText.color = Color.black;
        yield return new WaitForSeconds(chatDuration);
        chatCanvas.enabled = false;
    }
}
