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

    public AudioSource musicSource;
    public float lerpSpeed = 0.1f;

    public void Start()
    {
        Instance = this;
        targetVolume = musicSource.volume;
    }

    public void Update()
    {
        if(musicSource.volume != targetVolume) {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, targetVolume, lerpSpeed * Time.deltaTime);
        }
    }

    private float targetVolume = 0;
    public static void SetMusicVolume(float targetVolume, bool startMusic = false)
    {
        if (startMusic) {
            Instance.musicSource.Play();
        }
        Instance.targetVolume = targetVolume;
    }
}
