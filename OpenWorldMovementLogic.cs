using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class OpenWorldMovementLogic : UdonSharpBehaviour {
    public EngineController EngineControl;
    public Transform Map;
    public Vector3 AnchorCoordsPosition = Vector3.zero;
    [UdonSynced(UdonSyncMode.Smooth)] public Vector3 PosSync;
    [UdonSynced(UdonSyncMode.Smooth)] public Quaternion RotSync;
    public VRCPlayerApi localPlayer;
    // public Quaternion AnchorCoordsRotation;
    // public float maxX = 1000;
    // public float maxY = 1000;
    public float divider = 10f;
    void Start () {
        AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position;
        // AnchorCoordsRotation = EngineControl.VehicleMainObj.transform.rotation;
    }

    void Update(){
        if(!Networking.IsOwner(gameObject)){
            EngineControl.VehicleMainObj.transform.position = PosSync;
            EngineControl.VehicleMainObj.transform.rotation = RotSync;
        }
    }

    void FixedUpdate () {
        if (EngineControl != null && Map != null && (EngineControl.Piloting || EngineControl.Passenger) && EngineControl.CatapultStatus==0) {
            EngineControl.VehicleMainObj.transform.position = new Vector3 (AnchorCoordsPosition.x, EngineControl.VehicleMainObj.transform.position.y, AnchorCoordsPosition.z);
            // EngineControl.VehicleMainObj.transform.rotation = AnchorCoordsRotation;

            Map.transform.Translate (-(EngineControl.VehicleRigidbody.velocity * ( Time.deltaTime / divider))); //Divider set to 1. Maybe i should take that out. 
            PosSync = -Map.transform.position;
            RotSync = EngineControl.VehicleMainObj.transform.rotation;
        }else{
            AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position;
        }
    }
}
