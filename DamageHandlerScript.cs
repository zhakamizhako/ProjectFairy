using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DamageHandlerScript : UdonSharpBehaviour {
    public AircraftController EngineController;
    public GameObject level1Burn;
    public GameObject level2Burn;
    public GameObject level3Burn;
    public GameObject level4Burn;
    public GameObject DeadBurn;
    void Start () {

    }

    void Update () {
        //Start Burning
        if (level1Burn != null) {
            if (EngineController.Health < EngineController.FullHealth * .50) {
                level1Burn.SetActive (true);
            } else {
                level1Burn.SetActive (false);
            }
        }

        if (level2Burn != null) {
            if (EngineController.Health < EngineController.FullHealth * .40) {
                level2Burn.SetActive (true);
            } else {
                level2Burn.SetActive (false);
            }
        }

        if (level3Burn != null) {
            if (EngineController.Health < EngineController.FullHealth * .30) {
                level3Burn.SetActive (true);
            } else {
                level3Burn.SetActive (false);
            }
        }

        if (level4Burn != null) {
            if (EngineController.Health < EngineController.FullHealth * .20) {
                level4Burn.SetActive (true);
            } else {
                level4Burn.SetActive (false);
            }
        }

        if (DeadBurn != null) {
            if (EngineController.Health < 1f) {
                DeadBurn.SetActive (true);
            } else {
                DeadBurn.SetActive (false);
            }
        }
    }
}