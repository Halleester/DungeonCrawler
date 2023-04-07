using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternPickup : FlickeringLight, ILightable
{
    public GameObject lanterObj;
    public FlickeringLight[] lightsToUnlight;
    public float initialDelay = 1f;
    public float individualDelay = 0.2f;
    public GameObject blocker;

    public void LightObject()
    {
        SetLightLevel(0);
        lanterObj.SetActive(false);
        PlayerController.Instance.SetLanternActive(true);
        blocker.SetActive(false);
        StartCoroutine(FlameDieDelay());
    }

    private IEnumerator FlameDieDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        foreach (FlickeringLight light in lightsToUnlight) {
            light.SetLightLevel(0);
            yield return new WaitForSeconds(individualDelay);
        }
    }
}
