using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITrap
{
    public void PlayerEnteredTile();
}

public class SpikeTrap : MonoBehaviour, ITrap
{
    public float activateTime;

    public bool spikeActive;

    public Animator anim;

    public AudioSource audioSource;
    public AudioClip tickClip;
    public AudioClip upClip;
    public AudioClip downClip;
    public AudioClip stabbedClip;
    [Range(0, 1)]
    public float spikeVolume = 1;

    public void PlayerEnteredTile()
    {
        if(!spikeActive) {
            StartCoroutine(DelaySpike());
        }
    }

    public IEnumerator DelaySpike()
    {
        audioSource.PlayOneShot(tickClip, spikeVolume);
        yield return new WaitForSeconds(activateTime);
        anim.SetTrigger("Start");
        audioSource.PlayOneShot(upClip, spikeVolume);
    }

    public void Update()
    {
        if(spikeActive && !PlayerController.Instance.playerDead) {
            Vector3 playerPos = PlayerController.Instance.transform.position;
            if(Mathf.Abs(playerPos.x - transform.position.x) <= 0.5f && Mathf.Abs(playerPos.z - transform.position.z) <= 0.5f) {
                PlayerController.Instance.KillPlayer();
                audioSource.PlayOneShot(stabbedClip, spikeVolume);
            }
        }
    }

    public void ToggleSpikeState() {
        spikeActive = !spikeActive;
    }

    public void PlayDownClip()
    {
        audioSource.PlayOneShot(downClip, spikeVolume);
    }
}
