using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PassengerSeat : UdonSharpBehaviour {
    public EngineController EngineControl;
    public GameObject LeaveButton;
    public GameObject SeatAdjuster;
    private Transform PlaneMesh;
    private LayerMask Planelayer;
    public GameObject fHud;
    public CanvasHud CVH;
    public CanvasHud CVHVR;
    public GameObject ButtonSet;
    public MissileTrackerAndResponse mistracker;
    // public MissilePlaneSystem MissileControl;
    public WeaponSelector wp;
    private void Start () {
        Assert (EngineControl != null, "Start: EngineControl != null");
        Assert (LeaveButton != null, "Start: LeaveButton != null");
        Assert (SeatAdjuster != null, "Start: SeatAdjuster != null");

        PlaneMesh = EngineControl.PlaneMesh.transform;
        // Planelayer = PlaneMesh.gameObject.layer;
    }
    private void Interact () {
        EngineControl.Passenger = true;
        // Networking.SetOwner (EngineControl.localPlayer, gameObject);
        //Set CVH Params
        if (EngineControl.localPlayer.IsUserInVR ()) {
            if (CVHVR != null) {
                CVHVR.gameObject.SetActive (true);
            }
        } else {
            if (CVH != null) {
                CVH.gameObject.SetActive (true);
            }
        }
        // if (MissileControl != null) {
        //     MissileControl.showTargets = true;
        //     Networking.SetOwner (EngineControl.localPlayer, MissileControl.gameObject);
        // }
        if (wp != null) {
            wp.MissilePlaneSystems[wp.selectedSystem].showTargets = true;
            // foreach (MissilePlaneSystem ms in wp.MissilePlaneSystems) {
            // Networking.SetOwner (EngineControl.localPlayer, ms.gameObject);
            // }
        }

        if (LeaveButton != null) { LeaveButton.SetActive (true); }
        if (SeatAdjuster != null) { SeatAdjuster.SetActive (true); }
        if (ButtonSet != null) {
            ButtonSet.SetActive (true);
        }
        if (EngineControl.HUDControl != null) { EngineControl.HUDControl.gameObject.SetActive (true); }
        if (EngineControl.EffectsControl.CanopyOpen) EngineControl.CanopyCloseTimer = -100001;
        else EngineControl.CanopyCloseTimer = -1;
        EngineControl.localPlayer.UseAttachedStation ();
        // if (PlaneMesh != null) {
        //     Transform[] children = PlaneMesh.GetComponentsInChildren<Transform> ();
        //     foreach (Transform child in children) {
        //         child.gameObject.layer = EngineControl.OnboardPlaneLayer;
        //     }
        // }
        if (fHud != null) {
            fHud.SetActive (true);
        }
    }
    public override void OnStationExited (VRCPlayerApi player) {
        if (player.isLocal) {
            if (EngineControl != null) {
                EngineControl.Passenger = false;
                EngineControl.localPlayer.SetVelocity (EngineControl.CurrentVel);
            }
            if (LeaveButton != null) { LeaveButton.SetActive (false); }
            if (SeatAdjuster != null) { SeatAdjuster.SetActive (false); }
            if (EngineControl.HUDControl != null) { EngineControl.HUDControl.gameObject.SetActive (false); }
            if (CVH != null) {
                CVH.gameObject.SetActive (false);
            }
            if (CVHVR != null) {
                CVHVR.gameObject.SetActive (false);
            }
            if (wp != null) {
                wp.MissilePlaneSystems[wp.selectedSystem].showTargets = false;
                wp.MissilePlaneSystems[wp.selectedSystem].leavePlane ();
            }
            if (mistracker != null) {
                mistracker.cleanup ();
            }
            // if (MissileControl != null) {
            //     MissileControl.showTargets = false;
            //     MissileControl.leavePlane ();
            // }
            if (fHud != null) {
                fHud.SetActive (false);
            }
            if (ButtonSet != null) {
                ButtonSet.SetActive (false);
            }
            // if (PlaneMesh != null) {
            //     Transform[] children = PlaneMesh.GetComponentsInChildren<Transform> ();
            //     foreach (Transform child in children) {
            //         child.gameObject.layer = Planelayer;
            //     }
            // }
        }
    }
    private void Assert (bool condition, string message) {
        if (!condition) {
            Debug.LogError ("Assertion failed : '" + GetType () + " : " + message + "'", this);
        }
    }
}