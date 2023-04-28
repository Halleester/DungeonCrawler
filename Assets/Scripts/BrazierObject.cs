using UnityEngine;

public class BrazierObject : FlickeringLight, ILightable
{
    public Vector3 respawnPos;
    public float respawnRot;

    public void LightObject() {
        if(isLit) { return; }
        SetLightLevel(1);
        PlayerController.Instance.recentBrazier = this;
    }
}