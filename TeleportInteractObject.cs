
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeleportInteractObject : UdonSharpBehaviour
{
    public Transform Endpoint;
    private VRCPlayerApi localPlayer;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    void Interact(){
        if(Endpoint!=null){
            localPlayer.TeleportTo(Endpoint.position, Endpoint.rotation);
        }
    }
}
