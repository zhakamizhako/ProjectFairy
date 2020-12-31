using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AIObject : UdonSharpBehaviour
{
    // public GameObject MainBody;
    public AITurretScript[] TurretScripts; // These are Targetable Turrets, and may and may not be destroyable.
    public AITurretScript[] MainTurrets; // These are non Targetable Turrets, and undestroyable unless if the aiobject is dead.
    [UdonSynced(UdonSyncMode.None)] public bool enableMainTurrets = true;
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
    [Header("Waypoint Settings")]
    public GameObject[] Waypoints; //????????????
    public bool shouldGoNextWaypoint = true;
    public bool AIPathGobackToOrigin = false; //useful for patrolling AI objects.
    [UdonSynced(UdonSyncMode.None)] public int currentWaypointIndex = 0;
    //Not implemented yet...
    // public int[] TriggerWaypoints; // index - ?, entry - Waypoint number
    // public TriggerScript[] WaypointTrigger; // index - TriggerWaypoint rep, Trigger action

    [UdonSynced(UdonSyncMode.None)] public float Health = 100;
    [System.NonSerializedAttribute] [HideInInspector] public float fullHealth = 0;
    public float radius = 7000;
    [UdonSynced(UdonSyncMode.None)] public bool disabled = false;
    public bool dead = false;
    public bool damageable = false;
    [System.NonSerializedAttribute] [HideInInspector] [UdonSynced(UdonSyncMode.None)] public string TargetString = "";
    private float timerDead = 0;
    public bool revive = false;
    public LayerMask TargetingLayermask;
    public string type = "static"; //[ship, heavyair, air, static]
    public float disappearTrackerOn = 3f;
    public GameObject[] debugTargets;
    public Rigidbody AIRigidBody;
    // public GameObject Flare;
    // public float FlareCooldown = 15f;
    // public float FlareTimer = 0f;
    // public bool hasFlares = true;
    // private bool flareReady = true;
    //Air AI Behaviour
    private float startmass = 0;
    public float deadMass = 0;

    public bool shouldAttack = false;
    private bool shouldEvade = false;
    private float attackRange = 1000f;
    public float movementSpeed = 200f;
    public float maxMovementSpeed = 300f;
    public float RotSpeed = 15;
    public float distanceToChangeWaypoint = 50f;
    public MissileTrackerAndResponse FollowObject;
    // public bool AirHasTarget = false;
    // private bool isTurning = false;
    //Add variable speed
    //Change movement speed to maximum speed
    //create something like current speed
    //add shouldAttack
    //add ShouldFollow
    //Match the speeds, and compare distance.
    //Lessen speed according to distance
    //Rotate sideways according to the amount of turn direction
    //add dead handler... probably turn slowly towards the ground
    //add dead cleanup if air.
    //work on handler on trigger-on-waypoint
    //respawn handler
    //add attack behaviour
    //add evasive behaviour
    //add patrol behaviour (raycast)
    //add attack targets
    //add ignore targets?
    //flare if missile'd
    //flare cooldown
    //Add weird behaviours for a 'heavy' aircraft. Possibly the same shit + turrets.
    [UdonSynced(UdonSyncMode.None)] public Vector3 posSync;
    [UdonSynced(UdonSyncMode.None)] public Quaternion rotationSync;
    [UdonSynced(UdonSyncMode.None)] public Vector3 veloSync;
    private Vector3 rotLerp = new Vector3(0, 0, 0);
    // private float b = 0f;
    public float pingPosEvery = 15f;
    private float pingPosTimer = 0f;
    private int dirB = 0;
    public float rollRate = 0.02f;
    [System.NonSerializedAttribute] [HideInInspector] bool initDamagable = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initTargetable = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initRendered = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initDisabled = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initEnableMainTurrets = false;
    private bool deadplay = false;
    public AudioSource[] deadSounds;
    public int[] targetIndices;
    public float targetChangeTime = 5;
    private float targetChangeTimer = 0;
    public bool isSingleTargetOnly = false;
    bool notargetsCheck = false;
    bool isTargetableCheck = false;
    public GameObject AIClass;
    // public float LockAngle = 180;
    [Header("Target Update Settings")]
    public float updateTendency = 0.5f;
    public float updateTimer = 0f;
    [Header("Movement Update Setings")]
    public float updateMovementTendency = 0.5f;
    public float updateMovementTimer = 0f;
    public Transform RespawnArea; //Only valid for Flying. maybe.
    public bool isRespawnable;
    public float RespawnTime = 10f;
    private float RespawnTimer = 0f;
    public bool isCrashing = false;
    public bool hasCrashed = false;
    public GameObject deadParticle;
    public bool dontchaseTarget = false;
    public bool shouldAttackOnSight = false;
    [Header("Sync Options")]
    [Header("Enable Sync Object Position in Udon when disabling shouldPing and enable Move in master.")]
    public bool shouldOnlyMoveInMaster = false;
    public bool shouldPing = true;
    public bool shouldFollowLeft = false;
    public bool shouldFollowRight = false;
    public float formDistanceSlowdown = 100f;
    public float currentmoveSpeed = 0f;
    public float targetFloatDistance = 40f;
    public float formMultiplier = 4f;

    public bool debug_INDISTANCESLOWDOWN = false;
    public bool debug_OUTDISTANCESLOWDOWN = false;
    public bool debug_WITHINTHRESHORD = false;

    private bool updated = true;
    private int targetUpdateIndex = 0;
    private int targetUpdateIndex2 = 0;
    public RaycastHit[] TargetDetectionList;

    // private bool initShouldAttack = false;

    [System.NonSerializedAttribute] [HideInInspector] public VRCPlayerApi localPlayer;
    void Start()
    {
        fullHealth = Health;
        localPlayer = Networking.LocalPlayer;
        initDamagable = damageable;
        initDisabled = disabled;
        initEnableMainTurrets = enableMainTurrets;
        // initShouldAttack = shouldAttack;


        if (TrackerObject != null)
        {
            initRendered = TrackerObject.isRendered;
            initTargetable = TrackerObject.isTargetable;
        }

        if (AIRigidBody != null)
        {
            startmass = AIRigidBody.mass;
        }

        // if (Waypoints.Length > 0) {
        //     b = Vector3.Angle (gameObject.transform.forward, (Waypoints[currentWaypointIndex].gameObject.transform.position - gameObject.transform.position));
        // }
        updateTimer = Random.Range(0, updateTendency);
        updateMovementTimer = Random.Range(0, updateMovementTendency);
        currentmoveSpeed = movementSpeed;
    }

    public void removeTargets()
    {
        TargetString = null;
    }

    public void pingPos()
    {
        if (!Networking.IsOwner(gameObject))
        {
            AIClass.transform.position = posSync;
            AIClass.transform.rotation = rotationSync;
            AIRigidBody.velocity = veloSync;
        }
    }

    public void fireFlare()
    {
        //reserved
    }
    public void hitDamage()
    {

        if (localPlayer == null || localPlayer.IsOwner(gameObject))
        {
            if (damageable)
                Health += -1;
        }
    }

    public void reviveAI()
    {
        revive = true;
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Damage call?");
        if (localPlayer == null)
        {
            hitDamage();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hitDamage");
        }
    }
    void FixedUpdate()
    {
        //??
    }
    void LateUpdate()
    {
        LateUpdateLogic();
        // if (updateTimer > updateTendency)
        // {
        //     updateTimer = 0;
        //     if ((Networking.IsOwner(gameObject) || localPlayer == null) && disabled == false && dead == false)
        //     {
        //         if (TargetDetector != null)
        //         {
        //             TargetDetectionList = Physics.SphereCastAll(TargetDetector.position, radius, TargetDetector.forward, 5000, TargetingLayermask, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects
        //             if (TargetDetectionList.Length > 0)
        //             {
        //                 debugTargets = new GameObject[0];
        //                 targetIndices = new int[0];
        //                 for (int x = 0; x < hit.Length; x++)
        //                 {
        //                     for (int mm = 0; mm < PredefinedTargets.Targets.Length; mm++)
        //                     {
        //                         if ((hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse>() != null && hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse>().AI != null && hit[x].collider.gameObject.GetComponent<MissileTrackerAndResponse>().AI.Health > 0 && PredefinedTargets.Targets[mm] == hit[x].collider.gameObject) ||
        //                             (hit[x].collider.gameObject.GetComponent<HitDetector>() != null && hit[x].collider.gameObject.GetComponent<HitDetector>().Tracker != null && hit[x].collider.gameObject.GetComponent<HitDetector>().Tracker.gameObject == PredefinedTargets.Targets[mm] && hit[x].collider.gameObject.GetComponent<HitDetector>().EngineControl != null && hit[x].collider.gameObject.GetComponent<HitDetector>().EngineControl.Health > 0))
        //                         {
        //                             GameObject[] temp = new GameObject[debugTargets.Length + 1];
        //                             int[] tempTargetIndices = new int[debugTargets.Length + 1];
        //                             debugTargets.CopyTo(temp, 0);
        //                             targetIndices.CopyTo(tempTargetIndices, 0);
        //                             temp[temp.Length - 1] = hit[x].collider.gameObject;
        //                             tempTargetIndices[temp.Length - 1] = mm;
        //                             targetIndices = tempTargetIndices;
        //                             debugTargets = temp;
        //                         }
        //                     }
        //                     // Debug.DrawLine (TargetDetector.position, TargetDetector.position + TargetDetector.forward * hit[x].distance);
        //                 }
        //             }
        //         }
        //     }
        // }
        // else
        // {
        //     updateTimer = updateTimer + Time.deltaTime;
        // }

    }


    void LateUpdateLogic()
    {
        if ((Networking.IsOwner(gameObject) || localPlayer == null) && disabled == false && dead == false && TargetDetector != null)
        {
            if (updateTimer > updateTendency && updated)
            {
                TargetDetectionList = Physics.SphereCastAll(TargetDetector.position, radius, TargetDetector.forward, 5000, TargetingLayermask, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects
                debugTargets = new GameObject[0];
                targetIndices = new int[0];
                targetUpdateIndex2 = 0;
                targetUpdateIndex = 0;
                if (TargetDetectionList.Length > 0)
                {
                    updateTimer = 0;
                    updated = false;
                }
            }
            else
            {
                if (updateTimer < updateTendency) { updateTimer = updateTimer + Time.deltaTime; }
                if (TargetDetectionList != null && TargetDetectionList.Length > 0 && targetUpdateIndex < TargetDetectionList.Length)
                {
                    if ((TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<MissileTrackerAndResponse>() != null && TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<MissileTrackerAndResponse>().AI != null && TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<MissileTrackerAndResponse>().AI.Health > 0 && PredefinedTargets.Targets[targetUpdateIndex2] == TargetDetectionList[targetUpdateIndex].collider.gameObject) ||
                        (TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<HitDetector>() != null && TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<HitDetector>().Tracker != null && TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<HitDetector>().Tracker.gameObject == PredefinedTargets.Targets[targetUpdateIndex2] && TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<HitDetector>().EngineControl != null && TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<HitDetector>().EngineControl.Health > 0))
                    {
                        GameObject[] temp = new GameObject[debugTargets.Length + 1];
                        int[] tempTargetIndices = new int[debugTargets.Length + 1];
                        debugTargets.CopyTo(temp, 0);
                        targetIndices.CopyTo(tempTargetIndices, 0);
                        temp[temp.Length - 1] = TargetDetectionList[targetUpdateIndex].collider.gameObject;
                        tempTargetIndices[temp.Length - 1] = targetUpdateIndex2;
                        targetIndices = tempTargetIndices;
                        debugTargets = temp;
                    }
                    if (targetUpdateIndex2 + 1 < PredefinedTargets.Targets.Length)
                    {
                        targetUpdateIndex2 = targetUpdateIndex2 + 1;
                    }
                    else
                    {
                        targetUpdateIndex2 = 0;
                        targetUpdateIndex = targetUpdateIndex + 1;
                    }
                }
                else
                {
                    updated = true;
                }
            }
        }
    }

    void deadLogic()
    {
        if (type == "air")
        {
            if (AIRigidBody != null)
            {
                AIRigidBody.mass = deadMass;
                if (updateMovementTimer > updateMovementTendency)
                {
                    updateMovementTimer = 0;
                    // Quaternion temp = Quaternion.Lerp(AIClass.transform.rotation, Quaternion.Euler(AIRigidBody.velocity), Time.deltaTime);
                    AIRigidBody.transform.rotation = Quaternion.LookRotation(AIRigidBody.velocity);
                }
                else
                {
                    updateMovementTimer = updateMovementTimer + Time.deltaTime;
                }
            }

        }
    }

    void MovementLogic()
    {
        if (!dead)
        {
            if (type == "air")
            {
                float finalMovementSpeed = currentmoveSpeed;

                if (shouldAttack && !dontchaseTarget && debugTargets.Length > 0)
                { //Attack Logic
                    shouldFollowLeft = false;
                    shouldFollowRight = false;
                    Vector3 finalVectors;
                    Vector3 V;
                    Vector3 targetPos;
                    // Transform targetTransform;
                    if (debugTargets[0].gameObject.GetComponent<HitDetector>() != null)
                    {
                        V = debugTargets[0].gameObject.GetComponent<HitDetector>().EngineControl.CurrentVel;
                        targetPos = debugTargets[0].gameObject.GetComponent<HitDetector>().Tracker.Tailer.position;
                    }
                    else if (debugTargets[0].gameObject.GetComponent<AIObject>() != null && debugTargets[0].gameObject.GetComponent<AIObject>().AIClass != null && debugTargets[0].gameObject.GetComponent<AIObject>().AIRigidBody != null)
                    {
                        V = debugTargets[0].gameObject.GetComponent<AIObject>().AIRigidBody.velocity;
                        targetPos = debugTargets[0].gameObject.GetComponent<AIObject>().TrackerObject.Tailer.transform.position;
                    }
                    else
                    {
                        V = Vector3.zero;
                        targetPos = debugTargets[0].gameObject.transform.position;
                    }
                    finalVectors = FirstOrderIntercept(AIClass.gameObject.transform.position, AIRigidBody.velocity, AIRigidBody.velocity.magnitude, targetPos, V);
                    moveLogc(finalVectors, finalVectors);

                    // var bb = Vector3.Angle (gameObject.transform.forward, (targetPos - gameObject.transform.position));
                    // // if (!isTurning && bb > 1) {
                    // Vector3 perp = Vector3.Cross (gameObject.transform.forward, targetPos);
                    // float dir = Vector3.Dot (perp, targetPos);
                    // if (dir > 0) {
                    //     // b = -b;
                    //     dirB = 1;
                    // } else if (dir < 0) {
                    //     // b = b;
                    //     dirB = -1;
                    // }
                    // rotLerp = Vector3.Lerp (rotLerp, new Vector3 (gameObject.transform.localRotation.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.y, (bb) * dirB), rollRate * Time.deltaTime);

                    // gameObject.transform.localRotation = Quaternion.Euler (gameObject.transform.localRotation.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.y, rotLerp.z);

                    // var ObjectToTargetVector = finalVectors - AIClass.transform.position;
                    // var AIForward = AIClass.transform.forward;
                    // var targetDirection = ObjectToTargetVector.normalized;
                    // var rotationAxis = Vector3.Cross (AIForward, targetDirection);
                    // var deltaAngle = Vector3.Angle (AIForward, targetDirection);

                    // AIClass.transform.Rotate (rotationAxis, Mathf.Min (RotSpeed * Time.deltaTime, deltaAngle), Space.World);
                }
                if ((!shouldAttack || (shouldAttack && dontchaseTarget)) && Waypoints != null && Waypoints.Length > 0 && FollowObject == null)
                { // Waypoint Logic
                    shouldFollowLeft = false;
                    shouldFollowRight = false;
                    moveLogc(Waypoints[currentWaypointIndex].gameObject.transform.position, Waypoints[currentWaypointIndex].gameObject.transform.position);
                    // var bb = Vector3.Angle (gameObject.transform.forward, (Waypoints[currentWaypointIndex].gameObject.transform.position - gameObject.transform.position));
                    // // if (!isTurning && bb > 1) {
                    // Vector3 perp = Vector3.Cross (gameObject.transform.forward, Waypoints[currentWaypointIndex].gameObject.transform.position);
                    // float dir = Vector3.Dot (perp, Waypoints[currentWaypointIndex].gameObject.transform.up);
                    // if (dir > 0.0) {
                    //     // b = -b;
                    //     dirB = 1;
                    // } else if (dir < 0) {
                    //     // b = b;
                    //     dirB = -1;
                    // }

                    // rotLerp = Vector3.Lerp (rotLerp, new Vector3 (gameObject.transform.localRotation.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.y, (bb) * dirB), rollRate * Time.deltaTime);

                    // gameObject.transform.localRotation = Quaternion.Euler (gameObject.transform.localRotation.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.y, rotLerp.z);
                    // var ObjectToTargetVector = Waypoints[currentWaypointIndex].gameObject.transform.position - gameObject.transform.position;
                    // var AIForward = gameObject.transform.forward;
                    // var targetDirection = ObjectToTargetVector.normalized;
                    // var rotationAxis = Vector3.Cross (AIForward, targetDirection);
                    // var deltaAngle = Vector3.Angle (AIForward, targetDirection);

                    // gameObject.transform.Rotate (rotationAxis, Mathf.Min (RotSpeed * Time.deltaTime, deltaAngle), Space.World);

                    if (shouldGoNextWaypoint)
                    { // if false, circle around the waypoint. 
                        if (Vector3.Distance(gameObject.transform.position, Waypoints[currentWaypointIndex].gameObject.transform.position) < distanceToChangeWaypoint)
                        {
                            if (currentWaypointIndex + 1 < Waypoints.Length)
                            {
                                currentWaypointIndex = currentWaypointIndex + 1;
                            }
                            else
                            {
                                if (AIPathGobackToOrigin)
                                {
                                    currentWaypointIndex = 0;
                                }
                            }
                            if (Networking.IsOwner(gameObject))
                            {
                                // SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "pingPos");
                            }
                            // isTurning = false;
                        }
                    }
                }
                else if (!shouldAttack && FollowObject && !dontchaseTarget)
                { //Follow Object Logic
                    if (shouldFollowLeft == false && shouldFollowRight == false)
                    { //decide whether to follow left or right
                        int xb = Random.Range(0, 1);
                        if (xb == 0)
                        {
                            shouldFollowLeft = true;
                            shouldFollowRight = false;
                        }
                        if (xb == 1)
                        {
                            shouldFollowLeft = false;
                            shouldFollowRight = true;
                        }
                    }
                    if (FollowObject.AI != null)
                    {
                        finalMovementSpeed = followLogic(FollowObject.AI.AIRigidBody, FollowObject);
                    }
                    else if (FollowObject.EngineController != null)
                    {
                        finalMovementSpeed = followLogic(FollowObject.EngineController.VehicleRigidbody, FollowObject);
                    }
                    currentmoveSpeed = finalMovementSpeed;
                }

                //Finally, Move the object

                if (shouldOnlyMoveInMaster && Networking.IsOwner(gameObject))
                { // Finally
                    AIRigidBody.GetComponent<ConstantForce>().relativeForce = new Vector3(0, 0, finalMovementSpeed);
                }
                else if (!shouldOnlyMoveInMaster)
                {
                    AIRigidBody.GetComponent<ConstantForce>().relativeForce = new Vector3(0, 0, finalMovementSpeed);
                }

                if (shouldPing)
                { // Ping Object's Location
                    posSync = gameObject.transform.position;
                    rotationSync = gameObject.transform.rotation;
                    veloSync = AIRigidBody.velocity;

                    if (pingPosTimer < pingPosEvery)
                    {
                        pingPosTimer = pingPosTimer + Time.deltaTime;
                    }
                    if (pingPosTimer > pingPosEvery)
                    {
                        pingPosTimer = 0f;
                        if (Networking.IsOwner(gameObject))
                        {
                            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "pingPos");
                        }
                    }

                }

            }
        }

    }

    public void moveLogc(Vector3 TargetPos, Vector3 up)
    {
        var bb = Vector3.Angle(gameObject.transform.forward, (TargetPos - gameObject.transform.position));
        // if (!isTurning && bb > 1) {
        Vector3 perp = Vector3.Cross(gameObject.transform.forward, TargetPos);
        float dir = Vector3.Dot(perp, up);
        if (dir > 0.0)
        {
            // b = -b;
            dirB = 1;
        }
        else if (dir < 0)
        {
            // b = b;
            dirB = -1;
        }

        rotLerp = Vector3.Lerp(rotLerp, new Vector3(gameObject.transform.localRotation.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.y, (bb) * dirB), rollRate * Time.deltaTime);

        gameObject.transform.localRotation = Quaternion.Euler(gameObject.transform.localRotation.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.y, rotLerp.z);
        var ObjectToTargetVector = TargetPos - gameObject.transform.position;
        var AIForward = gameObject.transform.forward;
        var targetDirection = ObjectToTargetVector.normalized;
        var rotationAxis = Vector3.Cross(AIForward, targetDirection);
        var deltaAngle = Vector3.Angle(AIForward, targetDirection);

        gameObject.transform.Rotate(rotationAxis, Mathf.Min(RotSpeed * Time.deltaTime, deltaAngle), Space.World);
    }

    public float followLogic(Rigidbody ObjectVelocity, MissileTrackerAndResponse Tracker)
    {
        float finalMovespeed = currentmoveSpeed;
        Transform Following = null;
        if (shouldFollowLeft)
        {
            Following = FollowObject.FollowL;
        }
        if (shouldFollowRight)
        {
            Following = FollowObject.FollowR;
        }

        if (Following == null)
        {
            Following = Tracker.Tailer != null ? Tracker.Tailer : Tracker.gameObject.transform;
        }

        if (Vector3.Distance(AIClass.transform.position, Following.position) < formDistanceSlowdown)
        {
            finalMovespeed = Mathf.Lerp(ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, finalMovespeed, Time.deltaTime * -Vector3.Distance(AIClass.transform.position, Following.position));
            debug_WITHINTHRESHORD = false;
            debug_INDISTANCESLOWDOWN = true;
            debug_OUTDISTANCESLOWDOWN = false;
            // finalMovespeed = finalMovespeed - 10f;
        }
        else if (Vector3.Distance(AIClass.transform.position, Following.position) > formDistanceSlowdown && finalMovespeed < maxMovementSpeed)
        {
            finalMovespeed = Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / Vector3.Distance(AIClass.transform.position, Following.position));
            debug_WITHINTHRESHORD = false;
            debug_INDISTANCESLOWDOWN = false;
            debug_OUTDISTANCESLOWDOWN = true;
            // finalMovespeed = finalMovespeed + 10f;
        }
        else if (Vector3.Distance(AIClass.transform.position, Following.position) < targetFloatDistance && (ObjectVelocity.velocity.magnitude + 10 < AIRigidBody.velocity.magnitude && ObjectVelocity.velocity.magnitude - 10 > AIRigidBody.velocity.magnitude))
        {
            finalMovespeed = Mathf.Lerp(ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, finalMovespeed, Time.deltaTime);
            debug_WITHINTHRESHORD = true;
            debug_INDISTANCESLOWDOWN = false;
            debug_OUTDISTANCESLOWDOWN = false;
        }

        moveLogc(Following.position, Following.up);

        return finalMovespeed;
    }
    public void setTargetableAI()
    {
        TrackerObject.isTargetable = true;
        damageable = true;
    }

    public void crash()
    {
        if (AIObjectAnimator != null)
        {
            AIObjectAnimator.SetTrigger("crash");
        }
    }
    void onCollisionEnter(Collision collision)
    {
        if (dead && !hasCrashed)
        {
            crash();
        }
    }

    void Update()
    {
        if (!disabled)
        {
            if (!dead)
            {

                //MovementLogic
                if (updateMovementTimer > updateMovementTendency)
                {
                    MovementLogic();
                    updateMovementTimer = 0;
                }
                else
                {
                    updateMovementTimer = updateMovementTimer + Time.deltaTime;
                }

                if (Health <= 0)
                {
                    dead = true;
                    if (deadParticle != null && !deadParticle.activeSelf) { deadParticle.SetActive(true); }
                    if (Networking.IsOwner(gameObject))
                        TargetString = null;
                    if (type == "air")
                    {
                        AIRigidBody.useGravity = true;
                    }
                }
                if (Health < (fullHealth / 2))
                {
                    if (onHalfHealth != null)
                    {
                        if (Networking.IsOwner(gameObject))
                            onHalfHealth.run = true;
                    }
                }

                if (Networking.IsOwner(gameObject) || localPlayer == null)
                {

                    if (type == "air")
                    {
                        if (debugTargets != null && debugTargets.Length == 0)
                        {
                            shouldAttack = false;
                        }
                        else if (debugTargets != null && debugTargets.Length > 0)
                        {
                            if (shouldAttackOnSight)
                            {
                                shouldAttack = true;
                            }
                        }
                    }

                    if (type == "static" || type == "ship" || type == "heavyair" || type == "air")
                    {
                        if (TurretScripts != null && TurretScripts.Length > 0)
                        {
                            int aliveTurrets = 0;
                            foreach (AITurretScript g in TurretScripts)
                            {
                                if (!g.dead) { aliveTurrets = aliveTurrets + 1; }
                            }
                            if (aliveTurrets < enableMainTurretsOn + 1)
                            {
                                enableMainTurrets = true;
                                if (onDeadTurrets != null)
                                {
                                    onDeadTurrets.run = true;
                                }
                            }

                            if (aliveTurrets < enableMainTurretsOn + 1)
                            {
                                if (setDamagableOnOutOfTurrets)
                                {
                                    damageable = true;
                                }
                                if (setTargetableOnOutOfTurrets && TrackerObject != null)
                                {
                                    if (isTargetableCheck == false)
                                    {
                                        TrackerObject.isTargetable = true;
                                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "setTargetableAI");
                                    }
                                }
                            }
                            if (aliveTurrets < (TurretScripts.Length / 2))
                            {
                                if (onHalfTurrets != null)
                                {
                                    onHalfTurrets.run = true;
                                }
                            }
                        }
                        if (!updated)
                        {

                            if (debugTargets != null && debugTargets.Length > 0)
                            {
                                notargetsCheck = false;
                                if (targetChangeTimer < targetChangeTime)
                                {
                                    targetChangeTimer = targetChangeTimer + Time.deltaTime;
                                }
                                else
                                {
                                    TargetString = "";
                                    for (int c = 0; c < TurretScripts.Length; c++)
                                    {
                                        var skip = false;
                                        // Networking.SetOwner (Networking.GetOwner (gameObject), TurretScripts[c].gameObject);
                                        if (!TurretScripts[c].canTarget)
                                        {
                                            skip = true;
                                        }
                                        if (!skip)
                                        {
                                            int m = 0;
                                            if (!isSingleTargetOnly)
                                            {
                                                m = Random.Range(0, targetIndices.Length);
                                            }
                                            else
                                            {
                                                m = 0;
                                            }
                                            TargetString = TargetString + TurretScripts[c].idTurret + "=" + targetIndices[m] + ";";
                                        }
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
                                    if (enableMainTurrets)
                                    {
                                        for (int c = 0; c < MainTurrets.Length; c++)
                                        {
                                            var skip = false;

                                            if (!MainTurrets[c].canTarget)
                                            {
                                                skip = true;
                                            }
                                            if (!skip)
                                            {
                                                int m = 0;
                                                if (!isSingleTargetOnly)
                                                {
                                                    m = Random.Range(0, targetIndices.Length);
                                                }
                                                else
                                                {
                                                    m = 0;
                                                }
                                                TargetString = TargetString + MainTurrets[c].idTurret + "=" + targetIndices[m] + ";";
                                            }
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
                            }
                            else
                            {
                                if (notargetsCheck == false)
                                {
                                    if (localPlayer == null)
                                    {
                                        removeTargets();
                                    }
                                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "removeTargets");
                                    notargetsCheck = true;
                                }
                            }
                        }

                    }
                }
            }
        }

        if (revive)
        {
            Health = fullHealth;
            damageable = initDamagable;
            disabled = initDisabled;
            enableMainTurrets = initEnableMainTurrets;
            if (deadParticle != null && deadParticle.activeSelf) { deadParticle.SetActive(false); }
            isTargetableCheck = false;
            if (TrackerObject != null)
            {
                TrackerObject.isRendered = initRendered;
                TrackerObject.isTargetable = initTargetable;
            }
            if (AIObjectAnimator != null) { AIObjectAnimator.SetTrigger("alive"); }
            if (TurretScripts.Length > 0)
            {
                foreach (AITurretScript g in TurretScripts)
                {
                    g.revive = true;
                }
            }
            if (MainTurrets.Length > 0)
            {
                foreach (AITurretScript g in MainTurrets)
                {
                    g.revive = true;
                }
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
            if (onHalfTurrets != null)
            {
                onHalfTurrets.ran = false;
                // onHalfTurrets.ranSync = false;
                onHalfTurrets.stopped = false;
                onHalfTurrets.currentX = 0;
                for (int x = 0; x < onHalfTurrets.isRunning.Length; x++)
                {
                    onHalfTurrets.isRunning[x] = false;
                }
            }
            if (onDeadTurrets != null)
            {
                onDeadTurrets.ran = false;
                // onDeadTurrets.ranSync = false;
                onDeadTurrets.stopped = false;
                onDeadTurrets.currentX = 0;
                for (int x = 0; x < onDeadTurrets.isRunning.Length; x++)
                {
                    onDeadTurrets.isRunning[x] = false;
                }
            }
            if (type == "air" || type == "heavyair")
            {
                AIRigidBody.mass = startmass;
                AIRigidBody.useGravity = false;
            }
            revive = false;
            dead = false;
            deadplay = false;
        }
        if (dead)
        {
            if (type == "air" || type == "heavyair")
            {
                deadLogic();
            }
            if (Health > 0)
            { // in case.
                dead = false;
            }
            if (!hasCrashed)
            {

            }
            if (isRespawnable)
            {
                if (RespawnTimer < RespawnTime)
                {
                    RespawnTimer = RespawnTimer + Time.deltaTime;
                }
                else
                {
                    // dead = false;
                    if (RespawnArea != null)
                    {
                        AIClass.transform.position = RespawnArea.position;
                        AIClass.transform.rotation = RespawnArea.rotation;
                        currentWaypointIndex = 0;
                        if (AIRigidBody != null)
                        {
                            AIRigidBody.velocity = Vector3.zero;
                        }
                        RespawnTimer = 0;
                    }
                    if (Networking.LocalPlayer == null) { revive = true; }
                    // revive = true;
                    if (Networking.IsOwner(gameObject))
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "reviveAI");
                    }
                }
            }
            if (timerDead < disappearTrackerOn)
            {
                // if (Networking.IsOwner (gameObject))
                timerDead = timerDead + Time.deltaTime;
            }
            else
            {
                if (TrackerObject != null)
                {
                    TrackerObject.isTargetable = false;
                    TrackerObject.isRendered = false;
                }
            }
            if (AIObjectAnimator != null) { AIObjectAnimator.SetTrigger("dead"); }
            if (onDestroy != null)
            {
                onDestroy.run = true;
            }
            if (!deadplay)
            {
                if (Networking.IsOwner(gameObject))
                {
                    foreach (AITurretScript g in TurretScripts)
                    {
                        g.currentTargetIndex = -1;
                        g.Target = null;
                        g.Health = 0;
                    }
                    foreach (AITurretScript g in MainTurrets)
                    {
                        g.currentTargetIndex = -1;
                        g.Target = null;
                        g.Health = 0;
                    }
                    TargetString = null;
                }
                if (deadSounds != null && deadSounds.Length > 0)
                {
                    int random = Random.Range(0, deadSounds.Length);
                    deadSounds[random].Play();
                }
                // if (Networking.IsOwner (gameObject))
                deadplay = true;
            }
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