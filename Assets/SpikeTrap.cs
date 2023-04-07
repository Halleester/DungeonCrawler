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

    public void PlayerEnteredTile()
    {
        if(!spikeActive) {
            StartCoroutine(DelaySpike());
        }
    }

    public IEnumerator DelaySpike()
    {
        yield return new WaitForSeconds(activateTime);
        anim.SetTrigger("Start");
    }

    public void Update()
    {
        if(spikeActive) {
            Vector3 playerPos = PlayerController.Instance.transform.position;
            if(Mathf.Abs(playerPos.x - transform.position.x) <= 0.5f && Mathf.Abs(playerPos.z - transform.position.z) <= 0.5f) {
                PlayerController.Instance.KillPlayer();
            }
        }
    }

    public void ToggleSpikeState() {
        spikeActive = !spikeActive;
    }
}
