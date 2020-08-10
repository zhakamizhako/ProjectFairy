using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AIObject : UdonSharpBehaviour {
    public GameObject MainBody;
    public AITurretScript[] TurretScripts; // These are Targetable Turrets, and may and may not be destroyable.
    public AITurretScript[] MainTurrets; // These are non Targetable Turrets, and undestroyable unless if the aiobject is dead.
    public bool enableMainTurrets = true;
    public int enableMainTurretsOn = -1; // if -1, they're enabled by default. If specified a number, refers to how many turrets left before the AI enables these turrets.
    public bool setTargetableOnOutOfTurrets = true;
    public bool setDamagableOnOutOfTurrets = true;
    public Transform TargetDetector;
    public MissileTargets PredefinedTargets;
    public TriggerScript onDestroy;
    public TriggerScript onHalfHealth;
    public TriggerScript onHalfTurrets;
    public TriggerScript onDeadTurrets;
    public Animator AIObjectAnimator;
    public MissileTrackerAndResponse TrackerObject;
    public MissileTrackerAndResponse[] Waypoints; //????????????
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public float Health = 100;
    public float fullHealth = 0;
    public float radius = 7000;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool disabled = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool dead = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool damageable = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public string TargetString = "";
    private float timerDead = 0;
    public bool revive = false;
    public LayerMask layermask;
    public string type = "static"; //[ship, heavyair, air, static]
    public float disappearTrackerOn = 3f;
    public GameObject[] debugTargets;

    [System.NonSerializedAttribute][HideInInspector] bool initDamagable = false;
    [System.NonSerializedAttribute][HideInInspector] bool initTargetable = false;
    [System.NonSerializedAttribute][HideInInspector] bool initRendered = false;
    [System.NonSerializedAttribute][HideInInspector] bool initDisabled = false;
    [System.NonSerializedAttribute][HideInInspector] bool initEnableMainTurrets = false;
    private bool deadplay = false;
    public AudioSource deadSound;
    public int[] targetIndices;
    public float targetChangeTime = 5;
    private float targetChangeTimer = 0;
    bool notargetsCheck = false;
    [System.NonSerializedAttribute][HideInInspector] public VRCPlayerApi localPlayer;
    void Start () {
        fullHealth = Health;
        localPlayer = Networking.LocalPlayer;
        initDamagable = damageable;
        initDisabled = disabled;
        initEnableMainTurrets = initEnableMainTurrets;

        if (TrackerObject != null) {
            initRendered = TrackerObject.isRendered;
            initTargetable = TrackerObject.isTargetable;
        }
    }

    public void removeTargets () {
        TargetString = null;
    }
    public void hitDamage () {

        if (localPlayer == null || localPlayer.IsOwner (gameObject)) {
            if (damageable)
                Health += -1;
        }
    }

    void OnParticleCollision (GameObject other) {
        if (localPlayer == null) {
            hitDamage ();
        } else {
            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hitDamage");
        }
    }

    void Update () {
        if (!disabled) {
            if (!dead) {
                if (Health <= 0) {
                    dead = true;
                }
                if (Health < (fullHealth / 2)) {
                    if (onHalfHealth != null) {
                        if (Networking.IsOwner (gameObject))
                            onHalfHealth.run = true;
                    }
                }
                if (Networking.IsOwner (gameObject)) {
                    if (TurretScripts.Length > 0) {
                        int aliveTurrets = 0;
                        foreach (AITurretScript g in TurretScripts) {
                            if (!g.dead) { aliveTurrets = aliveTurrets + 1; }
                        }
                        if (aliveTurrets < enableMainTurretsOn + 1) {
                            enableMainTurrets = true;
                            if (onDeadTurrets != null) {
                                onDeadTurrets.run = true;
                            }
                        }
                        if (aliveTurrets < enableMainTurretsOn + 1) {
                            if (setDamagableOnOutOfTurrets) {
                                damageable = true;
                            }
                            if (setTargetableOnOutOfTurrets && TrackerObject != null) {
                                TrackerObject.isTargetable = true;
                            }
                        }
                        if (aliveTurrets < (TurretScripts.Length / 2)) {
                            if (onHalfTurrets != null) {
                                onHalfTurrets.run = true;
                            }
                        }
                    }

                    if (TargetDetector != null) {
                        RaycastHit[] hit = Physics.SphereCastAll (TargetDetector.position, radius, TargetDetector.forward, 5000, layermask, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects
                        if (hit.Length > 0) {
                            debugTargets = new GameObject[0];
                            targetIndices = new int[0];
                            for (int x = 0; x < hit.Length; x++) {
                                for (int mm = 0; mm < PredefinedTargets.Targets.Length; mm++) {
                                    if (hit[x].transform.gameObject.GetComponent<HitDetector> () != null && hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker != null && hit[x].transform.gameObject.GetComponent<HitDetector> ().Tracker.gameObject == PredefinedTargets.Targets[mm] && hit[x].transform.gameObject.GetComponent<HitDetector> ().EngineControl != null && hit[x].transform.gameObject.GetComponent<HitDetector> ().EngineControl.Health > 0) {
                                        GameObject[] temp = new GameObject[debugTargets.Length + 1];
                                        int[] tempTargetIndices = new int[debugTargets.Length + 1];
                                        for (int y = 0; y < temp.Length - 1; y++) {
                                            temp[y] = debugTargets[y];
                                            tempTargetIndices[y] = targetIndices[y];
                                        }
                                        temp[temp.Length - 1] = hit[x].transform.gameObject;
                                        tempTargetIndices[temp.Length - 1] = mm;
                                        targetIndices = tempTargetIndices;
                                        debugTargets = temp;
                                    }
                                }
                                Debug.DrawLine (TargetDetector.position, TargetDetector.position + TargetDetector.forward * hit[x].distance);
                            }
                        }
                    }

                    if (debugTargets.Length > 0) {
                        notargetsCheck = false;
                        if (targetChangeTimer < targetChangeTime) {
                            targetChangeTimer = targetChangeTimer + Time.deltaTime;
                        } else {
                            TargetString = "";
                            for (int c = 0; c < TurretScripts.Length; c++) {
                                // Networking.SetOwner (Networking.GetOwner (gameObject), TurretScripts[c].gameObject);
                                int m = Random.Range (0, targetIndices.Length);
                                TargetString = TargetString + TurretScripts[c].idTurret + "=" + targetIndices[m] + ";";
                                // TurretScripts[c].currentTargetIndex = targetIndices[m];
                                // if (
                                //     TurretScripts[c].TargetListTemp.Targets[TurretScripts[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> () != null &&
                                //     TurretScripts[c].TargetListTemp.Targets[TurretScripts[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController != null &&
                                //     TurretScripts[c].TargetListTemp.Targets[TurretScripts[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.localPlayer != null
                                // ) {
                                //     Transform[] childrena = TurretScripts[c].transform.GetComponentsInChildren<Transform>();
                                //     foreach (Transform child in childrena) {
                                //         Networking.SetOwner (Networking.GetOwner (gameObject), child.gameObject);
                                //     }
                                // //     Networking.SetOwner (Networking.GetOwner (TurretScripts[c].TargetListTemp.Targets[TurretScripts[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.gameObject), TurretScripts[c].gameObject);
                                // }
                            }
                            if (enableMainTurrets) {
                                for (int c = 0; c < MainTurrets.Length; c++) {
                                    int m = Random.Range (0, targetIndices.Length);
                                    TargetString = TargetString + MainTurrets[c].idTurret + "=" + targetIndices[m] + ";";
                                    // Networking.SetOwner (Networking.GetOwner (gameObject), TurretScripts[c].gameObject);
                                    // MainTurrets[c].currentTargetIndex = targetIndices[m];

                                    // if (
                                    //     MainTurrets[c].TargetListTemp.Targets[MainTurrets[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> () != null &&
                                    //     MainTurrets[c].TargetListTemp.Targets[MainTurrets[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController != null &&
                                    //     MainTurrets[c].TargetListTemp.Targets[MainTurrets[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.localPlayer != null
                                    // ) {
                                    //     // Transform[] children = MainTurrets[c].transform.GetComponentsInChildren<Transform>();
                                    //     // foreach (Transform child in children) {
                                    //     // Networking.SetOwner (Networking.GetOwner (MainTurrets[c].TargetListTemp.Targets[MainTurrets[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.gameObject), child.gameObject);    
                                    //     // }
                                    //     Networking.SetOwner (Networking.GetOwner (MainTurrets[c].TargetListTemp.Targets[MainTurrets[c].currentTargetIndex].GetComponent<MissileTrackerAndResponse> ().EngineController.gameObject), MainTurrets[c].gameObject);
                                    // }
                                }
                            }

                            targetChangeTimer = 0;
                        }
                    } else {
                        if (notargetsCheck == false) {
                            if (localPlayer == null) {
                                removeTargets ();
                            }
                            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "removeTargets");
                            notargetsCheck = true;
                        }
                    }
                }
            }
        }

        if (revive) {
            Health = fullHealth;
            damageable = initDamagable;
            disabled = initDisabled;
            enableMainTurrets = initEnableMainTurrets;
            if (TrackerObject != null) {
                TrackerObject.isRendered = initRendered;
                TrackerObject.isTargetable = initTargetable;
            }
            if (AIObjectAnimator != null) { AIObjectAnimator.SetTrigger ("alive"); }
            if (TurretScripts.Length > 0) {
                foreach (AITurretScript g in TurretScripts) {
                    g.revive = true;
                }
            }
            if (MainTurrets.Length > 0) {
                foreach (AITurretScript g in MainTurrets) {
                    g.revive = true;
                }
            }
            if (onDestroy != null) {
                onDestroy.ran = false;
                onDestroy.ranSync = false;
                onDestroy.stopped = false;
                onDestroy.currentX = 0;
                for (int x = 0; x < onDestroy.isRunning.Length; x++) {
                    onDestroy.isRunning[x] = false;
                }
            }
            if (onHalfHealth != null) {
                onHalfHealth.ran = false;
                onHalfHealth.ranSync = false;
                onHalfHealth.stopped = false;
                onHalfHealth.currentX = 0;
                for (int x = 0; x < onHalfHealth.isRunning.Length; x++) {
                    onHalfHealth.isRunning[x] = false;
                }
            }
            if (onHalfTurrets != null) {
                onHalfTurrets.ran = false;
                onHalfTurrets.ranSync = false;
                onHalfTurrets.stopped = false;
                onHalfTurrets.currentX = 0;
                for (int x = 0; x < onHalfTurrets.isRunning.Length; x++) {
                    onHalfTurrets.isRunning[x] = false;
                }
            }
            if (onDeadTurrets != null) {
                onDeadTurrets.ran = false;
                onDeadTurrets.ranSync = false;
                onDeadTurrets.stopped = false;
                onDeadTurrets.currentX = 0;
                for (int x = 0; x < onDeadTurrets.isRunning.Length; x++) {
                    onDeadTurrets.isRunning[x] = false;
                }
            }
            revive = false;
            dead = false;
            deadplay = false;
        }
        if (dead) {
            if (timerDead < disappearTrackerOn) {
                timerDead = timerDead + Time.deltaTime;
            }
            if (timerDead > disappearTrackerOn) {
                TrackerObject.isTargetable = false;
                TrackerObject.isRendered = false;
            }
            if (AIObjectAnimator != null) { AIObjectAnimator.SetTrigger ("dead"); }
            if (onDestroy != null) {
                if (Networking.IsOwner (gameObject))
                    onDestroy.run = true;
            }
            if (!deadplay) {
                if (Networking.IsOwner (gameObject)) {
                    foreach (AITurretScript g in TurretScripts) {
                        g.currentTargetIndex = -1;
                        g.Target = null;
                        g.Health = 0;
                    }
                    foreach (AITurretScript g in MainTurrets) {
                        g.currentTargetIndex = -1;
                        g.Target = null;
                        g.Health = 0;
                    }
                }
                if (deadSound != null) { deadSound.Play (); }
                deadplay = true;
            }
        }

    }

}