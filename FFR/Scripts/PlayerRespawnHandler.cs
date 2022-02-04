
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerRespawnHandler : UdonSharpBehaviour
{
    public Transform MapObject;
    public bool PlayerEntered = false;
    public PlayerUIScript UIScript;

    public OpenWorldMovementLogic OWMLDefault;
    public bool resetOWMLOnRespawn = false;

    // public void OnPlayerRespawn(VRCPlayerApi player)
    // {
    //     if (player == Networking.LocalPlayer)
    //     {
    //         
    //     }
    // }

    public void OnPlayerRespawn(VRCPlayerApi Player)
    {
        ResetWorld(Player);
    }
    public void OnPlayerTriggerEnter(VRCPlayerApi Player)
    {
        ResetWorld(Player);
    }

    public void ResetWorld(VRCPlayerApi Player)
    {
        if (!PlayerEntered && Networking.LocalPlayer == Player)
        {
            MapObject.position = Vector3.zero;
            PlayerEntered = true;

            if(UIScript.OWMLPlayer!=null)
            UIScript.OWMLPlayer.exitPersonOWML();

            if (resetOWMLOnRespawn && OWMLDefault)
            {
                UIScript.OWMLPlayer = OWMLDefault;
            }
            else
            {
                UIScript.OWMLPlayer = null;
            }
        }
    }
    public void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        PlayerEntered = false;
    }


}
