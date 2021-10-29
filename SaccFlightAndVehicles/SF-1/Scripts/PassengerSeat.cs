using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PassengerSeat : UdonSharpBehaviour
{
    public EngineController EngineControl;
    public GameObject LeaveButton;
    public GameObject SeatAdjuster;
    private LeaveVehicleButton LeaveButtonControl;
    public GameObject[] EnableObjectsOnStart;
    private Transform PlaneMesh;
    private LayerMask Planelayer;
    public GameObject fHud;
    public CanvasHud CVH;
    public CanvasHud CVHVR;
    public GameObject ButtonSet;
    public MissileTrackerAndResponse mistracker;
    public Transform teleportTo;
    public bool returnToNorah = false;
    public VRCStation x;
    // public MissilePlaneSystem MissileControl;
    public WeaponSelector wp;
    [UdonSynced(UdonSyncMode.None)] public bool Occupied = false;
    private void Start()
    {
        Assert(EngineControl != null, "Start: EngineControl != null");
        Assert(LeaveButton != null, "Start: LeaveButton != null");
        Assert(SeatAdjuster != null, "Start: SeatAdjuster != null");

        LeaveButtonControl = LeaveButton.GetComponent<LeaveVehicleButton>();

        PlaneMesh = EngineControl.PlaneMesh.transform;
        // Planelayer = PlaneMesh.gameObject.layer;
    }

    public void TPToSeat(){
        if(!Occupied)
        Interact();
    }

    private void Interact()
    {
        Occupied = true;
        EngineControl.PasengerEnterPlaneLocal();
        // Networking.SetOwner (EngineControl.localPlayer, gameObject);
        //Set CVH Params
        if (EngineControl.localPlayer.IsUserInVR())
        {
            if (CVHVR != null)
            {
                CVHVR.gameObject.SetActive(true);
            }
        }
        else
        {
            if (CVH != null)
            {
                CVH.gameObject.SetActive(true);
            }
        }
        // if (MissileControl != null) {
        //     MissileControl.showTargets = true;
        //     Networking.SetOwner (EngineControl.localPlayer, MissileControl.gameObject);
        // }
        if (wp != null)
        {
            wp.MissilePlaneSystems[wp.selectedSystem].showTargets = true;
            // foreach (MissilePlaneSystem ms in wp.MissilePlaneSystems) {
            // Networking.SetOwner (EngineControl.localPlayer, ms.gameObject);
            // }
        }

        if (LeaveButton != null) { LeaveButton.SetActive(true); }
        if (SeatAdjuster != null) { SeatAdjuster.SetActive(true); }
        if (ButtonSet != null)
        {
            ButtonSet.SetActive(true);
        }
        EngineControl.localPlayer.UseAttachedStation();
        // if (PlaneMesh != null) {
        //     Transform[] children = PlaneMesh.GetComponentsInChildren<Transform> ();
        //     foreach (Transform child in children) {
        //         child.gameObject.layer = EngineControl.OnboardPlaneLayer;
        //     }
        // }
        if (fHud != null)
        {
            fHud.SetActive(true);
        }

        if (mistracker != null)
        {
            mistracker.UIScript.PlayerAircraft = EngineControl;
        }
        if (EnableObjectsOnStart != null && EnableObjectsOnStart.Length > 0)
        {
            foreach (GameObject x in EnableObjectsOnStart)
            {
                x.SetActive(true);
            }
        }
    }

    public override void OnStationEntered(VRCPlayerApi player)
    {
        //voice range change to allow talking inside cockpit (after VRC patch 1008)
        if (player != null)
        {
            LeaveButtonControl.SeatedPlayer = player.playerId;
            if (player.isLocal)
            {
                foreach (LeaveVehicleButton crew in EngineControl.LeaveButtons)
                {
                    VRCPlayerApi guy = VRCPlayerApi.GetPlayerById(crew.SeatedPlayer);
                    if (guy != null)
                    {
                        SetVoiceInside(guy);
                    }
                }
            }
            else if (EngineControl.Piloting || EngineControl.Passenger)
            {
                SetVoiceInside(player);
            }
        }
    }
    public override void OnStationExited(VRCPlayerApi player)
    {
        PlayerExitPlane(player);
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (player.playerId == LeaveButtonControl.SeatedPlayer)
        {
            PlayerExitPlane(player);
        }
    }
    public void PlayerExitPlane(VRCPlayerApi player)
    {
        if(Networking.IsOwner(Networking.LocalPlayer, gameObject)){
            Occupied = false;
        }
        LeaveButtonControl.SeatedPlayer = -1;
        if (player != null)
        {
            SetVoiceOutside(player);
            if (player.isLocal)
            {
                foreach (LeaveVehicleButton crew in EngineControl.LeaveButtons)
                {
                    VRCPlayerApi guy = VRCPlayerApi.GetPlayerById(crew.SeatedPlayer);
                    if (guy != null)
                    {
                        SetVoiceOutside(guy);
                    }
                }

                if (EngineControl.EffectsControl != null)
                {
                    EngineControl.EffectsControl.PlaneAnimator.SetBool("deactivate_hud", false);
                }

                if (EngineControl != null)
                {
                    if (EngineControl.EffectsControl != null) { EngineControl.EffectsControl.PlaneAnimator.SetBool("localpassenger", false); }
                    EngineControl.Passenger = false;
                    EngineControl.localPlayer.SetVelocity(EngineControl.CurrentVel);
                    // EngineControl.MissilesIncoming = 0;
                    EngineControl.EffectsControl.PlaneAnimator.SetInteger("missilesincoming", 0);
                }
                if (LeaveButton != null) { LeaveButton.SetActive(false); }
                if (SeatAdjuster != null) { SeatAdjuster.SetActive(false); }
                if (EngineControl.HUDControl != null) { EngineControl.HUDControl.gameObject.SetActive(false); }
                if (CVH != null)
                {
                    CVH.gameObject.SetActive(false);
                }
                if (CVHVR != null)
                {
                    CVHVR.gameObject.SetActive(false);
                }
                if (wp != null)
                {
                    wp.MissilePlaneSystems[wp.selectedSystem].showTargets = false;
                    wp.MissilePlaneSystems[wp.selectedSystem].leavePlane();
                }
                if (mistracker != null)
                {
                    mistracker.cleanup();
                    mistracker.UIScript.PlayerAircraft = null;
                }
                if (EngineControl.OWML != null)
                {
                    if (returnToNorah) EngineControl.OWML.Map.position = Vector3.zero;
                }
                // if (MissileControl != null) {
                //     MissileControl.showTargets = false;
                //     MissileControl.leavePlane ();
                // }
                if (fHud != null)
                {
                    fHud.SetActive(false);
                }
                if (ButtonSet != null)
                {
                    ButtonSet.SetActive(false);
                }

                if (teleportTo != null && EngineControl.dead == true)
                {
                    player.TeleportTo(teleportTo.position, teleportTo.rotation);
                    player.SetVelocity(Vector3.zero);
                }

                if (EnableObjectsOnStart != null && EnableObjectsOnStart.Length > 0)
                {
                    foreach (GameObject x in EnableObjectsOnStart)
                    {
                        x.SetActive(false);
                    }
                }
                // if (PlaneMesh != null) {
                //     Transform[] children = PlaneMesh.GetComponentsInChildren<Transform> ();
                //     foreach (Transform child in children) {
                //         child.gameObject.layer = Planelayer;
                //     }
                // }
            }
        }
    }

    private void SetVoiceInside(VRCPlayerApi Player)
    {
        Player.SetVoiceDistanceNear(999999);
        Player.SetVoiceDistanceFar(1000000);
        Player.SetVoiceGain(.6f);
    }
    private void SetVoiceOutside(VRCPlayerApi Player)
    {
        Player.SetVoiceDistanceNear(0);
        Player.SetVoiceDistanceFar(25);
        Player.SetVoiceGain(15);
    }
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}
