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
    [HideInInspector] public bool isBodyDead = false;
    [HideInInspector] public bool isLWingDead = false;
    [HideInInspector] public bool isRWingDead = false;
    [HideInInspector] public bool isLRudderDead = false;
    [HideInInspector] public bool isRRudderDead = false;
    [HideInInspector] public bool isLAileronDead = false;
    [HideInInspector] public bool isRAileronDead = false;
    [HideInInspector] public bool isLElevatorDead = false;
    [HideInInspector] public bool isRElevatorDead = false;
    [HideInInspector] public bool isLEngineDead = false;
    [HideInInspector] public bool isREngineDead = false;
    [HideInInspector] public bool isLFlapDead = false;
    [HideInInspector] public bool isRFlapDead = false;
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

    [HideInInspector] public float initHealthBody = 0;
    [HideInInspector] public float initHealthLWing = 0;
    [HideInInspector] public float initHealthRWing = 0;
    [HideInInspector] public float initHealthLRudder = 0;
    [HideInInspector] public float initHealthRRudder = 0;
    [HideInInspector] public float initHealthLAileron = 0;
    [HideInInspector] public float initHealthRAileron = 0;
    [HideInInspector] public float initHealthLElevator = 0;
    [HideInInspector] public float initHealthRElevator = 0;
    [HideInInspector] public float initHealthLEngine = 0;
    [HideInInspector] public float initHealthREngine = 0;
    [HideInInspector] public float initHealthLFlap = 0;
    [HideInInspector] public float initHealthRFlap = 0;
    private float initPitchStrength = 0;
    private float initRollStrength = 0;
    private float initRollResponse = 0;
    private float initRollFriction = 0;
    private float initYawStrength = 0;
    private float initYawResponse = 0;
    private float initFlapsDragMulti = 0;
    private float initFlapsLiftMulti = 0;
    private float initStartPitchStrength = 0;
    private float initLift = 0;
    private float initPitchResponse = 0;
    private float initHighAoAMinControlPitch = 0;
    private float initHighAoAMinControlYaw = 0;
    private float initMaxAngleOfAttackPitch = 0;
    private float initMaxAngleOfAttackYaw = 0;
    private float initThrottleStrength = 0;
    private bool initHasAfterburner = false;

    public AudioSource[] componentExplosionSound;
    // public Animator DamageIndicatorAnimator;
    // public EffectsController EffectsControl;

    void Start () {
        if (EngineControl != null) {
            //EngineController Params
            initLift = EngineControl.Lift;
            //pitch
            initPitchStrength = EngineControl.PitchStrength;
            initStartPitchStrength = EngineControl.StartPitchStrength;
            initPitchResponse = EngineControl.PitchResponse;
            //roll
            initRollStrength = EngineControl.RollStrength;
            initRollFriction = EngineControl.RollFriction;
            initRollResponse = EngineControl.RollResponse;
            //yaw
            initYawResponse = EngineControl.YawResponse;
            initYawStrength = EngineControl.YawStrength;
            //aoa
            initMaxAngleOfAttackPitch = EngineControl.MaxAngleOfAttackPitch;
            initMaxAngleOfAttackYaw = EngineControl.MaxAngleOfAttackYaw;
            initHighAoAMinControlPitch = EngineControl.HighAoaMinControlPitch;
            initHighAoAMinControlYaw = EngineControl.HighAoaMinControlYaw;
            //engines
            initThrottleStrength = EngineControl.ThrottleStrength;
            initHasAfterburner = EngineControl.HasAfterburner;
            //flaps
            initFlapsDragMulti = EngineControl.FlapsDragMulti;
            initFlapsLiftMulti = EngineControl.FlapsLiftMulti;
        }
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
        if (Networking.IsOwner (gameObject) || EngineControl.localPlayer==null) {

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
                    EngineControl.StartPitchStrength = EngineControl.StartPitchStrength / 2;
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
                    EngineControl.StartPitchStrength = EngineControl.StartPitchStrength / 2;
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
            EngineControl.StartPitchStrength = initStartPitchStrength;
            EngineControl.PitchResponse = initPitchResponse;
            //Roll
            EngineControl.RollResponse = initRollResponse;
            EngineControl.RollStrength = initRollStrength;
            EngineControl.RollFriction = initRollFriction;
            //Yaw
            EngineControl.YawResponse = initYawResponse;
            EngineControl.YawStrength = initYawStrength;
            //AoA
            EngineControl.HighAoaMinControlPitch = initHighAoAMinControlPitch;
            EngineControl.HighAoaMinControlYaw = initHighAoAMinControlYaw;
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