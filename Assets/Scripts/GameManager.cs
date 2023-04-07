using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int worldUnits = 2;
    public int startingMatches = 20;

    public LayerMask playerMask;
    public LayerMask wallMask;

    public void Start()
    {
        Instance = this;
    }
}
