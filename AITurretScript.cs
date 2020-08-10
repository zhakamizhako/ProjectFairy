using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AITurretScript : UdonSharpBehaviour {
    public GameObject TurretBody; //body, rotate sideways
    public GameObject TurretAim; //Verticle Aim
    public float correctionWorld = -100f;
    public float correctionBody = -180f;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public float Health = 100;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public float fullHealth = 0;
    public GameObject Target;
    public AudioSource TurretFire;
    public MissileTrackerAndResponse TrackerObject;
    public TriggerScript onDestroy;
    public TriggerScript onHalfHealth;
    //If missile
    public GameObject MissileFab;
    public Transform[] MissileSpawnAreas;
    public AudioSource[] MissileFireSounds;
    //If CIWS
    public ParticleSystem ShootEffect;
    public ParticleSystem GunParticle;
    public ParticleSystem ExplosionEffect;
    public GameObject DeadFire;
    public float CooldownEachFire = 0f;
    public bool fireSoundPlayed = false;
    public bool isFiring = false;
    public float fireCooldown = 0;
    public Animator TurretAni;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool damageable = true;
    public bool revive = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public string mode = "idle";
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public string turretType = "CIWS"; // [CIWS, SAM, Flak, Cannon, etc.] 
    public float EulerSpreadFire = 0.6f;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool dead = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool shouldFire = false;
    [System.NonSerializedAttribute][HideInInspector] public VRCPlayerApi localPlayer;
    //Will wait for the new update to come. for now, its manual target assignment.
    public MissileTargets TargetListTemp;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public int currentTargetIndex = -1;
    public float timeToDisappear = 3f;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public int launchArea = 0;
    private float disappeartimer = 0f;
    // public float Range = 5000; //Set range. 
    private GameObject MissileRuntime;
    [System.NonSerializedAttribute][HideInInspector] bool initDamagable = false;
    [System.NonSerializedAttribute][HideInInspector] bool initTargetable = false;
    [System.NonSerializedAttribute][HideInInspector] bool initRendered = false;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public bool isFiringCiws = false;
    public string idTurret="";
    public AIObject mainAIObject;

    void Start () {
        fullHealth = Health;
        localPlayer = Networking.LocalPlayer;
        initDamagable = damageable;
        if (TrackerObject != null) {
            initTargetable = TrackerObject.isTargetable;
            initRendered = TrackerObject.isRendered;
        }
    }

    public void syncFireCIWS () {
        Debug.Log ("syncfireciws called");
        if (TurretAni != null) {
            TurretAni.SetTrigger ("fireciws");
        }
        if (TurretFire != null && !fireSoundPlayed) {
            TurretFire.Play ();
            fireSoundPlayed = true;
        }
    }

    public void syncFireMissile () {
        Debug.Log ("syncfiremissile called");
        // The next set of lines below basically what it does is it spawns the missile depending which pod is free if they're not on cooldown.
        // Debug.Log ("MissileSync Called");
        if (MissileSpawnAreas.Length > 0) {
            MissileRuntime = VRCInstantiate (MissileFab);
            // MissileRuntime.GetComponent<Rigidbody> ().velocity = PlaneOwner.velocity;
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
        } else {
            Debug.Log ("Someone forgot to place the spawn areas for the missiles. This script will not work without them.");
        }
    }

    public void syncFireFlak () {

    }

    public void syncStopFireCIWS () {
        Debug.Log ("StopFiring called");
        if (TurretAni != null && isFiringCiws) {
            TurretAni.SetTrigger ("stopfire");
            isFiringCiws = false;
        }
        if (TurretFire != null && fireSoundPlayed) {
            TurretFire.Stop ();
            fireSoundPlayed = false;
        }
    }

    void OnParticleCollision (GameObject other) {
        if (localPlayer == null) {
            hitDamage ();
        } else {

            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hitDamage");
        }
    }
    public void hitDamage () {

        if (localPlayer == null || localPlayer.IsOwner (gameObject)) {
            if (damageable)
                Health += -2;
        }
    }

    void Update () {
        if (Health < 1 && !dead) {
            dead = true;
            shouldFire = false;
            DeadFire.SetActive (true);
            Debug.Log ("A");
            syncStopFireCIWS ();
            if (TurretFire != null) {
                TurretFire.Stop ();
            }
        } else {
            dead = false;
        }
        if (Health < fullHealth * .50) {
            if (onHalfHealth != null) {
                onHalfHealth.run = true;
            }
        }
        //in case of sleeping
        if (dead) {
            if (DeadFire != null && !DeadFire.activeSelf) {
                DeadFire.SetActive (true);
            }
            if (disappeartimer < timeToDisappear) {
                disappeartimer = disappeartimer + Time.deltaTime;
            } else {
                if (TrackerObject != null) {
                    TrackerObject.isRendered = false;
                    TrackerObject.isTargetable = false;
                    TrackerObject.gameObject.layer = 0;
                }
            }
            if (onDestroy != null) {
                onDestroy.run = true;
            }
        }
        if (!dead) {
            if(mainAIObject!=null){//receive string, convert to string array and compare.
                if(mainAIObject.TargetString!=null){
                    string[] b = mainAIObject.TargetString.Split(';');
                    // Debug.Log(b);
                    for(int x=0;x<b.Length;x++){
                        string[] a = b[x].Split('=');
                        if(a[0]==idTurret){
                            currentTargetIndex = int.Parse(a[1]);
                        }
                    }
                }else{
                    currentTargetIndex = -1;
                }
            }
            if (DeadFire != null && DeadFire.activeSelf) {
                DeadFire.SetActive (false);
            }

            if (Target != null && TargetListTemp != null && currentTargetIndex == -1) {
                Target = null;
            }

            if (Target == null && TargetListTemp != null && currentTargetIndex != -1) {
                Debug.Log ("Target Index received");
                if (turretType == "CIWS") {
                    var bx = TargetListTemp.Targets[currentTargetIndex];
                    if (bx.GetComponent<MissileTrackerAndResponse> () != null) {
                        if (bx.GetComponent<MissileTrackerAndResponse> ().Tailer2 != null) {
                            Target = bx.GetComponent<MissileTrackerAndResponse> ().Tailer2.gameObject;
                        } else {
                            Target = bx.GetComponent<MissileTrackerAndResponse> ().Tailer.gameObject;
                        }
                    } else {
                        Target = bx.GetComponent<MissileTrackerAndResponse> ().gameObject;
                    }
                }
                if (turretType == "SAM") { Target = TargetListTemp.Targets[currentTargetIndex]; }
            }
            if (Target != null) {
                if (TurretBody != null && TurretAim != null) {
                    TurretAim.transform.LookAt (Target.transform, new Vector3 (0f, correctionWorld, 0f));
                    TurretBody.transform.LookAt (Target.transform, new Vector3 (0f, correctionWorld, 0f));
                    TurretBody.transform.eulerAngles = new Vector3 (correctionBody, TurretBody.transform.eulerAngles.y, 0);
                    if (EulerSpreadFire != 0) {
                        TurretAim.transform.eulerAngles = new Vector3 (
                            Random.Range (
                                TurretAim.transform.eulerAngles.x - EulerSpreadFire, TurretAim.transform.eulerAngles.x + EulerSpreadFire),
                            Random.Range (
                                TurretAim.transform.eulerAngles.y - EulerSpreadFire, TurretAim.transform.eulerAngles.y + EulerSpreadFire),
                            TurretAim.transform.eulerAngles.z);
                    }
                }

                if (shouldFire) {
                    if (turretType == "CIWS") {
                        if (!isFiringCiws) {
                            Debug.Log ("Firing CIWS");
                            if (localPlayer == null || Networking.IsOwner (gameObject)) {
                                if (localPlayer == null) { syncFireCIWS (); }
                                SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncFireCIWS");
                                isFiringCiws = true;
                            }
                        }

                    }
                    if (turretType == "FLAK") {
                        //Insert Missile fab, that explodes according to distance.
                    }
                    if (turretType == "SAM") {
                        if (localPlayer != null || Networking.IsOwner (gameObject)) {

                            if (!isFiring) {
                                isFiring = true;
                                fireCooldown = 0;
                                if (localPlayer == null)
                                    syncFireMissile ();
                                if (Networking.IsOwner (gameObject))
                                    SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncFireMissile");
                            }
                            if (isFiring && fireCooldown < CooldownEachFire) {
                                fireCooldown = fireCooldown + Time.deltaTime;
                            }
                            if (isFiring && fireCooldown > CooldownEachFire) {
                                isFiring = false;
                            }
                        }
                    }
                } else {
                    if (turretType == "CIWS") {
                        if (isFiringCiws) {
                            Debug.Log ("B");
                            if (localPlayer == null) { 
                            syncStopFireCIWS ();
                             }
                            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncStopFireCIWS");
                            isFiringCiws = false;
                        }
                    }
                    if (turretType == "FLAK") {

                    }
                    if (turretType == "SAM") {

                    }
                }
            } else {
                if (turretType == "CIWS") {
                    if (isFiringCiws) {
                        Debug.Log ("C");
                        if (localPlayer == null) { 
                        syncStopFireCIWS ();
                         }
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncStopFireCIWS");
                        isFiringCiws = false;
                    }

                }
                if (turretType == "FLAK") {

                }
                if (turretType == "SAM") {

                }
                // isFiring = false;
            }
        }
        if (revive) {
            Health = fullHealth;
            damageable = initDamagable;
            if (TrackerObject != null) {
                TrackerObject.isTargetable = initTargetable;
                TrackerObject.isRendered = initRendered;
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
            revive = false;
        }

    }
}