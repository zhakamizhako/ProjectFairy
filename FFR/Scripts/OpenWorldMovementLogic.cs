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
    // public Quaternion AnchorCoordsRotation;
    // public float maxX = 1000;
    // public float maxY = 1000;
    void Start()
    {
        if (Networking.IsOwner(EngineControl.gameObject))
        {
            AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position;
            PosSync = -Map.transform.position + AnchorCoordsPosition;
            RotSync = EngineControl.VehicleMainObj.transform.rotation;
        }
        // AnchorCoordsRotation = EngineControl.VehicleMainObj.transform.rotation;
    }

    void Update()
    {
        if (!EngineControl.Piloting || !EngineControl.Passenger)
        {
            EngineControl.VehicleMainObj.transform.position = PosSync;
            EngineControl.VehicleMainObj.transform.rotation = RotSync;
        }
    }

    void FixedUpdate()
    {
        if (EngineControl != null && Map != null && EngineControl.CatapultStatus == 0)
        {
            if (EngineControl.Piloting)
            {
                EngineControl.VehicleMainObj.transform.position = new Vector3(AnchorCoordsPosition.x, testY ? AnchorCoordsPosition.y : EngineControl.VehicleMainObj.transform.position.y, AnchorCoordsPosition.z);
                // EngineControl.VehicleMainObj.transform.rotation = AnchorCoordsRotation;

                Map.transform.Translate(-(EngineControl.VehicleRigidbody.velocity * (Time.deltaTime / 1))); //Divider set to 1. Maybe i should take that out. 
                PosSync = -Map.transform.position + AnchorCoordsPosition;
                RotSync = EngineControl.VehicleMainObj.transform.rotation;
            }
            else if(EngineControl.Passenger){
                Map.transform.Translate(PosSync);
                EngineControl.VehicleMainObj.transform.rotation = RotSync;
            }

        }
        else
        {
            AnchorCoordsPosition = EngineControl.VehicleMainObj.transform.position;
        }
    }
}
