
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class InstanceownerChecker : UdonSharpBehaviour
{
    public Text textDisplay;

    public override void OnPlayerJoined(VRCPlayerApi player){
        textDisplay.text = Networking.GetOwner(gameObject).displayName;
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        textDisplay.text = Networking.GetOwner(gameObject).displayName;
    }
}
