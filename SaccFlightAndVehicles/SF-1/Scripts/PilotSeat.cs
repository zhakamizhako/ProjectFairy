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
    private Transform PlaneMesh;
    private LayerMask Planelayer = 0;
    public MissilePlaneSystem MissileControl;
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
    public bool testMode = false;
    private void Start()
    {
        Assert(EngineControl != null, "Start: EngineControl != null");
        Assert(LeaveButton != null, "Start: LeaveButton != null");
        Assert(Gun_pilot != null, "Start: Gun_pilot != null");
        Assert(SeatAdjuster != null, "Start: SeatAdjuster != null");

        PlaneMesh = EngineControl.PlaneMesh.transform;
        //get the layer of the plane as set by the world creator
        // Planelayer = PlaneMesh.gameObject.layer;
        if (ButtonSet != null)
        {
            ButtonSet.SetActive(false);
        }
    }
    private void Interact() //entering the plane
    {
        if (EngineControl.VehicleMainObj != null) { Networking.SetOwner(EngineControl.localPlayer, EngineControl.VehicleMainObj); }
        if (LeaveButton != null) { LeaveButton.SetActive(true); }
        if (ButtonSet != null)
        {
            ButtonSet.SetActive(true);
        }
        if (EngineControl != null)
        {
            EngineControl.VehicleRigidbody.angularDrag = 0; //set to something nonzero when you're not owner to prevent juddering motion on collisions
            if (!EngineControl.InEditor)
            {
                if(!testMode)
                EngineControl.localPlayer.UseAttachedStation();
                Networking.SetOwner(EngineControl.localPlayer, EngineControl.gameObject);
                Networking.SetOwner(EngineControl.localPlayer, EngineControl.VehicleMainObj.gameObject);
                if (EngineControl.hbcontroller != null) { Networking.SetOwner(EngineControl.localPlayer, EngineControl.hbcontroller.gameObject); }
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

            }
            EngineControl.Piloting = true;
            //canopy closed/open sound
            if (EngineControl.EffectsControl.CanopyOpen) EngineControl.CanopyCloseTimer = -100000 - EngineControl.CanopyCloseTime;
            else EngineControl.CanopyCloseTimer = -1; //less than 0
            if (EngineControl.dead) EngineControl.Health = 100; //dead is true for the first 5 seconds after spawn, this might help with spontaneous explosions
        }
        if (EngineControl.EffectsControl != null)
        {
            if (!EngineControl.InEditor) { Networking.SetOwner(EngineControl.localPlayer, EngineControl.EffectsControl.gameObject); }
            EngineControl.IsFiringGun = false;
            EngineControl.EffectsControl.Smoking = false;
        }
        if (EngineControl.HUDControl != null)
        {
            if (!EngineControl.InEditor) { Networking.SetOwner(EngineControl.localPlayer, EngineControl.HUDControl.gameObject); }
            EngineControl.HUDControl.gameObject.SetActive(true);
        }
        if (Gun_pilot != null) { Gun_pilot.SetActive(true); }
        if (SeatAdjuster != null) { SeatAdjuster.SetActive(true); }
        if (wp != null)
        {
            Networking.SetOwner(EngineControl.localPlayer, wp.gameObject);
            wp.MissilePlaneSystems[wp.selectedSystem].showTargets = true;
            foreach (MissilePlaneSystem ms in wp.MissilePlaneSystems)
            {
                Networking.SetOwner(EngineControl.localPlayer, ms.gameObject);
            }
        }
        // if (MissileControl != null) {
        //     MissileControl.showTargets = true;
        //     Networking.SetOwner (EngineControl.localPlayer, MissileControl.gameObject);
        // }
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

        if(EnableObjectsOnStart!=null && EnableObjectsOnStart.Length >0){
            foreach(GameObject x in EnableObjectsOnStart){
                x.SetActive(true);
            }
        }

        //hopefully prevents explosions when you enter the plane
        EngineControl.VehicleRigidbody.velocity = EngineControl.CurrentVel;
        EngineControl.Gs = 0;
        EngineControl.LastFrameVel = EngineControl.CurrentVel;

        //Make sure EngineControl.AAMCurrentTargetEngineControl is correct
    }
    public override void OnStationEntered(VRCPlayerApi player)
    {
        EngineControl.PilotName = player.displayName;
        EngineControl.Pilot = player;

        EngineControl.dead = false; //Plane stops being invincible if someone gets in, also acts as redundancy incase someone missed the notdead respawn event

        //old WakeUp();
        EngineControl.EffectsControl.DoEffects = 0f;
        EngineControl.SoundControl.DoSound = 0f;
        foreach (AudioSource thrust in EngineControl.SoundControl.Thrust)
        {
            thrust.gameObject.SetActive(true);
        }
        foreach (AudioSource idle in EngineControl.SoundControl.PlaneIdle)
        {
            idle.gameObject.SetActive(true);
        }
        if (!EngineControl.SoundControl.PlaneDistantNull) EngineControl.SoundControl.PlaneDistant.gameObject.SetActive(true);
        if (!EngineControl.SoundControl.PlaneWindNull) EngineControl.SoundControl.PlaneWind.gameObject.SetActive(true);
        if (!EngineControl.SoundControl.PlaneInsideNull) EngineControl.SoundControl.PlaneInside.gameObject.SetActive(true);
        if (EngineControl.SoundControl.soundsoff)
        {
            EngineControl.SoundControl.PlaneIdleVolume = 0;
            EngineControl.SoundControl.PlaneDistantVolume = 0;
            EngineControl.SoundControl.PlaneThrustVolume = 0;
        }
        EngineControl.SoundControl.soundsoff = false;
    }
    public override void OnStationExited(VRCPlayerApi player)
    {
        EngineControl.PilotName = string.Empty;
        EngineControl.Pilot = null;

        if (player.isLocal)
        {
            EngineControl.Piloting = false;
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
            }
            // if (MissileControl != null) {
            //     MissileControl.showTargets = false;
            //     MissileControl.leavePlane ();
            // }
            if (EngineControl.Ejected)
            {
                EngineControl.localPlayer.SetVelocity(EngineControl.CurrentVel + EngineControl.VehicleMainObj.transform.up * 25);
                EngineControl.Ejected = false;
            }
            else EngineControl.localPlayer.SetVelocity(EngineControl.CurrentVel);
            EngineControl.EjectTimer = 2;
            EngineControl.Hooked = false;
            EngineControl.BrakeInput = 0;
            EngineControl.LTriggerTapTime = 1;
            EngineControl.RTriggerTapTime = 1;
            EngineControl.Taxiinglerper = 0;
            EngineControl.PlayerThrottle = 0;
            EngineControl.LGripLastFrame = false;
            EngineControl.RGripLastFrame = false;
            EngineControl.LStickSelection = 0;
            EngineControl.RStickSelection = 0;
            EngineControl.BrakeInput = 0;
            EngineControl.LTriggerLastFrame = false;
            EngineControl.RTriggerLastFrame = false;
            // EngineControl.HUDControl.MenuSoundCheckLast = 0;
            EngineControl.EffectsControl.PlaneAnimator.SetInteger("missilesincoming", 0);
            if (EngineControl.CatapultStatus == 1) { EngineControl.CatapultStatus = 0; } //keep launching if launching, otherwise unlock from catapult

            if (LeaveButton != null) { LeaveButton.SetActive(false); }
            if (EngineControl.EffectsControl != null)
            {
                EngineControl.IsFiringGun = false;
                EngineControl.EffectsControl.Smoking = false;
            }
            if (Gun_pilot != null) { Gun_pilot.SetActive(false); }
            if (SeatAdjuster != null) { SeatAdjuster.SetActive(false); }
            if (EngineControl.HUDControl != null) { EngineControl.HUDControl.gameObject.SetActive(false); }
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
                OWML.Map.position = Vector3.zero;
            }
            if (teleportTo != null && EngineControl.dead == true)
            {
                player.TeleportTo(teleportTo.position, teleportTo.rotation);
                player.SetVelocity(Vector3.zero);
            }
            if(EnableObjectsOnStart!=null && EnableObjectsOnStart.Length >0){
            foreach(GameObject x in EnableObjectsOnStart){
                x.SetActive(false);
            }
        }

            //set plane's layer back
            // if (PlaneMesh != null) {
            //     Transform[] children = PlaneMesh.GetComponentsInChildren<Transform> ();
            //     foreach (Transform child in children) {
            //         child.gameObject.layer = Planelayer;
            //     }
            // }
        }
    }
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}