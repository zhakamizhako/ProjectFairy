using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EngineController : UdonSharpBehaviour {
    public GameObject VehicleMainObj;
    public LeaveVehicleButton[] LeaveButtons;
    public EffectsController EffectsControl;
    public SoundController SoundControl;
    public HUDController HUDControl;
    public Transform PlaneMesh;
    public int OnboardPlaneLayer = 19;
    public Transform CenterOfMass;
    public Transform PitchMoment;
    public Transform YawMoment;
    public Transform GroundDetector;
    public Transform HookDetector;
    [UdonSynced (UdonSyncMode.None)] public float Health = 23f;
    public LayerMask ResupplyLayer;
    public LayerMask HookCableLayer;
    public Transform CatapultDetector;
    public LayerMask CatapultLayer;
    [UdonSynced (UdonSyncMode.None)] public float GunAmmoInSeconds = 12;
    public bool HasAfterburner = true;
    public bool HasLimits = true;
    public bool HasFlare = true;
    public bool HasCatapult = true;
    public bool HasBrake = true;
    public bool HasAltHold = true;
    /*     public bool HasTRIM = true; */
    public bool HasCanopy = true;
    public bool HasCruise = true;
    public bool HasGun = true;
    public bool HasGear = true;
    public bool HasFlaps = true;
    public bool HasHook = true;
    public bool HasSmoke = true;
    public float ThrottleStrength = 20f;
    public float AfterburnerThrustMulti = 1.5f;
    public float AccelerationResponse = 4.5f;
    public float EngineSpoolDownSpeedMulti = .5f;
    public float AirFriction = 0.0004f;
    public float PitchStrength = 5f;
    public float PitchThrustVecMulti = 0f;
    public float PitchFriction = 24f;
    public float PitchResponse = 12f;
    public float ReversingPitchStrengthMulti = 2;
    public float YawStrength = 3f;
    public float YawThrustVecMulti = 0f;
    public float YawFriction = 15f;
    public float YawResponse = 12f;
    public float ReversingYawStrengthMulti = 2.4f;
    public float RollStrength = 450f;
    public float RollThrustVecMulti = 0f;
    public float RollFriction = 90f;
    public float RollResponse = 12f;
    public float ReversingRollStrengthMulti = 1.6f; //reversing = AoA > 90
    public float PitchDownStrMulti = .8f;
    public float PitchDownLiftMulti = .8f;
    public float RotMultiMaxSpeed = 220f;
    //public float StickInputPower = 1.7f;
    public float VelStraightenStrPitch = 0.035f;
    public float VelStraightenStrYaw = 0.045f;
    public float MaxAngleOfAttackPitch = 25f;
    public float MaxAngleOfAttackYaw = 40f;
    public float AoaCurveStrength = 2f; //1 = linear, >1 = convex, <1 = concave
    public float HighAoaMinControlPitch = 0.2f;
    public float HighAoaMinControlYaw = 0.2f;
    public float HighPitchAoaMinLift = 0.2f;
    public float HighYawAoaMinLift = 0.2f;
    public float TaxiRotationSpeed = 35f;
    public float TaxiRotationResponse = 2.5f;
    public float Lift = 0.00015f;
    public float SidewaysLift = .17f;
    public float MaxLift = 10f;
    public float VelLift = 1f;
    public float MaxGs = 40f;
    public float GDamage = 10f;
    public float LandingGearDragMulti = 1.3f;
    public float FlapsDragMulti = 1.4f;
    public float FlapsLiftMulti = 1.35f;
    public float AirbrakeStrength = 4f;
    public float GroundBrakeStrength = 6f;
    public float GroundBrakeSpeed = 40f;
    public float HookedBrakeStrength = 65f;
    public float HookedBrakeMaxDistance = 90f;
    public float CatapultLaunchStrength = 50f;
    public float CatapultLaunchTime = 2f;
    public float TakeoffAssist = 5f;
    public float TakeoffAssistSpeed = 50f;
    public float GLimiter = 12f;
    public float AoALimiter = 15f;
    public float CanopyCloseTime = 1.8f;
    public float SeaLevel = -10f;
    public Vector3 Wind;
    public float WindGustStrength = 15;
    public float WindGustiness = 0.03f;
    public float WindTurbulanceScale = 0.0001f;
    public float SoundBarrierStrength = 0.0003f;
    public float SoundBarrierWidth = 20f;

    //best to remove synced variables if you aren't using them
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public float BrakeInput;
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public float Throttle = 0f;
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public Vector3 CurrentVel = Vector3.zero;
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public float Gs = 1f;
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public float AngleOfAttack; //MAX of yaw & pitch aoa //used by effectscontroller
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public Vector3 SmokeColor = Vector3.one;
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public bool IsFiringGun = false;
    [System.NonSerializedAttribute][UdonSynced (UdonSyncMode.None)] public bool Occupied = false; //this is true if someone is sitting in pilot seat
    // [System.NonSerializedAttribute] [UdonSynced(UdonSyncMode.None)] public Vector3 AGMTarget;

    [System.NonSerializedAttribute] public VRCPlayerApi Pilot;
    [System.NonSerializedAttribute] public string PilotName;
    [System.NonSerializedAttribute] public bool FlightLimitsEnabled = true;
    [System.NonSerializedAttribute] public ConstantForce VehicleConstantForce;
    [System.NonSerializedAttribute] public Rigidbody VehicleRigidbody;
    [System.NonSerializedAttribute] public Color SmokeColor_Color;
    private float LerpedRoll;
    private float LerpedPitch;
    private float LerpedYaw;
    [System.NonSerializedAttribute] public int RStickSelection = 0;
    [System.NonSerializedAttribute] public int LStickSelection = 0;
    private Vector2 VRPitchRollInput;
    [System.NonSerializedAttribute] public bool LGripLastFrame = false;
    [System.NonSerializedAttribute] public bool LTriggerLastFrame = false;
    [System.NonSerializedAttribute] public bool RTriggerLastFrame = false;
    Vector3 JoystickPos;
    Vector3 JoystickPosYaw;
    Quaternion PlaneRotDif;
    Quaternion JoystickDifference;
    Quaternion JoystickZeroPoint;
    Quaternion PlaneRotLastFrame;
    private float ThrottleDifference;
    [System.NonSerializedAttribute] public float PlayerThrottle;
    private float TempThrottle;
    private float handpos;
    private float ThrottleZeroPoint;
    private float TempSpeed;
    private float TempZoom;
    private float ZoomZeroPoint;
    private float ZoomDifference;
    [System.NonSerializedAttribute] public float SetSpeed;
    private float SpeedZeroPoint;
    private float SmokeHoldTime;
    private bool SetSmokeLastFrame;
    private Vector3 HandPosSmoke;
    private Vector3 SmokeZeroPoint;
    private float EjectZeroPoint;
    [System.NonSerializedAttribute] public float EjectTimer = 1;
    [System.NonSerializedAttribute] public bool Ejected = false;
    [System.NonSerializedAttribute] public float LTriggerTapTime = 1;
    [System.NonSerializedAttribute] public float RTriggerTapTime = 1;
    /*     private bool DoTrim;
        private Vector3 HandPosTrim;
        private Vector3 TrimZeroPoint;
        private Vector2 TempTrim;
        private Vector2 TrimDifference;
        [System.NonSerializedAttribute] public Vector2 Trim; */
    [System.NonSerializedAttribute] public bool RGripLastFrame = false;
    private float downspeed;
    private float sidespeed;
    [System.NonSerializedAttribute] public float ThrottleInput = 0f;
    private float roll = 0f;
    private float pitch = 0f;
    private float yaw = 0f;
    [System.NonSerializedAttribute] public float FullHealth;
    [System.NonSerializedAttribute] public bool Taxiing = false;
    [System.NonSerializedAttribute] public float RollInput = 0f;
    [System.NonSerializedAttribute] public float PitchInput = 0f;
    [System.NonSerializedAttribute] public float YawInput = 0f;
    [System.NonSerializedAttribute] public bool Piloting = false;
    [System.NonSerializedAttribute] public bool InEditor = true;
    [System.NonSerializedAttribute] public bool InVR = false;
    [System.NonSerializedAttribute] public bool Passenger = false;
    [System.NonSerializedAttribute] public Vector3 LastFrameVel = Vector3.zero;
    [System.NonSerializedAttribute] public VRCPlayerApi localPlayer;
    [System.NonSerializedAttribute] public bool dead = false;
    [System.NonSerializedAttribute] public float AtmoshpereFadeDistance;
    [System.NonSerializedAttribute] public float AtmosphereHeightThing;
    public float AtmosphereThinningStart = 12192f; //40,000 feet
    public float AtmosphereThinningEnd = 19812; //65,000 feet
    private float Atmosphere;
    [System.NonSerializedAttribute] public float rotlift;
    [System.NonSerializedAttribute] public float AngleOfAttackPitch;
    [System.NonSerializedAttribute] public float AngleOfAttackYaw;
    private float AoALiftYaw;
    private float AoALiftPitch;
    private Vector3 Pitching;
    private Vector3 Yawing;
    [System.NonSerializedAttribute] public float Taxiinglerper;
    private float GearDrag;
    private float FlapsGearBrakeDrag;
    private float FlapsDrag;
    private float FlapsLift;
    private float ReversingPitchStrength;
    private float ReversingYawStrength;
    private float ReversingRollStrength;
    private float ReversingPitchStrengthZero;
    private float ReversingYawStrengthZero;
    private float ReversingRollStrengthZero;
    [System.NonSerializedAttribute] public bool Cruise;
    private float CruiseProportional = .1f;
    private float CruiseIntegral = .1f;
    private float CruiseIntegrator;
    private float CruiseIntegratorMax = 5;
    private float CruiseIntegratorMin = -5;
    private float Cruiselastframeerror;
    private float AltHoldPitchProportional = 1f;
    private float AltHoldPitchIntegral = 1f;
    private float AltHoldPitchIntegrator;
    //private float AltHoldPitchIntegratorMax = .1f;
    //private float AltHoldPitchIntegratorMin = -.1f;
    //private float AltHoldPitchDerivative = 4;
    //private float AltHoldPitchDerivator;
    private float AltHoldPitchlastframeerror;
    private float AltHoldRollProportional = -.005f;
    [System.NonSerializedAttribute] public bool AltHold;
    [System.NonSerializedAttribute] public bool Hooked = false;
    [System.NonSerializedAttribute] public float HookedTime = 0f;
    private Vector3 HookedLoc;
    private Vector3 TempSmokeCol = Vector3.zero;
    [System.NonSerializedAttribute] public float Speed;
    [System.NonSerializedAttribute] public float AirSpeed;
    [System.NonSerializedAttribute] public bool IsOwner = false;
    private Vector3 FinalWind; //includes Gusts
    [System.NonSerializedAttribute] public Vector3 AirVel;
    private float StillWindMulti;
    private int ThrustVecGrounded;
    private float SoundBarrier;
    [System.NonSerializedAttribute] private float Afterburner = 1;
    [System.NonSerializedAttribute] public int CatapultStatus = 0;
    private Vector3 CatapultLockPos;
    private Quaternion CatapultLockRot;
    private float CatapultLaunchTimeStart;
    public float StartPitchStrength;
    [System.NonSerializedAttribute] public float CanopyCloseTimer = -100000;
    [UdonSynced (UdonSyncMode.None)] public float Fuel = 7200;
    public float FuelConsumption = 2;
    public float FuelConsumptionABMulti = 4.4f;

    [System.NonSerializedAttribute] public float FullFuel;
    private bool ResupplyingLastFrame = false;
    private float LastResupplyTime = 0;
    [System.NonSerializedAttribute] public float FullGunAmmo;
    private int PilotingInt; //1 if piloting 0 if not
    /* [System.NonSerializedAttribute] */
    private bool WeaponSelected = false;
    private int CatapultDeadTimer = 0; //needed to be invincible for a frame when entering catapult
    // [System.NonSerializedAttribute] public bool AtGCamNull = true;//used by HudController
    private int OutsidePlaneLayer;
    public Vector3 Spawnposition;
    public Vector3 Spawnrotation;
    public HitboxControllerAndEffects hbcontroller;
    private bool isBrakeTriggered = false;
    private bool brakeInputKey = false;
    public MissileTrackerAndResponse mistracker;
    //float MouseX;
    //float MouseY;
    //float mouseysens = 1; //mouse input can't be used because it's used to look around even when in a seat
    //float mousexsens = 1;

    public void FlapToggle () {
        if (EffectsControl != null) {
            if (HasFlaps) {
                EffectsControl.Flaps = !EffectsControl.Flaps;
            }
        }
    }

    public void CanopyToggle () {
        if (HasCanopy) {
            if (CanopyCloseTimer < (-100000 - CanopyCloseTime)) {
                EffectsControl.CanopyOpen = false;
                if (InEditor) CanopyClosing ();
                else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyClosing"); }
            } else if (CanopyCloseTimer < 0 && CanopyCloseTimer > -10000) {
                EffectsControl.CanopyOpen = true;
                if (InEditor) CanopyOpening ();
                else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyOpening"); }
            }
        }
    }

    public void GearToggle () {
        if (EffectsControl != null) {
            if (HasGear) {
                EffectsControl.GearUp = !EffectsControl.GearUp;
            }
        }
    }
    public void LimiterToggle () {
        if (HasLimits) {
            FlightLimitsEnabled = !FlightLimitsEnabled;
        }
    }

    public void CallBrake () {
        if (HasBrake) {
            Debug.Log("BRAKING");
            isBrakeTriggered = true;
        }
    }

    public void ReleaseBrake () {
        if (HasBrake) {
            Debug.Log("RELEASE");
            isBrakeTriggered = false;
        }
    }

    public void CallAfterburner () {
        if (HasAfterburner) {
            EffectsControl.AfterburnerOn = !EffectsControl.AfterburnerOn;
            if (EffectsControl.AfterburnerOn) {
                Afterburner = AfterburnerThrustMulti;
                if (ThrottleInput > 0.6) {
                    if (InEditor) {
                        PlayABOnSound ();
                    } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayABOnSound");
                }
            } else { Afterburner = 1; }
        }
    }

    private void Start () {
        Assert (VehicleMainObj != null, "Start: VehicleMainObj != null");
        Assert (LeaveButtons.Length > 0, "Start: Leavebutton Set");
        Assert (EffectsControl != null, "Start: EffectsControl != null");
        Assert (SoundControl != null, "Start: SoundControl != null");
        Assert (HUDControl != null, "Start: HUDControl != null");
        Assert (PlaneMesh != null, "Start: PlaneMesh != null");
        Assert (CenterOfMass != null, "Start: CenterOfMass != null");
        Assert (PitchMoment != null, "Start: PitchMoment != null");
        Assert (YawMoment != null, "Start: YawMoment != null");
        Assert (GroundDetector != null, "Start: GroundDetector != null");
        Assert (HookDetector != null, "Start: HookDetector != null");
        // Assert(AtGCam != null, "Start: AGMCam != null");
        Assert (CatapultDetector != null, "Start: CatapultDetector != null");
        OutsidePlaneLayer = PlaneMesh.gameObject.layer;

        // if (AtGCam != null) AtGCamNull = false;

        //these two are only used in editor
        VehicleMainObj.transform.localPosition = Vector3.zero;
        VehicleMainObj.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Spawnposition = VehicleMainObj.transform.position;
        Spawnrotation = VehicleMainObj.transform.rotation.eulerAngles;

        FullHealth = Health;
        FullFuel = Fuel;
        FullGunAmmo = GunAmmoInSeconds;
        CatapultLaunchTimeStart = CatapultLaunchTime;

        StartPitchStrength = PitchStrength; //used for takeoff assist
        if (AtmosphereThinningStart > AtmosphereThinningEnd) { AtmosphereThinningEnd = AtmosphereThinningStart; }
        VehicleRigidbody = VehicleMainObj.GetComponent<Rigidbody> ();
        VehicleConstantForce = VehicleMainObj.GetComponent<ConstantForce> ();
        localPlayer = Networking.LocalPlayer;
        if (localPlayer == null) { InEditor = true; Piloting = true; } else {
            InEditor = false;
            if (localPlayer.IsUserInVR ()) { InVR = true; }
        }
        float scaleratio = CenterOfMass.transform.lossyScale.magnitude / Vector3.one.magnitude;
        VehicleRigidbody.centerOfMass = CenterOfMass.localPosition * scaleratio; //correct position if scaled

        AtmoshpereFadeDistance = (AtmosphereThinningEnd + SeaLevel) - (AtmosphereThinningStart + SeaLevel); //for finding atmosphere thinning gradient
        AtmosphereHeightThing = (AtmosphereThinningStart + SeaLevel) / (AtmoshpereFadeDistance); //used to add back the height to the atmosphere after finding gradient

        //used to set each rotation axis' reversing behaviour to inverted if 0 thrust vectoring, and not inverted if thrust vectoring is non-zero.
        //the variables are called 'Zero' because they ask if thrustvec is set to 0.

        ReversingPitchStrengthZero = PitchThrustVecMulti == 0 ? -ReversingPitchStrengthMulti : 1;
        ReversingYawStrengthZero = YawThrustVecMulti == 0 ? -ReversingYawStrengthMulti : 1;
        ReversingRollStrengthZero = RollThrustVecMulti == 0 ? -ReversingRollStrengthMulti : 1;

        if (!HasCanopy) {
            EffectsControl.CanopyOpen = false;
            if (InEditor) CanopyClosing ();
        }
    }

    private void LateUpdate () {
        float DeltaTime = Time.deltaTime;
        if (!InEditor) { IsOwner = localPlayer.IsOwner (VehicleMainObj); }
        if (!EffectsControl.GearUp && Physics.Raycast (GroundDetector.position, GroundDetector.TransformDirection (Vector3.down), .44f, 2049 /* Default and Environment */ )) { Taxiing = true; } else { Taxiing = false; }

        if (IsOwner || InEditor) //works in editor or ingame
        {
            if (!dead) {
                if (CenterOfMass.position.y < SeaLevel) //kill plane if in sea
                {
                    if (InEditor) //editor
                        Explode ();
                    else //VRC
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Explode");
                }
                //G/crash Damage
                Health += -Mathf.Clamp ((Gs - MaxGs) * DeltaTime * GDamage, 0f, 99999f); //take damage of GDamage per second per G above MaxGs
                if (Health <= 0f) //plane is ded
                {
                    if (InEditor) //editor
                        Explode ();
                    else //VRC
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Explode");
                }
            }
            if (!Piloting) { Occupied = false; } //should make vehicle respawnable if player disconnects while occupying
            Atmosphere = Mathf.Clamp (-(CenterOfMass.position.y / AtmoshpereFadeDistance) + 1 + AtmosphereHeightThing, 0, 1);
            CurrentVel = VehicleRigidbody.velocity; //because rigidbody values aren't accessable by non-owner players
            Speed = CurrentVel.magnitude;
            float gustx = (Time.time * WindGustiness) + (VehicleMainObj.transform.position.x * WindTurbulanceScale);
            float gustz = (Time.time * WindGustiness) + (VehicleMainObj.transform.position.z * WindTurbulanceScale);
            FinalWind = Vector3.Normalize (new Vector3 ((Mathf.PerlinNoise (gustx + 9000, gustz) - .5f), /* (Mathf.PerlinNoise(gustx - 9000, gustz - 9000) - .5f) */ 0, (Mathf.PerlinNoise (gustx, gustz + 9999) - .5f))) * WindGustStrength;
            FinalWind = (FinalWind + Wind) * Atmosphere;
            AirVel = VehicleRigidbody.velocity - FinalWind;
            AirSpeed = AirVel.magnitude;
            AngleOfAttackPitch = Vector3.SignedAngle (VehicleMainObj.transform.forward, AirVel, VehicleMainObj.transform.right);
            AngleOfAttackYaw = Vector3.SignedAngle (VehicleMainObj.transform.forward, AirVel, VehicleMainObj.transform.up);

            //angle of attack stuff, pitch and yaw are calculated seperately
            //pitch and yaw each have a curve for when they are within the 'MaxAngleOfAttack' and a linear version up to 90 degrees, which are Max'd (using Mathf.Clamp) for the final result.
            //the linear version is used for high aoa, and is 0 when at 90 degrees, 1 at 0(multiplied by HighAoaMinControlx). When at more than 90 degrees, the control comes back with the same curve but the inputs are inverted. (unless thrust vectoring is enabled) The invert code is elsewhere.
            AoALiftPitch = Mathf.Min (Mathf.Abs (AngleOfAttackPitch) / MaxAngleOfAttackPitch, Mathf.Abs (Mathf.Abs (AngleOfAttackPitch) - 180) / MaxAngleOfAttackPitch); //angle of attack as 0-1 float, for backwards and forwards
            AoALiftPitch = -AoALiftPitch + 1;
            AoALiftPitch = -Mathf.Pow ((1 - AoALiftPitch), AoaCurveStrength) + 1; //give it a curve

            float AoALiftPitchMin = Mathf.Min (Mathf.Abs (AngleOfAttackPitch) / 90, Mathf.Abs (Mathf.Abs (AngleOfAttackPitch) - 180) / 90); //linear version to 90 for high aoa
            AoALiftPitchMin = Mathf.Clamp ((-AoALiftPitchMin + 1) * HighAoaMinControlPitch, 0, 1);
            AoALiftPitch = Mathf.Clamp (AoALiftPitch, AoALiftPitchMin, 1);

            AoALiftYaw = Mathf.Min (Mathf.Abs (AngleOfAttackYaw) / MaxAngleOfAttackYaw, Mathf.Abs ((Mathf.Abs (AngleOfAttackYaw) - 180)) / MaxAngleOfAttackYaw);
            AoALiftYaw = -AoALiftYaw + 1;
            AoALiftYaw = -Mathf.Pow ((1 - AoALiftYaw), AoaCurveStrength) + 1; //give it a curve

            float AoALiftYawMin = Mathf.Min (Mathf.Abs (AngleOfAttackYaw) / 90, Mathf.Abs (Mathf.Abs (AngleOfAttackYaw) - 180) / 90); //linear version to 90 for high aoa
            AoALiftYawMin = Mathf.Clamp ((-AoALiftPitchMin + 1) * HighAoaMinControlYaw, 0, 1);
            AoALiftYaw = Mathf.Clamp (AoALiftYaw, AoALiftYawMin, 1);

            AngleOfAttack = Mathf.Max (AngleOfAttackPitch, AngleOfAttackYaw);

            //used to create air resistance for updown and sideways if your movement direction is in those directions
            //to add physics to plane's yaw and pitch, accel angvel towards velocity, and add force to the plane
            //and add wind
            sidespeed = Vector3.Dot (AirVel, VehicleMainObj.transform.right);
            downspeed = Vector3.Dot (AirVel, VehicleMainObj.transform.up) * -1;

            if (downspeed < 0) //air is hitting plane from above
            {
                downspeed *= PitchDownLiftMulti;
            }

            //speed related values
            float SpeedLiftFactor = Mathf.Clamp (AirSpeed * AirSpeed * Lift, 0, MaxLift);
            rotlift = Mathf.Min (AirSpeed / RotMultiMaxSpeed, 1); //using a simple linear curve for increasing control as you move faster

            if (Piloting) {
                PilotingInt = 1;
                Occupied = true;
                //collect inputs
                int Wf = Input.GetKey (KeyCode.W) ? 1 : 0; //inputs as floats
                int Sf = Input.GetKey (KeyCode.S) ? -1 : 0;
                int Af = Input.GetKey (KeyCode.A) ? -1 : 0;
                int Df = Input.GetKey (KeyCode.D) ? 1 : 0;
                int Qf = Input.GetKey (KeyCode.Q) ? -1 : 0;
                int Ef = Input.GetKey (KeyCode.E) ? 1 : 0;
                int upf = Input.GetKey (KeyCode.UpArrow) ? 1 : 0;
                int downf = Input.GetKey (KeyCode.DownArrow) ? -1 : 0;
                int leftf = Input.GetKey (KeyCode.LeftArrow) ? -1 : 0;
                int rightf = Input.GetKey (KeyCode.RightArrow) ? 1 : 0;
                bool Shift = Input.GetKey (KeyCode.LeftShift);
                bool Ctrl = Input.GetKey (KeyCode.LeftControl);
                int Shiftf = Shift ? 1 : 0;
                int LeftControlf = Ctrl ? 1 : 0;
                Vector2 LStick;
                Vector2 RStick;
                LStick.x = Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
                LStick.y = Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryThumbstickVertical");
                RStick.x = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryThumbstickHorizontal");
                RStick.y = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryThumbstickVertical");
                float LGrip = Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryHandTrigger");
                float RGrip = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryHandTrigger");
                float LTrigger = LTrigger = Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryIndexTrigger");
                float RTrigger = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger");
                //MouseX = Input.GetAxisRaw("Mouse X");
                //MouseY = Input.GetAxisRaw("Mouse Y");

                //close canopy when moving fast, can't fly with it open
                if (EffectsControl.CanopyOpen && Speed > 20) {
                    if (CanopyCloseTimer < -100000 + CanopyCloseTime) {
                        EffectsControl.CanopyOpen = false;
                        if (InEditor) CanopyClosing ();
                        else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyClosing"); }
                    }
                }

                ///////////////////KEYBOARD CONTROLS////////////////////////////////////////////////////////          
                if (EffectsControl.Smoking) {
                    int keypad7 = Input.GetKey (KeyCode.Keypad7) ? 1 : 0;
                    int Keypad4 = Input.GetKey (KeyCode.Keypad4) ? 1 : 0;
                    int Keypad8 = Input.GetKey (KeyCode.Keypad8) ? 1 : 0;
                    int Keypad5 = Input.GetKey (KeyCode.Keypad5) ? 1 : 0;
                    int Keypad9 = Input.GetKey (KeyCode.Keypad9) ? 1 : 0;
                    int Keypad6 = Input.GetKey (KeyCode.Keypad6) ? 1 : 0;
                    SmokeColor.x = Mathf.Clamp (SmokeColor.x + ((keypad7 - Keypad4) * DeltaTime), 0, 1);
                    SmokeColor.y = Mathf.Clamp (SmokeColor.y + ((Keypad8 - Keypad5) * DeltaTime), 0, 1);
                    SmokeColor.z = Mathf.Clamp (SmokeColor.z + ((Keypad9 - Keypad6) * DeltaTime), 0, 1);
                }
                if (Input.GetKeyDown (KeyCode.F2) && HasCruise) {
                    SetSpeed = AirSpeed;
                    Cruise = !Cruise;
                }
                if (Input.GetKeyDown (KeyCode.F1) && HasLimits) {
                    FlightLimitsEnabled = !FlightLimitsEnabled;
                }
                // if (Input.GetKeyDown (KeyCode.C) && HasCatapult) {
                //     if (CatapultStatus == 1) {
                //         CatapultStatus = 2;
                //         if (InEditor) {
                //             CatapultLaunchEffects ();
                //         } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CatapultLaunchEffects");
                //     }
                // }
                if (Input.GetKeyDown (KeyCode.H) && HasHook) {
                    if (HookDetector != null) {
                        EffectsControl.HookDown = !EffectsControl.HookDown;
                    }
                    Hooked = false;
                }
                if (Input.GetKeyDown (KeyCode.F3) && HasAltHold) {
                    AltHold = !AltHold;
                }
                if (Speed < 20 && Input.GetKey (KeyCode.Z) && HasCanopy) {
                    if (CanopyCloseTimer < (-100000 - CanopyCloseTime)) {
                        EffectsControl.CanopyOpen = false;
                        if (InEditor) CanopyClosing ();
                        else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyClosing"); }
                    } else if (CanopyCloseTimer < 0 && CanopyCloseTimer > -10000) {
                        EffectsControl.CanopyOpen = true;
                        if (InEditor) CanopyOpening ();
                        else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyOpening"); }
                    }
                }

                if (Input.GetKeyDown (KeyCode.T) && HasAfterburner) {
                    EffectsControl.AfterburnerOn = !EffectsControl.AfterburnerOn;
                    if (EffectsControl.AfterburnerOn) {
                        Afterburner = AfterburnerThrustMulti;
                        if (ThrottleInput > 0.6) {
                            if (InEditor) {
                                PlayABOnSound ();
                            } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayABOnSound");
                        }
                    } else { Afterburner = 1; }
                }

                //with keys 1-4 we select weapons, if they are already selectet, deselect them.
                if (Input.GetKeyDown (KeyCode.Alpha1) && HasGun) {
                    if (RStickSelection == 1) {
                        if (InEditor) {
                            RStick0 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                        RStickSelection = 0;
                    } else {
                        if (HUDControl != null) { HUDControl.GUN_TargetSpeedLerper = 0; } //reset targeting lerper
                        if (InEditor) {
                            RStick1 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick1");
                        RStickSelection = 1;
                    }
                }

                if (Input.GetKeyDown (KeyCode.Alpha2)) {
                    if (RStickSelection == 2) {
                        if (InEditor) {
                            RStick0 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                        RStickSelection = 0;
                    } else {
                        if (InEditor) {
                            RStick2 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick2");
                        RStickSelection = 2;
                    }
                }

                if (Input.GetKeyDown (KeyCode.Alpha3)) {
                    if (RStickSelection == 3) {
                        if (InEditor) {
                            RStick0 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                        RStickSelection = 0;
                    } else {
                        if (InEditor) {
                            RStick3 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick3");
                        RStickSelection = 3;
                    }
                }

                if (Input.GetKeyDown (KeyCode.Alpha4)) {
                    if (RStickSelection == 4) {
                        if (InEditor) {
                            RStick0 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                        RStickSelection = 0;
                    } else {
                        if (InEditor) {
                            RStick4 ();
                        } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick4");
                        RStickSelection = 4;
                    }
                }

                if (Input.GetKeyDown (KeyCode.G) && HasGear && CatapultStatus == 0) {
                    EffectsControl.GearUp = !EffectsControl.GearUp;
                }
                if (Input.GetKeyDown (KeyCode.F) && HasFlaps) {
                    EffectsControl.Flaps = !EffectsControl.Flaps;
                }
                if (Input.GetKeyDown (KeyCode.Alpha5) && HasSmoke) {
                    EffectsControl.Smoking = !EffectsControl.Smoking;
                }
                if (Input.GetKeyDown (KeyCode.X) && HasFlare) {
                    if (InEditor) { LaunchFlares (); } //editor
                    else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LaunchFlares"); } //ingame
                }
                //////////////////END OF KEYBOARD CONTROLS////////////////////////////////////////////////////////
                //brake is done later because it has to be after switches

                //LStick Selection wheel
                // if (LStick.magnitude >.7f && InVR) {
                //     float stickdir = Vector2.SignedAngle (new Vector2 (-0.382683432365f, 0.923879532511f), LStick);

                //     if (stickdir > 135) //down
                //     {
                //         if (HasBrake)
                //             LStickSelection = 5;
                //     } else if (stickdir > 90) //downleft
                //     {
                //         if (HasAltHold)
                //             LStickSelection = 6;
                //     } else if (stickdir > 45) //left
                //     {
                //         if (HasCanopy)
                //             LStickSelection = 7;
                //     } else if (stickdir > 0) //upleft
                //     {
                //         if (HasCruise)
                //             LStickSelection = 8;
                //     } else if (stickdir > -45) //up
                //     {
                //         if (HasAfterburner)
                //             LStickSelection = 1;
                //     } else if (stickdir > -90) //upright
                //     {
                //         if (HasLimits)
                //             LStickSelection = 2;
                //     } else if (stickdir > -135) //right
                //     {
                //         if (HasFlare)
                //             LStickSelection = 3;
                //     } else //downright
                //     {
                //         if (HasCatapult)
                //             LStickSelection = 4;
                //     }
                // }

                // //RStick Selection wheel
                // if (RStick.magnitude >.7f && InVR) {
                //     float stickdir = Vector2.SignedAngle (new Vector2 (-0.382683432365f, 0.923879532511f), RStick); //that number is 22.5 degrees to the left of straight up
                //     //R stick value is manually synced using events because i don't want to use too many synced variables.
                //     //the value can be used in the animator to open bomb bay doors when bombs are selected, etc.
                //     //The WeaponSelected variable helps us not send more broadcasts than we need to.
                //     if (stickdir > 135) //down
                //     {
                //         if (HasGear) {
                //             if (WeaponSelected) {
                //                 if (InEditor) {
                //                     WeaponSelected = false;
                //                     RStick0 ();
                //                 } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                //             }
                //             RStickSelection = 5;
                //         }
                //     } else if (stickdir > 90) //downleft
                //     {
                //         if (HasFlaps) {
                //             if (WeaponSelected) {
                //                 WeaponSelected = false;
                //                 if (InEditor) {
                //                     RStick0 ();
                //                 } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                //             }
                //             RStickSelection = 6;
                //         }
                //     } else if (stickdir > 45) //left
                //     {
                //         if (HasHook) {
                //             if (WeaponSelected) {
                //                 WeaponSelected = false;
                //                 if (InEditor) {
                //                     RStick0 ();
                //                 } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                //             }
                //             RStickSelection = 7;
                //         }
                //     } else if (stickdir > 0) //upleft
                //     {
                //         if (HasSmoke) {
                //             if (WeaponSelected) {
                //                 WeaponSelected = false;
                //                 if (InEditor) {
                //                     RStick0 ();
                //                 } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick0");
                //             }
                //             RStickSelection = 8;
                //         }
                //     } else if (stickdir > -45) //up
                //     {
                //         if (HasGun && RStickSelection != 1) {
                //             if (HUDControl != null) { HUDControl.GUN_TargetSpeedLerper = 0; } //reset targeting lerper
                //             WeaponSelected = true;
                //             if (InEditor) {
                //                 RStick1 ();
                //             } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick1");
                //             RStickSelection = 1;
                //         }
                //     } else if (stickdir > -90) //upright
                //     {
                //         if (RStickSelection != 2) {
                //             WeaponSelected = true;
                //             if (InEditor) {
                //                 RStick2 ();
                //             } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick2");
                //             RStickSelection = 2;
                //         }
                //     } else if (stickdir > -135) //right
                //     {
                //         if (RStickSelection != 3) {

                //             WeaponSelected = true;
                //             if (InEditor) {
                //                 RStick3 ();
                //             } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick3");
                //             RStickSelection = 3;
                //         }
                //     } else //downright
                //     {
                //         if (RStickSelection != 4) {
                //             WeaponSelected = true;
                //             if (InEditor) {
                //                 RStick4 ();
                //             } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RStick4");
                //             RStickSelection = 4;
                //         }
                //     }
                // }

                // LTriggerTapTime += DeltaTime;
                // switch (LStickSelection) {
                //     case 0: //player just got in and hasn't selected anything
                //         BrakeInput = 0;
                //         break;
                //     case 1: //Cruise
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame) {
                //                 EffectsControl.AfterburnerOn = !EffectsControl.AfterburnerOn;
                //                 if (EffectsControl.AfterburnerOn) {
                //                     Afterburner = AfterburnerThrustMulti;
                //                     if (ThrottleInput > 0.6) {
                //                         if (InEditor) {
                //                             PlayABOnSound ();
                //                         } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayABOnSound");
                //                     }
                //                 } else { Afterburner = 1; }
                //             }
                //             LTriggerLastFrame = true;
                //         } else { LTriggerLastFrame = false; }
                //         BrakeInput = 0;
                //         break;
                //     case 2: //LIMIT
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame) {
                //                 FlightLimitsEnabled = !FlightLimitsEnabled;
                //             }

                //             LTriggerLastFrame = true;
                //         } else { LTriggerLastFrame = false; }
                //         BrakeInput = 0;
                //         break;
                //     case 3: //Flare
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame) {
                //                 if (InEditor) { LaunchFlares (); } //editor
                //                 else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LaunchFlares"); } //ingame
                //             }

                //             AltHold = false;
                //             IsFiringGun = false;
                //             RTriggerLastFrame = true;
                //         } else { LTriggerLastFrame = false; }

                //         BrakeInput = 0;
                //         break;
                //     case 4: //Catapult
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame) {
                //                 if (CatapultStatus == 1) {
                //                     CatapultStatus = 2;
                //                     if (InEditor) {
                //                         CatapultLaunchEffects ();
                //                     } else SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CatapultLaunchEffects");
                //                 }
                //             }

                //             LTriggerLastFrame = true;
                //         } else { LTriggerLastFrame = false; }

                //         BrakeInput = 0;
                //         break;
                //     case 5: //Brake
                //         BrakeInput = LTrigger;

                //         if (LTrigger > 0.75) { LTriggerLastFrame = true; } else { LTriggerLastFrame = false; }
                //         break;
                //     case 6: //Alt. Hold
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame) AltHold = !AltHold;
                //             LTriggerLastFrame = true;
                //         } else { LTriggerLastFrame = false; }
                //         //this used to be TRIM
                //         /*                             if (LTrigger > 0.75 || (Input.GetKey(KeyCode.Alpha4)))
                //                                     {
                //                                         if (!LTriggerLastFrame)
                //                                         {
                //                                             if (InVR)
                //                                             {
                //                                                 HandPosTrim = VehicleMainObj.transform.InverseTransformPoint(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position);
                //                                                 TrimZeroPoint = HandPosTrim;
                //                                                 TempTrim = new Vector2(Trim.y, Trim.x);//it's inverted because i want X to be pitch and y to be yaw
                //                                             }
                //                                             if (LTriggerTapTime > .4f)//no double tap
                //                                             {
                //                                                 LTriggerTapTime = 0;
                //                                                 DoTrim = true;
                //                                             }
                //                                             else//double tap detected, reset trim
                //                                             {
                //                                                 DoTrim = false;
                //                                                 Trim = new Vector2(0, 0);
                //                                             }
                //                                         }
                //                                         if (InVR && DoTrim)
                //                                         {
                //                                             //VR Set Trim
                //                                             HandPosTrim = VehicleMainObj.transform.InverseTransformPoint(localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position);
                //                                             TrimDifference = (TrimZeroPoint - HandPosTrim) * 2f;
                //                                             Trim.x = Mathf.Clamp(TempTrim.y + TrimDifference.y, -1, 1);
                //                                             Trim.y = Mathf.Clamp(TempTrim.x + -TrimDifference.x, -1, 1);
                //                                         }
                //                                         LTriggerLastFrame = true;
                //                                     }
                //                                     else { LTriggerLastFrame = false; } */
                //         BrakeInput = 0;
                //         break;
                //     case 7: //Canopy
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame && Speed < 20) {
                //                 if (CanopyCloseTimer < (-100000 - CanopyCloseTime)) {
                //                     EffectsControl.CanopyOpen = false;
                //                     if (InEditor) CanopyClosing ();
                //                     else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyClosing"); }
                //                 } else if (CanopyCloseTimer < 0 && CanopyCloseTimer > -10000) {
                //                     EffectsControl.CanopyOpen = true;
                //                     if (InEditor) CanopyOpening ();
                //                     else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyOpening"); }
                //                 }
                //             }

                //             //ejection
                //             if (InVR) {
                //                 float handposL = VehicleMainObj.transform.InverseTransformPoint (localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.LeftHand).position).y;
                //                 float handposR = VehicleMainObj.transform.InverseTransformPoint (localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.RightHand).position).y;
                //                 if (!LTriggerLastFrame && (handposL - handposR) < 0.15f) {
                //                     EjectZeroPoint = handposL;
                //                     EjectTimer = 0;
                //                 }
                //                 if (handposL - EjectZeroPoint >.5f && EjectTimer < 1) {
                //                     Ejected = true;
                //                     foreach (LeaveVehicleButton seat in LeaveButtons) {
                //                         if (seat != null) seat.ExitStation ();
                //                     }
                //                     if (HasCanopy) {
                //                         EffectsControl.CanopyOpen = true;
                //                         SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CanopyOpening");
                //                     }
                //                 }
                //             }

                //             EjectTimer += DeltaTime;
                //             LTriggerLastFrame = true;
                //         } else {
                //             LTriggerLastFrame = false;
                //             EjectTimer = 2;
                //         }
                //         BrakeInput = 0;
                //         break;
                //     case 8: //Afterburner
                //         if (LTrigger > 0.75) {
                //             if (!LTriggerLastFrame) {
                //                 if (!Cruise) {
                //                     SetSpeed = AirSpeed;
                //                     Cruise = true;
                //                 }
                //                 if (LTriggerTapTime >.4f) //no double tap
                //                 {
                //                     LTriggerTapTime = 0;
                //                 } else //double tap detected, turn off cruise
                //                 {
                //                     Cruise = false;
                //                     PlayerThrottle = ThrottleInput;
                //                 }
                //             }

                //             //VR Set Speed
                //             if (InVR) {

                //                 handpos = VehicleMainObj.transform.InverseTransformPoint (localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.LeftHand).position).z;
                //                 if (!LTriggerLastFrame) {
                //                     SpeedZeroPoint = handpos;
                //                     TempSpeed = SetSpeed;
                //                 }
                //                 float SpeedDifference = (SpeedZeroPoint - handpos) * -600;
                //                 SetSpeed = Mathf.Floor (Mathf.Clamp (TempSpeed + SpeedDifference, 0, 2000));

                //             }
                //             LTriggerLastFrame = true;
                //         } else { LTriggerLastFrame = false; }
                //         BrakeInput = 0;
                //         break;
                // }

                // RTriggerTapTime += DeltaTime;
                // switch (RStickSelection) {
                //     case 0: //player just got in and hasn't selected anything
                //         break;
                //     case 1: //GUN
                //         if ((RTrigger > 0.75 || (Input.GetKey (KeyCode.Space))) && GunAmmoInSeconds > 0) {
                //             IsFiringGun = true;
                //             GunAmmoInSeconds = Mathf.Max (GunAmmoInSeconds - DeltaTime, 0);
                //             RTriggerLastFrame = true;
                //         } else { IsFiringGun = false; RTriggerLastFrame = false; }

                //         break;
                //     case 2: //AAM

                //     case 3: //AGM

                //     case 4: //Bomb

                //     case 5: //GEAR
                //         if (RTrigger > 0.75) {
                //             if (!RTriggerLastFrame && CatapultStatus == 0) { EffectsControl.GearUp = !EffectsControl.GearUp; }
                //             RTriggerLastFrame = true;
                //         } else { RTriggerLastFrame = false; }

                //         IsFiringGun = false;
                //         break;
                //     case 6: //flaps
                //         if (RTrigger > 0.75) {
                //             if (!RTriggerLastFrame) EffectsControl.Flaps = !EffectsControl.Flaps;

                //             IsFiringGun = false;
                //             RTriggerLastFrame = true;
                //         } else { RTriggerLastFrame = false; }
                //         IsFiringGun = false;
                //         break;
                //     case 7: //Hook
                //         if (RTrigger > 0.75) {
                //             if (!RTriggerLastFrame) {
                //                 if (HookDetector != null) {
                //                     EffectsControl.HookDown = !EffectsControl.HookDown;
                //                 }
                //                 Hooked = false;
                //             }

                //             RTriggerLastFrame = true;
                //         } else { RTriggerLastFrame = false; }
                //         IsFiringGun = false;
                //         break;
                //     case 8: //Smoke
                //         if (RTrigger > 0.75) {
                //             //you can change smoke colour by holding down the trigger and waving your hand around. x/y/z = r/g/b
                //             if (!RTriggerLastFrame) {
                //                 if (InVR) {
                //                     HandPosSmoke = VehicleMainObj.transform.InverseTransformPoint (localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.RightHand).position);
                //                     SmokeZeroPoint = HandPosSmoke;
                //                     TempSmokeCol = SmokeColor;
                //                 }
                //                 EffectsControl.Smoking = !EffectsControl.Smoking;
                //                 SmokeHoldTime = 0;
                //             }
                //             if (InVR) {
                //                 SmokeHoldTime += DeltaTime;
                //                 if (SmokeHoldTime >.4f) {

                //                     //VR Set Smoke
                //                     HandPosSmoke = VehicleMainObj.transform.InverseTransformPoint (localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.RightHand).position);

                //                     Vector3 SmokeDifference = (SmokeZeroPoint - HandPosSmoke) * 8f;
                //                     SmokeColor.x = Mathf.Clamp (TempSmokeCol.x + SmokeDifference.x, 0, 1);
                //                     SmokeColor.y = Mathf.Clamp (TempSmokeCol.y + SmokeDifference.y, 0, 1);
                //                     SmokeColor.z = Mathf.Clamp (TempSmokeCol.z + SmokeDifference.z, 0, 1);
                //                 }
                //             }

                //             IsFiringGun = false;
                //             RTriggerLastFrame = true;
                //         } else { RTriggerLastFrame = false; }

                //         IsFiringGun = false;
                //         break;
                // }
                if (Input.GetKey (KeyCode.B)) {
                    brakeInputKey = true;
                }
                if (HasBrake) {
                    if (!Input.GetKey (KeyCode.B)) {
                        brakeInputKey = false;
                    }
                    if (Input.GetKey (KeyCode.B) || isBrakeTriggered) {
                        BrakeInput = 1;
                    }
                    if (!brakeInputKey && isBrakeTriggered) {
                        BrakeInput = 1;
                    }
                    if (brakeInputKey && !isBrakeTriggered) {
                        BrakeInput = 1;
                    }
                    if (!brakeInputKey && !isBrakeTriggered) {
                        BrakeInput = 0;
                    }
                }

                //VR Joystick
                if (RGrip > 0.75) {
                    PlaneRotDif = VehicleMainObj.transform.rotation * Quaternion.Inverse (PlaneRotLastFrame); //difference in plane's rotation since last frame
                    JoystickZeroPoint = PlaneRotDif * JoystickZeroPoint; //zero point rotates with the plane so it appears still to the pilot
                    if (!RGripLastFrame) //first frame you gripped joystick
                    {
                        PlaneRotDif = Quaternion.identity;
                        JoystickZeroPoint = localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.RightHand).rotation; //rotation of the controller relative to the plane when it was pressed
                    }
                    //difference between the plane and the hand's rotation, and then the difference between that and the JoystickZeroPoint
                    JoystickDifference = (Quaternion.Inverse (VehicleMainObj.transform.rotation) * localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.RightHand).rotation) * Quaternion.Inverse (JoystickZeroPoint);
                    JoystickPosYaw = (JoystickDifference * VehicleMainObj.transform.forward); //angles to vector
                    JoystickPosYaw.y = 0;
                    JoystickPos = (JoystickDifference * VehicleMainObj.transform.up);
                    VRPitchRollInput = new Vector2 (JoystickPos.x, JoystickPos.z) * 1.41421f;

                    RGripLastFrame = true;
                    //making a circular joy stick square
                    //pitch and roll
                    if (Mathf.Abs (VRPitchRollInput.x) > Mathf.Abs (VRPitchRollInput.y)) {
                        if (Mathf.Abs (VRPitchRollInput.x) > 0) {
                            float temp = VRPitchRollInput.magnitude / Mathf.Abs (VRPitchRollInput.x);
                            VRPitchRollInput *= temp;
                        }
                    } else if (Mathf.Abs (VRPitchRollInput.y) > 0) {
                        float temp = VRPitchRollInput.magnitude / Mathf.Abs (VRPitchRollInput.y);
                        VRPitchRollInput *= temp;
                    }
                    //yaw
                    if (Mathf.Abs (JoystickPosYaw.x) > Mathf.Abs (JoystickPosYaw.z)) {
                        if (Mathf.Abs (JoystickPosYaw.x) > 0) {
                            float temp = JoystickPosYaw.magnitude / Mathf.Abs (JoystickPosYaw.x);
                            JoystickPosYaw *= temp;
                        }
                    } else if (Mathf.Abs (JoystickPosYaw.z) > 0) {
                        float temp = JoystickPosYaw.magnitude / Mathf.Abs (JoystickPosYaw.z);
                        JoystickPosYaw *= temp;
                    }

                } else {
                    JoystickPosYaw.x = 0;
                    VRPitchRollInput = Vector3.zero;
                    RGripLastFrame = false;
                }
                PlaneRotLastFrame = VehicleMainObj.transform.rotation;

                //VR Throttle
                if (LGrip > 0.75) {
                    handpos = VehicleMainObj.transform.InverseTransformPoint (localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.LeftHand).position).z;
                    if (!LGripLastFrame) {
                        ThrottleZeroPoint = handpos;
                        TempThrottle = PlayerThrottle;
                    }
                    ThrottleDifference = ThrottleZeroPoint - handpos;
                    ThrottleDifference *= -14;

                    PlayerThrottle = Mathf.Clamp (TempThrottle + ThrottleDifference, 0, 1);
                    LGripLastFrame = true;
                } else {
                    LGripLastFrame = false;
                }

                PlayerThrottle = Mathf.Clamp (PlayerThrottle + ((Shiftf - LeftControlf) * .5f * DeltaTime), 0, 1);

                if (Taxiing) {
                    AngleOfAttack = 0; // prevent stall sound and aoavapor when on ground
                    Cruise = false;
                    AltHold = false;
                    //rotate if trying to turn
                    Taxiinglerper = Mathf.Lerp (Taxiinglerper, YawInput * TaxiRotationSpeed * DeltaTime, TaxiRotationResponse * DeltaTime);
                    VehicleMainObj.transform.Rotate (Vector3.up, Taxiinglerper);

                    StillWindMulti = Mathf.Clamp (Speed / 10, 0, 1);
                    ThrustVecGrounded = 0;

                    PitchStrength = StartPitchStrength + (TakeoffAssist * Mathf.Min ((Speed / TakeoffAssistSpeed), 1)); //stronger pitch when moving fast and taxiing to help with taking off

                    if (BrakeInput > 0 && Speed < GroundBrakeSpeed && !Hooked) {
                        if (Speed > BrakeInput * GroundBrakeStrength * DeltaTime) {
                            VehicleRigidbody.velocity += -CurrentVel.normalized * BrakeInput * GroundBrakeStrength * DeltaTime;
                        } else {
                            VehicleRigidbody.velocity = Vector3.zero;
                        }
                    }

                    if (Physics.Raycast (GroundDetector.position, VehicleMainObj.transform.TransformDirection (Vector3.down), 1f, ResupplyLayer)) {
                        if (!ResupplyingLastFrame) {
                            LastResupplyTime = Time.time;
                        }
                        if (Time.time - LastResupplyTime > 1) {
                            //only play the sound if we're actually repairing/getting ammo/fuel
                            // if (!SoundControl.ReloadingNull && (NumAAM != FullAAMs || NumAGM != FullAGMs || NumBomb != FullBombs || Fuel < FullFuel - 10 || GunAmmoInSeconds != FullGunAmmo || Health != FullHealth))
                            //     SoundControl.Reloading.Play();
                            // LastResupplyTime = Time.time;
                            // NumAAM = (int)Mathf.Min(NumAAM + Mathf.Max(Mathf.Floor(FullAAMs / 10), 1), FullAAMs);
                            // NumAGM = (int)Mathf.Min(NumAGM + Mathf.Max(Mathf.Floor(FullAGMs / 5), 1), FullAGMs);
                            // NumBomb = (int)Mathf.Min(NumBomb + Mathf.Max(Mathf.Floor(FullBombs / 5), 1), FullBombs);

                            // /*                             Debug.Log(string.Concat("fuel ", Fuel));
                            //                             Debug.Log(string.Concat("FullFuel ", FullFuel));
                            //                             Debug.Log(string.Concat("Health ", Health));
                            //                             Debug.Log(string.Concat("FullHealth ", FullHealth));
                            //                             Debug.Log(string.Concat("GunAmmoInSeconds ", GunAmmoInSeconds));
                            //                             Debug.Log(string.Concat("FullGunAmmo ", FullGunAmmo)); */
                            // Fuel = Mathf.Min(Fuel + (FullFuel / 25), FullFuel);
                            // GunAmmoInSeconds = Mathf.Min(GunAmmoInSeconds + (FullGunAmmo / 20), FullGunAmmo);
                            // Health = Mathf.Min(Health + (FullHealth / 30), FullHealth);
                        }
                        ResupplyingLastFrame = true;
                    } else ResupplyingLastFrame = false;
                    //check for catapult below us and attach if there is one    
                    if (HasCatapult && CatapultStatus == 0) {
                        // RaycastHit hit;
                        // if (Physics.Raycast (CatapultDetector.position, CatapultDetector.TransformDirection (Vector3.down), out hit, 1f, CatapultLayer)) {
                        //     Transform CatapultTrigger = hit.collider.transform; //get the transform from the trigger hit

                        //     //Hit detected, check if the plane is facing in the right direction..
                        //     if (Vector3.Angle (VehicleMainObj.transform.forward, CatapultTrigger.transform.forward) < 15) {
                        //         //then lock the plane to the catapult! Works with the catapult in any orientation whatsoever.
                        //         CatapultLockRot = CatapultTrigger.transform.rotation; //rotation to lock the plane to on the catapult
                        //         VehicleMainObj.transform.rotation = CatapultLockRot; //set the plane to the locked rotation so the next step is done at the right angle
                        //         Vector3 temp = VehicleMainObj.transform.InverseTransformPoint (CatapultTrigger.transform.position); //relative position of the catapult to our plane
                        //         temp.y = 0; //zero out height because we don't want to move up/down
                        //         temp = VehicleMainObj.transform.TransformPoint (temp); //convert relative coords back to global
                        //         VehicleMainObj.transform.position += VehicleMainObj.transform.position - temp; //move plane to catapult

                        //         //here we do the same thing as above but with our own catapult detector and the trigger so that the front wheel locks to the correct position on the catapult
                        //         //might be a more efficient way to do this
                        //         temp = VehicleMainObj.transform.InverseTransformPoint (CatapultDetector.transform.position);
                        //         Vector3 temp2 = VehicleMainObj.transform.InverseTransformPoint (CatapultTrigger.transform.position) - temp;
                        //         temp2.x = 0;
                        //         temp2.z = 0;
                        //         temp = VehicleMainObj.transform.TransformPoint (temp);
                        //         temp2 = VehicleMainObj.transform.TransformPoint (temp2);
                        //         VehicleMainObj.transform.position = CatapultTrigger.transform.position + (VehicleMainObj.transform.position - temp) + (VehicleMainObj.transform.position - temp2);

                        //         CatapultLockPos = VehicleMainObj.transform.position;
                        //         VehicleRigidbody.velocity = Vector3.zero;
                        //         CatapultStatus = 1; //locked to catapult

                        //         //use dead to make plane invincible for 1 frame when entering the catapult to prevent damage which will be worse the higher your framerate is
                        //         dead = true;
                        //         CatapultDeadTimer = 2; //to make

                        //         if (InEditor) CatapultLockSound ();
                        //         else { SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CatapultLockSound"); }
                        //         /*  if (!SoundControl.CatapultLockNull)
                        //          {
                        //              //SoundControl.CatapultLock.play();
                        //          } */
                        //     }
                        // }
                    }
                } else {
                    PitchStrength = StartPitchStrength;
                    StillWindMulti = 1;
                    ThrustVecGrounded = 1;
                    Taxiinglerper = 0;
                }
                //Cruise PI Controller
                if (Cruise && !LGripLastFrame && !Shift && !Ctrl) {
                    int equals = Input.GetKey (KeyCode.Equals) ? 1 : 0;
                    int minus = Input.GetKey (KeyCode.Minus) ? 1 : 0;
                    SetSpeed = Mathf.Clamp (SetSpeed + (equals - minus), 0, 2000);

                    float error = (SetSpeed - AirSpeed);

                    CruiseIntegrator += error * DeltaTime;
                    CruiseIntegrator = Mathf.Clamp (CruiseIntegrator, CruiseIntegratorMin, CruiseIntegratorMax);

                    //float Derivator = Mathf.Clamp(((error - lastframeerror) / DeltaTime),DerivMin, DerivMax);

                    ThrottleInput = CruiseProportional * error;
                    ThrottleInput += CruiseIntegral * CruiseIntegrator;
                    //ThrottleInput += Derivative * Derivator; //works but spazzes out real bad
                    ThrottleInput = PlayerThrottle = Mathf.Clamp (ThrottleInput, 0, 1);
                } else //if cruise control disabled, use inputs
                {
                    ThrottleInput = PlayerThrottle;
                }
                Fuel = Mathf.Clamp (Fuel - ((FuelConsumption * Mathf.Max (ThrottleInput, 0.35f)) * DeltaTime), 0, FullFuel);
                if (Fuel < 200) ThrottleInput = Mathf.Clamp (ThrottleInput * (Fuel / 200), 0, 1);

                if (ThrottleInput < .6f) { EffectsControl.AfterburnerOn = false; Afterburner = 1; }
                if (AltHold && !RGripLastFrame) //alt hold enabled, and player not holding joystick
                {
                    Vector3 localAngularVelocity = transform.InverseTransformDirection (VehicleRigidbody.angularVelocity);
                    //Altitude hold PI Controller

                    int upsidedown = Vector3.Dot (Vector3.up, VehicleMainObj.transform.up) > 0 ? 1 : -1;
                    float error = CurrentVel.normalized.y - (localAngularVelocity.x * upsidedown * 2.5f); //(Vector3.Dot(VehicleRigidbody.velocity.normalized, Vector3.up));

                    AltHoldPitchIntegrator += error * DeltaTime;
                    //AltHoldPitchIntegrator = Mathf.Clamp(AltHoldPitchIntegrator, AltHoldPitchIntegratorMin, AltHoldPitchIntegratorMax);
                    //AltHoldPitchDerivator = (error - AltHoldPitchlastframeerror) / DeltaTime;
                    AltHoldPitchlastframeerror = error;
                    PitchInput = AltHoldPitchProportional * error;
                    PitchInput += AltHoldPitchIntegral * AltHoldPitchIntegrator;
                    //PitchInput += AltHoldPitchDerivative * AltHoldPitchDerivator; //works but spazzes out real bad
                    PitchInput = Mathf.Clamp (PitchInput, -1, 1);
                    AltHoldPitchlastframeerror = error;

                    //Roll
                    float ErrorRoll = VehicleMainObj.transform.localEulerAngles.z;
                    if (ErrorRoll > 180) { ErrorRoll -= 360; }

                    //lock upside down if rotated more than 90
                    if (ErrorRoll > 90) {
                        ErrorRoll -= 180;
                        PitchInput *= -1;
                    } else if (ErrorRoll < -90) {
                        ErrorRoll += 180;
                        PitchInput *= -1;
                    }

                    RollInput = Mathf.Clamp (AltHoldRollProportional * ErrorRoll, -1, 1);

                    YawInput = 0;

                    //flight limit internally enabled when alt hold is enabled
                    float GLimitStrength = Mathf.Clamp (-(Gs / GLimiter) + 1, 0, 1);
                    float AoALimitStrength = Mathf.Clamp (-(Mathf.Abs (AngleOfAttack) / AoALimiter) + 1, 0, 1);
                    float Limits = Mathf.Min (GLimitStrength, AoALimitStrength);
                    PitchInput *= Limits;
                } else //alt hold disabled, player has control
                {
                    // if (!InVR) {
                    //     VRPitchRollInput = LStick;
                    //     JoystickPosYaw.x = RStick.x;
                    //     //make stick input square
                    //     if (Mathf.Abs (VRPitchRollInput.x) > Mathf.Abs (VRPitchRollInput.y)) {
                    //         if (Mathf.Abs (VRPitchRollInput.x) > 0) {
                    //             float temp = VRPitchRollInput.magnitude / Mathf.Abs (VRPitchRollInput.x);
                    //             VRPitchRollInput *= temp;
                    //         }
                    //     } else if (Mathf.Abs (VRPitchRollInput.y) > 0) {
                    //         float temp = VRPitchRollInput.magnitude / Mathf.Abs (VRPitchRollInput.y);
                    //         VRPitchRollInput *= temp;
                    //     }
                    // }
                    //'-input' are used by effectscontroller, and multiplied by 'strength' for final values
                    if (FlightLimitsEnabled && !Taxiing && AngleOfAttack < AoALimiter) //flight limits are enabled
                    {
                        float GLimitStrength = Mathf.Clamp (-(Gs / GLimiter) + 1, 0, 1);
                        float AoALimitStrength = Mathf.Clamp (-(Mathf.Abs (AngleOfAttack) / AoALimiter) + 1, 0, 1);
                        float Limits = Mathf.Min (GLimitStrength, AoALimitStrength);
                        PitchInput = Mathf.Clamp ( /*(MouseY * mouseysens + Lstick.y + */ VRPitchRollInput.y + Wf + Sf + downf + upf, -1, 1) * Limits;
                        YawInput = Mathf.Clamp (Qf + Ef + JoystickPosYaw.x, -1, 1) * Limits;
                    } else //player is in full control
                    {
                        PitchInput = Mathf.Clamp ( /*(MouseY * mouseysens + Lstick.y + */ VRPitchRollInput.y + Wf + Sf + downf + upf, -1, 1);
                        YawInput = Mathf.Clamp (Qf + Ef + JoystickPosYaw.x, -1, 1);
                    }
                    //roll isn't subject to flight limits
                    RollInput = Mathf.Clamp ((( /*(MouseX * mousexsens) + */ VRPitchRollInput.x + Af + Df + leftf + rightf) * -1), -1, 1);
                }

                //ability to adjust input to be more precise at low amounts. 'exponant'
                /* pitchinput = pitchinput > 0 ? Mathf.Pow(pitchinput, StickInputPower) : -Mathf.Pow(Mathf.Abs(pitchinput), StickInputPower);
                yawinput = yawinput > 0 ? Mathf.Pow(yawinput, StickInputPower) : -Mathf.Pow(Mathf.Abs(yawinput), StickInputPower);
                rollinput = rollinput > 0 ? Mathf.Pow(rollinput, StickInputPower) : -Mathf.Pow(Mathf.Abs(rollinput), StickInputPower); */

                //if moving backwards, controls invert (if thrustvectoring is set to 0 strength for that axis)
                if ((Vector3.Dot (AirVel, VehicleMainObj.transform.forward) > 0)) //normal, moving forward
                {
                    ReversingPitchStrength = 1;
                    ReversingYawStrength = 1;
                    ReversingRollStrength = 1;
                } else //moving backward. The 'Zero' values are set in start(). Explanation there.
                {
                    ReversingPitchStrength = ReversingPitchStrengthZero;
                    ReversingYawStrength = ReversingYawStrengthZero;
                    ReversingRollStrength = ReversingRollStrengthZero;
                }

                //flip ur Vehicle to upright and stop rotating
                /*if (Input.GetButtonDown("Oculus_CrossPlatform_Button2") || (Input.GetKeyDown(KeyCode.T)))
                 {
                     VehicleMainObj.transform.rotation = Quaternion.Euler(VehicleMainObj.transform.rotation.eulerAngles.x, VehicleMainObj.transform.rotation.eulerAngles.y, 0f);
                     VehicleRigidbody.angularVelocity *= .3f;
                 }*/

                pitch = Mathf.Clamp (PitchInput /*  + Trim.x */ , -1, 1) * PitchStrength * ReversingPitchStrength;
                yaw = Mathf.Clamp (-YawInput /*  - Trim.y */ , -1, 1) * YawStrength * ReversingYawStrength;
                roll = RollInput * RollStrength * ReversingRollStrength;

                if (pitch > 0) {
                    pitch *= PitchDownStrMulti;
                }

                //wheel colliders are broken, this workaround stops the plane from being 'sticky' when you try to start moving it. Heard it doesn't happen so bad if rigidbody weight is realistic.
                if (Speed < .2 && ThrottleInput > 0)
                    VehicleRigidbody.velocity = VehicleRigidbody.transform.forward * 0.25f;
            } else {
                Occupied = false; //make vehicle respawnable if player disconnects while occupying
                //brake is always on if the plane is on the ground because we can't work out how to use wheel colliders properly
                if (Speed > GroundBrakeStrength * DeltaTime) {
                    VehicleRigidbody.velocity += -CurrentVel.normalized * GroundBrakeStrength * DeltaTime;
                } else VehicleRigidbody.velocity = Vector3.zero;
                PilotingInt = 0;
                roll = 0;
                pitch = 0;
                yaw = 0;
                RollInput = 0;
                PitchInput = 0;
                YawInput = 0;
                ThrottleInput = 0;
                /*                 PitchInput = Trim.x;
                                YawInput = Trim.y; */
            }

            //thrust vecotring airplanes have a minimum rotation speed
            float minlifttemp = rotlift * Mathf.Min (AoALiftPitch, AoALiftYaw);
            pitch *= Mathf.Max (PitchThrustVecMulti * ThrustVecGrounded, minlifttemp);
            yaw *= Mathf.Max (YawThrustVecMulti * ThrustVecGrounded, minlifttemp);
            roll *= Mathf.Max (RollThrustVecMulti * ThrustVecGrounded, minlifttemp);

            //rotation inputs are done, now we can set the minimum lift/drag when at high aoa, this should be high than 0 because if it's 0 you will have 0 drag when at 90 degree AoA.
            AoALiftPitch = Mathf.Clamp (AoALiftPitch, HighPitchAoaMinLift, 1);
            AoALiftYaw = Mathf.Clamp (AoALiftYaw, HighYawAoaMinLift, 1);

            //Lerp the inputs for 'engine response', throttle decrease response is slower than increase (EngineSpoolDownMulti)
            if (Throttle < ThrottleInput) {
                Throttle = Mathf.Lerp (Throttle, ThrottleInput, AccelerationResponse * DeltaTime); // ThrottleInput * ThrottleStrengthForward;
            } else {
                Throttle = Mathf.Lerp (Throttle, ThrottleInput, AccelerationResponse * EngineSpoolDownSpeedMulti * DeltaTime); // ThrottleInput * ThrottleStrengthForward;
            }

            //Lerp the inputs for 'rotation response'
            LerpedRoll = Mathf.Lerp (LerpedRoll, roll, RollResponse * DeltaTime);
            LerpedPitch = Mathf.Lerp (LerpedPitch, pitch, PitchResponse * DeltaTime);
            LerpedYaw = Mathf.Lerp (LerpedYaw, yaw, YawResponse * DeltaTime);

            //check for catching a cable with hook
            if (EffectsControl.HookDown) {
                if (Physics.Raycast (HookDetector.position, Vector3.down, 2f, HookCableLayer) && !Hooked) {
                    HookedLoc = VehicleMainObj.transform.position;
                    Hooked = true;
                    HookedTime = Time.time;
                    EffectsControl.PlaneAnimator.SetTrigger ("hooked");
                }
            }
            //slow down if hooked and on the ground
            if (Hooked && Taxiing) {
                if (Vector3.Distance (VehicleMainObj.transform.position, HookedLoc) > HookedBrakeMaxDistance) //real planes take around 80-90 meters to stop on a carrier
                {
                    //if you go further than HookedBrakeMaxDistance you snap the cable and it hurts your plane by the % of the amount of time left of the 2 seconds it should have taken to stop you.
                    float HookedDelta = (Time.time - HookedTime);
                    if (HookedDelta < 2) {
                        Health -= ((-HookedDelta + 2) / 2) * FullHealth;
                    }
                    Hooked = false;
                    if (InEditor) {
                        PlayCableSnap ();
                    } else {
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayCableSnap");
                    }

                }

                if (Speed > HookedBrakeStrength * DeltaTime) {
                    VehicleRigidbody.velocity += -CurrentVel.normalized * HookedBrakeStrength * DeltaTime;
                } else {
                    VehicleRigidbody.velocity = Vector3.zero;
                }
                //Debug.Log("hooked");
            }

            //flaps drag and lift
            if (EffectsControl.Flaps) {
                FlapsDrag = FlapsDragMulti;
                FlapsLift = FlapsLiftMulti;
            } else {
                FlapsDrag = 1;
                FlapsLift = 1;
            }
            //gear drag
            if (EffectsControl.GearUp) { GearDrag = 1; } else { GearDrag = LandingGearDragMulti; }
            FlapsGearBrakeDrag = (GearDrag + FlapsDrag + (BrakeInput * AirbrakeStrength)) - 1; //combine these so we don't have to do as much in fixedupdate

            switch (CatapultStatus) {
                case 0: //normal
                    //do lift
                    Vector3 FinalInputAcc = new Vector3 (-sidespeed * SidewaysLift * SpeedLiftFactor * AoALiftYaw * Atmosphere, // X Sideways
                        (downspeed * FlapsLift * PitchDownLiftMulti * SpeedLiftFactor * AoALiftPitch * Atmosphere) + (SpeedLiftFactor * AoALiftPitch * VelLift * Atmosphere), // Y Up
                        Throttle * ThrottleStrength * Afterburner * Atmosphere); // Z Forward

                    //used to add rotation friction
                    Vector3 localAngularVelocity = transform.InverseTransformDirection (VehicleRigidbody.angularVelocity);

                    //roll + rotational frictions
                    Vector3 FinalInputRot = new Vector3 (-localAngularVelocity.x * PitchFriction * rotlift * AoALiftPitch * AoALiftYaw * Atmosphere, // X Pitch
                        -localAngularVelocity.y * YawFriction * rotlift * AoALiftPitch * AoALiftYaw * Atmosphere, // Y Yaw
                        (LerpedRoll * Atmosphere) + (-localAngularVelocity.z * RollFriction * rotlift * AoALiftPitch * AoALiftYaw * Atmosphere)); // Z Roll

                    //create values for use in fixedupdate (control input and straightening forces)
                    Pitching = (((VehicleMainObj.transform.up * LerpedPitch * Atmosphere + (VehicleMainObj.transform.up * downspeed * VelStraightenStrPitch * AoALiftPitch * rotlift * Atmosphere)) * 90)) * PilotingInt;
                    Yawing = (((VehicleMainObj.transform.right * LerpedYaw * Atmosphere + (-VehicleMainObj.transform.right * sidespeed * VelStraightenStrYaw * AoALiftYaw * rotlift * Atmosphere)) * 90)) * PilotingInt;

                    VehicleConstantForce.relativeForce = FinalInputAcc;
                    VehicleConstantForce.relativeTorque = FinalInputRot;
                    break;
                case 1: //locked on catapult
                    //dead == invincible, turn off once a frame has passed since attaching
                    if (dead) {
                        CatapultDeadTimer -= 1;
                        if (CatapultDeadTimer == 0) dead = false;
                    }

                    VehicleConstantForce.relativeForce = Vector3.zero;
                    VehicleConstantForce.relativeTorque = Vector3.zero;

                    CatapultLaunchTime = CatapultLaunchTimeStart;
                    // VehicleMainObj.transform.position = CatapultLockPos;
                    // VehicleMainObj.transform.rotation = CatapultLockRot;
                    VehicleRigidbody.velocity = Vector3.zero;
                    VehicleRigidbody.angularVelocity = Vector3.zero;
                    break;
                case 2: //launching
                    // VehicleMainObj.transform.rotation = CatapultLockRot;
                    VehicleConstantForce.relativeForce = new Vector3 (0, 0, CatapultLaunchStrength);
                    //lock all movment except for forward movement
                    Vector3 temp = VehicleMainObj.transform.InverseTransformDirection (VehicleRigidbody.velocity);
                    temp.x = 0;
                    temp.y = 0;
                    temp = VehicleMainObj.transform.TransformDirection (temp);
                    VehicleRigidbody.velocity = temp;
                    VehicleRigidbody.angularVelocity = Vector3.zero;
                    VehicleConstantForce.relativeTorque = Vector3.zero;
                    // CatapultLaunchTime -= DeltaTime;
                    // if (CatapultLaunchTime < 0) {
                        // dead = false; //just in casesd
                        // CatapultStatus = 0;
                        // Taxiinglerper = 0;
                    // }
                    break;
            }

            SoundBarrier = (-Mathf.Clamp (Mathf.Abs (Speed - 343) / SoundBarrierWidth, 0, 1) + 1) * SoundBarrierStrength;
        } else //non-owners need to know these values
        {
            Speed = CurrentVel.magnitude; //wind speed is local anyway, so just use ground speed for non-owners
            //VRChat doesn't set Angular Velocity to 0 when you're not the owner of a rigidbody (it seems),
            //causing spazzing, the script handles angular drag it itself, so when we're not owner of the plane, set this value to stop spazzing
            VehicleRigidbody.angularDrag = .3f;
            //AirVel = VehicleRigidbody.velocity - Wind;
            //AirSpeed = AirVel.magnitude;
        }
        SmokeColor_Color = new Color (SmokeColor.x, SmokeColor.y, SmokeColor.z);
        CanopyCloseTimer -= DeltaTime;
    }
    private void FixedUpdate () {
        if (IsOwner || InEditor) {
            float DeltaTime = Time.deltaTime;
            //lerp velocity toward 0 to simulate air friction
            VehicleRigidbody.velocity = Vector3.Lerp (VehicleRigidbody.velocity, FinalWind * StillWindMulti, ((((AirFriction + SoundBarrier) * FlapsGearBrakeDrag) * Atmosphere) * 90) * DeltaTime);
            //apply pitching using pitch moment
            VehicleRigidbody.AddForceAtPosition (Pitching * DeltaTime, PitchMoment.position, ForceMode.Force);
            //apply yawing using yaw moment
            VehicleRigidbody.AddForceAtPosition (Yawing * DeltaTime, YawMoment.position, ForceMode.Force);
            //calc Gs
            LastFrameVel.y += (-9.81f * DeltaTime); //add gravity
            Gs = Vector3.Distance (LastFrameVel, VehicleRigidbody.velocity) / (9.81f * DeltaTime);
            LastFrameVel = VehicleRigidbody.velocity;
        }
    }
    public void LaunchFlares () {
        EffectsControl.PlaneAnimator.SetTrigger ("flares");
    }

    //In soundcontroller, CanopyCloseTimer < -100000 means play inside canopy sounds and between -100000 and 0 means play outside sounds.
    //The value is set above these numbers by the length of the animation, and delta time is removed from it each frame.
    //This code adds or removes 100000 based on the situation, + the time it takes for the animation to play.
    //This part is effectively disabled (by not allowing toggling if in transition) because it isn't reliable enough ->>//If the Opening animation is playing when you tell it to close it keeps the time from that animation so that the timing of the sound changing is always correct.
    public void CanopyOpening () {
        if (CanopyCloseTimer > 0)
            CanopyCloseTimer -= 100000 + CanopyCloseTime;
        else
            CanopyCloseTimer = -100000;
    }
    public void CanopyClosing () {
        if (CanopyCloseTimer > (-100000 - CanopyCloseTime) && CanopyCloseTimer < 0)
            CanopyCloseTimer += 100000 + ((CanopyCloseTime * 2) + 0.1f); //the 0.1 is for the delay in the animator that is needed because it's not set to write defaults
        else
            CanopyCloseTimer = CanopyCloseTime;
    }
    public void PlayABOnSound () {
        if ((Piloting || Passenger) && (CanopyCloseTimer < 0 && CanopyCloseTimer > -100000)) {
            if (!SoundControl.ABOnInsideNull)
                SoundControl.ABOnInside.Play ();
        } else {
            if (!SoundControl.ABOnOutsideNull)
                SoundControl.ABOnOutside.Play ();
        }
    }
    public void CatapultLaunchEffects () {
        VehicleRigidbody.WakeUp (); //i don't think it actually sleeps anyway but this might help other clients sync the launch faster idk
        if (EffectsControl.CatapultSteam != null) { EffectsControl.CatapultSteam.Play (); }
        if (Piloting || Passenger) {
            if (!SoundControl.CatapultLaunchNull) {
                SoundControl.CatapultLaunch.Play ();
            }
        } else {
            if (!SoundControl.CatapultLaunchNull) {
                SoundControl.CatapultLaunch.Play ();
            }
        }
    }
    public void CatapultLockSound () {
        VehicleRigidbody.Sleep (); //don't think this actually works
        if (!SoundControl.CatapultLockNull)
            SoundControl.CatapultLock.Play ();
    }
    //these are used for syncing weapon selection for bomb bay doors animation etc
    public void RStick0 () //Rstick is something other than a weapon
    {
        EffectsControl.PlaneAnimator.SetInteger ("weapon", 0);
    }
    public void RStick1 () //GUN
    {
        EffectsControl.PlaneAnimator.SetInteger ("weapon", 1);
    }
    public void RStick2 () //AAM
    {
        EffectsControl.PlaneAnimator.SetInteger ("weapon", 2);
    }
    public void RStick3 () //AGM
    {
        EffectsControl.PlaneAnimator.SetInteger ("weapon", 3);
    }
    public void RStick4 () //Bomb
    {
        EffectsControl.PlaneAnimator.SetInteger ("weapon", 4);
    }
    public void Explode () //all the things players see happen when the vehicle explodes
    {
        EffectsControl.DoEffects = 0f; //keep awake

        dead = true;
        EffectsControl.GearUp = false;
        EffectsControl.HookDown = false;
        BrakeInput = 0;
        FlightLimitsEnabled = true;
        Cruise = false;
        if (!EffectsControl.FrontWheelNull) EffectsControl.FrontWheel.localRotation = Quaternion.identity;
        //EngineControl.Trim = Vector2.zero;
        if (HasCanopy) {
            EffectsControl.CanopyOpen = true;
            CanopyCloseTimer = -100001;
        }
        Hooked = false;
        GunAmmoInSeconds = FullGunAmmo;
        Fuel = FullFuel;
        RStickSelection = 0;
        LStickSelection = 0;

        //play sonic boom if it was going to play before it exploded
        if (SoundControl.playsonicboom && SoundControl.silent) {
            if (!SoundControl.SonicBoomNull) {
                int rand = Random.Range (0, SoundControl.SonicBoom.Length);
                if (SoundControl.SonicBoom[rand] != null) {
                    SoundControl.SonicBoom[rand].pitch = Random.Range (.94f, 1.2f);
                    SoundControl.SonicBoom[rand].PlayDelayed ((SoundControl.SonicBoomDistance - SoundControl.SonicBoomWave) / 343);
                }
            }
        }
        SoundControl.playsonicboom = false;
        SoundControl.silent = false;

        SoundControl.PlaneIdlePitch = 0;
        SoundControl.PlaneIdleVolume = 0;
        SoundControl.PlaneThrustVolume = 0;
        SoundControl.PlaneDistantVolume = 0;

        if (!SoundControl.PlaneDistantNull) { SoundControl.PlaneDistant.volume = 0; }

        foreach (AudioSource thrust in SoundControl.Thrust) {
            thrust.pitch = 0;
            thrust.volume = 0;
        }
        foreach (AudioSource idle in SoundControl.PlaneIdle) {
            idle.pitch = 0;
            idle.volume = 0;
        }

        if (IsOwner || InEditor) {
            // VehicleRigidbody.velocity = Vector3.zero;
            Health = FullHealth; //turns off low health smoke
            Fuel = FullFuel;
        }

        if (SoundControl != null && !SoundControl.ExplosionNull) {
            int rand = Random.Range (0, SoundControl.Explosion.Length);
            if (SoundControl.Explosion[rand] != null) {
                SoundControl.Explosion[rand].Play (); //explosion sound has travel time
            }
        }

        //pilot and passenger are dropped out of the plane
        if ((Piloting || Passenger) && !InEditor) {
            foreach (LeaveVehicleButton seat in LeaveButtons) {
                seat.ExitStation ();
            }
        }
        if(mistracker!=null)
        mistracker.cleanup();
        EffectsControl.Tarps = false;
        hbcontroller.Respawn ();
        EffectsControl.PlaneAnimator.SetTrigger ("explode");

    }
    public void PlayCableSnap () {
        if (!SoundControl.CableSnapNull) { SoundControl.CableSnap.Play (); }
    }
    private void Assert (bool condition, string message) {
        if (!condition) {
            Debug.LogError ("Assertion failed : '" + GetType () + " : " + message + "'", this);
        }
    }
}