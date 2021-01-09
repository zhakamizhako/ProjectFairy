
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerRespawnHandler : UdonSharpBehaviour
{
    public Transform MapObject;
    private bool PlayerEntered = false;
    public override void OnPlayerCollisionEnter(VRCPlayerApi Player){
        if(!PlayerEntered){
            MapObject.position = Vector3.zero;
            PlayerEntered = true;
        }
    }

    public override void OnPlayerCollisionExit(VRCPlayerApi player)
    {
        PlayerEntered = false;
    }
}
