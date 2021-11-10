
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerSlapper : UdonSharpBehaviour
{
    private bool PlayerEntered = false;
    private VRCPlayerApi Owner;
    public float batForce = 3000;
    public override void OnPlayerCollisionEnter(VRCPlayerApi Player)
    {
        if(Player!=Owner){
            Vector3 Playerf = Player.GetVelocity();
            Vector3 vel = Playerf * batForce;
            Player.SetVelocity(vel);
        }
    }

    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        PlayerEntered = false;
    }

    public void OnOwnershipTransferred(VRCPlayerApi player)
    {
        Owner = player;
    }
}
