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
    public TriggerScript EngageTrigger;
    public TriggerScript[] EngageTriggers;
    public TriggerScript[] onDestroys;

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
    [UdonSynced(UdonSyncMode.None)] public string TargetString = "";
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
    public bool disableAfterDead = false;
    private float disableTimer = 0;
    public float disableTime = 15f;
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
    [UdonSynced(UdonSyncMode.Smooth)] public Vector3 posSync;
    [UdonSynced(UdonSyncMode.Smooth)] public Quaternion rotationSync;
    [UdonSynced(UdonSyncMode.Smooth)] public Vector3 veloSync;
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
    [System.NonSerializedAttribute] [HideInInspector] public float startDragSpeed = 0f;
    [System.NonSerializedAttribute] [HideInInspector] public float startAngularDrag = 0f;
    private bool deadplay = false;
    public AudioSource[] deadSounds;
    public int[] targetIndices;
    public float targetChangeTime = 5;
    public float targetChangeTimer = 0;
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
    public bool useSmoothingSync = false;
    public bool uselocalPosition = false;

    public bool shouldFollowLeft = false;
    public bool shouldFollowRight = false;
    public float formDistanceSlowdown = 100f;
    public float currentmoveSpeed = 0f;
    public float targetFloatDistance = 40f;
    public float formMultiplier = 4f;

    public bool debug_INDISTANCESLOWDOWN = false;
    public bool debug_OUTDISTANCESLOWDOWN = false;
    public bool debug_WITHINTHRESHORD = false;

    public bool updated = true;
    private int targetUpdateIndex = 0;
    private int targetUpdateIndex2 = 0;
    public RaycastHit[] TargetDetectionList;
    private ConstantForce AiConstantForce;
    private bool turretStandby = false;
    public bool disableTurretsOnStandby = true;

    //Cost saving measure in order to save for other turrets.
    public GameObject MissileFab;
    public Transform[] MissileSpawnArea;

    public float distanceTooClose = 100f;
    public float distanceTooCloseGround = 800f;
    public float distanceExit = 800f;
    public Transform GroundOffset;
    public bool tooClose = false;
    public bool cleanupFollowObjectWhenDead = true;

    private bool ranTriggerHalf = false;
    private bool ranTriggerDead = false;
    private bool ranTriggerDeadTurrets = false;

    public int aliveTurretsPrivate = 0;
    public int turretindex = 0;
    // public bool turretCheck = false;
    private float damageReceived = 0f;
    private Transform AIClassTransform;
    private Transform GameObjectTransform;
    private bool triggerDeadTurretsRan = false;
    private bool triggerDeadTriggerRan = false;
    private bool triggerHalfTurretRan = false;
    private bool triggerHalfHealthRan = false;
    private bool onDestroyRan = false;
    private bool isInFollow = false;
    // public float output = 0f;
    // public float output2 = 0f;
    // public float output3 = 0f;
    // public float output4 = 0f;
    // public float outputSolve = 0f;
    // public float outputSolve2 = 0f;
    // public float outputSolve3 = 0f;
    // public float outputSolve4 = 0f;

    public float CurrentVelocity = 0f;
    public float TargetVelocity = 0f;
    public GameObject SmokeObject;

    // private bool initShouldAttack = false;

    [System.NonSerializedAttribute] [HideInInspector] public VRCPlayerApi localPlayer;
    void Start()
    {
        fullHealth = Health;
        localPlayer = Networking.LocalPlayer;
        initDamagable = damageable;
        initDisabled = disabled;
        initEnableMainTurrets = enableMainTurrets;
        aliveTurretsPrivate = 0;
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
        if (AIRigidBody != null)
        {
            AiConstantForce = AIRigidBody.GetComponent<ConstantForce>();
        }
        AIClassTransform = AIClass.transform;
        GameObjectTransform = gameObject.transform;
        if (type == "air")
        {
            startDragSpeed = AIRigidBody != null ? AIRigidBody.drag : 0f;
            startAngularDrag = AIRigidBody != null ? AIRigidBody.drag : 0f;
        }

    }

    public void removeTargets()
    {
        TargetString = null;
    }

    public void pingPos()
    {
        if (!Networking.IsOwner(gameObject))
        {
            if (uselocalPosition)
            {
                AIClassTransform.localPosition = posSync;
            }
            else
            {
                AIClassTransform.position = posSync;
            }

            AIClassTransform.rotation = rotationSync;
            AIRigidBody.velocity = veloSync;
        }
    }

    public void SmokeOn(){
        SmokeObject.SetActive(true);
    }

    public void SmokeOff(){
        SmokeObject.SetActive(false);
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

    public void DestroyAI()
    {
        if (Networking.IsOwner(gameObject) || Networking.LocalPlayer == null)
            Health = 0;
    }

    void OnParticleCollision(GameObject other)
    {
        // Debug.Log("Damage call?");
        // var getch =  other.GetComponent<GunParticle>();
        // if(other!=null && getch!=null && getch.fromShot!=null )
        if (localPlayer == null)
        {
            hitDamage();
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hitDamage");
        }
    }
    // void FixedUpdate()
    // {

    // }
    void LateUpdate()
    {
        LateUpdateLogic();
        if (disableTurretsOnStandby) checkStandby();
    }

    void checkStandby()
    {
        if (TargetString == null || TargetString == "")
        {
            if (!turretStandby)
            {
                if (TurretScripts != null && TurretScripts.Length > 0)
                    foreach (AITurretScript x in TurretScripts)
                    {
                        x.sleep = true;
                    }
                if (MainTurrets != null && MainTurrets.Length > 0)
                    foreach (AITurretScript x in MainTurrets)
                    {
                        x.sleep = true;
                    }
                turretStandby = true;
            }
        }
        else
        {
            if (turretStandby)
            {
                foreach (AITurretScript x in TurretScripts)
                {
                    x.sleep = false;
                }
                foreach (AITurretScript x in MainTurrets)
                {
                    x.sleep = false;
                }
                turretStandby = false;
            }
        }
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
                    MissileTrackerAndResponse currentSelection = TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<MissileTrackerAndResponse>() != null ? TargetDetectionList[targetUpdateIndex].collider.gameObject.GetComponent<MissileTrackerAndResponse>() : null;
                    if (currentSelection != null && ((currentSelection.AI != null && currentSelection.AI.Health > 0) || (currentSelection.AITurret != null && currentSelection.AITurret.Health > 0) || (currentSelection.EngineController != null && currentSelection.EngineController.Health > 0 && currentSelection.EngineController.Occupied)) && currentSelection == PredefinedTargets.Targets[targetUpdateIndex2])
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
                    // Quaternion temp = Quaternion.Lerp(AIClassTransform.rotation, Quaternion.Euler(AIRigidBody.velocity), Time.deltaTime);
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

                // Chase Target Logic
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
                    if ((AIClassTransform.position.y - (GroundOffset != null ? GroundOffset.position.y : 0f) < distanceTooCloseGround) || Vector3.Distance(AIClassTransform.position, targetPos) < distanceTooClose)
                    {
                        tooClose = true;
                    }
                    else if (tooClose && ((AIClassTransform.position.y - (GroundOffset != null ? GroundOffset.position.y : 0f) > distanceExit) || Vector3.Distance(AIClassTransform.position, targetPos) > distanceExit))
                    {
                        tooClose = false;
                    }


                    finalVectors = !tooClose ? FirstOrderIntercept(AIClass.gameObject.transform.position, AIRigidBody.velocity, AIRigidBody.velocity.magnitude, targetPos, V) : FirstOrderIntercept(AIClass.gameObject.transform.position, AIRigidBody.velocity, AIRigidBody.velocity.magnitude, new Vector3(AIClass.gameObject.transform.position.x, 10000f, AIClass.gameObject.transform.position.z), AIRigidBody.velocity);
                    moveLogc(finalVectors, finalVectors);
                }

                if ((!shouldAttack || (shouldAttack && dontchaseTarget)) && Waypoints != null && Waypoints.Length > 0 && FollowObject == null)
                { // Waypoint Logic
                    shouldFollowLeft = false;
                    shouldFollowRight = false;
                    moveLogc(Waypoints[currentWaypointIndex].gameObject.transform.position, Waypoints[currentWaypointIndex].gameObject.transform.position);
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

                    if (cleanupFollowObjectWhenDead && (FollowObject.AI != null && FollowObject.AI.Health < 1) || (FollowObject.EngineController != null && (FollowObject.EngineController.dead || FollowObject.EngineController.Health < 0)))
                    {
                        FollowObject = null;
                    }
                }

                //Finally, Move the object

                if (shouldOnlyMoveInMaster && Networking.IsOwner(gameObject))
                { // Finally
                    AiConstantForce.relativeForce = new Vector3(0, 0, currentmoveSpeed);
                }
                else if (!shouldOnlyMoveInMaster)
                {
                    AiConstantForce.relativeForce = new Vector3(0, 0, currentmoveSpeed);
                }

                if (shouldPing && !useSmoothingSync)
                { // Ping Object's Location
                    if (uselocalPosition)
                    {
                        posSync = GameObjectTransform.localPosition;
                    }
                    else
                    {
                        posSync = GameObjectTransform.position;
                    }
                    rotationSync = GameObjectTransform.rotation;
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
                if (useSmoothingSync && shouldPing)
                {
                    posSync = uselocalPosition ? GameObjectTransform.localPosition : GameObjectTransform.position;
                    rotationSync = GameObjectTransform.rotation;
                    veloSync = AIRigidBody.velocity;
                }

                veloSync = AIRigidBody.velocity;

            }
        }

    }

    public void moveLogc(Vector3 TargetPos, Vector3 up)
    {
        var bb = Vector3.Angle(GameObjectTransform.forward, (TargetPos - GameObjectTransform.position));
        // if (!isTurning && bb > 1) {
        Vector3 perp = Vector3.Cross(GameObjectTransform.forward, TargetPos);
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

        rotLerp = Vector3.Lerp(rotLerp, new Vector3(GameObjectTransform.localRotation.eulerAngles.x, GameObjectTransform.localRotation.eulerAngles.y, (bb) * dirB), rollRate * Time.deltaTime);

        GameObjectTransform.localRotation = Quaternion.Euler(GameObjectTransform.localRotation.eulerAngles.x, GameObjectTransform.localRotation.eulerAngles.y, rotLerp.z);
        var ObjectToTargetVector = TargetPos - GameObjectTransform.position;
        var AIForward = GameObjectTransform.forward;
        var targetDirection = ObjectToTargetVector.normalized;
        var rotationAxis = Vector3.Cross(AIForward, targetDirection);
        var deltaAngle = Vector3.Angle(AIForward, targetDirection);

        GameObjectTransform.Rotate(rotationAxis, Mathf.Min(RotSpeed * Time.deltaTime, deltaAngle), Space.World);
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

        float dist = Vector3.Distance(AIClassTransform.position, Following.position);
        CurrentVelocity = AIRigidBody.velocity.magnitude;
        TargetVelocity = ObjectVelocity.velocity.magnitude;

        if (dist < formDistanceSlowdown && dist > targetFloatDistance)
        {
            float temp = 0f;
            if (CurrentVelocity > TargetVelocity)
            {
                temp = -Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f;
            }
            else
            {
                temp = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f; ;
            }

            finalMovespeed = Mathf.Clamp(finalMovespeed + temp, 0, maxMovementSpeed);
            // if (currentmoveSpeed < maxMovementSpeed)
            // {
            // float temp = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime * 0.01f;
            // float temp = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime * 0.1f;!!
            // temp = +Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime;
            // output = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime;
            // finalMovespeed = finalMovespeed + temp;!!
            // float force = (CurrentVelocity * AIRigidBody.mass * TargetVelocity);
            // force = (AIRigidBody.drag * force) / (1 - 0.02f * AIRigidBody.drag);

            // finalMovespeed = force;
            // }
            // else
            // {
            //     finalMovespeed = currentmoveSpeed - 2f;
            // }
            debug_WITHINTHRESHORD = false;
            debug_INDISTANCESLOWDOWN = true;
            debug_OUTDISTANCESLOWDOWN = false;
            // if (!isInFollow)
            // {
            //     AIRigidBody.angularDrag = startAngularDrag;
            //     AIRigidBody.drag = startDragSpeed;
            //     isInFollow = false;
            // }
            // finalMovespeed = finalMovespeed - 10f;
        }
        else if (dist > formDistanceSlowdown)
        {
            finalMovespeed = Mathf.Clamp(Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist), 0, maxMovementSpeed);
            // output = Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist);

            // output = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime;
            // output2 = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, maxMovementSpeed, TargetVelocity - CurrentVelocity ) * (Time.deltaTime * dist * formMultiplier) * 0.1f;
            // output3 = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity ) * (Time.deltaTime * dist / formMultiplier) * 0.1f;
            // output4 =  (finalMovespeed / CurrentVelocity ) / (TargetVelocity / 1);
            // output3 = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist * Time.deltaTime);
            // output4 = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist / Time.deltaTime);

            // // outputSolve = finalMovespeed + output2;
            // // outputSolve2 = finalMovespeed - output2;
            // outputSolve3 = Mathf.Lerp(finalMovespeed, Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime, dist);

            // float wtf = ((1/2*(AIRigidBody.mass == 0 ? 1f : AIRigidBody.mass)*finalMovespeed) / Time.deltaTime) * finalMovespeed;

            // finalMovespeed = force;

            // outputSolve4 = wtf;

            debug_WITHINTHRESHORD = false;
            debug_INDISTANCESLOWDOWN = false;
            debug_OUTDISTANCESLOWDOWN = true;
            // if (isInFollow)
            // {
            //     AIRigidBody.angularDrag = startAngularDrag;
            //     AIRigidBody.drag = startDragSpeed;
            //     isInFollow = false;
            // }


            // finalMovespeed = finalMovespeed + 10f;
        }
        else if (dist < targetFloatDistance && (dist < formDistanceSlowdown))
        {
            // finalMovespeed = Mathf.Lerp(ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, finalMovespeed, dist);
            // output = Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist);
            // float temp = 0f;

            // Vector3 forwardSpeed = (AIRigidBody.transform.forward * desiredSpeed);
            // float force = (CurrentVelocity * AIRigidBody.mass * TargetVelocity);
            // force = (AIRigidBody.drag * force) / (1 - 0.02f * AIRigidBody.drag);

            // finalMovespeed = force;


            float temp = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime * 0.1f;
            // temp = finalMovespeed - Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist * Time.deltaTime);
            // output = Mathf.Lerp(AIRigidBody.GetComponent<ConstantForce>().relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime;
            finalMovespeed = finalMovespeed - temp;

            debug_WITHINTHRESHORD = true;
            debug_INDISTANCESLOWDOWN = false;
            debug_OUTDISTANCESLOWDOWN = false;
            // if (!isInFollow)
            // {
            //     AIRigidBody.drag = ObjectVelocity.drag;
            //     AIRigidBody.angularDrag = ObjectVelocity.angularDrag;
            //     isInFollow = true;
            // }
        }

        moveLogc(Following.position, Following.up);

        return finalMovespeed;
    }
    public void setTargetableAI()
    {
        TrackerObject.isTargetable = true;
        damageable = true;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!dead && Networking.IsOwner(gameObject))
        {
            if (shouldPing)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "pingPos");
            }
        }
        // if(disableAfterDead){
        //     gameObject.SetActive(false);
        // }
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
        // Debug.LogError("COLLIDE");
    }

    void Update()
    {
        if (!dead && (type == "air" || type == "heavyair"))
        {
            if (updateMovementTimer > updateMovementTendency)
            {
                MovementLogic();
                updateMovementTimer = 0;
            }
            else
            {
                updateMovementTimer = updateMovementTimer + Time.deltaTime;
            }
        }

        if (!disabled)
        {
            if (!dead)
            {
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
                    if (onHalfHealth != null && !triggerHalfHealthRan)
                    {
                        if (Networking.IsOwner(gameObject))
                        {
                            onHalfHealth.run = true;
                        }
                        triggerHalfHealthRan = true;
                    }
                }

                if (Networking.IsOwner(gameObject) || localPlayer == null)
                {

                    if (type == "air" || type == "static")
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

                    if (TurretScripts != null && TurretScripts.Length > 0 || MainTurrets != null && MainTurrets.Length > 0)
                    {
                        if (type == "static" || type == "ship" || type == "heavyair" || type == "air")
                        {
                            if (TurretScripts != null && TurretScripts.Length > 0)
                            {
                                if (turretindex < TurretScripts.Length)
                                {
                                    if (!TurretScripts[turretindex].dead)
                                    {
                                        aliveTurretsPrivate = aliveTurretsPrivate + 1;
                                    }
                                    turretindex = turretindex + 1;
                                }
                                else
                                {
                                    if (aliveTurretsPrivate < enableMainTurretsOn + 1)
                                    {
                                        enableMainTurrets = true;
                                        if (onDeadTurrets != null && !triggerDeadTurretsRan)
                                        {
                                            onDeadTurrets.run = true;
                                            triggerDeadTurretsRan = true;
                                            // Debug.Log("TURRET END"); 
                                        }
                                    }

                                    if ((aliveTurretsPrivate < enableMainTurretsOn + 1) || (aliveTurretsPrivate < TurretScripts.Length + 1))
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
                                                isTargetableCheck = true;
                                            }
                                        }
                                    }
                                    if (aliveTurretsPrivate < (TurretScripts.Length / 2))
                                    {
                                        if (onHalfTurrets != null && !triggerHalfTurretRan)
                                        {
                                            onHalfTurrets.run = true;
                                            triggerHalfTurretRan = true;
                                        }
                                    }
                                    aliveTurretsPrivate = 0;
                                    turretindex = 0;
                                }
                            }
                            if (updated)
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
            triggerDeadTurretsRan = false;
            triggerDeadTriggerRan = false;
            triggerHalfTurretRan = false;
            triggerHalfHealthRan = false;
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
            if (disableAfterDead)
            {
                if (disableTimer > disableTime)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    disableTimer += Time.deltaTime;
                }
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
                        AIClassTransform.position = RespawnArea.position;
                        AIClassTransform.rotation = RespawnArea.rotation;
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
            if (onDestroy != null && !onDestroyRan)
            {
                onDestroy.run = true;
                onDestroyRan = true;
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