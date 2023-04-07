using UnityEngine;

public class BrazierObject : FlickeringLight, ILightable
{
    public Vector3 respawnPos;
    public float respawnRot;

    public void LightObject() {
        if(isLit) { return; }
        isLit = true;
        currentLitLevel = 1;
        PlayerController.Instance.recentBrazier = this;
    }
}