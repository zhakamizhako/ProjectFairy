using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class MissilePlaneSystem : UdonSharpBehaviour {
    // public GameObject MissileFab;
    // public GameObject MissileSpawnArea;
    public GameObject[] MissileSpawnAreas; // Object array for missile spawn areas. If you have like 8 mthfking missile pods, there you have it.
    public AudioSource[] MissileFireSounds; // Object array for Missile fire sounds. Can add as many as you want 
    public GameObject MissileFab; // Object for missile object
    private bool[] MissileFired; // Array to indicate whether the missile has been fired or not
    public AircraftController EngineController; // ahAHAHAHAHAHAHAHahaha
    public SoundController SoundController; // For missile alert;
    public AudioSource NextTarget; // Next Target sfx
    public AudioSource NoTargets; // No target Sfx
    public MissileTargets MissileTargetScript; // Targets List. :3
    public MissileTargeterParent misTarget; // Missile Target Assigner
    public GameObject MyTracker; // If it's your own tracker marker...
    public Transform Player; // Lol. I think i should remove this.
    public Rigidbody PlaneOwner; // Ummmmm another VEHICLEOBJECT main.
    public GameObject vehicleMain; // Vehicle Object to adapt velocity from it
    public GameObject TargetChangeHUD; // Target Change Indicator
    public GameObject NoTargetHUD; // No target Indicator
    public GameObject SelfTarget; // Targetting self indicator
    public GameObject LockSightHUD;

    //Missile Tracker shit
    private float timerLocking = 0;
    public float timerToLock = 3;
    public float minimumRange = 300;
    public float range = 100;
    public float radius = 5;
    private bool isLocking = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] private bool isLocked = false;
    public LayerMask layermask;
    public Transform TargetDetector;
    public Transform ChangeTargetDetector;
    public GameObject currentHitObject;
    public float hitDistance;
    private float frameLast = 0;
    private bool updateCursor = false;
    public float updateIntervals = 0.08f;

    public AudioSource locking;
    public AudioSource locked;
    // public MissileScript MissileScript2;
    // private float cooldownmissile1 = 0;
    // private float cooldownmissile2 = 0;
    private float cooldownhud = 0;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public int launchArea = 0;
    private Vector3 ScaleTarget = new Vector3 (0.5f, 0.5f, 0.1f);
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public int selectedTargetIndex = -1;
    [HideInInspector] public GameObject SelectedTarget;
    public bool showTargets = false; //Called by PilotSeat
    public bool tempShow = false; //Called from Editor

    private bool pauseScript = false; // In case of events. 
    private bool buttonDown = false;
    private float RGrip = 0;
    private bool RGriplastframetrigger = false;
    private float DPAD_RIGHT = 0;
    private bool DPAD_RIGHT_last_trigger = false;
    private float DPAD_DOWN = 0;
    private bool DPAD_DOWN_last_trigger = false;
    public int Missiles = 0;
    public float[] CooldownMissiles;
    public GameObject[] CooldownMissileHUD;
    public float timeToCooldown = 10;
    public ParticleSystem[] flareObject;
    public GameObject Tailer;
    public ParentConstraint TailerConstraint;
    public AudioSource FlareSound;
    public bool hasFlares = false;
    public float flareCooldown = 15;
    public float flareCountdown = 0;
    public float returnTracker = 3;
    // public Vector3 translationOffset;
    public Text FlareText;
    [System.NonSerializedAttribute][HideInInspector] public bool isFiredFlare = false;
    [System.NonSerializedAttribute][HideInInspector] public GameObject MissileRuntime;

    [System.NonSerializedAttribute][HideInInspector] public VRCPlayerApi localPlayer;

    public float changeTargetDetectorRadius = 5000;
    public float changeTargetDetectorRange = 20000;
    void Start () { //Initialize Missile Packs and status
        if (LockSightHUD != null) {
            LockSightHUD.SetActive (false);
        }
        if (Missiles != null) {
            MissileFired = new bool[Missiles];
            CooldownMissiles = new float[Missiles];
            for (int x = 0; x < MissileFired.Length; x++) {
                MissileFired[x] = false;
                CooldownMissiles[x] = 0;
            }
        }
        //Hide targets in case.
        foreach (GameObject go in MissileTargetScript.Targets) {
            if (go != null) {
                var TrackerObject = go.GetComponent<MissileTrackerAndResponse> ().TargetIconRender;
                TrackerObject.SetActive (false);
                // go.GetComponent<MissileTrackerAndResponse> ().MainObject.layer = 0;
                // go.layer = 0;
            }
        }
    }

    public void flareSync () {
        if (hasFlares && flareObject.Length > 0) {
            if (!isFiredFlare) {
                if (Tailer != null) {
                    Tailer.GetComponent<BoxCollider> ().enabled = true;
                    TailerConstraint.constraintActive = false;
                    Tailer.GetComponent<Rigidbody> ().AddRelativeForce (Vector3.forward * -20);
                    isFiredFlare = true;
                    if (FlareSound != null)
                        FlareSound.Play ();
                    if (FlareText != null)
                        FlareText.text = "FLARE: EMPTY";
                    foreach (ParticleSystem fo in flareObject) {
                        // if (!isHeavyPlane) {
                        ParticleSystem.EmissionModule v = fo.emission;
                        v.enabled = true;
                        // }
                    }
                } else {
                    Debug.Log ("Put the Flare inside a Tailer, and Tailer must exist in the outside, and must be constrained with the parent Gameobject.");
                }
            }
        } else if (hasFlares && flareObject == null) {
            Debug.Log ("Hey wtf. Put the damn flare objects or else it wont work!");
        }
    }

    //For the copilot to see what you're targetting. Also can be used for tracking later
    public void changeTargetSync () {
        // Will have to implement disabled targets in the future for story mode and advances.
        if (selectedTargetIndex != -1) {
            misTarget.Target = MissileTargetScript.Targets[selectedTargetIndex];
        } else {
            misTarget.Target = null;
        }

        misTarget.noTarget = true;
        // Debug.Log ("Target Sync Complete");
        // Debug.Log (misTarget.Target);
        // Debug.Log ("-----------------------");
        isLocking = false;
        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
        isLocked = false;
        timerLocking = 0f;
        if (locked != null) { locked.Stop (); }
        if (locking != null) { locking.Stop (); }

        // MissileFab.GetComponent<MissileScript>().Target = SelectedTarget;
    }

    public void SetLocked () {
        misTarget.noTarget = false;
    }

    private void Update () {
        //Flare Cooldown
        if (flareObject.Length > 0) {
            if (isFiredFlare) {
                if (flareCountdown > returnTracker) {
                    Tailer.GetComponent<BoxCollider> ().enabled = false;
                    TailerConstraint.constraintActive = true;
                    foreach (ParticleSystem fo in flareObject) {
                        ParticleSystem.EmissionModule v = fo.emission;
                        v.enabled = false;
                    }
                }
                if (flareCountdown < flareCooldown) {
                    flareCountdown = flareCountdown + Time.deltaTime;
                } else {
                    isFiredFlare = false;
                    flareCountdown = 0;
                    if (FlareText != null)
                        FlareText.text = "FLARE: RDY";
                }
            }
        }
        if (MissileFab != null) {
            if (CooldownMissileHUD != null && CooldownMissileHUD.Length > 0) {
                for (int b = 0; b < Missiles; b++) {
                    if (MissileFired[b] == true) {
                        if (CooldownMissileHUD[b].activeSelf)
                            CooldownMissileHUD[b].SetActive (false);
                    } else if (MissileFired[b] == false) {
                        if (!CooldownMissileHUD[b].activeSelf)
                            CooldownMissileHUD[b].SetActive (true);
                    }

                    if (CooldownMissiles[b] < timeToCooldown && MissileFired[b] == true) {
                        CooldownMissiles[b] = CooldownMissiles[b] + Time.deltaTime;
                        // Debug.Log(CooldownMissileHUD[b]);
                    } else {
                        MissileFired[b] = false;
                        CooldownMissiles[b] = 0f;
                    }
                }
            }
        }
        // if (MissileFab != null) { //Show missile hud status
        //     for (int b = 0; b < MissileFab.Length; b++) {
        //         if (Missiles[b].GetComponent<MissileScript> ().fired == false) {
        //             MissileFired[b] = false;
        //         }
        //     }
        // }

        //Brute force Syncer. Fk that.
        if (selectedTargetIndex != -1 && misTarget != null) {
            if (MissileTargetScript.Targets[selectedTargetIndex] != misTarget.Target)
                misTarget.Target = MissileTargetScript.Targets[selectedTargetIndex];
            if (isLocked) {
                misTarget.noTarget = false;
            } else {
                misTarget.noTarget = true;
            }
        }

        //Cooldown time for the tgt indicators
        if (cooldownhud < 1f) {
            cooldownhud += Time.deltaTime;
        }
        if (cooldownhud > 1f) {
            if (NoTargetHUD != null) {
                NoTargetHUD.SetActive (false);
            }
            if (TargetChangeHUD != null) {
                TargetChangeHUD.SetActive (false);
            }
            if (SelfTarget != null) {
                SelfTarget.SetActive (false);
            }
        }

        if (EngineController.dead) {
            isLocking = false;
            isLocked = false;
            if (localPlayer == null) {
                StopTrackingSync ();
            }
            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
            if (locking != null) {
                locking.Stop ();
            }
            if (locked != null) {
                locked.Stop ();
            }
        }

        if (EngineController.localPlayer == null || EngineController.Piloting || EngineController.Passenger) {
            tempShow = true;
            //Render Targets
            foreach (GameObject go in MissileTargetScript.Targets) { //Local only update targets
                if (go != null) {
                    var TrackerObject = go.GetComponent<MissileTrackerAndResponse> ().TargetIconRender;
                    var bc = go.GetComponent<MissileTrackerAndResponse> ();
                    if (showTargets && go != MyTracker && bc.isRendered) {
                        TrackerObject.SetActive (true);
                        // Transform tb = TrackerObject.GetComponent<Transform> ();
                        // tb.localRotation = Quaternion.Euler(tb.localRotation.x, tb.localRotation.y, vehicleMain.GetComponent<Transform>().rotation.z);
                        float dist = 0f;
                        if (EngineController.localPlayer != null) {
                            dist = Vector3.Distance (EngineController.localPlayer.GetPosition (), TrackerObject.GetComponent<Transform> ().position);
                        } else {
                            dist = Vector3.Distance (Player.position, TrackerObject.GetComponent<Transform> ().position);
                        }
                        TrackerObject.GetComponent<Transform> ().localScale = ScaleTarget * dist;
                        if (EngineController.localPlayer != null) {
                            TrackerObject.GetComponent<Transform> ().LookAt (EngineController.localPlayer.GetPosition ()); //Stick icon rotation to player
                        } else {
                            TrackerObject.GetComponent<Transform> ().LookAt (Player); //Stick icon rotation to player
                        }
                        var TrackerScript = go.GetComponent<MissileTrackerAndResponse> ();
                        if (TrackerScript.TrackerText != null) {
                            var words = "";
                            if (TrackerScript.MainObject != null) { //Target Name
                                words = words + "" + TrackerScript.MainObject.name;
                            }
                            if (TrackerScript.EngineController != null) { // For Player Based Targets
                                if (TrackerScript.EngineController.Occupied && TrackerScript.EngineController.piloted != null) { //Pilot Name
                                    words = words + "\n" + TrackerScript.EngineController.piloted;
                                }
                                if (TrackerScript.EngineController.Health > 0) { //Health
                                    words = words + "\nHP:" + TrackerScript.EngineController.Health;
                                    if (TrackerObject.GetComponent<Renderer> ().material.color != Color.white && (selectedTargetIndex != -1 && go != MissileTargetScript.Targets[selectedTargetIndex])) {
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
                                    } else if (selectedTargetIndex != -1 && go == MissileTargetScript.Targets[selectedTargetIndex]) {
                                        if (TrackerObject.GetComponent<Renderer> ().material.color != Color.yellow)
                                            TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.yellow);
                                    } else if (selectedTargetIndex == -1) {
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
                                    }
                                } else {
                                    words = words + "\nDestroyed";
                                    if (TrackerObject.GetComponent<Renderer> ().material.color != Color.red)
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
                                }

                            }
                            if (TrackerScript.AITurret != null) { // For Turret Based Targets

                                if (TrackerScript.AITurret.Health > 0 && TrackerScript.AITurret.damageable) { //Health
                                    words = words + "\nHP:" + TrackerScript.AITurret.Health;
                                    if (TrackerObject.GetComponent<Renderer> ().material.color != Color.white && (selectedTargetIndex != -1 && go != MissileTargetScript.Targets[selectedTargetIndex])) {
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
                                    } else if (selectedTargetIndex != -1 && go == MissileTargetScript.Targets[selectedTargetIndex]) {
                                        if (TrackerObject.GetComponent<Renderer> ().material.color != Color.yellow)
                                            TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.yellow);
                                    } else if (selectedTargetIndex == -1) {
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
                                    }
                                } else {
                                    words = words + "\nDestroyed";
                                    if (TrackerObject.GetComponent<Renderer> ().material.color != Color.red)
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
                                }

                            }
                            if (TrackerScript.AI != null) {
                                if (TrackerScript.AI.Health > 0 && TrackerScript.AI.damageable) { //Health
                                    words = words + "\nHP:" + TrackerScript.AI.Health;
                                    if (TrackerObject.GetComponent<Renderer> ().material.color != Color.white && (selectedTargetIndex != -1 && go != MissileTargetScript.Targets[selectedTargetIndex])) {
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
                                    } else if (selectedTargetIndex != -1 && go == MissileTargetScript.Targets[selectedTargetIndex]) {
                                        if (TrackerObject.GetComponent<Renderer> ().material.color != Color.yellow)
                                            TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.yellow);
                                    } else if (selectedTargetIndex == -1) {
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
                                    }
                                } else if (TrackerScript.AI.Health <= 0) {
                                    words = words + "\nDestroyed";
                                    if (TrackerObject.GetComponent<Renderer> ().material.color != Color.red)
                                        TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
                                }
                            }
                            //Distance
                            float distance = 0f;
                            if (TrackerScript.EngineController != null && TrackerScript.EngineController.localPlayer != null) {
                                distance = Vector3.Distance (EngineController.localPlayer.GetPosition (), TrackerObject.GetComponent<Transform> ().transform.position);
                            } else {
                                distance = Vector3.Distance (Player.transform.position, TrackerObject.GetComponent<Transform> ().transform.position);
                            }

                            words = words + "\n" + Mathf.Round (distance);
                            TrackerScript.TrackerText.text = words;
                        }

                    } else {
                        TrackerObject.SetActive (false);
                    }
                }
            }
        } else if ((!EngineController.Piloting || !EngineController.Passenger) && tempShow) {
            foreach (GameObject go in MissileTargetScript.Targets) {
                var TrackerObject = go.GetComponent<MissileTrackerAndResponse> ().TargetIconRender;
                TrackerObject.SetActive (false);
            }
            tempShow = false;
        }
        // ^---- Very efficient programming, i know. =/

        //Lock Logic, To be shown too as Copilot
        if (isLocking && timerLocking < timerToLock) {
            timerLocking = timerLocking + Time.deltaTime;
            if (timerLocking > timerToLock) {
                isLocked = true;
                SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetLocked");
                if (locking != null) {
                    locking.Stop ();
                }
                if (locked != null) {
                    locked.Play ();
                }
            }
        }

        if (isLocked) {
            misTarget.noTarget = false;
        }

        if (frameLast < updateIntervals) {
            frameLast = frameLast + Time.deltaTime;
            updateCursor = false;
        } else if (frameLast > updateIntervals) {
            frameLast = 0;
            updateCursor = true;
        }

        if (LockSightHUD != null) { //rendering the lock sight
            float dist = 0f;

            if (EngineController.localPlayer != null) {
                LockSightHUD.transform.LookAt (EngineController.localPlayer.GetPosition ());
                dist = Vector3.Distance (EngineController.localPlayer.GetPosition (), LockSightHUD.transform.position);
                LockSightHUD.transform.localScale = new Vector3 (1, 1, 1) * dist;
            } else {
                dist = Vector3.Distance (Player.position, LockSightHUD.transform.position);
                LockSightHUD.transform.LookAt (Player);
                LockSightHUD.transform.localScale = new Vector3 (1, 1, 1) * dist;
            }

            if (isLocking) {
                Vector3 randomVectors = new Vector3 (Random.Range (-50f, 50f), Random.Range (-50f, 50f), Random.Range (-50f, 50f));
                LockSightHUD.SetActive (true);
                if (updateCursor) {
                    LockSightHUD.transform.position = MissileTargetScript.Targets[selectedTargetIndex].transform.position + randomVectors;
                }

                if (isLocking && !isLocked)
                    LockSightHUD.GetComponent<Transform> ().GetChild (0).GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            } else {
                LockSightHUD.SetActive (false);
            }

            if (isLocked) {
                LockSightHUD.transform.position = MissileTargetScript.Targets[selectedTargetIndex].transform.position;
                LockSightHUD.GetComponent<Transform> ().GetChild (0).GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
                //  SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetLocked");
            }
        }

        if (EngineController.localPlayer == null || EngineController.Piloting) { // Only runs in pilot logic
            if (Input.GetKeyDown (KeyCode.X) || Input.GetButton ("Oculus_CrossPlatform_Button2")) {
                if (EngineController.localPlayer == null)
                    flareSync ();
                SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "flareSync");
            }

            if (MissileFab != null) {
                //Check Cooldowns for both missile sides

                //Locking Mechanism Logic begins here.
                int lastTarget = selectedTargetIndex;
                if (TargetDetector != null && selectedTargetIndex != -1) {
                    RaycastHit[] hit = Physics.SphereCastAll (TargetDetector.position, radius, TargetDetector.forward, range, layermask, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects
                    if (hit.Length > 0) {
                        bool found = false;
                        for (int x = 0; x < hit.Length; x++) {
                            if (hit[x].transform.gameObject.GetComponent<HitDetector> () != null) {
                                if (hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker == misTarget.Target.GetComponent<MissileTrackerAndResponse> ()) {
                                    currentHitObject = hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker.gameObject;
                                    hitDistance = hit[x].distance;
                                    found = true;
                                    break;
                                }
                            } else if (hit[x].transform.gameObject.GetComponent<MissileTrackerAndResponse> () != null) {
                                if (hit[x].transform.gameObject.GetComponent<MissileTrackerAndResponse> () == misTarget.Target.GetComponent<MissileTrackerAndResponse> ()) {
                                    currentHitObject = hit[x].transform.gameObject.GetComponent<MissileTrackerAndResponse> ().gameObject;
                                    hitDistance = hit[x].distance;
                                    found = true;
                                    break;
                                }
                            }
                        }

                        if (found) {
                            var bb = currentHitObject.GetComponent<MissileTrackerAndResponse> ();
                            if (!isLocking) {
                                // var bb = currentHitObject.GetComponent<HitDetector> ().Tracker;
                                var b1 = misTarget.Target.GetComponent<MissileTrackerAndResponse> ();
                                if ((bb != null) && (bb == b1) && (hitDistance > minimumRange)) {
                                    isLocking = true;
                                    locking.Play ();
                                    if (EngineController.localPlayer == null) {
                                        TrackingSync ();
                                    }
                                    SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TrackingSync");
                                    // break;
                                } else {
                                    // Debug.Log ("Nein");
                                    if (isLocking) {
                                        isLocking = false;
                                        if (EngineController.localPlayer == null)
                                            StopTrackingSync ();
                                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
                                    }
                                    if(lastTarget!=-1){
                                        MissileTargetScript.Targets[lastTarget].GetComponent<MissileTrackerAndResponse>().isTracking = false;
                                    }
                                    misTarget.noTarget = true;
                                    isLocking = false;
                                    isLocked = false;
                                    timerLocking = 0;
                                    if (locking != null) {
                                        locking.Stop ();
                                    }
                                    if (locked != null) {
                                        locked.Stop ();
                                    }
                                }
                            }
                        } else {
                            currentHitObject = null;
                            hitDistance = 0;

                            if (isLocking) {
                                isLocking = false;
                                if (EngineController.localPlayer == null)
                                    StopTrackingSync ();
                                SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
                            }
                            misTarget.noTarget = true;
                            isLocking = false;
                            isLocked = false;
                            timerLocking = 0;
                            if (locking != null) {
                                locking.Stop ();
                            }
                            if (locked != null) {
                                locked.Stop ();
                            }
                        }

                        // MissileTrackerAndResponse bb = null;
                        // for (int x = 0; x < hit.Length; x++) { // skim through the list of detected targets
                        //     currentHitObject = hit[x].transform.gameObject;
                        //     hitDistance = hit[x].distance;  ````            ````
                        //     Transform[] children = currentHitObject.GetComponentsInChildren<Transform> ();
                        //     if (currentHitObject.GetComponent<HitDetector> () != null) {
                        //         if (misTarget.Target.GetComponent<MissileTrackerAndResponse> () == currentHitObject.GetComponent<HitDetector> ().Tracker)
                        //             bb = currentHitObject.GetComponent<HitDetector> ().Tracker;
                        //         break;
                        //     } else if (currentHitObject.GetComponent<MissileTrackerAndResponse> () != null) {
                        //         if ((misTarget.Target.GetComponent<MissileTrackerAndResponse> () == currentHitObject.GetComponent<MissileTrackerAndResponse> ())) {
                        //             bb = currentHitObject.GetComponent<MissileTrackerAndResponse> ();
                        //             break;
                        //         }
                        //     }
                        // }

                        // if (!isLocking) {
                        //     if (selectedTargetIndex != -1) {
                        //         // var bb = currentHitObject.GetComponent<HitDetector> ().Tracker;
                        //         var b1 = misTarget.Target.GetComponent<MissileTrackerAndResponse> ();
                        //         if ((bb != null) && (bb == b1) && (hitDistance > minimumRange)) {
                        //             isLocking = true;
                        //             locking.Play ();
                        //             if (EngineController.localPlayer == null) {
                        //                 TrackingSync ();
                        //             }
                        //             SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TrackingSync");
                        //             // break;
                        //         } else {
                        //             Debug.Log ("Nein");
                        //             if (isLocking) {
                        //                 isLocking = false;
                        //                 if (EngineController.localPlayer == null)
                        //                     StopTrackingSync ();
                        //                 SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
                        //             }
                        //             misTarget.noTarget = true;
                        //             isLocking = false;
                        //             isLocked = false;
                        //             timerLocking = 0;
                        //             if (locking != null) {
                        //                 locking.Stop ();
                        //             }
                        //             if (locked != null) {
                        //                 locked.Stop ();
                        //             }
                        //         }
                        //     }
                        // }
                    } else {
                        hitDistance = range;
                        currentHitObject = null;
                        if (isLocking) {
                            isLocking = false;
                            if (EngineController.localPlayer == null)
                                StopTrackingSync ();
                            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
                        }
                        misTarget.noTarget = true;
                        isLocking = false;
                        isLocked = false;
                        timerLocking = 0;
                        if (locking != null) {
                            locking.Stop ();
                        }
                        if (locked != null) {
                            locked.Stop ();
                        }
                    }
                    Debug.DrawLine (TargetDetector.position, TargetDetector.position + TargetDetector.forward * hitDistance);
                }

                //End Locking mechanism

                //On Missile Fire
                if (MissileFab != null && Player != null) {
                    RGrip = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryHandTrigger");
                    DPAD_RIGHT = Input.GetAxisRaw ("Joy1 Axis 6");
                    DPAD_DOWN = Input.GetAxisRaw ("Joy1 Axis 7");

                    //Change Target
                    if ((Input.GetKeyDown (KeyCode.Y) || RGrip > 0.5 || DPAD_RIGHT > 0 || DPAD_RIGHT < 0) && (!RGriplastframetrigger && !DPAD_RIGHT_last_trigger)) {
                        if (DPAD_RIGHT > 0 || DPAD_RIGHT < 0) { DPAD_RIGHT_last_trigger = true; }
                        if (RGrip > 0.5f) { RGriplastframetrigger = true; }
                        ChangeTargetCommand ();
                    } else if ((RGrip < 0.5) && RGriplastframetrigger) {
                        RGriplastframetrigger = false;
                    }
                    if (Input.GetAxisRaw ("Joy1 Axis 6") == 0 && DPAD_RIGHT_last_trigger == true) {
                        DPAD_RIGHT_last_trigger = false;
                    }

                    //Fire Missile
                    if ((Input.GetKeyDown (KeyCode.Space) || (Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryHandTrigger") >.5f || DPAD_DOWN < 0 || DPAD_DOWN > 0) && (buttonDown == false && DPAD_DOWN_last_trigger == false))) {
                        if (Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryHandTrigger") >.5f) { buttonDown = true; }
                        if (DPAD_DOWN < 0 || DPAD_DOWN > 0) { DPAD_DOWN_last_trigger = true; }
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "MissileSync");
                        if (EngineController.localPlayer == null) //so it works in editor
                        {
                            MissileSync ();
                        }
                    }
                    if (Input.GetAxisRaw ("Oculus_CrossPlatform_PrimaryHandTrigger") < .5f && buttonDown == true) {
                        buttonDown = false;
                    }
                    if (Input.GetAxisRaw ("Joy1 Axis 7") == 0 && DPAD_DOWN_last_trigger == true) {
                        DPAD_DOWN_last_trigger = false;
                    }
                } else {
                    // Debug.Log ("errr wtf");
                }
            }
        }
    }

    public void TrackingSync () {
        var targetToWarn = misTarget.Target;
        if (EngineController.localPlayer != null) {
            if (targetToWarn != null) {
                targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = true;
                // Debug.Log ("Tracking alert");
                // Debug.Log (targetToWarn);
                // Debug.Log ("--------------------");
            }

        } else if (EngineController.localPlayer == null) {
            targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = true;
        }
    }

    // public void newChangeTargetCommand(){
    //     if(EngineController.localPlayer==null || EngineController.Piloting){
    //         if(MissileTargetScript.Targets.Length > 0){
    //             RaycastHit[] hit = Physics.SphereCastAll (ChangeTargetDetector.position, radius, ChangeTargetDetector.forward, range, layermask, QueryTriggerInteraction.UseGlobal); 
    //             if(hit.Length > 0){

    //             }
    //         }
    //     }
    // }

    public void ChangeTargetCommand () {
        if (EngineController.localPlayer == null || EngineController.Piloting) {
            if (MissileTargetScript.Targets.Length > 0) {
                for (;;) {
                    if (selectedTargetIndex+1 != -1 && selectedTargetIndex + 1 < MissileTargetScript.Targets.Length) {
                        // selectedTargetIndex = selectedTargetIndex + 1
                        // Debug.Log ("TargetChange");
                        // Debug.Log (MyTracker);
                        // Debug.Log (MissileTargetScript.Targets[selectedTargetIndex + 1]);
                        // Debug.Log ("-------");
                        if ((MyTracker == MissileTargetScript.Targets[selectedTargetIndex + 1] || MissileTargetScript.Targets[selectedTargetIndex + 1].GetComponent<MissileTrackerAndResponse> ().isTargetable == false)) {
                            if(selectedTargetIndex+1 < MissileTargetScript.Targets.Length){
                                selectedTargetIndex = selectedTargetIndex + 1;
                            }else{
                                selectedTargetIndex = -2;
                            }
                            continue;
                        } else {
                            // foreach (GameObject item in MissileTargetScript.Targets) {
                            //     if (item.GetComponent<MissileTrackerAndResponse> ().AITurret == null) {
                            //         item.layer = 0;
                            //     } else {
                            //         item.GetComponent<MissileTrackerAndResponse>  ().MainObject.layer = 0;
                            //     }
                            // }
                            // if (MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AITurret != null) {
                            //     MissileTargetScript.Targets[selectedTargetIndex].layer = 23;
                            // } else {
                            //     MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().MainObject.layer = 23;
                            // }
                            if (TargetChangeHUD != null) {
                                if (NextTarget != null)
                                    NextTarget.Play ();
                                TargetChangeHUD.SetActive (true);
                                if (SelfTarget != null)
                                    SelfTarget.SetActive (false);
                                cooldownhud = 0;

                            }
                        }
                    } else {
                        // selectedTargetIndex = -1;
                        if (NoTargets != null)
                            NoTargets.Play ();
                        if (NoTargetHUD != null) {
                            NoTargetHUD.SetActive (true);
                            cooldownhud = 0;
                        }
                        if (TargetChangeHUD != null) {
                            TargetChangeHUD.SetActive (false);
                        }
                        if (SelfTarget != null) {
                            SelfTarget.SetActive (false);
                        }
                    }

                    if (selectedTargetIndex + 1 < MissileTargetScript.Targets.Length) {
                        selectedTargetIndex = selectedTargetIndex + 1;
                    } else {
                        selectedTargetIndex = -1;
                    }

                    if (selectedTargetIndex != -1) {
                        break;
                        // Debug.Log ("Target Select::");

                        // Debug.Log (misTarget.Target);
                        // Debug.Log ("Assign Success");
                    } else {
                        misTarget.Target = null;
                        misTarget.noTarget = true;
                        break;
                        // Debug.Log ("No Targets Selected");
                    }
                }

                if (EngineController.localPlayer == null) //editor mode. 
                    changeTargetSync ();
                SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "changeTargetSync");

                for (int i = 0; i < MissileTargetScript.Targets.Length; i++) {
                    var grender = MissileTargetScript.Targets[i].GetComponent<MissileTrackerAndResponse> ().TargetIconRender.GetComponent<Renderer> (); //If it crashes because of this, its either you haven't setup the correct tracker or the targets array.
                    if (i != selectedTargetIndex) {
                        grender.material.SetColor ("_Color", Color.white); //Not selected
                    } else {
                        grender.material.SetColor ("_Color", Color.yellow); //Selected Target
                    }
                }
            }
        }
    }

    public void StopTrackingSync () {
        var targetToWarn = misTarget.Target;
        if (EngineController.localPlayer != null) {
            if (targetToWarn != null) {
                // Debug.Log ("Stop Tracking");
                // Debug.Log (targetToWarn);
                // Debug.Log ("--------------------");
                targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = false;
            }
        } else if (EngineController.localPlayer == null) {
            targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = false;
        }
    }

    public void MissileSync () {
        // The next set of lines below basically what it does is it spawns the missile depending which pod is free if they're not on cooldown.
        // Debug.Log ("MissileSync Called");
        if (MissileSpawnAreas.Length > 0) {
            for (int i = 0; i < Missiles; i++) {
                if (MissileFired[i] == false) {
                    MissileFired[i] = true;
                    MissileRuntime = VRCInstantiate (MissileFab);

                    MissileRuntime.GetComponent<Rigidbody> ().velocity = PlaneOwner.velocity;
                    MissileRuntime.GetComponent<Collider> ().enabled = false;
                    if (launchArea + 1 < MissileSpawnAreas.Length) {
                        launchArea = launchArea + 1;
                    } else {
                        launchArea = 0;
                    }
                    MissileRuntime.GetComponent<Transform> ().position = MissileSpawnAreas[launchArea].GetComponent<Transform> ().position;
                    MissileRuntime.GetComponent<Transform> ().rotation = MissileSpawnAreas[launchArea].GetComponent<Transform> ().rotation;
                    if (MissileFireSounds.Length > 0) {
                        int rInt = Random.Range (0, MissileFireSounds.Length);
                        MissileFireSounds[rInt].Play ();
                    }
                    // if (isLocked) { MissileRuntime.GetComponent<MissileScript> ().ShouldTrack = true; } else { MissileRuntime.GetComponent<MissileScript> ().ShouldTrack = false; }
                    // MissileRuntime.GetComponent<MissileScript> ().fired = true;
                    MissileRuntime.SetActive (true);
                    break;
                } else {
                    continue;
                }
            }
        } else {
            Debug.Log ("Someone forgot to place the spawn areas for the missiles. This script will not work without them.");
        }
    }
}