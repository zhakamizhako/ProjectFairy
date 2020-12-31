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
    public EngineController EngineController; // ahAHAHAHAHAHAHAHahaha
    public AudioSource NextTarget; // Next Target sfx
    public AudioSource NoTargets; // No target Sfx
    public MissileTargets MissileTargetScript; // Targets List. :3
    public MissileTargeterParent misTarget; // Missile Target Assigner
    public GameObject MyTracker; // If it's your own tracker marker...
    public Transform Player; // Lol. I think i should remove this.
    public GameObject TargetChangeHUD; // Target Change Indicator
    public GameObject NoTargetHUD; // No target Indicator
    public GameObject SelfTarget; // Targetting self indicator
    public GameObject LockSightHUD;
    private RaycastHit[] objects;
    public Transform SpawnParent;

    public float lockAngle = 75f;
    public bool isGun = false;
    public float LerpMultiplier = 1f;

    //Missile Tracker shit
    [HideInInspector] public float timerLocking = 0;
    public float timerToLock = 3;
    public float minimumRange = 300;
    public float range = 100;
    public float radius = 5;
    [HideInInspector] public bool isLocking = false;
    [HideInInspector] public bool isLocked = false;
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
    private bool deadExecute = false;
    public bool CanTarget = true;
    private float cooldownhud = 0;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public int launchArea = 0;
    private Vector3 ScaleTarget = new Vector3 (0.6f, 0.6f, 0.1f);
    [UdonSynced (UdonSyncMode.None)] public int selectedTargetIndex = -1;
    public bool showTargets = false; //Called by PilotSeat
    public bool tempShow = false; //Called from Editor
    private bool buttonDown = false;
    private float RGrip = 0;
    private bool RGriplastframetrigger = false;
    private float DPAD_RIGHT = 0;
    private bool DPAD_RIGHT_last_trigger = false;
    private float DPAD_DOWN = 0;
    private bool DPAD_DOWN_last_trigger = false;
    public int Missiles = 0;
    private float[] CooldownMissiles;
    public GameObject[][] CooldownMissileHUD;
    public float timeToCooldown = 10;
    // public ParticleSystem[] flareObject;
    // public GameObject Tailer;
    // public ParentConstraint TailerConstraint;
    // public AudioSource FlareSound;
    // public bool hasFlares = false;
    // public int flaresCount = 3;
    // public int flaresNow = 3;
    // public float flareCooldown = 15;
    // public float flareCountdown = 0;
    public float returnTracker = 3;
    public bool isBomber = false;
    // public Vector3 translationOffset;
    // public Text FlareText;
    public int[] targetIndices;
    // [System.NonSerializedAttribute][HideInInspector] public bool isFiredFlare = false;
    [System.NonSerializedAttribute][HideInInspector] public GameObject MissileRuntime;
    [System.NonSerializedAttribute][HideInInspector] public VRCPlayerApi localPlayer;
    public MissileTrackerAndResponse[] TGTList;
    public int TGTLISTIndex = -1;
    public float changeTargetDetectorRadius = 5000;
    public float changeTargetDetectorRange = 20000;
    public Vector3 SpawnScaleOffset = new Vector3 (1, 1, 1);
    [HideInInspector][UdonSynced (UdonSyncMode.None)] public bool gunFiring = false;
    public AudioSource gunAudio;
    public Animator gunAnimator;
    public float gunSpeed;
    public float overheatTime=10f;
    public float overheatTimer = 0f;
    public float overheatCool = 1f;
    public bool isOverheating;
    public GameObject gunIcon;
    [Header ("Internal Settings")]
    public float UpdateTendency = 0.1f;
    public float UpdateTimer = 0f;
    private int currentIndex = 0;
    private int prevSelected = -1;
    public bool selectedWeapon = false;

    void Start () { //Initialize Missile Packs and status
        if (LockSightHUD != null) {
            LockSightHUD.SetActive (false);
        }
        // if (FlareText != null) {
        //     FlareText.text = "FLARES: " + flaresNow;
        // }
        if (Missiles > 0) {
            MissileFired = new bool[Missiles];
            CooldownMissiles = new float[Missiles];
            for (int x = 0; x < MissileFired.Length; x++) {
                MissileFired[x] = false;
                CooldownMissiles[x] = 0;
            }
        }
        //Hide targets in case.
        if(!EngineController.Piloting || !EngineController.Passenger)//Just to make sure it isn't altered in the middle.
        foreach (GameObject go in MissileTargetScript.Targets) {
            if (go != null) {
                var TrackerObject = go.GetComponent<MissileTrackerAndResponse> ().TargetIconRender;
                TrackerObject.SetActive (false);
                // go.GetComponent<MissileTrackerAndResponse> ().MainObject.layer = 0;
                // go.layer = 0;
            }
        }
        selectedWeapon = false;
    }

    // public void callFlares(){
    //     if (EngineController.localPlayer == null)
    //                 flareSync ();
    //             SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "flareSync");
    // }

    public void leavePlane () {

        if (!isBomber) {

            if (!isGun && MissileFab != null && misTarget != null) {
                if (EngineController.localPlayer == null) {

                    StopTrackingSync ();
                } else {

                    SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
                }

                if (locking != null) { locking.Stop (); }

            }
        }
        if (isGun) {
            if (gunIcon != null) {
                gunIcon.SetActive (false);
            }
        }
        for (int bx = 0; bx < MissileTargetScript.Targets.Length; bx++) {
            var TrackerObject = MissileTargetScript.Targets[bx];
            if (TrackerObject != null)
                TrackerObject.GetComponent<MissileTrackerAndResponse> ().ShowTargets = false;
        }
    }

    // public void flareSync () {
    //     if (hasFlares && flareObject.Length > 0) {
    //         if (flaresNow > 0) {
    //             if (Tailer != null) {
    //                 flareCountdown = 0;
    //                 Tailer.GetComponent<BoxCollider> ().enabled = true;
    //                 TailerConstraint.constraintActive = false;
    //                 Tailer.GetComponent<Rigidbody> ().AddRelativeForce (Vector3.forward * -20);
    //                 // isFiredFlare = true;
    //                 flaresNow = flaresNow - 1;
    //                 if (FlareSound != null)
    //                     FlareSound.Play ();

    //                 if (flaresNow == 0) {
    //                     if (FlareText != null)
    //                         FlareText.text = "FLARES: EMPTY";
    //                 } else {
    //                     if (FlareText != null)
    //                         FlareText.text = "FLARES: " + flaresNow;
    //                 }
    //                 foreach (ParticleSystem fo in flareObject) {
    //                     // if (!isHeavyPlane) {
    //                     ParticleSystem.EmissionModule v = fo.emission;
    //                     v.enabled = true;
    //                     // }
    //                 }
    //             } else {
    //                 Debug.Log ("Put the Flare inside a Tailer, and Tailer must exist in the outside, and must be constrained with the parent Gameobject.");
    //             }
    //         }
    //     } else if (hasFlares && flareObject == null) {
    //         Debug.Log ("Hey wtf. Put the damn flare objects or else it wont work!");
    //     }
    // }

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

    // private void LateUpdate () {

    // }

    private void Update () {

        //Selected Target Syncer for all players
        if (selectedTargetIndex != -1 && misTarget != null) {
            if (MissileTargetScript.Targets[selectedTargetIndex] != misTarget.Target)
                misTarget.Target = MissileTargetScript.Targets[selectedTargetIndex];
            if (Networking.IsOwner (gameObject)) {
                if (isLocked) {
                    misTarget.noTarget = false;
                } else {
                    misTarget.noTarget = true;
                }
            }
        }

        if (EngineController.Health < 1 || EngineController.dead) {
            if (!deadExecute) {
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
                if (LockSightHUD != null) {
                    LockSightHUD.SetActive (false);
                }
                deadExecute = true;
            }
        } else if (EngineController.Health > 1) {
            if (deadExecute) {
                deadExecute = false;
            }
        }

        if (isGun && gunAnimator != null) {
            if (gunFiring) {
                gunAnimator.SetBool ("firing", true);
            } else {
                gunAnimator.SetBool ("firing", false);
            }
        }
        // if (flareObject.Length > 0) {
        //     if (flaresNow < flaresCount) {
        //         if (flareCountdown > returnTracker) {
        //             Tailer.GetComponent<BoxCollider> ().enabled = false;
        //             TailerConstraint.constraintActive = true;
        //             foreach (ParticleSystem fo in flareObject) {
        //                 ParticleSystem.EmissionModule v = fo.emission;
        //                 v.enabled = false;
        //             }
        //         }
        //         if (flareCountdown < flareCooldown) {
        //             flareCountdown = flareCountdown + Time.deltaTime;
        //         } else {
        //             flaresNow = flaresNow + 1;
        //             // isFiredFlare = false;
        //             flareCountdown = 0;
        //             if (FlareText != null)
        //                 FlareText.text = "FLARES: " + flaresNow;
        //         }
        //     }
        // }

        if (EngineController.Occupied) {
            if (!isGun && MissileFab != null) {
                for (int b = 0; b < Missiles; b++) {
                    if (CooldownMissileHUD != null && CooldownMissileHUD.Length > 0) {
                        if (MissileFired[b] == true) {
                            // if (CooldownMissileHUD[b].activeSelf)
                            for (int x = 0; x < CooldownMissileHUD[b].Length; x++) {
                                if(CooldownMissileHUD[b][x].activeSelf)
                                CooldownMissileHUD[b][x].SetActive (false);
                            }
                        } else if (MissileFired[b] == false) {
                            for (int x = 0; x < CooldownMissileHUD[b].Length; x++) {
                                if(!CooldownMissileHUD[b][x].activeSelf)
                                CooldownMissileHUD[b][x].SetActive (true);
                            }
                            // if (!CooldownMissileHUD[b].activeSelf)
                            // CooldownMissileHUD[b].SetActive (true);
                        }
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

            //Lock Logic, To be shown too as Copilot
            if (isLocking && timerLocking < timerToLock) {
                timerLocking = timerLocking + Time.deltaTime;
                if (timerLocking > timerToLock) {
                    isLocked = true;
                    misTarget.noTarget = false;
                    SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetLocked");
                    if (locking != null) {
                        locking.Stop ();
                    }
                    if (locked != null) {
                        locked.Play ();
                    }
                }
            }

            // if (isLocked) {
            //     misTarget.noTarget = false;
            // }

            if (isLocking) {
                if (frameLast < updateIntervals) {
                    frameLast = frameLast + Time.deltaTime;
                    updateCursor = false;
                } else if (frameLast > updateIntervals) {
                    frameLast = 0;
                    updateCursor = true;
                }
            }
        }

        //Cooldown time for the tgt indicators
        if (cooldownhud < 1f) {
            cooldownhud += Time.deltaTime;
        }
        if (cooldownhud > 1f) {
            if (NoTargetHUD != null && NoTargetHUD.activeSelf) {
                NoTargetHUD.SetActive (false);
            }
            if (TargetChangeHUD != null && TargetChangeHUD.activeSelf) {
                TargetChangeHUD.SetActive (false);
            }
            if (SelfTarget != null && SelfTarget.activeSelf) {
                SelfTarget.SetActive (false);
            }
        }
        // if(!EngineController.Piloting){
        //     misTarget.
        //     selectedTargetIndex = -1;

        // }

        if (EngineController.localPlayer == null || EngineController.Piloting || EngineController.Passenger) {
            tempShow = true;
            //Render Targets
            if (selectedTargetIndex != -1 &&
                (MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController != null &&
                    MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.Health <= 0 ||
                    (MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AI != null &&
                        MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AI.Health <= 0) ||
                    (MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AITurret != null &&
                        MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AITurret.Health <= 0))) {
                newChangeTargetCommand ();
            }
            if (showTargets) {
                if (selectedTargetIndex != -1 && MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> () != null) {
                    MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().isSelected = true;
                }
                if (prevSelected != -1 && MissileTargetScript.Targets[prevSelected].GetComponent<MissileTrackerAndResponse> () != null) {
                    MissileTargetScript.Targets[prevSelected].GetComponent<MissileTrackerAndResponse> ().isSelected = false;
                }

                if (MissileTargetScript.Targets[currentIndex] != null && (selectedTargetIndex != currentIndex) && MyTracker != MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ()) {
                    var TrackerObject = MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ();
                    TrackerObject.ShowTargets = true;

                    if (selectedTargetIndex != -1 && MissileTargetScript.Targets[selectedTargetIndex].gameObject != TrackerObject.gameObject) {
                        TrackerObject.isSelected = false;
                    }
                }
            } else {
                if (MissileTargetScript.Targets[currentIndex] != null) {
                    MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ().ShowTargets = false;
                    MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ().isSelected = false;
                }
            }
            if (currentIndex + 1 < MissileTargetScript.Targets.Length) {
                currentIndex = currentIndex + 1;
            } else {
                currentIndex = 0;
            }

            // if (showTargets){
            //     // foreach (GameObject go in MissileTargetScript.Targets) { //Local only update targets
            //         if (MissileTargetScript.Targets[currentIndex] != null) {
            //             var TrackerObject = MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ().TargetIconRender;
            //             var bc = MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ();
            //             if (showTargets && MissileTargetScript.Targets[currentIndex] != MyTracker && bc.isRendered) {
            //                 if (!TrackerObject.activeSelf) { TrackerObject.SetActive (true); }
            //                 // Transform tb = TrackerObject.GetComponent<Transform> ();
            //                 // tb.localRotation = Quaternion.Euler(tb.localRotation.x, tb.localRotation.y, vehicleMain.GetComponent<Transform>().rotation.z);
            //                 float dist = 0f;
            //                 if (EngineController.localPlayer != null) {
            //                     dist = Vector3.Distance (EngineController.localPlayer.GetPosition (), TrackerObject.GetComponent<Transform> ().position);
            //                 } else {
            //                     dist = Vector3.Distance (Player.position, TrackerObject.GetComponent<Transform> ().position);
            //                 }
            //                 TrackerObject.GetComponent<Transform> ().localScale = ScaleTarget * dist;
            //                 if (EngineController.localPlayer != null) {
            //                     TrackerObject.GetComponent<Transform> ().LookAt (EngineController.localPlayer.GetPosition ()); //Stick icon rotation to player
            //                 } else {
            //                     TrackerObject.GetComponent<Transform> ().LookAt (Player); //Stick icon rotation to player
            //                 }
            //                 var TrackerScript = MissileTargetScript.Targets[currentIndex].GetComponent<MissileTrackerAndResponse> ();
            //                 if (TrackerScript.TrackerText != null) {
            //                     var words = "";
            //                     if (TrackerScript.MainObject != null) { //Target Name
            //                         words = words + "" + TrackerScript.MainObject.name;
            //                     }
            //                     if (TrackerScript.EngineController != null) { // For Player Based Targets
            //                         if (TrackerScript.EngineController.Occupied && TrackerScript.EngineController.PilotName != null) { //Pilot Name
            //                             words = words + "\n" + TrackerScript.EngineController.PilotName;
            //                         }
            //                         if (TrackerScript.EngineController.Health > 0 && !TrackerScript.EngineController.dead) { //Health
            //                             words = words + "\nHP:" + TrackerScript.EngineController.Health;
            //                             if (TrackerObject.GetComponent<Renderer> ().material.color != Color.white && (selectedTargetIndex != -1 && MissileTargetScript.Targets[currentIndex] != MissileTargetScript.Targets[selectedTargetIndex])) {
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            //                             } else if (selectedTargetIndex != -1 && MissileTargetScript.Targets[currentIndex] == MissileTargetScript.Targets[selectedTargetIndex]) {
            //                                 if (TrackerObject.GetComponent<Renderer> ().material.color != Color.yellow)
            //                                     TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.yellow);
            //                             } else if (selectedTargetIndex == -1) {
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            //                             }
            //                         } else {
            //                             words = words + "\nDestroyed";
            //                             if (TrackerObject.GetComponent<Renderer> ().material.color != Color.red)
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
            //                         }

            //                     }
            //                     if (TrackerScript.AITurret != null) { // For Turret Based Targets

            //                         if (TrackerScript.AITurret.Health > 0 && TrackerScript.AITurret.damageable) { //Health
            //                             words = words + "\nHP:" + TrackerScript.AITurret.Health;
            //                             if (TrackerObject.GetComponent<Renderer> ().material.color != Color.white && (selectedTargetIndex != -1 && MissileTargetScript.Targets[currentIndex] != MissileTargetScript.Targets[selectedTargetIndex])) {
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            //                             } else if (selectedTargetIndex != -1 && MissileTargetScript.Targets[currentIndex] == MissileTargetScript.Targets[selectedTargetIndex]) {
            //                                 if (TrackerObject.GetComponent<Renderer> ().material.color != Color.yellow)
            //                                     TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.yellow);
            //                             } else if (selectedTargetIndex == -1) {
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            //                             }
            //                         } else {
            //                             words = words + "\nDestroyed";
            //                             if (TrackerObject.GetComponent<Renderer> ().material.color != Color.red)
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
            //                         }

            //                     }
            //                     if (TrackerScript.AI != null) { // For AI based targets
            //                         if (TrackerScript.AI.Health > 0 && TrackerScript.AI.damageable) { //Health
            //                             words = words + "\nHP:" + TrackerScript.AI.Health;
            //                             if (TrackerObject.GetComponent<Renderer> ().material.color != Color.white && (selectedTargetIndex != -1 && MissileTargetScript.Targets[currentIndex] != MissileTargetScript.Targets[selectedTargetIndex])) {
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            //                             } else if (selectedTargetIndex != -1 && MissileTargetScript.Targets[currentIndex] == MissileTargetScript.Targets[selectedTargetIndex]) {
            //                                 if (TrackerObject.GetComponent<Renderer> ().material.color != Color.yellow)
            //                                     TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.yellow);
            //                             } else if (selectedTargetIndex == -1) {
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);
            //                             }
            //                         } else if (TrackerScript.AI.Health <= 0) {
            //                             words = words + "\nDestroyed";
            //                             if (TrackerObject.GetComponent<Renderer> ().material.color != Color.red)
            //                                 TrackerObject.GetComponent<Renderer> ().material.SetColor ("_Color", Color.red);
            //                         }
            //                     }
            //                     //Distance
            //                     float distance = 0f;
            //                     if (TrackerScript.EngineController != null && TrackerScript.EngineController.localPlayer != null) {
            //                         distance = Vector3.Distance (EngineController.localPlayer.GetPosition (), TrackerObject.GetComponent<Transform> ().transform.position);
            //                     } else {
            //                         distance = Vector3.Distance (Player.transform.position, TrackerObject.GetComponent<Transform> ().transform.position);
            //                     }

            //                     words = words + "\n" + Mathf.Round (distance);
            //                     TrackerScript.TrackerText.text = words;
            //                 }

            //             } else {
            //                 if (TrackerObject.activeSelf) { TrackerObject.SetActive (false); }
            //             }
            //         }
            //     // }
            //     if(currentIndex + 1 < MissileTargetScript.Targets.Length){
            //         currentIndex = currentIndex + 1;
            //     }else{
            //         currentIndex = 0;
            //     }
            // }

        } else if ((!EngineController.Piloting || !EngineController.Passenger) && tempShow) {
            for (int bx = 0; bx < MissileTargetScript.Targets.Length; bx++) {
                var TrackerObject = MissileTargetScript.Targets[bx];
                if (TrackerObject != null)
                    TrackerObject.GetComponent<MissileTrackerAndResponse> ().ShowTargets = false;
            }
            // foreach (GameObject go in MissileTargetScript.Targets) { }
            tempShow = false;
        }
        // ^---- Very efficient programming, i know. =/

        if (LockSightHUD != null) { //rendering the lock sight
            float dist = 0f;

            if (isLocking || isLocked) {
                if (EngineController.localPlayer != null) {
                    LockSightHUD.transform.LookAt (EngineController.localPlayer.GetPosition ());
                    dist = Vector3.Distance (EngineController.localPlayer.GetPosition (), LockSightHUD.transform.position);
                    LockSightHUD.transform.localScale = new Vector3 (1, 1, 1) * dist;
                } else {
                    dist = Vector3.Distance (Player.position, LockSightHUD.transform.position);
                    LockSightHUD.transform.LookAt (Player);
                    LockSightHUD.transform.localScale = new Vector3 (1, 1, 1) * dist;
                }
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

        if((EngineController.localPlayer==null || EngineController.Piloting) && !selectedWeapon && isGun){
            if(gunIcon!=null && gunIcon.activeSelf){
                gunIcon.SetActive(false);
            }
        }

        if ((EngineController.localPlayer == null || EngineController.Piloting) && selectedWeapon) { // Only runs in pilot logic
            // if (Input.GetKeyDown (KeyCode.X) ) {
            //     callFlares();
            // }

            if (!isGun && MissileFab != null) {
                if (CanTarget && !isGun) {

                    //Locking Mechanism Logic begins here.
                    // int lastTarget = selectedTargetIndex;
                    if (UpdateTimer > UpdateTendency) {
                        UpdateTimer = 0f;
                        if (TargetDetector != null && selectedTargetIndex != -1) {
                            RaycastHit[] hit = Physics.SphereCastAll (TargetDetector.position, radius, TargetDetector.forward, range, layermask, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects
                            if (hit.Length > 0) {
                                bool found = false;
                                for (int x = 0; x < hit.Length; x++) {
                                    // if (hit[x].transform.gameObject.GetComponent<HitDetector> () != null) {
                                    //     if (hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker == misTarget.Target.GetComponent<MissileTrackerAndResponse> ()) {
                                    //         currentHitObject = hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker.gameObject;
                                    //         hitDistance = Vector3.Distance (EngineController.VehicleMainObj.transform.position, hit[x].transform.position);
                                    //         found = true;
                                    //         break;
                                    //     }
                                    // } else 
                                    if (hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse> () != null) {
                                        if (hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse> () == misTarget.Target.GetComponent<MissileTrackerAndResponse> ()) {
                                            currentHitObject = hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse> ().gameObject;
                                            hitDistance = Vector3.Distance (EngineController.VehicleMainObj.transform.position, hit[x].transform.position);
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
                                        var distance = Vector3.Distance (TargetDetector.position, bb.gameObject.transform.position);
                                        var angle = Vector3.Angle(TargetDetector.position, b1.Tailer!=null ? b1.Tailer.position : b1.transform.position);
                                        if ((bb != null) && (bb == b1) && (hitDistance > minimumRange) && distance > minimumRange && distance < range) {
                                            isLocking = true;
                                            locking.Play ();
                                            if (EngineController.localPlayer == null) {
                                                TrackingSync ();
                                            }
                                            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TrackingSync");
                                            // break;
                                        } else {
                                            if (isLocking) {
                                                isLocking = false;
                                                if (EngineController.localPlayer == null)
                                                    StopTrackingSync ();
                                                SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");
                                            }
                                            // if (lastTarget != -1) {
                                            //     MissileTargetScript.Targets[lastTarget].GetComponent<MissileTrackerAndResponse> ().isTracking = false;
                                            // }
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
                                    } else if (isLocking) {
                                        var b1 = misTarget.Target.GetComponent<MissileTrackerAndResponse> ();
                                        var distance = Vector3.Distance (TargetDetector.position, bb.gameObject.transform.position);
                                        if (distance < minimumRange || distance > range) {
                                            isLocking = false;
                                            isLocked = false;
                                            misTarget.noTarget = true;
                                            if (locking != null) {
                                                locking.Stop ();
                                            }
                                            if (locked != null) {
                                                locked.Stop ();
                                            }
                                            if (EngineController.localPlayer == null)
                                                StopTrackingSync ();
                                            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StopTrackingSync");

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
                    } else {
                        UpdateTimer = UpdateTimer + Time.deltaTime;
                    }
                }
                //End Locking mechanism

                //On Missile Fire
                if (MissileFab != null && Player != null) {
                    // RGrip = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryHandTrigger");
                    DPAD_RIGHT = Input.GetAxisRaw ("Joy1 Axis 6");
                    DPAD_DOWN = Input.GetAxisRaw ("Joy1 Axis 7");

                    //Change Target
                    if (CanTarget && (Input.GetKeyDown (KeyCode.Y) || DPAD_RIGHT > 0 || DPAD_RIGHT < 0) && (!DPAD_RIGHT_last_trigger)) {
                        if (DPAD_RIGHT > 0 || DPAD_RIGHT < 0) { DPAD_RIGHT_last_trigger = true; }
                        // if (RGrip > 0.5f) { RGriplastframetrigger = true; }
                        newChangeTargetCommand ();
                    }
                    if (Input.GetAxisRaw ("Joy1 Axis 6") == 0 && DPAD_RIGHT_last_trigger == true) {
                        DPAD_RIGHT_last_trigger = false;
                    }

                    //Fire Weapon
                    if ((Input.GetKeyDown (KeyCode.Space) || (Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger") >.5f || DPAD_DOWN < 0 || DPAD_DOWN > 0) && (buttonDown == false && DPAD_DOWN_last_trigger == false))) {
                        if (Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger") >.5f) { buttonDown = true; }
                        if (DPAD_DOWN < 0 || DPAD_DOWN > 0) { DPAD_DOWN_last_trigger = true; }
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "MissileSync");
                        if (EngineController.localPlayer == null) //so it works in editor
                        {
                            MissileSync ();
                        }
                    }
                    if (Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger") < .5f && buttonDown == true) {
                        buttonDown = false;
                    }
                    if (Input.GetAxisRaw ("Joy1 Axis 7") == 0 && DPAD_DOWN_last_trigger == true) {
                        DPAD_DOWN_last_trigger = false;
                    }
                } else {
                    // Debug.Log ("errr wtf");
                }
            } else if (isGun) {
                if (selectedTargetIndex != -1) {
                    Vector3 finalVectors;
                    Vector3 V;
                    var distance = Vector3.Distance (EngineController.VehicleMainObj.transform.position, MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().gameObject.transform.position);
                    if (MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController != null) {
                        V = MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.CurrentVel;
                    } else if (MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AI != null && MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AI.AIClass != null && MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AI.AIRigidBody != null) {
                        V = MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().AI.AIRigidBody.velocity;
                    } else {
                        V = Vector3.zero;
                    }
                    finalVectors = FirstOrderIntercept (EngineController.VehicleMainObj.transform.position, EngineController.VehicleMainObj.GetComponent<Rigidbody> ().velocity, gunSpeed, MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ().gameObject.transform.position, V);
                    if (gunIcon != null) {
                        if(!gunIcon.activeSelf)
                        gunIcon.SetActive (true);
                        var b = Vector3.Lerp (gunIcon.transform.position, finalVectors, LerpMultiplier * Time.deltaTime);
                        gunIcon.transform.position = b;
                        gunIcon.transform.LookAt (EngineController.VehicleMainObj.transform);
                        gunIcon.transform.localScale = ScaleTarget * distance;
                    }
                } else {
                    if(gunIcon.activeSelf)
                    gunIcon.SetActive (false);
                }
                // RGrip = Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryHandTrigger");
                // DPAD_RIGHT = Input.GetAxisRaw ("Joy1 Axis 6");
                // DPAD_DOWN = Input.GetAxisRaw ("Joy1 Axis 7");

                //Change Target
                if (CanTarget && (Input.GetKeyDown (KeyCode.Y) || DPAD_RIGHT > 0 || DPAD_RIGHT < 0) && (!DPAD_RIGHT_last_trigger)) {
                    if (DPAD_RIGHT > 0 || DPAD_RIGHT < 0) { DPAD_RIGHT_last_trigger = true; }
                    // if (RGrip > 0.5f) { RGriplastframetrigger = true; }
                    newChangeTargetCommand ();
                }
                if (Input.GetAxisRaw ("Joy1 Axis 6") == 0 && DPAD_RIGHT_last_trigger == true) {
                    DPAD_RIGHT_last_trigger = false;
                }

                //Fire Weapon
                if ((Input.GetKeyDown (KeyCode.Space) || (Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger") >.5f || DPAD_DOWN < 0 || DPAD_DOWN > 0) && (buttonDown == false && DPAD_DOWN_last_trigger == false))) {
                    if (Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger") >.5f) { buttonDown = true; }
                    if (DPAD_DOWN < 0 || DPAD_DOWN > 0) { DPAD_DOWN_last_trigger = true; }
                    // if (EngineController.localPlayer == null) //so it works in editor
                    // {
                    //     gunFiring = true;
                    // }
                    gunFiring = true;
                }
                if (Input.GetAxisRaw ("Oculus_CrossPlatform_SecondaryIndexTrigger") < .5f && buttonDown == true) {
                    buttonDown = false;
                    gunFiring = false;
                }
                if (Input.GetAxisRaw ("Joy1 Axis 7") == 0 && DPAD_DOWN_last_trigger == true) {
                    DPAD_DOWN_last_trigger = false;
                }
                if (Input.GetKeyUp (KeyCode.Space)) {
                    gunFiring = false;
                }
            }
        }
    }

    public void TrackingSync () {
        var targetToWarn = misTarget.Target;
        if (EngineController.localPlayer != null) {
            if (targetToWarn != null) {
                targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = true;
            }

        } else if (EngineController.localPlayer == null) {
            targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = true;
        }
    }

    public void newChangeTargetCommand () {
        if (EngineController.localPlayer == null || EngineController.Piloting) {
            prevSelected = selectedTargetIndex;
            int lastSelectedTarget = selectedTargetIndex;
            if (lastSelectedTarget != -1) {
                var bt = MissileTargetScript.Targets[selectedTargetIndex].GetComponent<MissileTrackerAndResponse> ();
                if (bt != null) {
                    bt.isTracking = false;
                }
            }
            if (MissileTargetScript.Targets.Length > 0) { //Target List Refresh
                RaycastHit[] hit = Physics.SphereCastAll (ChangeTargetDetector.position, changeTargetDetectorRadius, ChangeTargetDetector.forward, changeTargetDetectorRange, layermask, QueryTriggerInteraction.UseGlobal);
                if (hit.Length > 0) {
                    objects = hit;
                    TGTList = new MissileTrackerAndResponse[0];
                    targetIndices = new int[0];
                    for (int x = 0; x < hit.Length; x++) {
                        for (int mm = 0; mm < MissileTargetScript.Targets.Length; mm++) {
                            if (hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse> () != null) {
                                MissileTrackerAndResponse b = null;
                                // if (hit[x].transform.gameObject.GetComponent<HitDetector> () != null) {
                                //     if (hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker != null) {
                                //         b = hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker;
                                //     }
                                // }
                                if (hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse> () != null) {
                                    b = hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse> ();
                                } else {
                                    b = null;
                                }
                                if (b != null && b == MissileTargetScript.Targets[mm].GetComponent<MissileTrackerAndResponse> () && b != MyTracker.GetComponent<MissileTrackerAndResponse> () && b.isTargetable) {
                                    MissileTrackerAndResponse[] temp = new MissileTrackerAndResponse[TGTList.Length + 1];
                                    int[] tempTargetIndices = new int[targetIndices.Length + 1];
                                    for (int y = 0; y < temp.Length - 1; y++) {
                                        temp[y] = TGTList[y];
                                        tempTargetIndices[y] = targetIndices[y];
                                    }
                                    temp[temp.Length - 1] = b;
                                    tempTargetIndices[temp.Length - 1] = mm;
                                    targetIndices = tempTargetIndices;
                                    TGTList = temp;
                                }
                            }
                        }
                    }
                }
            }
            bool found = false;
            for (int x = 0; x < TGTList.Length; x++) {

                if (TGTLISTIndex + 1 < TGTList.Length && TGTList[TGTLISTIndex + 1] != null) {
                    TGTLISTIndex = TGTLISTIndex + 1;
                    found = true;
                    break;
                } else {
                    continue;
                }
            }

            if (found == false) {
                TGTLISTIndex = -1;
            }

            if (TGTLISTIndex == -1) {
                selectedTargetIndex = -1;
                isLocked = false;
                isLocking = false;
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
            } else {
                selectedTargetIndex = targetIndices[TGTLISTIndex];
                if (TargetChangeHUD != null) {
                    if (NextTarget != null)
                        NextTarget.Play ();
                    TargetChangeHUD.SetActive (true);
                    if (SelfTarget != null)
                        SelfTarget.SetActive (false);
                    if (NoTargetHUD != null)
                        NoTargetHUD.SetActive (false);
                    cooldownhud = 0;
                }
            }
        }
        if (EngineController.localPlayer == null) //editor mode. 
            changeTargetSync ();
        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "changeTargetSync");
        // Debug.DrawLine (ChangeTargetDetector.position, ChangeTargetDetector.position + ChangeTargetDetector.forward * hitDistance);
    }

    // public void ChangeTargetCommand () {
    //     if (EngineController.localPlayer == null || EngineController.Piloting) {
    //         if (MissileTargetScript.Targets.Length > 0) {
    //             for (;;) {
    //                 if (selectedTargetIndex + 1 != -1 && selectedTargetIndex + 1 < MissileTargetScript.Targets.Length) {

    //                     if (selectedTargetIndex + 1 != -1 && (MissileTargetScript.Targets[selectedTargetIndex + 1] != null || MissileTargetScript.Targets[selectedTargetIndex + 1].gameObject.activeInHierarchy != false) && MissileTargetScript.Targets[selectedTargetIndex + 1].GetComponent<MissileTrackerAndResponse> () != null && (MyTracker == MissileTargetScript.Targets[selectedTargetIndex + 1] || MissileTargetScript.Targets[selectedTargetIndex + 1].GetComponent<MissileTrackerAndResponse> ().isTargetable == false)) {
    //                         if (selectedTargetIndex + 1 < MissileTargetScript.Targets.Length) {
    //                             selectedTargetIndex = selectedTargetIndex + 1;
    //                         } else {
    //                             selectedTargetIndex = -2;
    //                         }
    //                         continue;
    //                     } else {
    //                         if (TargetChangeHUD != null) {
    //                             if (NextTarget != null)
    //                                 NextTarget.Play ();
    //                             TargetChangeHUD.SetActive (true);
    //                             if (SelfTarget != null)
    //                                 SelfTarget.SetActive (false);
    //                             cooldownhud = 0;
    //                         }
    //                     }
    //                 } else {
    //                     // selectedTargetIndex = -1;
    //                     if (NoTargets != null)
    //                         NoTargets.Play ();
    //                     if (NoTargetHUD != null) {
    //                         NoTargetHUD.SetActive (true);
    //                         cooldownhud = 0;
    //                     }
    //                     if (TargetChangeHUD != null) {
    //                         TargetChangeHUD.SetActive (false);
    //                     }
    //                     if (SelfTarget != null) {
    //                         SelfTarget.SetActive (false);
    //                     }
    //                 }

    //                 if (selectedTargetIndex + 1 < MissileTargetScript.Targets.Length) {
    //                     selectedTargetIndex = selectedTargetIndex + 1;
    //                 } else {
    //                     selectedTargetIndex = -1;
    //                 }

    //                 if (selectedTargetIndex != -1) {
    //                     break;
    //                 } else {
    //                     misTarget.Target = null;
    //                     misTarget.noTarget = true;
    //                     break;
    //                 }
    //             }

    //             if (EngineController.localPlayer == null) //editor mode. 
    //                 changeTargetSync ();
    //             SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "changeTargetSync");

    //             for (int i = 0; i < MissileTargetScript.Targets.Length; i++) {
    //                 if (MissileTargetScript.Targets[i] != null && MissileTargetScript.Targets[i].GetComponent<MissileTrackerAndResponse> () != null) {
    //                     var grender = MissileTargetScript.Targets[i].GetComponent<MissileTrackerAndResponse> ().TargetIconRender.GetComponent<Renderer> (); //If it crashes because of this, its either you haven't setup the correct tracker or the targets array.
    //                     if (i != selectedTargetIndex) {
    //                         grender.material.SetColor ("_Color", Color.white); //Not selected
    //                     } else {
    //                         grender.material.SetColor ("_Color", Color.yellow); //Selected Target
    //                     }
    //                 }
    //             }
    //         }
    //     }
    // }

    public void StopTrackingSync () {
        var targetToWarn = misTarget.Target;
        if (misTarget.Target != null) {
            if (EngineController.localPlayer != null) {
                if (targetToWarn != null) {
                    targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = false;
                }
            } else if (EngineController.localPlayer == null) {
                targetToWarn.GetComponent<MissileTrackerAndResponse> ().isTracking = false;
            }
        }
        misTarget.noTarget = true;
    }

    public void MissileSync () {
        // The next set of lines below basically what it does is it spawns the missile depending which pod is free if they're not on cooldown.
        // Debug.Log ("MissileSync Called");
        if (MissileSpawnAreas.Length > 0) {
            for (int i = 0; i < Missiles; i++) {
                if (MissileFired[i] == false) {
                    MissileFired[i] = true;
                    MissileRuntime = VRCInstantiate (MissileFab);

                    MissileRuntime.GetComponent<Rigidbody> ().velocity = EngineController.CurrentVel;
                    MissileRuntime.GetComponent<Collider> ().enabled = false;
                    if (launchArea + 1 < MissileSpawnAreas.Length) {
                        launchArea = launchArea + 1;
                    } else {
                        launchArea = 0;
                    }
                    MissileRuntime.GetComponent<Transform> ().localScale = SpawnScaleOffset;
                    MissileRuntime.GetComponent<Transform> ().position = MissileSpawnAreas[launchArea].GetComponent<Transform> ().position;
                    MissileRuntime.GetComponent<Transform> ().rotation = MissileSpawnAreas[launchArea].GetComponent<Transform> ().rotation;
                    if(SpawnParent!=null)   
                    MissileRuntime.GetComponent<Transform>().SetParent(SpawnParent);
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

    //Tools
    public Vector3 FirstOrderIntercept (
        Vector3 shooterPosition,
        Vector3 shooterVelocity,
        float shotSpeed,
        Vector3 targetPosition,
        Vector3 targetVelocity) {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        float t = FirstOrderInterceptTime (
            shotSpeed,
            targetRelativePosition,
            targetRelativeVelocity
        );
        return targetPosition + t * (targetRelativeVelocity);
    }

    public float FirstOrderInterceptTime (
        float shotSpeed,
        Vector3 targetRelativePosition,
        Vector3 targetRelativeVelocity) {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
            return 0f;

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs (a) < 0.001f) {
            float t = -targetRelativePosition.sqrMagnitude /
                (
                    2f * Vector3.Dot (
                        targetRelativeVelocity,
                        targetRelativePosition
                    )
                );
            return Mathf.Max (t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot (targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f) { //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt (determinant)) / (2f * a),
                t2 = (-b - Mathf.Sqrt (determinant)) / (2f * a);
            if (t1 > 0f) {
                if (t2 > 0f)
                    return Mathf.Min (t1, t2); //both are positive
                else
                    return t1; //only t1 is positive
            } else
                return Mathf.Max (t2, 0f); //don't shoot back in time
        } else if (determinant < 0f) //determinant < 0; no intercept path
            return 0f;
        else //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max (-b / (2f * a), 0f); //don't shoot back in time
    }
}