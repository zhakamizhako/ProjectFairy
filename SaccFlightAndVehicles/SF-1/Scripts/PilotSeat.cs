
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PilotSeat : UdonSharpBehaviour
{
    public EngineController EngineControl;
    public GameObject LeaveButton;
    public GameObject Gun_pilot;
    public GameObject SeatAdjuster;
    private LeaveVehicleButton LeaveButtonControl;
    public WeaponSelector wp;
    private ParticleSystem.CollisionModule gunpilotcol;
    public GameObject fHud;
    public CanvasHud CVH;
    public CanvasHud CVHVR;
    public GameObject ButtonSet;
    public Transform teleportTo;
    public MissileTrackerAndResponse mistracker;
    public OpenWorldMovementLogic OWML;
    public GameObject[] EnableObjectsOnStart;
    public bool EnableOWMLByDefault = true;
    public bool returnToNorah = false;
    [UdonSharp.UdonSynced(UdonSyncMode.None)] public bool EnabledEngine = true;
    public bool useUniversalCanvas = false;
    private bool sit = false;
    public void EnableOWML()
    {
        // if(EngineControl.Piloting || EngineControl.Passenger){
        // OWML.gameObject.SetActive(true);
        OWML.EnableScript();
        Debug.Log("NYAHOI!");
        // }
    }

    public void DisableKinematic()
    {
        EngineControl.VehicleRigidbody.isKinematic = false;
    }
    public void callEnableEngine()
    {
        if (Networking.IsOwner(EngineControl.localPlayer, gameObject))
        {
            EnabledEngine = true;
        }
    }

    public void callEnableOWML()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "EnableOWML");
    }

    private void Start()
    {
        Assert(EngineControl != null, "Start: EngineControl != null");
        Assert(LeaveButton != null, "Start: LeaveButton != null");
        Assert(Gun_pilot != null, "Start: Gun_pilot != null");
        Assert(SeatAdjuster != null, "Start: SeatAdjuster != null");

        LeaveButtonControl = LeaveButton.GetComponent<LeaveVehicleButton>();
    }
    private void Interact()//entering the plane
    {
        if (EnabledEngine)
        {
            EngineStart();
            // EngineControl.PilotEnterPlaneLocal();
        }
        if (ButtonSet != null)
        {
            ButtonSet.SetActive(true);
        }
        if (OWML != null)
        {
            Networking.SetOwner(EngineControl.localPlayer, OWML.gameObject);
        }

        EngineControl.localPlayer.UseAttachedStation();
        if (LeaveButton != null) { LeaveButton.SetActive(true); }
        if (Gun_pilot != null) { Gun_pilot.SetActive(true); }
        if (SeatAdjuster != null) { SeatAdjuster.SetActive(true); }
        if (mistracker != null) { mistracker.UIScript.PlayerAircraft = EngineControl; }
        sit = true;
    }

    public void globalCall()
    {
        EngineControl.EffectsControl.DoEffects = 0f;
        EngineControl.SoundControl.Wakeup();
    }

    public void EngineStart()
    {
        if (EngineControl.Passenger || sit == false)
        {
            return;
        }

        EngineControl.PilotEnterPlaneLocal();
        EngineControl.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "globalCall");

        if (EngineControl.hbcontroller != null) { Networking.SetOwner(EngineControl.localPlayer, EngineControl.hbcontroller.gameObject); }
        if (EngineControl.localPlayer.IsUserInVR() || useUniversalCanvas)
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

        if (wp != null)
        {
            Networking.SetOwner(EngineControl.localPlayer, wp.gameObject);
            wp.MissilePlaneSystems[wp.selectedSystem].showTargets = true;
            foreach (MissilePlaneSystem ms in wp.MissilePlaneSystems)
            {
                Networking.SetOwner(EngineControl.localPlayer, ms.gameObject);
                Networking.SetOwner(EngineControl.localPlayer, ms.misTarget.gameObject);
            }
        }
        if (OWML != null)
        {
            if (EnableOWMLByDefault)
            {
                OWML.ScriptEnabled = true;
            }


        }
        if (mistracker != null)
        {
            Networking.SetOwner(EngineControl.localPlayer, mistracker.gameObject);
        }
        if (fHud != null)
        {
            fHud.SetActive(true);
        }

        if (EnableObjectsOnStart != null && EnableObjectsOnStart.Length > 0)
        {
            foreach (GameObject x in EnableObjectsOnStart)
            {
                x.SetActive(true);
            }
        }
        globalCall();
    }
    public override void OnStationEntered(VRCPlayerApi player)
    {
        if (player != null)
        {
            // if(EnabledEngine){
            EngineControl.PilotEnterPlaneGlobal(player);
            // }
            //voice range change to allow talking inside cockpit (after VRC patch 1008)
            LeaveButtonControl.SeatedPlayer = player.playerId;
            if (player.isLocal)
            {
                foreach (LeaveVehicleButton crew in EngineControl.LeaveButtons)
                {//get get a fresh VRCPlayerAPI every time to prevent players who left leaving a broken one behind and causing crashes
                    VRCPlayerApi guy = VRCPlayerApi.GetPlayerById(crew.SeatedPlayer);
                    if (guy != null)
                    {
                        SetVoiceInside(guy);
                    }
                }

                if (EnableOWMLByDefault)
                    OWML.ScriptEnabled = true;
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
        LeaveButtonControl.SeatedPlayer = -1;
        if (player != null && player.isLocal)
        {
            EngineControl.PilotExitPlane(player);
            SetVoiceOutside(player);
            if (EngineControl.EffectsControl != null)
            {
                EngineControl.EffectsControl.PlaneAnimator.SetBool("activate_hud", false);
            }
            if (LeaveButton != null) { LeaveButton.SetActive(false); }
            if (Gun_pilot != null) { Gun_pilot.SetActive(false); }
            if (SeatAdjuster != null) { SeatAdjuster.SetActive(false); }
            if (player.isLocal)
            {
                //undo voice distances of all players inside the vehicle
                foreach (LeaveVehicleButton crew in EngineControl.LeaveButtons)
                {
                    VRCPlayerApi guy = VRCPlayerApi.GetPlayerById(crew.SeatedPlayer);
                    if (guy != null)
                    {
                        SetVoiceOutside(guy);
                    }
                }

                // OWML.ScriptEnabled = false;
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
                    mistracker.cleanupOBJMarkers();
                    mistracker.UIScript.PlayerAircraft = null;
                }
                if (fHud != null)
                {
                    fHud.SetActive(false);
                }
                if (ButtonSet != null)
                {
                    ButtonSet.SetActive(false);
                }
                if (OWML != null)
                {
                    if (returnToNorah) OWML.Map.position = Vector3.zero;
                }

                if (EnableObjectsOnStart != null && EnableObjectsOnStart.Length > 0)
                {
                    foreach (GameObject x in EnableObjectsOnStart)
                    {
                        x.SetActive(false);
                    }
                }

            }
        }
        sit = false;
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
            Debug.LogWarning("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}
