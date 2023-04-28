using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public List<GameObject> objectsLightingPlayer = new List<GameObject>();

    [Header("Movement Settings")]
    public bool smoothMove = true;
    public float moveTimeLength = 0.1f;
    public AnimationCurve moveCurve;
    [Space(20)]
    public bool smoothTurn = true;
    public float turnTimeLength = 0.1f;
    public AnimationCurve rotateCurve;
    [Space(20)]
    public float delayTime = 0.1f;
    public Animator anim;

    public LayerMask trapMask;

    [Header("Raycast Settings")]
    public Vector3 raycastOffset = new Vector3(0, 0.5f, 0);

    [Header("Camera Settings")]
    public Camera cam;
    public AnimationCurve camMoveRate;
    public float xAngleRange = 60;
    public float yAngleRange = 60;
    public float camReturnRate = 5f;

    private bool cursorDown = false;
    private Quaternion startingCamAngle;

    private Vector3 targetRot;
    private Vector3 prevTargetRot;
    private float rotProgress = 0;

    private Vector3 targetPos;
    private Vector3 prevTargetPos;
    private float posProgress = 0;

    private bool isMoving = false;
    private bool isMoveDelayed = false;
    private bool isRotating = false;
    private bool isRotateDelayed = false;
    public bool playerDead = false;

    public BrazierObject recentBrazier;

    private enum MoveDirection { Forward, Backwards, Left, Right };

    [Header("Light Settings")]
    public GameObject lanternObj;
    public bool lanternActive = false;
    public ParticleSystem candleParticles;
    public AnimationCurve candleSizeCurve;
    public float flameLossRate = 0.01f;
    public float flameLossFromSwing = 0.2f;

    private float visualLitLevel = 1;
    public float visualChangeRate = 0.1f;
    public AnimationCurve lightBrightnessFalloff;
    public AnimationCurve lightRangeFalloff;

    [Space(20)]
    public float swingTime = 2f;
    public LayerMask lightHitMask;
    public Light pointLight;
    public Light glowLight;
    public TMP_Text matchText;

    [Space(20)]
    public float flickerRangeIntensity = 0.2f;
    public float flickerBrightnessIntensity = 0.1f;
    public float flickerRate = 0.3f;
    public float flickerRandomness = 4f;

    private float flickerTime = 0;

    private float startBrightness;
    private float startRange;

    private float glowBrightness;

    private bool isSwinging = false;

    [Header("Light Info")]
    public int currentMatches;
    public float lanternLitLevel = 1;

    [Header("Game Over Settings")]
    public float deathDelayTime = 2f;
    public float deathBreathTime = 2f;
    public float deathTitleDelay = 1f;
    public GameObject jumpscareObj;

    [Header("Audio Settings")]
    public AudioSource footSource;
    public AudioClip[] stepClips;
    [Range(0, 1)]
    public float stepVolume = 1f;

    [Space(20)]
    public AudioSource candleSource;
    [Range(0, 1)]
    public float candleBurnVolume = 1;
    public AudioSource candleEffectSource;
    [Range(0, 1)]
    public float candleEffectVolume = 1;
    public AudioClip candleExtinguishClip;
    public AudioClip candleLightClip;

    [Space(20)]
    public AudioSource playerSource;
    public AudioClip breathQuicken;
    [Range(0, 1)]
    public float breathVolume = 1;

    // Start is called before the first frame update
    private void Start()
    {
        Instance = this;

        targetPos = Vector3Int.RoundToInt(transform.position);
        transform.position = targetPos;
        targetRot = Vector3Int.RoundToInt(transform.rotation.eulerAngles / 90) * 90;
        transform.rotation = Quaternion.Euler(targetRot);

        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;

        startingCamAngle = cam.transform.localRotation;

        currentMatches = GameManager.Instance.startingMatches;
        matchText.text = "" + currentMatches;

        startBrightness = pointLight.intensity;
        startRange = pointLight.range;
        glowBrightness = glowLight.intensity;
        /*var candleMain = candleParticles.main;
        candleMain.startColor = pointLight.color;*/

        SetLanternActive(lanternActive);
    }

    public void SetLanternActive(bool active)
    {
        lanternObj.SetActive(active);
        lanternActive = active;
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        if (horizontalInput == 1) MovePlayer(MoveDirection.Right);
        if (horizontalInput == -1) MovePlayer(MoveDirection.Left);
        if (verticalInput == 1) MovePlayer(MoveDirection.Forward);
        if (verticalInput == -1) MovePlayer(MoveDirection.Backwards);
        float rotationInput = Input.GetAxisRaw("Rotation");
        if (rotationInput == 1) RotatePlayer(false);
        if (rotationInput == -1) RotatePlayer(true);

        if(cursorDown)
        {
            Vector2 mouseScreenScale = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            float camXAngle = xAngleRange * camMoveRate.Evaluate(mouseScreenScale.x);
            float camYAngle = yAngleRange * camMoveRate.Evaluate(mouseScreenScale.y);

            cam.transform.localRotation = Quaternion.AngleAxis(camXAngle, Vector3.up) * Quaternion.AngleAxis(camYAngle, -Vector3.right) * startingCamAngle;
        } else
        {
            cam.transform.localRotation = Quaternion.RotateTowards(cam.transform.localRotation, startingCamAngle, camReturnRate * Time.deltaTime);
        }
        

        if(Input.GetButtonDown("Look"))
        {
            cursorDown = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if(Input.GetButtonUp("Look"))
        {
            cursorDown = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(Input.GetButtonDown("Swing") && !isSwinging && !playerDead) {
            if(lanternActive) {
                if (lanternLitLevel != 0) {
                    StartCoroutine(SwingLight());
                    anim.SetTrigger("Swing");
                }
                else if (currentMatches > 0) {
                    StartCoroutine(SwingLight());
                    AddToLampLitLevel(1);
                    currentMatches--;
                    matchText.text = "" + currentMatches;
                }
            } else {
                CheckForSwingContact();
            }
        }

        if(Input.GetButtonDown("Extinguish")) {
            AddToLampLitLevel(-lanternLitLevel);
        }
        
        // Add a flicker effect to the fire if we're not extinquishing it
        flickerTime += Time.deltaTime * (1 - Random.Range(-flickerRandomness, flickerRandomness)) * Mathf.PI;
        float flickerAmount = Mathf.Sin(flickerTime * flickerRate);
        flickerAmount *= lanternLitLevel;
        // Visually updates the lit level of the lantern
        visualLitLevel = Mathf.MoveTowards(visualLitLevel, lanternLitLevel, visualChangeRate);
        pointLight.range = lightRangeFalloff.Evaluate(visualLitLevel + flickerAmount * flickerRangeIntensity) * startRange;
        pointLight.intensity = lightBrightnessFalloff.Evaluate(visualLitLevel + flickerAmount * flickerBrightnessIntensity) * startBrightness;

        glowLight.intensity = visualLitLevel * glowBrightness;

        candleParticles.gameObject.transform.localScale = Vector3.one * candleSizeCurve.Evaluate(visualLitLevel);

        candleSource.volume = candleBurnVolume * visualLitLevel;
    }

    private IEnumerator SwingLight()
    {
        isSwinging = true;
        yield return new WaitForSeconds(swingTime);
        isSwinging = false;
    }

    public void CheckForSwingContact()
    {
        if ((lanternLitLevel == 0 && lanternActive) || IsWallInDir(Vector3Int.RoundToInt(transform.forward) * GameManager.Instance.worldUnits)) { return; }
        if (lanternActive) {
            AddToLampLitLevel(-flameLossFromSwing);
        }
        float worldUnits = GameManager.Instance.worldUnits;
        float halfWorldUnits = worldUnits / 2;
        Collider[] hitColliders = Physics.OverlapBox(transform.position + (Vector3)(Vector3Int.RoundToInt(transform.forward)) * worldUnits, new Vector3(halfWorldUnits, halfWorldUnits, halfWorldUnits), Quaternion.identity, lightHitMask);
        foreach(Collider collider in hitColliders) {
            ILightable lightInterface = collider.gameObject.GetComponent<ILightable>();
            if(lightInterface != null) {
                lightInterface.LightObject();
                Debug.Log("Lit " + collider.gameObject.name);
                continue;
            }
            IEnemy enemyInterface = collider.gameObject.GetComponent<IEnemy>();
            if(enemyInterface != null) {
                enemyInterface.SwungAt(transform.forward);
                Debug.Log("Hit enemy " + collider.gameObject.name);
                continue;
            }
        }
    }

    private void MovePlayer(MoveDirection dir)
    {
        if(isMoving || isRotating || isMoveDelayed || isSwinging || playerDead) { return; }

        Vector3 moveDir = Vector3.zero;

        switch (dir)
        {
            case MoveDirection.Forward:
                moveDir = transform.forward;
                break;
            case MoveDirection.Backwards:
                moveDir = -transform.forward;
                break;
            case MoveDirection.Left:
                moveDir = -transform.right;
                break;
            case MoveDirection.Right:
                moveDir = transform.right;
                break;
        }

        moveDir = Vector3Int.RoundToInt(moveDir) * GameManager.Instance.worldUnits;

        if(IsWallInDir(moveDir)) { return; }

        if(IsFloorInDir(moveDir)) {
            isMoving = true;
            prevTargetPos = targetPos;
            targetPos += moveDir;
            PlayStepSound();
        }
    }

    public void PlayStepSound()
    {
        AudioClip randomStep = stepClips[Random.Range(0, stepClips.Length)];
        footSource.PlayOneShot(randomStep, stepVolume);
    }

    public bool IsWallInDir(Vector3 dir)
    {
        RaycastHit hit;
        // Check if wall is in move direction, don't move if found one
        Debug.DrawRay(transform.position + raycastOffset, dir, Color.yellow, 1f);
        if (Physics.Raycast(transform.position + raycastOffset, dir, out hit, GameManager.Instance.worldUnits, GameManager.Instance.wallMask))
        {
            return true;
        }
        return false;
    }

    public bool IsFloorInDir(Vector3 dir)
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + dir, Vector3.down, Color.cyan, 3f);
        // Check if floor is at the move position, otherwise don't move
        if (Physics.Raycast(transform.position + dir, Vector3.down, out hit, 1.1f, GameManager.Instance.wallMask))
        {
            return true;
        }
        return false;
    }

    private void RotatePlayer(bool turnLeft)
    {
        if (isMoving || isRotating || isRotateDelayed || playerDead) { return; }
        prevTargetRot = targetRot;
        isRotating = true;
        targetRot += Vector3.up * 90f * (turnLeft ? -1 : 1);
    }

    private void FixedUpdate()
    {
        if(isMoving)
        {
            if(smoothMove) {
                posProgress += Time.fixedDeltaTime * 1/ moveTimeLength;
                if (posProgress > 1) { posProgress = 1; }
                transform.position = Vector3.LerpUnclamped(prevTargetPos, targetPos, moveCurve.Evaluate(posProgress));
                if(transform.position == targetPos) {
                    posProgress = 0;
                    transform.position = targetPos;
                    isMoving = false;
                    StartCoroutine(DelayMove(true));
                    ActivateTileTraps();
                }
            } else {
                transform.position = targetPos;
                isMoving = false;
            }
                
        }

        if(isRotating)
        {
            if(smoothTurn) {
                rotProgress += Time.fixedDeltaTime * 1/ turnTimeLength;
                if(rotProgress > 1) { rotProgress = 1; }
                transform.rotation = Quaternion.LerpUnclamped(Quaternion.Euler(prevTargetRot), Quaternion.Euler(targetRot), rotateCurve.Evaluate(rotProgress));
                if(transform.rotation == Quaternion.Euler(targetRot)) {
                    rotProgress = 0;
                    transform.rotation = Quaternion.Euler(targetRot);
                    isRotating = false;
                    StartCoroutine(DelayMove(false));
                }
            } else {
                transform.rotation = Quaternion.Euler(targetRot);
                isRotating = false;
            }
        }

        if(lanternLitLevel > 0 && !isSwinging && lanternActive) {
            AddToLampLitLevel(Time.fixedDeltaTime * -flameLossRate);
        }
    }

    public void ActivateTileTraps()
    {
        float worldUnits = GameManager.Instance.worldUnits;
        float halfWorldUnits = worldUnits / 2;
        Collider[] hitColliders = Physics.OverlapBox(transform.position, new Vector3(halfWorldUnits, halfWorldUnits, halfWorldUnits), Quaternion.identity, trapMask);
        foreach (Collider collider in hitColliders)
        {
            ITrap trapInterface = collider.gameObject.GetComponent<ITrap>();
            if (trapInterface != null)
            {
                trapInterface.PlayerEnteredTile();
                Debug.Log("Activated trap \"" + collider.gameObject.name + "\"");
            }
        }
    }

    private void AddToLampLitLevel(float value)
    {
        bool wasLit = lanternLitLevel > 0;
        lanternLitLevel += value;
        lanternLitLevel = Mathf.Clamp01(lanternLitLevel);

        if (lanternLitLevel <= 0 && currentMatches <= 0) {
            StartCoroutine(GameOver());
        }

        bool isLit = lanternLitLevel > 0;
        if(isLit != wasLit) {
            if(isLit && candleLightClip) {
                candleEffectSource.PlayOneShot(candleLightClip, candleEffectVolume);
            } else if(!isLit && candleExtinguishClip) {
                candleEffectSource.PlayOneShot(candleExtinguishClip, candleEffectVolume);
            }
        }
    }

    private IEnumerator DelayMove(bool isMove)
    {
        if (isMove)
            isMoveDelayed = true;
        else
            isRotateDelayed = true;

        yield return new WaitForSeconds(delayTime);

        if (isMove)
            isMoveDelayed = false;
        else
            isRotateDelayed = false;
    }

    public static bool PlayerInLight() {
        return Instance.objectsLightingPlayer.Count > 0 || (Instance.lanternLitLevel > 0 && Instance.lanternActive);
    }

    public void KillPlayer(bool unlight = false) {
        if(playerDead) { return; }
        Debug.Log("Player Dieded :(");
        playerDead = true;
        StartCoroutine(RespawnPlayer(unlight));
    }

    public IEnumerator GameOver()
    {
        if(onFinale) { yield break; }
        GameManager.SetMusicVolume(0);
        yield return new WaitForSeconds(deathDelayTime);
        if (onFinale) { yield break; }
        playerSource.PlayOneShot(breathQuicken, breathVolume);
        // Play sound effect of heavy breathing
        yield return new WaitForSeconds(deathBreathTime);
        // Jumpscare and kick to menu
        jumpscareObj.SetActive(true);
        playerDead = true;
        yield return new WaitForSeconds(deathTitleDelay);
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Title");
    }

    public IEnumerator RespawnPlayer(bool unlight)
    {
        if(unlight) { AddToLampLitLevel(-lanternLitLevel); }
        yield return new WaitForSeconds(1);
        Vector3 respawnPos = recentBrazier != null ? recentBrazier.respawnPos : new Vector3(0, 1, 0);
        float respawnRot = recentBrazier != null ? recentBrazier.respawnRot : 90;

        transform.position = respawnPos;
        targetPos = respawnPos;
        prevTargetPos = respawnPos;
        transform.rotation = Quaternion.Euler(0, respawnRot, 0);
        targetRot = transform.rotation.eulerAngles;
        prevTargetRot = targetRot;
        playerDead = false;

        if(currentMatches > 0) {
            AddToLampLitLevel(1);
            currentMatches--;
            matchText.text = "" + currentMatches;
        } else {
            AddToLampLitLevel(-lanternLitLevel);
            recentBrazier?.SetLightLevel(0);
        }
        
    }

    private bool onFinale = false;
    public void Finale()
    {
        onFinale = true;
        GameManager.SetMusicVolume(0);
        StartCoroutine(FinaleScene());
    }

    public IEnumerator FinaleScene()
    {
        yield return new WaitForSeconds(8f);
        recentBrazier?.SetLightLevel(0);
        AddToLampLitLevel(-lanternLitLevel);
        yield return new WaitForSeconds(2f);
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Title");
    }
}
