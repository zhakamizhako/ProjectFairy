
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerRespawnHandler : UdonSharpBehaviour
{
    public Transform MapObject;
    public bool PlayerEntered = false;
    public void OnPlayerTriggerEnter (VRCPlayerApi Player){
        if(!PlayerEntered && Networking.LocalPlayer == Player){
            MapObject.position = Vector3.zero;
            PlayerEntered = true;
        }
    }
    public void OnPlayerTriggerExit (VRCPlayerApi player)
    {
        PlayerEntered = false;
    }


}
