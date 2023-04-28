using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemToggler : MonoBehaviour, ILightable
{
    public GameObject[] objectArray;
    private bool isLit = false;

    public Animator finale;

    public void LightObject()
    {
        if(isLit) { return; }
        isLit = true;
        foreach(GameObject obj in objectArray) {
            obj.SetActive(!obj.activeSelf);
        }

        if(finale != null)
        {
            finale.SetTrigger("Start");
            PlayerController.Instance.Finale();
        }
    }
}
