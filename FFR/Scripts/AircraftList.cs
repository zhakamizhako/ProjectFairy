
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AircraftList : UdonSharpBehaviour
{
    public GameObject PlaneParent;
    public VehicleRespawnButton VehicleButton;
    public EngineController EngineController;
    [TextArea] public string VehicleDescription;
    public string AircraftName;

    void Start(){
        gameObject.SetActive(false);
    }
}
