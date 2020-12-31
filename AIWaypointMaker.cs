
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AIWaypointMaker : UdonSharpBehaviour
{  

    private VRCPlayerApi localPlayer = null;
    private bool enabled = false;
    public GameObject waypointObject;
    private GameObject waypointRuntime;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        if(localPlayer == null) { enabled = true; }
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.V)){
            if(waypointObject !=null){
                waypointRuntime = VRCInstantiate (waypointObject);
                waypointRuntime.transform.position = gameObject.transform.position;
                waypointRuntime.transform.rotation = gameObject.transform.rotation;
            }
        }
    }
}
