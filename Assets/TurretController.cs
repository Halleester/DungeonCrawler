using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public float fireDelay;
    [Range(0, 1)]
    public float warningIntoDelayPercent;

    public float maxFireRange = 40f;

    private float fireTimer;
    private float lineOfSightTimer;

    [SerializeField]
    private bool canSeePlayer;

    private bool hasLineOfSight;
    private bool playerIsLit;

    public Transform lookAtTransform;

    private Vector3 startingTargetPos;

    public Renderer eyeRenderer;
    public Material defaultEye;
    public Material warningEye;
    public Material shootingEye;
    public Material angryEye;

    public float attackMoveTime = 1f;
    public float respawnTime = 2f;

    private Vector3 startPos;

    public enum TurretState { Locating, Firing }
    public TurretState turretState = TurretState.Locating;

    public float angryRange;
    public float killRange;

    [Header("Audio Settings")]
    public AudioSource screamSource;
    [Range(0, 1)]
    public float screamerVolume = 1;
    public AudioClip idleClip;
    public AudioClip spotClip;
    public AudioClip attackClip;

    public void Start()
    {
        startPos = transform.position;
        screamSource.clip = idleClip;
        screamSource.time = Random.Range(0, idleClip.length);
        screamSource.Play();
    }

    public void Update() {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(transform.position, PlayerController.Instance.transform.position - transform.position, out hit, maxFireRange, GameManager.Instance.playerMask);
        hasLineOfSight = false;
        // Check if wall is in move direction, don't spot player if hit one
        if (hitSomething)
        {
            if (hit.collider.tag == "Player")
            {
                hasLineOfSight = true;
                Debug.DrawRay(transform.position, PlayerController.Instance.transform.position - transform.position, Color.red);
            }
        }
        playerIsLit = PlayerController.PlayerInLight();
        canSeePlayer = playerIsLit && hasLineOfSight;

        CheckForStateChange();

        if(turretState == TurretState.Firing && !PlayerController.Instance.playerDead)
        {
            Vector3 lookDir = PlayerController.Instance.transform.position - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);
            fireTimer -= Time.deltaTime;
            if(fireTimer <= fireDelay * warningIntoDelayPercent && !setAttackFace) {
                eyeRenderer.material = shootingEye;
                screamSource.clip = attackClip;
                screamSource.time = 0;
                screamSource.Play();
                setAttackFace = true;
            }
            if(fireTimer <= 0) {
                // Kill player
                Debug.Log("Shot player");
                StartCoroutine(MoveToPlayer());
                PlayerController.Instance.KillPlayer(true);
            }
        }

        if(turretState == TurretState.Locating)
        {
            float dist = Vector3.Distance(PlayerController.Instance.transform.position, startPos);
            if (dist <= angryRange && !playerNear)
            {
                playerNear = true;
                eyeRenderer.material = angryEye;
            }
            if(dist > angryRange && playerNear) {
                ChangeState(TurretState.Locating);
                playerNear = false;
            }
            if(dist <= killRange && !PlayerController.Instance.playerDead) {
                eyeRenderer.material = shootingEye;
                screamSource.clip = attackClip;
                screamSource.Play();
                setAttackFace = true;
                StartCoroutine(MoveToPlayer());
                PlayerController.Instance.KillPlayer(true);
            }
        }
    }

    private bool playerNear = false;

    private bool setAttackFace = false;
    public IEnumerator MoveToPlayer()
    {
        Vector3 extraDistance = Vector3.Normalize(PlayerController.Instance.transform.position - startPos) * GameManager.Instance.worldUnits;
        float timer = attackMoveTime;
        while(timer > 0)
        {
            timer -= Time.deltaTime;
            if(timer < 0) {
                timer = 0;
            }
            transform.position = Vector3.Lerp(PlayerController.Instance.transform.position + extraDistance, startPos, (timer / attackMoveTime));
            yield return null;
        }
        yield return new WaitForSeconds(respawnTime);
        transform.position = startPos;
        ChangeState(TurretState.Locating);
        screamSource.clip = idleClip;
        screamSource.time = 0;
        screamSource.Play();
        setAttackFace = false;
    }

    public void CheckForStateChange()
    {
        switch(turretState)
        {
            case TurretState.Locating:
                if(canSeePlayer) {
                    ChangeState(TurretState.Firing);
                    screamSource.clip = spotClip;
                    screamSource.time = 0;
                    screamSource.Play();
                }
                break;
            default:
                if(!canSeePlayer && !PlayerController.Instance.playerDead) {
                    ChangeState(TurretState.Locating);
                    screamSource.clip = idleClip;
                    screamSource.time = 0;
                    screamSource.Play();
                    setAttackFace = false;
                }
                break;
        }
    }

    public void ChangeState(TurretState newState)
    {
        turretState = newState;
        switch (newState)
        {
            case TurretState.Locating:
                eyeRenderer.material = defaultEye;
                break;
            case TurretState.Firing:
                playerNear = false;
                fireTimer = fireDelay;
                startingTargetPos = lookAtTransform.position;
                eyeRenderer.material = warningEye;
                break;
        }
    }
}
