using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILightable
{
    public void LightObject();
}

public class FlickeringLight : MonoBehaviour
{
    [Header("Light Settings")]
    public bool isLit = false;
    public Light pointLight;
    protected float targetBrightness = 0.8f;
    protected float targetRange = 3;

    public float visualChangeRate = 0.1f;
    public AnimationCurve lightBrightnessFalloff;
    public AnimationCurve lightRangeFalloff;

    protected float currentLitLevel = 0;
    protected float visualLitLevel = 0;

    public ParticleSystem smokeParticles;
    public ParticleSystem fireParticles;
    public bool fireAsLightColor = true;

    protected float smokeEmission;
    protected float smokeSize;
    protected float fireEmission;
    protected float fireSize;

    [Header("Flicker Settings")]
    public float flickerRangeIntensity = 0.2f;
    public float flickerBrightnessIntensity = 0.1f;
    public float flickerRate = 0.3f;
    public float flickerRandomness = 4f;

    protected float flickerTime = 0;

    protected bool prevSeePlayer = false;

    public virtual void Start()
    {
        currentLitLevel = isLit ? 1 : 0;
        visualLitLevel = currentLitLevel;

        targetBrightness = pointLight.intensity;
        targetRange = pointLight.range;
        if (!isLit)
        {
            pointLight.intensity = 0;
        }


        if(smokeParticles) {
            smokeEmission = smokeParticles.emission.rateOverTime.constant;
            smokeSize = smokeParticles.main.startSize.constant;
        }

        if (fireParticles) { 
            fireEmission = fireParticles.emission.rateOverTime.constant;
            fireSize = fireParticles.main.startSize.constant;

            if(fireAsLightColor) {
                var fireMain = fireParticles.main;
                fireMain.startColor = pointLight.color;
            }
        }
    }

    public void SetLightLevel(float litLevel)
    {
        currentLitLevel = litLevel;
        isLit = currentLitLevel > 0;
    }

    public virtual void Update()
    {
        // Flickering light logic
        flickerTime += Time.deltaTime * (1 - Random.Range(-flickerRandomness, flickerRandomness)) * Mathf.PI;
        float flickerAmount = Mathf.Sin(flickerTime * flickerRate);
        flickerAmount *= isLit ? 1 : 0;

        visualLitLevel = Mathf.MoveTowards(visualLitLevel, currentLitLevel, visualChangeRate);

        // Check if the light can raycast to the player (and is lit)
        if (PlayerController.Instance != null)
        {
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(transform.position, PlayerController.Instance.transform.position - transform.position, out hit, visualLitLevel * targetRange, GameManager.Instance.playerMask);
            bool hitPlayer = false;
            if (hitSomething && isLit)
            {
                if (hit.collider.tag == "Player")
                {
                    hitPlayer = true;
                    Debug.DrawRay(transform.position, PlayerController.Instance.transform.position - transform.position, Color.red);
                }
            }

            // Marking the player if this light can see them
            if (hitPlayer && !prevSeePlayer)
            {
                PlayerController.Instance.objectsLightingPlayer.Add(gameObject);
            }
            else if (!hitPlayer && prevSeePlayer)
            {
                PlayerController.Instance.objectsLightingPlayer.Remove(gameObject);
            }
            prevSeePlayer = hitPlayer;
        }

        pointLight.range = lightRangeFalloff.Evaluate(visualLitLevel + flickerAmount * flickerRangeIntensity) * targetRange;
        pointLight.intensity = lightBrightnessFalloff.Evaluate(visualLitLevel + flickerAmount * flickerBrightnessIntensity) * targetBrightness;

        // Update the particle effects
        if (fireParticles) {
            var fireSystem = fireParticles.main;
            var fireEmitter = fireParticles.emission;
            fireSystem.startSize = fireSize * visualLitLevel;
            fireEmitter.rateOverTime = fireEmission * visualLitLevel;
        }
        if (smokeParticles) {
            
            var smokeSystem = smokeParticles.main;
            var smokeEmitter = smokeParticles.emission;
            smokeSystem.startSize = smokeSize * visualLitLevel;
            smokeEmitter.rateOverTime = smokeEmission * visualLitLevel;
        }
    }

}