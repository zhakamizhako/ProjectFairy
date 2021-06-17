using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class HitboxControllerAndEffects : UdonSharpBehaviour {
    public EngineController EngineControl;
    public GameObject Body;
    public GameObject LWing;
    public GameObject RWing;
    public GameObject LRudder;
    public GameObject RRudder;
    public GameObject LAileron;
    public GameObject RAileron;
    public GameObject LElevator;
    public GameObject RElevator;
    public GameObject LEngine;
    public GameObject REngine;
    public GameObject LFlap;
    public GameObject RFlap;
    // public GameObject debrisBody;
    // public GameObject debrisLWing;
    // public GameObject debrisRWing;
    // public GameObject debrisLRudder;
    // public GameObject debrisRRudder;
    // public GameObject debrisLAileron;
    // public GameObject debrisRAileron;
    // public GameObject debrisLElevator;
    // public GameObject debrisRElevator;
    // public GameObject debrisLEngine;
    // public GameObject debrisREngine;
    // public GameObject debrisLFlap;
    // public GameObject debrisRFlap;
    [System.NonSerializedAttribute] public bool isBodyDead = false;
    [System.NonSerializedAttribute] public bool isLWingDead = false;
    [System.NonSerializedAttribute] public bool isRWingDead = false;
    [System.NonSerializedAttribute] public bool isLRudderDead = false;
    [System.NonSerializedAttribute] public bool isRRudderDead = false;
    [System.NonSerializedAttribute] public bool isLAileronDead = false;
    [System.NonSerializedAttribute] public bool isRAileronDead = false;
    [System.NonSerializedAttribute] public bool isLElevatorDead = false;
    [System.NonSerializedAttribute] public bool isRElevatorDead = false;
    [System.NonSerializedAttribute] public bool isLEngineDead = false;
    [System.NonSerializedAttribute] public bool isREngineDead = false;
    [System.NonSerializedAttribute] public bool isLFlapDead = false;
    [System.NonSerializedAttribute] public bool isRFlapDead = false;
    [UdonSynced (UdonSyncMode.None)] public float HealthBody = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthLWing = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthRWing = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthLRudder = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthRRudder = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthLAileron = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthRAileron = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthLElevator = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthRElevator = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthLEngine = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthREngine = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthLFlap = 100;
    [UdonSynced (UdonSyncMode.None)] public float HealthRFlap = 100;

    [System.NonSerializedAttribute] public float initHealthBody = 0;
    [System.NonSerializedAttribute] public float initHealthLWing = 0;
    [System.NonSerializedAttribute] public float initHealthRWing = 0;
    [System.NonSerializedAttribute] public float initHealthLRudder = 0;
    [System.NonSerializedAttribute] public float initHealthRRudder = 0;
    [System.NonSerializedAttribute] public float initHealthLAileron = 0;
    [System.NonSerializedAttribute] public float initHealthRAileron = 0;
    [System.NonSerializedAttribute] public float initHealthLElevator = 0;
    [System.NonSerializedAttribute] public float initHealthRElevator = 0;
    [System.NonSerializedAttribute] public float initHealthLEngine = 0;
    [System.NonSerializedAttribute] public float initHealthREngine = 0;
    [System.NonSerializedAttribute] public float initHealthLFlap = 0;
    [System.NonSerializedAttribute] public float initHealthRFlap = 0;
    [System.NonSerializedAttribute] public float initPitchStrength = 0;
    [System.NonSerializedAttribute] public float initRollStrength = 0;
    [System.NonSerializedAttribute] public float initRollResponse = 0;
    [System.NonSerializedAttribute] public float initRollFriction = 0;
    [System.NonSerializedAttribute] public float initYawStrength = 0;
    [System.NonSerializedAttribute] public float initYawResponse = 0;
    [System.NonSerializedAttribute] public float initFlapsDragMulti = 0;
    [System.NonSerializedAttribute] public float initFlapsLiftMulti = 0;
     public float initStartPitchStrength = 0;
    [System.NonSerializedAttribute] public float initLift = 0;
    [System.NonSerializedAttribute] public float initPitchResponse = 0;
    [System.NonSerializedAttribute] public float initHighAoAMinControlPitch = 0;
    [System.NonSerializedAttribute] public float initHighAoAMinControlYaw = 0;
    [System.NonSerializedAttribute] public float initMaxAngleOfAttackPitch = 0;
    [System.NonSerializedAttribute] public float initMaxAngleOfAttackYaw = 0;
    [System.NonSerializedAttribute] public float initThrottleStrength = 0;
    [System.NonSerializedAttribute] public bool initHasAfterburner = false;
    [System.NonSerializedAttribute] public bool initd = false; // execution order stuff.

    public AudioSource[] componentExplosionSound;
    // public Animator DamageIndicatorAnimator;
    // public EffectsController EffectsControl;

    void Start () {
        // if (EngineControl != null) {
        //     //EngineController Params
        //     initLift = EngineControl.Lift;
        //     //pitch
        //     initPitchStrength = EngineControl.PitchStrength;
        //     // initStartPitchStrength = EngineControl.StartPitchStrength;
        //     initPitchResponse = EngineControl.PitchResponse;
        //     //roll
        //     initRollStrength = EngineControl.RollStrength;
        //     initRollFriction = EngineControl.RollFriction;
        //     initRollResponse = EngineControl.RollResponse;
        //     //yaw
        //     initYawResponse = EngineControl.YawResponse;
        //     initYawStrength = EngineControl.YawStrength;
        //     //aoa
        //     initMaxAngleOfAttackPitch = EngineControl.MaxAngleOfAttackPitch;
        //     initMaxAngleOfAttackYaw = EngineControl.MaxAngleOfAttackYaw;
        //     initHighAoAMinControlPitch = EngineControl.HighAoaMinControlPitch;
        //     initHighAoAMinControlYaw = EngineControl.HighAoaMinControlYaw;
        //     //engines
        //     initThrottleStrength = EngineControl.ThrottleStrength;
        //     initHasAfterburner = EngineControl.HasAfterburner;
        //     //flaps
        //     initFlapsDragMulti = EngineControl.FlapsDragMulti;
        //     initFlapsLiftMulti = EngineControl.FlapsLiftMulti;
        // }
        initHealthBody = HealthBody;
        initHealthLWing = HealthLWing;
        initHealthRWing = HealthRWing;
        initHealthLRudder = HealthLRudder;
        initHealthRRudder = HealthRRudder;
        initHealthLAileron = HealthLAileron;
        initHealthRAileron = HealthRAileron;
        initHealthLElevator = HealthLElevator;
        initHealthRElevator = HealthRElevator;
        initHealthLEngine = HealthLEngine;
        initHealthREngine = HealthREngine;
        initHealthLFlap = HealthLFlap;
        initHealthRFlap = HealthRFlap;

    }
    void Update () {
        if (initd &&Networking.IsOwner (gameObject) || EngineControl.localPlayer==null) {
            //If Deif(HealthBody!=null){  }
            if (Body != null) {
                if (HealthBody <= 0 && isBodyDead == false) {
                    Body.SetActive (false);
                    isBodyDead = true;
                    Explode ();
                } else if (HealthBody > 0 && isBodyDead) {
                    isBodyDead = false;
                    Body.SetActive (true);
                }
            }
            if (LWing != null) {
                if (HealthLWing <= 0 && isLWingDead == false) {
                    LWing.SetActive (false);
                    isLWingDead = true;
                    Explode ();
                    EngineControl.Lift = EngineControl.Lift / 2;
                    EngineControl.RollFriction = EngineControl.RollFriction / 2;
                    EngineControl.MaxAngleOfAttackPitch = EngineControl.MaxAngleOfAttackPitch / 2;
                } else if (HealthLWing > 0 && isLWingDead) {
                    isLWingDead = false;
                    LWing.SetActive (true);
                }
            }
            if (RWing != null) {
                if (HealthRWing <= 0 && isRWingDead == false) {
                    RWing.SetActive (false);
                    isRWingDead = true;
                    Explode ();
                    EngineControl.Lift = EngineControl.Lift / 2;
                    EngineControl.RollFriction = EngineControl.RollFriction / 2;
                    EngineControl.MaxAngleOfAttackPitch = EngineControl.MaxAngleOfAttackPitch / 2;
                } else if (HealthRWing > 0 && isRWingDead) {
                    isRWingDead = false;
                    RWing.SetActive (true);
                }
            }
            if (LRudder != null) {
                if (HealthLRudder <= 0 && isLRudderDead == false) {
                    LRudder.SetActive (false);
                    isLRudderDead = true;
                    Explode ();
                    EngineControl.YawStrength = EngineControl.YawStrength / 2;
                    EngineControl.MaxAngleOfAttackYaw = EngineControl.MaxAngleOfAttackYaw / 2;
                } else if (HealthLRudder > 0 && isLRudderDead) {
                    isLRudderDead = false;
                    LRudder.SetActive (true);
                }
            }
            if (RRudder != null) {
                if (HealthRRudder <= 0 && isRRudderDead == false) {
                    RRudder.SetActive (false);
                    isRRudderDead = true;
                    Explode ();
                    EngineControl.YawStrength = EngineControl.YawStrength / 2;
                    EngineControl.MaxAngleOfAttackYaw = EngineControl.MaxAngleOfAttackYaw / 2;
                } else if (HealthRRudder > 0 && isRRudderDead) {
                    isRRudderDead = false;
                    RRudder.SetActive (true);
                }
            }
            if (LAileron != null) {
                if (HealthLAileron <= 0 && isLAileronDead == false) {
                    LAileron.SetActive (false);
                    isLAileronDead = true;
                    Explode ();
                    EngineControl.RollStrength = EngineControl.RollStrength / 2;
                    EngineControl.RollResponse = EngineControl.RollResponse / 2;
                } else if (HealthLAileron > 0 && isLAileronDead) {
                    isLAileronDead = false;
                    LAileron.SetActive (true);
                }
            }
            if (RAileron != null) {
                if (HealthRAileron <= 0 && isRAileronDead == false) {
                    RAileron.SetActive (false);
                    isRAileronDead = true;
                    Explode ();
                    EngineControl.RollStrength = EngineControl.RollStrength / 2;
                    EngineControl.RollResponse = EngineControl.RollResponse / 2;
                } else if (HealthRAileron > 0 && isRAileronDead) {
                    isRAileronDead = false;
                    RAileron.SetActive (true);
                }
            }
            if (LElevator != null) {
                if (HealthLElevator <= 0 && isLElevatorDead == false) {
                    LElevator.SetActive (false);
                    isLElevatorDead = true;
                    Explode ();
                    EngineControl.PitchStrength = EngineControl.PitchStrength / 2;
                    // EngineControl.StartPitchStrength = EngineControl.StartPitchStrength / 2;
                    EngineControl.PitchResponse = EngineControl.PitchResponse / 2;
                } else if (HealthLElevator > 0 && isLElevatorDead) {
                    isLElevatorDead = false;
                    LElevator.SetActive (true);
                }
            }
            if (RElevator != null) {
                if (HealthRElevator <= 0 && isRElevatorDead == false) {
                    RElevator.SetActive (false);
                    isRElevatorDead = true;
                    Explode ();
                    EngineControl.PitchStrength = EngineControl.PitchStrength / 2;
                    // EngineControl.StartPitchStrength = EngineControl.StartPitchStrength / 2;
                    EngineControl.PitchResponse = EngineControl.PitchResponse / 2;
                } else if (HealthRElevator > 0 && isRElevatorDead) {
                    isRElevatorDead = false;
                    RElevator.SetActive (true);
                }
            }
            if (LEngine != null) {
                if (HealthLEngine <= 0 && isLEngineDead == false) {
                    LEngine.SetActive (false);
                    isLEngineDead = true;
                    EngineControl.ThrottleStrength = EngineControl.ThrottleStrength / 2;
                    EngineControl.HasAfterburner = false;
                    EngineControl.EffectsControl.AfterburnerOn = false;
                    Explode ();
                } else if (HealthLEngine > 0 && isLEngineDead) {
                    isLEngineDead = false;
                    LEngine.SetActive (true);
                }
            }
            if (REngine != null) {
                if (HealthREngine <= 0 && isREngineDead == false) {
                    REngine.SetActive (false);
                    isREngineDead = true;
                    EngineControl.ThrottleStrength = EngineControl.ThrottleStrength / 2;
                    EngineControl.HasAfterburner = false;
                    EngineControl.EffectsControl.AfterburnerOn = false;
                    Explode ();
                } else if (HealthREngine > 0 && isREngineDead) {
                    isREngineDead = false;
                    REngine.SetActive (true);
                }
            }
            if (LFlap != null) {
                if (HealthLFlap <= 0 && isLFlapDead == false) {
                    LFlap.SetActive (false);
                    isLFlapDead = true;
                    Explode ();
                    EngineControl.FlapsLiftMulti = EngineControl.FlapsLiftMulti / 2;
                    EngineControl.FlapsDragMulti = EngineControl.FlapsDragMulti / 2;
                } else if (HealthLFlap > 0 && isLFlapDead) {
                    isLFlapDead = false;
                    LFlap.SetActive (true);
                }
            }
            if (RFlap != null) {
                if (HealthRFlap <= 0 && isRFlapDead == false) {
                    RFlap.SetActive (false);
                    isRFlapDead = true;
                    Explode ();
                    EngineControl.FlapsLiftMulti = EngineControl.FlapsLiftMulti / 2;
                    EngineControl.FlapsDragMulti = EngineControl.FlapsDragMulti / 2;
                } else if (HealthRFlap > 0 && isRFlapDead) {
                    isRFlapDead = false;
                    RFlap.SetActive (true);
                }
            }
        }
        else if(initd && !Networking.IsOwner(gameObject)){ 
             if (Body != null) {
                if (HealthBody <= 0){
                    Body.SetActive (false);
                } else if (HealthBody > 0 ){
                    Body.SetActive (true);
                }
            }
            if (LWing != null) {
                if (HealthLWing <= 0 ){
                    LWing.SetActive (false);
                } else if (HealthLWing > 0 ){
                    LWing.SetActive (true);
                }
            }
            if (RWing != null) {
                if (HealthRWing <= 0 ){
                    RWing.SetActive (false);
                } else if (HealthRWing > 0 ){
                    RWing.SetActive (true);
                }
            }
            if (LRudder != null) {
                if (HealthLRudder <= 0 ){
                    LRudder.SetActive (false);
                } else if (HealthLRudder > 0 ){
                    LRudder.SetActive (true);
                }
            }
            if (RRudder != null) {
                if (HealthRRudder <= 0 ){
                    RRudder.SetActive (false);
                } else if (HealthRRudder > 0 ){
                    RRudder.SetActive (true);
                }
            }
            if (LAileron != null) {
                if (HealthLAileron <= 0 ){
                    LAileron.SetActive (false);
                } else if (HealthLAileron > 0 ){
                    LAileron.SetActive (true);
                }
            }
            if (RAileron != null) {
                if (HealthRAileron <= 0 ){
                    RAileron.SetActive (false);
                } else if (HealthRAileron > 0 ){
                    RAileron.SetActive (true);
                }
            }
            if (LElevator != null) {
                if (HealthLElevator <= 0 ){
                    LElevator.SetActive (false);
                } else if (HealthLElevator > 0 ){
                    LElevator.SetActive (true);
                }
            }
            if (RElevator != null) {
                if (HealthRElevator <= 0 ){
                    RElevator.SetActive (false);
                } else if (HealthRElevator > 0 ){
                    RElevator.SetActive (true);
                }
            }
            if (LEngine != null) {
                if (HealthLEngine <= 0 ){
                    LEngine.SetActive (false);
                    Explode ();
                } else if (HealthLEngine > 0 ){
                    LEngine.SetActive (true);
                }
            }
            if (REngine != null) {
                if (HealthREngine <= 0 ){
                    REngine.SetActive (false);
                    Explode ();
                } else if (HealthREngine > 0 ){
                    REngine.SetActive (true);
                }
            }
            if (LFlap != null) {
                if (HealthLFlap <= 0 ){
                    LFlap.SetActive (false);
                } else if (HealthLFlap > 0 ){
                    LFlap.SetActive (true);
                }
            }
            if (RFlap != null) {
                if (HealthRFlap <= 0 ){
                    RFlap.SetActive (false);
                    Explode ();
                } else if (HealthRFlap > 0 ){
                    RFlap.SetActive (true);
                }
            }
        }
    }

    public void Explode () {
        if (Networking.IsOwner (gameObject))
            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "callPublic");
        if(EngineControl.localPlayer==null){
            callPublic();
        }
    }

    public void callPublic () {
        if (componentExplosionSound.Length > 0) {
            int rand = Random.Range (0, componentExplosionSound.Length);
            componentExplosionSound[rand].Play ();
        }
    }
    public void Respawn () {
        if (EngineControl.localPlayer==null || Networking.IsOwner (gameObject)) {
            // isBodyDead = false;
            // isLWingDead = false;
            // isRWingDead = false;
            // isLRudderDead = false;
            // isRRudderDead = false;
            // isLAileronDead = false;
            // isRAileronDead = false;
            // isLElevatorDead = false;
            // isRElevatorDead = false;
            // isLEngineDead = false;
            // isREngineDead = false;
            // isLFlapDead = false;
            // isRFlapDead = false;
            HealthBody = initHealthBody;
            HealthLWing = initHealthLWing;
            HealthRWing = initHealthRWing;
            HealthLRudder = initHealthLRudder;
            HealthRRudder = initHealthRRudder;
            HealthLAileron = initHealthLAileron;
            HealthRAileron = initHealthRAileron;
            HealthLElevator = initHealthLElevator;
            HealthRElevator = initHealthRElevator;
            HealthLEngine = initHealthLEngine;
            HealthREngine = initHealthREngine;
            HealthLFlap = initHealthLFlap;
            HealthRFlap = initHealthRFlap;

            EngineControl.Lift = initLift;
            //Pitch
            EngineControl.PitchStrength = initPitchStrength;
            // EngineControl.StartPitchStrength = initStartPitchStrength;
            EngineControl.PitchResponse = initPitchResponse;
            //Roll
            EngineControl.RollResponse = initRollResponse;
            EngineControl.RollStrength = initRollStrength;
            EngineControl.RollFriction = initRollFriction;
            //Yaw
            EngineControl.YawResponse = initYawResponse;
            EngineControl.YawStrength = initYawStrength;
            //AoA
            EngineControl.HighPitchAoaMinControl = initHighAoAMinControlPitch;
            EngineControl.HighYawAoaMinControl = initHighAoAMinControlYaw;
            EngineControl.MaxAngleOfAttackPitch = initMaxAngleOfAttackPitch;
            EngineControl.MaxAngleOfAttackYaw = initMaxAngleOfAttackYaw;
            //Engines
            EngineControl.HasAfterburner = initHasAfterburner;
            EngineControl.ThrottleStrength = initThrottleStrength;
            //Flaps
            EngineControl.FlapsDragMulti = initFlapsDragMulti;
            EngineControl.FlapsLiftMulti = initFlapsLiftMulti;
        }

    }

}