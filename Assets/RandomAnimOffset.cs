using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimOffset : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().SetFloat("Offset", Random.Range(0f, 1f));
    }
}
