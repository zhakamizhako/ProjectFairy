using UdonSharp;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AITurretScript : UdonSharpBehaviour
{
    public GameObject TurretBody; //body, rotate sideways
    public GameObject TurretAim; //Verticle Aim
    public float correctionWorld = -100f;
    public float correctionBody = -180f;
    [UdonSynced(UdonSyncMode.None)] public float Health = 100;
    [System.NonSerializedAttribute] [HideInInspector] public float fullHealth = 0;
    public MissileTrackerAndResponse Target;
    public AudioSource TurretFire;
    public MissileTrackerAndResponse TrackerObject;
    public TriggerScript onDestroy;
    public TriggerScript onHalfHealth;
    //If missile
    public GameObject MissileFab;
    public Transform[] MissileSpawnAreas;
    public AudioSource[] MissileFireSounds;
    public AudioSource[] ExplosionSounds;
    //If CIWS
    // public ParticleSystem ShootEffect;
    // public ParticleSystem GunParticle;
    // public ParticleSystem ExplosionEffect;
    public GameObject DeadFire;
    public GameObject Flare;
    public float FlareCooldown = 15f;
    public float FlareTimer = 0f;
    public bool hasFlares = true;
    private bool flareReady = true;
    public float CooldownEachFire = 0f;
    public bool fireSoundPlayed = false;
    public bool isFiring = false;
    public float fireCooldown = 1;
    public float burstTime = -1f; //infinite for no limit
    private float burstTimer = 0f;
    public float cooldownEachBursts = 0.5f;
    private bool coolingdownBurst = false;
    private bool explosionSoundPlayed = false;
    public Animator TurretAni;
    [UdonSynced(UdonSyncMode.None)] public bool damageable = true;
    public bool revive = false;
    public string turretType = "CIWS"; // [CIWS, SAM, Flak, Cannon, etc.] 
    public float EulerSpreadFire = 0.6f;
    public float BulletSpeed = 1000f;
    public bool dead = false;
    public bool shouldFire = true;
    [System.NonSerializedAttribute] [HideInInspector] public VRCPlayerApi localPlayer;
    //Will wait for the new update to come. for now, its manual target assignment.
    public MissileTargets TargetListTemp;
    [System.NonSerializedAttribute] [HideInInspector] public int currentTargetIndex = -1;
    public float timeToDisappear = 3f;
    [System.NonSerializedAttribute] [HideInInspector] [UdonSynced(UdonSyncMode.None)] public int launchArea = 0;
    private float disappeartimer = 0f;
    // public float Range = 5000; //Set range. 
    private GameObject MissileRuntime;
    [System.NonSerializedAttribute] [HideInInspector] bool initDamagable = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initTargetable = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initRendered = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initShouldFire = false;
    [System.NonSerializedAttribute] [HideInInspector] [UdonSynced(UdonSyncMode.None)] public bool isFiringCiws = false;
    public string idTurret = "";
    public AIObject mainAIObject;
    public Rigidbody mainAIRigidBody;
    private string aitargetstring = null;
    private bool changeTarget = false;
    //checker whether if turret has a line of sight
    public Transform LineOfSightDetector;
    public float lineOfSightTimer = 0.5f;
    public float lineOfSightTime = 0f;
    public float MaxRange = 7000;
    public float sphereRadius = 30;
    public LayerMask LineOfSightCollisionLayers;
    public bool hasFound = false;
    public bool LoSStatic = false;
    public bool canTarget = true;
    public GameObject DebugHit;
    public Transform DebugEstimate;
    public Transform SpawnParent;
    public float LockAngle = 35f;
    public bool isAngleSpecific = false;
    public bool WarnTargetOnLock = true;
    private bool isSamReady = false;
    public bool sleep = false;
    public float TimerLimitFlak = 3f;
    private float speedflak = 1000f;
    public float minmultiplier = 2f;
    public float maxmultiplier = 4f;

    void Start()
    {
        fullHealth = Health;
        localPlayer = Networking.LocalPlayer;
        initDamagable = damageable;
        if (TrackerObject != null)
        {
            initTargetable = TrackerObject.isTargetable;
            initRendered = TrackerObject.isRendered;
            initShouldFire = shouldFire;
        }
        if(turretType=="FLAK" && MissileFab!=null){
            speedflak = MissileFab.GetComponent<MissileScript>().missileSpeed;
        }
    }

    public void syncFireCIWS()
    {
        // Debug.Log ("syncfireciws called");
        if (TurretAni != null)
        {
            TurretAni.SetTrigger("fireciws");
        }
        if (TurretFire != null && !fireSoundPlayed)
        {
            TurretFire.Play();
            fireSoundPlayed = true;
        }
    }

    public void syncFireMissile()
    {
        // Debug.Log ("syncfiremissile called");
        // The next set of lines below basically what it does is it spawns the missile depending which pod is free if they're not on cooldown.
        // Debug.Log ("MissileSync Called");
        if (MissileSpawnAreas.Length > 0)
        {
            MissileRuntime = VRCInstantiate(MissileFab);
            // MissileRuntime.GetComponent<Rigidbody> ().velocity = PlaneOwner.velocity;
            MissileRuntime.GetComponent<Collider>().enabled = false;
            if (Networking.IsOwner(gameObject))
                if (launchArea + 1 < MissileSpawnAreas.Length)
                {
                    launchArea = launchArea + 1;
                }
                else
                {
                    launchArea = 0;
                }
            MissileRuntime.GetComponent<Transform>().position = MissileSpawnAreas[launchArea].GetComponent<Transform>().position;
            MissileRuntime.GetComponent<Transform>().rotation = MissileSpawnAreas[launchArea].GetComponent<Transform>().rotation;
            if (SpawnParent != null)
                MissileRuntime.GetComponent<Transform>().SetParent(SpawnParent);
            if (MissileFireSounds.Length > 0)
            {
                int rInt = Random.Range(0, MissileFireSounds.Length);
                MissileFireSounds[rInt].Play();
            }
            // if (isLocked) { MissileRuntime.GetComponent<MissileScript> ().ShouldTrack = true; } else { MissileRuntime.GetComponent<MissileScript> ().ShouldTrack = false; }
            // MissileRuntime.GetComponent<MissileScript> ().fired = true;
            MissileRuntime.SetActive(true);
        }
        else
        {
            // Debug.Log ("Someone forgot to place the spawn areas for the missiles. This script will not work without them.");
        }
    }

    public void syncFireFlak()
    {

    }

    public void syncStopFireCIWS()
    {
        // Debug.Log ("StopFiring called");
        if (TurretAni != null && isFiringCiws)
        {
            // TurretAni.SetTrigger("stopfire");
            TurretAni.SetBool("fireciws", false);
            if (Networking.IsOwner(gameObject))
                isFiringCiws = false;
        }
        if (TurretFire != null && fireSoundPlayed)
        {
            TurretFire.Stop();
            fireSoundPlayed = false;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (localPlayer == null)
        {
            hitDamage();
        }
        else
        {

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hitDamage");
        }
        Debug.LogError("PARTICLE CONTACT");
    }
    public void hitDamage()
    {

        if (localPlayer == null || localPlayer.IsOwner(gameObject))
        {
            if (damageable)
                Health += -2;
        }
    }

    void Update()
    {
        if (Health < 1 && !dead)
        {
            dead = true;
            if (!explosionSoundPlayed && ExplosionSounds != null && ExplosionSounds.Length > 0)
            {
                int r = Random.Range(0, ExplosionSounds.Length);
                ExplosionSounds[r].Play();
                explosionSoundPlayed = true;
            }
            if (Networking.IsOwner(gameObject))
            {
                shouldFire = false;
                currentTargetIndex = -1;
                // if (Networking.IsOwner (gameObject) || localPlayer == null) {
                // Debug.Log ("A");
                if (turretType == "CIWS")
                {
                    if (localPlayer == null) { syncStopFireCIWS(); }
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncStopFireCIWS");
                }
                // }
            }
            if (DeadFire != null)
                DeadFire.SetActive(true);
            if (TurretFire != null)
            {
                TurretFire.Stop();
            }
        }
        else if (Health > 1)
        {
            if (dead) { dead = false; explosionSoundPlayed = false; }
        }
        if (Health < fullHealth * .50)
        {
            if (onHalfHealth != null)
            {
                onHalfHealth.run = true;
            }
        }
        //in case of sleeping
        if (dead)
        {
            if (DeadFire != null && !DeadFire.activeSelf)
            {
                DeadFire.SetActive(true);
            }
            if (disappeartimer < timeToDisappear)
            {
                disappeartimer = disappeartimer + Time.deltaTime;
            }
            else
            {
                if (TrackerObject != null)
                {
                    TrackerObject.isRendered = false;
                    TrackerObject.isTargetable = false;
                    // TrackerObject.gameObject.layer = 0;
                }
            }
            if (Target != null)
            {
                Target.isTracking = false;
            }
            if (onDestroy != null)
            {
                onDestroy.run = true;
            }
        }
        if (!dead && !sleep)
        {
            //sync with everyone
            if (turretType == "CIWS" && TurretAni != null)
            {
                if (isFiringCiws)
                {
                    TurretAni.SetBool("fireciws", true);
                }
                else
                {
                    TurretAni.SetBool("fireciws", false);
                }
            }
            if (canTarget)
            {
                if (mainAIObject != null)
                { //receive string, convert to string array and compare.
                    if (mainAIObject.TargetString != null && aitargetstring != mainAIObject.TargetString)
                    {
                        aitargetstring = mainAIObject.TargetString;
                        string[] b = mainAIObject.TargetString.Split(';');
                        // Debug.Log(b);
                        for (int x = 0; x < b.Length; x++)
                        {
                            string[] a = b[x].Split('=');
                            if (a[0] == idTurret)
                            {
                                if (int.Parse(a[1]) != currentTargetIndex)
                                {
                                    currentTargetIndex = int.Parse(a[1]);
                                    changeTarget = true;
                                }
                            }
                        }
                    }
                    else if (mainAIObject.TargetString == null)
                    {
                        currentTargetIndex = -1;
                    }
                }
            }
            if (DeadFire != null && DeadFire.activeSelf)
            {
                DeadFire.SetActive(false);
            }

            if (Target != null && TargetListTemp != null && currentTargetIndex == -1)
            {
                Target.isTracking = false;
                Target = null;
                aitargetstring = null;
                isFiringCiws = false;
                if (TurretAni != null)
                {
                    TurretAni.SetBool("fireciws", false);
                }
                // targetVelocity = Vector3.zero;
            }

            if (localPlayer == null || Networking.IsOwner(gameObject))
            {
                if (!mainAIObject.shouldAttack)
                {
                    shouldFire = false;
                }
                else if (mainAIObject.shouldAttack)
                {
                    shouldFire = true;
                }
            }

            if (changeTarget && TargetListTemp != null && currentTargetIndex != -1)
            {
                changeTarget = false;
                // Debug.Log ("Target Index received");
                if (turretType == "CIWS")
                {
                    var bx = TargetListTemp.Targets[currentTargetIndex];
                    if (bx.GetComponent<MissileTrackerAndResponse>() != null)
                    {
                        // if (bx.GetComponent<MissileTrackerAndResponse> ().EngineController != null) {
                        //     targetVelocity = bx.GetComponent<MissileTrackerAndResponse> ().EngineController.CurrentVel;
                        // } else if (bx.GetComponent<MissileTrackerAndResponse> ().AI != null && bx.GetComponent<MissileTrackerAndResponse> ().AI.AIClass != null && bx.GetComponent<MissileTrackerAndResponse> ().AI.AIRigidBody != null) {
                        //     targetVelocity = bx.GetComponent<MissileTrackerAndResponse> ().AI.AIRigidBody.velocity;
                        // }
                        if (bx.GetComponent<MissileTrackerAndResponse>().MainObject != null)
                        {
                            Target = bx.GetComponent<MissileTrackerAndResponse>();
                        }
                    }
                    else
                    {
                        Target = bx.GetComponent<MissileTrackerAndResponse>();
                    }
                }
                if (turretType == "SAM") { Target = TargetListTemp.Targets[currentTargetIndex]; }
                if (turretType == "FLAK") { Target = TargetListTemp.Targets[currentTargetIndex]; }
            }
            if (Target != null)
            {
                if (TurretBody != null && TurretAim != null)
                {
                    Vector3 finalVectors;
                    Vector3 V = Vector3.zero;
                    Vector3 currVelocity;
                    if (mainAIRigidBody != null)
                    {
                        currVelocity = mainAIRigidBody.velocity;
                    }
                    else
                    {
                        currVelocity = Vector3.zero;
                    }
                    if (currentTargetIndex != -1 && TargetListTemp.Targets[currentTargetIndex] != null &&
                        TargetListTemp.Targets[currentTargetIndex].EngineController != null
                    )
                    {
                        V = TargetListTemp.Targets[currentTargetIndex].EngineController.CurrentVel;
                    }
                    else if (currentTargetIndex != -1 && TargetListTemp.Targets[currentTargetIndex] != null &&
                      TargetListTemp.Targets[currentTargetIndex].AI != null && TargetListTemp.Targets[currentTargetIndex].AI.AIRigidBody != null)
                    {
                        V = TargetListTemp.Targets[currentTargetIndex].AI.veloSync;
                    }
                    else
                    {
                        V = Vector3.zero;
                    }
                    if (turretType == "LAZER" && coolingdownBurst)
                    {
                        finalVectors = FirstOrderIntercept(TurretBody.transform.position, currVelocity, BulletSpeed, Target.transform.position, V);
                        TurretAim.transform.LookAt(finalVectors, new Vector3(0f, correctionWorld, 0f));
                        TurretBody.transform.LookAt(finalVectors, new Vector3(0f, correctionWorld, 0f));
                        TurretBody.transform.eulerAngles = new Vector3(correctionBody, TurretBody.transform.eulerAngles.y, 0);
                    }
                    else
                    {
                        finalVectors = FirstOrderIntercept(TurretBody.transform.position, currVelocity, BulletSpeed, Target.transform.position, V);
                        TurretAim.transform.LookAt(finalVectors, new Vector3(0f, correctionWorld, 0f));
                        TurretBody.transform.LookAt(finalVectors, new Vector3(0f, correctionWorld, 0f));
                        TurretBody.transform.eulerAngles = new Vector3(correctionBody, TurretBody.transform.eulerAngles.y, 0);
                    }
                    if ((turretType == "CIWS" && isFiringCiws && EulerSpreadFire != 0))
                    {
                        TurretAim.transform.eulerAngles = new Vector3(
                            Random.Range(
                                TurretAim.transform.eulerAngles.x - EulerSpreadFire, TurretAim.transform.eulerAngles.x + EulerSpreadFire),
                            Random.Range(
                                TurretAim.transform.eulerAngles.y - EulerSpreadFire, TurretAim.transform.eulerAngles.y + EulerSpreadFire),
                            TurretAim.transform.eulerAngles.z);
                    }
                    if (DebugEstimate != null)
                    {
                        DebugEstimate.position = finalVectors;
                    }
                }

                if (shouldFire)
                {
                    if (turretType == "CIWS")
                    {
                        if (LineOfSightDetector != null)
                        {

                            if (lineOfSightTimer > lineOfSightTime)
                            {
                                lineOfSightTimer = 0f;
                                RaycastHit hit;
                                if (!LoSStatic)
                                {
                                    LineOfSightDetector.LookAt(Target.transform.position);
                                    Physics.Raycast(LineOfSightDetector.position, LineOfSightDetector.forward, out hit, MaxRange, LineOfSightCollisionLayers, QueryTriggerInteraction.Collide);
                                }
                                else
                                {
                                    Physics.SphereCast(LineOfSightDetector.position, sphereRadius, LineOfSightDetector.forward, out hit, MaxRange, LineOfSightCollisionLayers, QueryTriggerInteraction.Collide);
                                }
                                bool found = false;
                                if (hit.collider != null)
                                {
                                    DebugHit = hit.transform.gameObject;
                                    GameObject hitting = hit.transform.gameObject;
                                    Debug.DrawRay(LineOfSightDetector.position, LineOfSightDetector.position + LineOfSightDetector.forward * hit.distance, Color.yellow);
                                    bool forced = false;
                                    if (hitting.GetComponent<MissileTrackerAndResponse>() != null)
                                    {
                                        if (hitting.GetComponent<MissileTrackerAndResponse>().MainObject == null)
                                        {
                                            forced = true;
                                        }
                                    }
                                    if (hitting.transform == Target.transform)
                                    {
                                        found = true;
                                    }
                                    else if (forced)
                                    {
                                        found = true;
                                    }
                                }
                                else
                                {
                                    Debug.DrawRay(LineOfSightDetector.position, LineOfSightDetector.position + LineOfSightDetector.forward * MaxRange, Color.white);
                                    DebugHit = null;
                                }
                                if (found)
                                {
                                    isFiringCiws = true;
                                    hasFound = true;
                                }
                                else
                                {
                                    isFiringCiws = false;
                                    hasFound = false;
                                }
                            }
                            else
                            {
                                lineOfSightTimer = lineOfSightTimer + Time.deltaTime;
                            }
                        }
                        // if (isFiringCiws) {
                        if (!coolingdownBurst && burstTimer > burstTime)
                        {
                            burstTimer = 0f;
                            coolingdownBurst = true;
                        }
                        else
                        {
                            burstTimer = burstTimer + Time.deltaTime;
                        }
                        if (coolingdownBurst && burstTimer > cooldownEachBursts)
                        {
                            coolingdownBurst = false;
                            burstTimer = 0f;
                        }
                        else
                        {
                            burstTimer = burstTimer + Time.deltaTime;
                        }

                        // if (!isFiringCiws) {
                        //     // Debug.Log ("Firing CIWS");
                        //     if (localPlayer == null || Networking.IsOwner (gameObject)) {
                        //         if (localPlayer == null) { syncFireCIWS (); }
                        //         SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncFireCIWS");
                        //         isFiringCiws = true;
                        //     }
                        // }
                        if (isFiringCiws && !coolingdownBurst)
                        {
                            if (TurretAni != null) { }
                            TurretAni.SetBool("fireciws", true);
                            if (TurretFire != null && !fireSoundPlayed)
                            {
                                TurretFire.Play();
                                fireSoundPlayed = true;
                            }
                        }
                        else if (!isFiringCiws || coolingdownBurst)
                        {
                            if (TurretAni != null)
                                TurretAni.SetBool("fireciws", false);
                            if (TurretFire != null && fireSoundPlayed)
                            {
                                TurretFire.Stop();
                                fireSoundPlayed = false;
                            }
                        }

                    }
                    if (turretType == "LAZER")
                    {
                        if (LineOfSightDetector != null)
                        {

                            if (lineOfSightTimer > lineOfSightTime)
                            {
                                lineOfSightTimer = 0f;
                                RaycastHit hit;
                                if (!LoSStatic)
                                {
                                    LineOfSightDetector.LookAt(Target.transform.position);
                                    Physics.Raycast(LineOfSightDetector.position, LineOfSightDetector.forward, out hit, MaxRange, LineOfSightCollisionLayers, QueryTriggerInteraction.Collide);
                                }
                                else
                                {
                                    Physics.SphereCast(LineOfSightDetector.position, sphereRadius, LineOfSightDetector.forward, out hit, MaxRange, LineOfSightCollisionLayers, QueryTriggerInteraction.Collide);
                                }
                                bool found = false;
                                if (hit.collider != null)
                                {
                                    DebugHit = hit.transform.gameObject;
                                    GameObject hitting = hit.transform.gameObject;
                                    Debug.DrawRay(LineOfSightDetector.position, LineOfSightDetector.position + LineOfSightDetector.forward * hit.distance, Color.yellow);
                                    bool forced = false;
                                    if (hitting.GetComponent<MissileTrackerAndResponse>() != null)
                                    {
                                        if (hitting.GetComponent<MissileTrackerAndResponse>().MainObject == null)
                                        {
                                            forced = true;
                                        }
                                    }
                                    if (hitting.transform == Target.transform)
                                    {
                                        found = true;
                                    }
                                    else if (forced)
                                    {
                                        found = true;
                                    }
                                }
                                else
                                {
                                    Debug.DrawRay(LineOfSightDetector.position, LineOfSightDetector.position + LineOfSightDetector.forward * MaxRange, Color.white);
                                    DebugHit = null;
                                }
                                if (found)
                                {
                                    isFiringCiws = true;
                                    hasFound = true;
                                }
                                else
                                {
                                    isFiringCiws = false;
                                    hasFound = false;
                                }
                            }
                            else
                            {
                                lineOfSightTimer = lineOfSightTimer + Time.deltaTime;
                            }
                        }
                        // if (isFiringCiws) {
                        if (!coolingdownBurst && burstTimer > burstTime)
                        {
                            burstTimer = 0f;
                            coolingdownBurst = true;
                        }
                        else
                        {
                            burstTimer = burstTimer + Time.deltaTime;
                        }
                        if (coolingdownBurst && burstTimer > cooldownEachBursts)
                        {
                            coolingdownBurst = false;
                            burstTimer = 0f;
                        }
                        else
                        {
                            burstTimer = burstTimer + Time.deltaTime;
                        }
                        if (isFiringCiws && !coolingdownBurst)
                        {
                            if (TurretAni != null) { }
                            TurretAni.SetBool("fireciws", true);
                            if (TurretFire != null && !fireSoundPlayed)
                            {
                                TurretFire.Play();
                                fireSoundPlayed = true;
                            }
                        }
                        else if (!isFiringCiws || coolingdownBurst)
                        {
                            if (TurretAni != null)
                                TurretAni.SetBool("fireciws", false);
                            if (TurretFire != null && fireSoundPlayed)
                            {
                                TurretFire.Stop();
                                fireSoundPlayed = false;
                            }
                        }

                    }
                    if (turretType == "FLAK")
                    {
                        float distanceTarget = Vector3.Distance(LineOfSightDetector.position, Target.gameObject.transform.position);
                        float randa = Random.Range(minmultiplier, maxmultiplier);
                        TimerLimitFlak = (distanceTarget / speedflak) * randa ;
                        if (localPlayer == null || Networking.IsOwner(gameObject))
                        {
                            if (isAngleSpecific)
                            {
                                var ObjectToTargetVector = Target.gameObject.transform.position - LineOfSightDetector.transform.position;
                                var AIForward = LineOfSightDetector.forward;
                                var targetDirection = ObjectToTargetVector.normalized;
                                if (Vector3.Angle(AIForward, targetDirection) < LockAngle)
                                {
                                    isSamReady = true;
                                    // Target.isTracking = true;
                                }
                                else
                                {
                                    isSamReady = false;
                                    // Target.isTracking = false;
                                }
                            }
                            else
                            {
                                isSamReady = true;
                            }

                            if (!isFiring && isSamReady)
                            {
                                isFiring = true;
                                fireCooldown = 0;
                                if (Networking.IsOwner(gameObject) || localPlayer == null)
                                    if (localPlayer == null) { syncFireMissile(); }
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncFireMissile");
                            }
                            if (isFiring && fireCooldown < CooldownEachFire)
                            {
                                fireCooldown = fireCooldown + Time.deltaTime;
                            }
                            if (isFiring && fireCooldown > CooldownEachFire)
                            {
                                isFiring = false;
                            }
                        }
                    }
                    if (turretType == "static")
                    {
                        //well, static.
                    }
                    if (turretType == "SAM")
                    {
                        if (localPlayer == null || Networking.IsOwner(gameObject))
                        {
                            if (isAngleSpecific)
                            {
                                var ObjectToTargetVector = Target.gameObject.transform.position - LineOfSightDetector.transform.position;
                                var AIForward = LineOfSightDetector.forward;
                                var targetDirection = ObjectToTargetVector.normalized;
                                if (Vector3.Angle(AIForward, targetDirection) < LockAngle)
                                {
                                    isSamReady = true;
                                    Target.isTracking = true;
                                }
                                else
                                {
                                    isSamReady = false;
                                    Target.isTracking = false;
                                }
                            }
                            else
                            {
                                isSamReady = true;
                            }

                            if (!isFiring && isSamReady)
                            {
                                isFiring = true;
                                fireCooldown = 0;
                                if (Networking.IsOwner(gameObject) || localPlayer == null)
                                    if (localPlayer == null) { syncFireMissile(); }
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncFireMissile");
                            }
                            if (isFiring && fireCooldown < CooldownEachFire)
                            {
                                fireCooldown = fireCooldown + Time.deltaTime;
                            }
                            if (isFiring && fireCooldown > CooldownEachFire)
                            {
                                isFiring = false;
                            }
                        }
                    }
                }
                else
                {
                    if (Networking.IsOwner(gameObject) || localPlayer == null)
                    {
                        if (turretType == "CIWS")
                        {
                            if (isFiringCiws)
                            {
                                // Debug.Log ("B");
                                if (localPlayer == null)
                                {
                                    syncStopFireCIWS();
                                }
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncStopFireCIWS");
                                isFiringCiws = false;
                                TurretAni.SetBool("fireciws", false);
                            }
                        }
                        if (turretType == "FLAK")
                        {

                        }
                        if (turretType == "SAM")
                        {

                        }
                    }
                    if (TurretAni != null)
                    {
                        isFiringCiws = false;
                        TurretAni.SetBool("fireciws", false);
                    }
                    if (Target != null)
                    {
                        Target.isTracking = false;
                    }


                }
            }
            else
            {
                if (Networking.IsOwner(gameObject) || localPlayer == null)
                {
                    if (turretType == "CIWS")
                    {
                        if (isFiringCiws)
                        {
                            // Debug.Log ("B");
                            if (localPlayer == null)
                            {
                                syncStopFireCIWS();
                            }
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "syncStopFireCIWS");
                            isFiringCiws = false;

                        }
                    }
                    if (turretType == "FLAK")
                    {

                    }
                    if (turretType == "SAM")
                    {

                    }
                }
            }
        }
        if (revive)
        {
            Health = fullHealth;
            damageable = initDamagable;
            shouldFire = initShouldFire;
            if (TrackerObject != null)
            {
                TrackerObject.isTargetable = initTargetable;
                TrackerObject.isRendered = initRendered;
            }
            if (onDestroy != null)
            {
                onDestroy.ran = false;
                // onDestroy.ranSync = false;
                onDestroy.stopped = false;
                onDestroy.currentX = 0;
                for (int x = 0; x < onDestroy.isRunning.Length; x++)
                {
                    onDestroy.isRunning[x] = false;
                }
            }
            if (onHalfHealth != null)
            {
                onHalfHealth.ran = false;
                // onHalfHealth.ranSync = false;
                onHalfHealth.stopped = false;
                onHalfHealth.currentX = 0;
                for (int x = 0; x < onHalfHealth.isRunning.Length; x++)
                {
                    onHalfHealth.isRunning[x] = false;
                }
            }
            revive = false;
        }

    }

    //Tools
    public Vector3 FirstOrderIntercept(
        Vector3 shooterPosition,
        Vector3 shooterVelocity,
        float shotSpeed,
        Vector3 targetPosition,
        Vector3 targetVelocity)
    {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        float t = FirstOrderInterceptTime(
            shotSpeed,
            targetRelativePosition,
            targetRelativeVelocity
        );
        return targetPosition + t * (targetRelativeVelocity);
    }
    public float FirstOrderInterceptTime(
        float shotSpeed,
        Vector3 targetRelativePosition,
        Vector3 targetRelativeVelocity)
    {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
            return 0f;

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -targetRelativePosition.sqrMagnitude /
                (
                    2f * Vector3.Dot(
                        targetRelativeVelocity,
                        targetRelativePosition
                    )
                );
            return Mathf.Max(t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        { //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
            {
                if (t2 > 0f)
                    return Mathf.Min(t1, t2); //both are positive
                else
                    return t1; //only t1 is positive
            }
            else
                return Mathf.Max(t2, 0f); //don't shoot back in time
        }
        else if (determinant < 0f) //determinant < 0; no intercept path
            return 0f;
        else //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
    }
}