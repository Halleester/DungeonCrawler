using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTrigger : MonoBehaviour, ITrap
{
    public FlickeringLight[] lights;
    public GameObject[] objects;

    private bool entered = false;

    public void PlayerEnteredTile()
    {
        if(entered) { return; }
        entered = true;
        foreach(GameObject obj in objects)
        {
            obj.SetActive(!obj.activeSelf);
        }
        foreach(FlickeringLight light in lights) {
            light.SetLightLevel(0);
        }
    }

}
