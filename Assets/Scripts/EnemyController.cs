using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IEnemy
{
    public void SwungAt(Vector3 fromDir);
}

public class EnemyController : MonoBehaviour, IEnemy
{
    [Header("Move Settings")]
    public float moveTimeLength = 0.1f;
    public float moveDelayTime = 0.1f;
    public AnimationCurve moveCurve;

    public float swingDelay = 2f;
    public float swingReduceMultiplier = 2f;
    private float actualSwingDelay;

    [SerializeField]
    private bool canSeePlayer;

    private Vector3 targetPos;
    private Vector3 prevTargetPos;
    private float posProgress = 0;

    private bool isMoving = false;
    private bool isDelaying = false;

    public void Start()
    {
        targetPos = Vector3Int.RoundToInt(transform.position);
        transform.position = targetPos;
        actualSwingDelay = swingDelay;
    }

    public void Update() {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position, PlayerController.Instance.transform.position - transform.position, out hit, Mathf.Infinity, GameManager.Instance.playerMask);
        bool hitPlayer = false;
        // Check if wall is in move direction, don't spot player if hit one
        if (hitSomething) {
            if (hit.collider.tag == "Player") {
                hitPlayer = true;
                Debug.DrawRay(transform.position, PlayerController.Instance.transform.position - transform.position, Color.red);
            }
        }
        canSeePlayer = PlayerController.PlayerInLight() && hitPlayer;
    }

    private Vector3 lockedDir;
    private Vector3 prevPlayerPos;
    private bool useSwingDelay = false;
    private Coroutine delayRoutine;
    private void FixedUpdate()
    {
        if (isMoving)
        {
            posProgress += Time.fixedDeltaTime * 1 / moveTimeLength;
            if (posProgress > 1) { posProgress = 1; }
            transform.position = Vector3.LerpUnclamped(prevTargetPos, targetPos, moveCurve.Evaluate(posProgress));
            if (transform.position == targetPos && !isDelaying)
            {
                posProgress = 0;
                isMoving = false;
                transform.position = targetPos;
                delayRoutine = StartCoroutine(DelayMove());
            }
        } else if(swungAt) {
            if(useSwingDelay) { actualSwingDelay /= swingReduceMultiplier; }
            useSwingDelay = true;
            swungAt = false;
            if(delayRoutine != null) {
                isDelaying = false;
                StopCoroutine(delayRoutine);
            }
            if (!IsWallInDir(escapeDir) && IsFloorInDir(escapeDir))
            {
                isMoving = true;
                prevTargetPos = targetPos;
                targetPos += escapeDir;
            } else {
                StartCoroutine(DelayMove());
            }
        } else if(!isDelaying) // Determine where to move
        {
            if(canSeePlayer) // Chase logic
            {
                if(prevPlayerPos != PlayerController.Instance.transform.position) { lockedDir = Vector3.zero; }
                Vector3 dirToPlayer = Vector3.Normalize(PlayerController.Instance.transform.position - transform.position);
                Debug.DrawRay(transform.position + Vector3.up, dirToPlayer, Color.white);
                List<Vector3> directions = new List<Vector3> { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                List<Tuple<Vector3, float>> sortedDirection = new List<Tuple<Vector3, float>>();
                foreach(Vector3 dir in directions) {
                    float difference = Vector3.Dot(dirToPlayer, dir);
                    sortedDirection.Add(new Tuple<Vector3, float>(dir, difference));
                }
                sortedDirection = sortedDirection.OrderByDescending(x => x.Item2).ToList();
                bool inLoop = true;
                int loopCounter = 0;
                while(inLoop)
                {
                    loopCounter++;
                    foreach (Tuple<Vector3, float> dirTuple in sortedDirection) {
                        Vector3 dir = dirTuple.Item1 * GameManager.Instance.worldUnits;
                        if (dir == lockedDir * -1) {
                            continue;
                        }
                        if (!IsWallInDir(dir) && IsFloorInDir(dir)) {
                            isMoving = true;
                            prevTargetPos = targetPos;
                            targetPos += dir;
                            lockedDir = dir;
                            inLoop = false;
                            break;
                        } else if (lockedDir == dir) {
                            lockedDir = Vector3.zero;
                            break;
                        }
                    }
                    if(loopCounter > 3) {
                        Debug.Log("Forcing out of loop :(");
                        break;
                    }
                }
                
            }
        }
        prevPlayerPos = PlayerController.Instance.transform.position;
    }

    public bool IsWallInDir(Vector3 dir)
    {
        RaycastHit hit;
        // Check if wall is in move direction, don't move if found one
        Debug.DrawRay(transform.position, dir, Color.blue, 1f);
        if (Physics.Raycast(transform.position, dir, out hit, GameManager.Instance.worldUnits, GameManager.Instance.wallMask))
        {
            return true;
        }
        return false;
    }

    public bool IsFloorInDir(Vector3 dir)
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + dir, Vector3.down, Color.green, 3f);
        // Check if floor is at the move position, otherwise don't move
        if (Physics.Raycast(transform.position + dir, Vector3.down, out hit, 1.1f, GameManager.Instance.wallMask))
        {
            return true;
        }
        return false;
    }

    private IEnumerator DelayMove()
    {
        isDelaying = true;
        yield return new WaitForSeconds(useSwingDelay ? actualSwingDelay : moveDelayTime);
        useSwingDelay = false;
        actualSwingDelay = swingDelay;
        isDelaying = false;
    }

    private bool swungAt = false;
    private Vector3 escapeDir;
    public void SwungAt(Vector3 fromDir)
    {
        swungAt = true;
        escapeDir = Vector3Int.RoundToInt(fromDir) * GameManager.Instance.worldUnits;
    }
}
