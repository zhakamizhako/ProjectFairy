using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class OpenWorldMovementLogic : UdonSharpBehaviour
{
    public EngineController EngineControl;
    public Transform Map;
    public Vector3 AnchorCoordsPosition = Vector3.zero;
    [UdonSynced(UdonSyncMode.Smooth)] public Vector3 PosSync;
    [UdonSynced(UdonSyncMode.Smooth)] public Quaternion RotSync;
    public VRCPlayerApi localPlayer;
    public bool testY = false;
    private bool respawnCall = false;
    public bool syncRotate = false;
    private Quaternion startRot;
    private Vector3 startPos;
    private bool Moved = false;
    public Transform PlayerParent;
    // public Quaternion AnchorCoordsRotation;
    // public float maxX = 1000;
    // public float maxY = 1000;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        if(EngineControl.Piloting){
            Networking.SetOwner(localPlayer, EngineControl.gameObject);
            AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position;
            PosSync = -Map.transform.position + AnchorCoordsPosition;
            RotSync = EngineControl.VehicleMainObj.transform.rotation;
            startPos = EngineControl.VehicleMainObj.transform.position;
        }
        // AnchorCoordsRotation = EngineControl.VehicleMainObj.transform.rotation;
    }

    public void CallForRespawn(){
        respawnCall = true;
        AnchorCoordsPosition = startPos;
        respawnCall = false;
    }
    void Update()
    {
        if ((EngineControl.Pilot != localPlayer && (!EngineControl.Passenger)) && !respawnCall)
        {
            EngineControl.VehicleMainObj.transform.position = PosSync;
            if(syncRotate)
            EngineControl.VehicleMainObj.transform.rotation = RotSync;
        }else{

        }
        // }else if(!EngineControl.Occupied && ){

        //     EngineControl.VehicleMainObj.transform.position = AnchorCoordsPosition;
        // }
        MovementLogic();
    }

    void MovementLogic(){
        if (EngineControl != null && Map != null && EngineControl.CatapultStatus == 0 && !respawnCall && (EngineControl.Piloting || EngineControl.Passenger))
        {
            if (EngineControl.Piloting && !EngineControl.Taxiing)
            {
                EngineControl.VehicleMainObj.transform.position = new Vector3(AnchorCoordsPosition.x, testY ? AnchorCoordsPosition.y : EngineControl.VehicleMainObj.transform.position.y, AnchorCoordsPosition.z);
                // EngineControl.VehicleMainObj.transform.rotation = AnchorCoordsRotation;

                Map.transform.Translate(-(EngineControl.VehicleRigidbody.velocity * (Time.deltaTime))); //Divider set to 1. Maybe i should take that out. 
                PosSync = -Map.transform.position + AnchorCoordsPosition;
                if(syncRotate)
                RotSync = EngineControl.VehicleMainObj.transform.rotation;
            }
            else if (EngineControl.Passenger)
            {
                Map.position = -PosSync + AnchorCoordsPosition;
                EngineControl.VehicleMainObj.transform.position = new Vector3(AnchorCoordsPosition.x, testY? AnchorCoordsPosition.y : PosSync.y, AnchorCoordsPosition.z);
                if(syncRotate)
                EngineControl.VehicleMainObj.transform.rotation = RotSync;
            }
            else
            {
                AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position;
            }

        }

         AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position; 
    }
}
