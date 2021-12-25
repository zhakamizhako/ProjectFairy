using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AIObject : UdonSharpBehaviour
{
    // public GameObject MainBody;
    [Header("AI Options")]
    [UdonSynced(UdonSyncMode.None)] public float Health = 100;
    [System.NonSerializedAttribute] [HideInInspector] public float fullHealth = 0;
    [UdonSynced(UdonSyncMode.None)] public bool disabled = false;
    public Rigidbody AIRigidBody;
    public bool dead = false;
    public float disappearTrackerOn = 3f;
    public AudioSource[] deadSounds;
    public GameObject AIClass;
    public bool damageable = false;
    private float timerDead = 0;
    public bool revive = false;
    public string type = "static"; //[ship, heavyair, air, static]
    private float startmass = 0;
    public float deadMass = 0;
    public bool disableAfterDead = false;
    private float disableTimer = 0;
    public float disableTime = 15f;
    public bool disableTurretsOnStandby = true;
    public AITurretScript[] TurretScripts; // These are Targetable Turrets, and may and may not be destroyable.
    public AITurretScript[] MainTurrets; // These are non Targetable Turrets, and undestroyable unless if the aiobject is dead.
    [UdonSynced(UdonSyncMode.None)] public bool enableMainTurrets = true;
    public int enableMainTurretsOn = -1; // if -1, they're enabled by default. If specified a number, refers to how many turrets left before the AI enables these turrets.
    public bool setTargetableOnOutOfTurrets = true;
    public bool setDamagableOnOutOfTurrets = true;
    public Transform TargetDetector;
    public MissileTargets PredefinedTargets;
    public Animator AIObjectAnimator;
    public MissileTrackerAndResponse TrackerObject;
    [HideInInspector] public bool updated = true;
    public float RotSpeed = 15;
    public GameObject deadParticle;

    public GameObject SmokeObject;


    [Header("Trigger Scripts")]
    public TriggerScript onDestroy;
    public TriggerScript onHalfHealth;
    public TriggerScript onHalfTurrets;
    public TriggerScript onDeadTurrets;
    public TriggerScript EngageTrigger;
    public TriggerScript[] EngageTriggers;
    public TriggerScript[] onDestroys;

    [Header("Targeting Options")]
    [UdonSynced(UdonSyncMode.None)] public string TargetString = "";
    public int[] targetIndices;
    public float targetChangeTime = 5;
    public float targetChangeTimer = 0;
    public LayerMask TargetingLayermask;
    public float radius = 7000;
    public float updateTendency = 0.5f;
    public float updateTimer = 0f;
    private int targetUpdateIndex = 0;
    private int targetUpdateIndex2 = 0;
    public bool isSingleTargetOnly = false;
    bool notargetsCheck = false;
    bool isTargetableCheck = false;
    public RaycastHit[] TargetDetectionList;
    public MissileTrackerAndResponse[] debugTargets;

    [Header("Movement Update Setings")]
    public float updateMovementTendency = 0.5f;
    public float updateMovementTimer = 0f;
    public Transform RespawnArea; //Only valid for Flying. maybe.
    public bool isRespawnable;
    public float RespawnTime = 10f;
    private float RespawnTimer = 0f;
    public bool isCrashing = false;
    public bool hasCrashed = false;
    public bool useTranslate = false;
    public bool dontchaseTarget = false;
    public bool shouldAttackOnSight = false;
    private int dirB = 0;
    public float rollRate = 0.02f;
    public bool shouldAttack = false;
    private bool shouldEvade = false;
    private float attackRange = 1000f;
    public float movementSpeed = 200f;
    public float maxMovementSpeed = 300f;


    [Header("Formation Settings")]
    public bool shouldFollowLeft = false;
    public bool shouldFollowRight = false;
    public float formDistanceSlowdown = 100f;
    public float currentmoveSpeed = 0f;
    public float targetFloatDistance = 40f;
    public float formMultiplier = 4f;
    public float distanceTooClose = 1000f;
    public float distanceTooCloseGround = 1200f;
    public float distanceExit = 1300f;
    public Transform GroundOffset;
    public bool tooClose = false;
    public bool tooCloseGround = false;
    public bool cleanupFollowObjectWhenDead = true;
    public MissileTrackerAndResponse FollowObject;

    [Header("Formation Debugging")]
    public bool debug_INDISTANCESLOWDOWN = false;
    public bool debug_OUTDISTANCESLOWDOWN = false;
    public bool debug_WITHINTHRESHORD = false;
    public float CurrentVelocity = 0f;
    public float TargetVelocity = 0f;
    public Transform DebugObjectMovement;

    [Header("Waypoint Settings")]
    public GameObject[] Waypoints; //????????????
    public float distanceToChangeWaypoint = 50f;
    public bool shouldGoNextWaypoint = true;
    public bool AIPathGobackToOrigin = false; //useful for patrolling AI objects.
    [UdonSynced(UdonSyncMode.None)] public int currentWaypointIndex = 0;
    public Transform[] EscapePointTransforms;
    public Vector3[] EscapePointTransformVectors;
    //Not implemented yet...
    // public int[] TriggerWaypoints; // index - ?, entry - Waypoint number
    // public TriggerScript[] WaypointTrigger; // index - TriggerWaypoint rep, Trigger action



    //To add flares.

    // public GameObject Flare;
    // public float FlareCooldown = 15f;
    // public float FlareTimer = 0f;
    // public bool hasFlares = true;
    // private bool flareReady = true;
    //Air AI Behaviour



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

    [System.NonSerializedAttribute] [UdonSynced(UdonSyncMode.Smooth)] public Vector3 posSync;

    [System.NonSerializedAttribute] [UdonSynced(UdonSyncMode.Smooth)] public Quaternion rotationSync;

    [System.NonSerializedAttribute] [UdonSynced(UdonSyncMode.Smooth)] public Vector3 veloSync;
    private Vector3 rotLerp = new Vector3(0, 0, 0);
    // private float b = 0f;

    [System.NonSerializedAttribute] [HideInInspector] bool initDamagable = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initTargetable = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initRendered = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initDisabled = false;
    [System.NonSerializedAttribute] [HideInInspector] bool initEnableMainTurrets = false;
    [System.NonSerializedAttribute] [HideInInspector] public float startDragSpeed = 0f;
    [System.NonSerializedAttribute] [HideInInspector] public float startAngularDrag = 0f;
    private bool deadplay = false;

    // public float LockAngle = 180;


    [Header("Sync Options")]
    [Header("Enable Sync Object Position in Udon when disabling shouldPing and enable Move in master.")]
    public bool shouldOnlyMoveInMaster = false;
    public bool shouldPing = true;
    public bool useSmoothingSync = false;
    public bool uselocalPosition = false;
    public float pingPosEvery = 15f;
    private float pingPosTimer = 0f;
    private ConstantForce AiConstantForce;
    private bool turretStandby = false;

    //Cost saving measure in order to save for other turrets.
    // public GameObject MissileFab;
    // public Transform[] MissileSpawnArea;

    private bool ranTriggerHalf = false;
    private bool ranTriggerDead = false;
    private bool ranTriggerDeadTurrets = false;

    [System.NonSerializedAttribute] public int aliveTurretsPrivate = 0;
    [System.NonSerializedAttribute] public int turretindex = 0;
    // public bool turretCheck = false;
    private float damageReceived = 0f;
    [System.NonSerializedAttribute] public Transform AIClassTransform;
    [System.NonSerializedAttribute] public Transform GameObjectTransform;
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



    [System.NonSerializedAttribute] public Vector3 startArea;
    [System.NonSerializedAttribute] public Quaternion startAreaRotation;

    // private bool initShouldAttack = false;

    [System.NonSerializedAttribute] [HideInInspector] public VRCPlayerApi localPlayer;
    public bool inited = false;

    private int tooCloseRandomizer = 0; //0 -> down, 1 -> left, 2-> right, 3-> up, 4-> back, 5-> forward
    public PlayerUIScript UIScript;
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
        else
        {
            Debug.LogError("AI Has no Rigidbody. Switching to Translate Mode");
            useTranslate = true;
        }

        // if (Waypoints.Length > 0) {
        //     b = Vector3.Angle (gameObject.transform.forward, (Waypoints[currentWaypointIndex].gameObject.transform.position - gameObject.transform.position));
        // }
        updateTimer = Random.Range(0, updateTendency);
        updateMovementTimer = Random.Range(0, updateMovementTendency);
        currentmoveSpeed = movementSpeed;
        if (AIRigidBody != null)
        {
            var b = AIRigidBody.GetComponent<ConstantForce>();
            if (b != null) AiConstantForce = b;

        }

        AIClassTransform = AIClass.transform;
        GameObjectTransform = gameObject.transform;
        if (type == "air")
        {
            startDragSpeed = AIRigidBody != null ? AIRigidBody.drag : 0f;
            startAngularDrag = AIRigidBody != null ? AIRigidBody.drag : 0f;
        }

        startArea = AIClassTransform.transform.localPosition;
        startAreaRotation = AIClassTransform.transform.localRotation;
        inited = true;

        if(UIScript==null){
            UIScript = TrackerObject!=null ? 
                        TrackerObject.UIScript :
                        null;
                        
            if(UIScript==null)
            Debug.LogError("[MISSING UI SCRIPT] WARNING! MISSING UI SCRIPT!");
        }

        // EngineController x;
        // x.VehicleRigidbody.angularVelocity
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

    public void SmokeOn()
    {
        SmokeObject.SetActive(true);
    }

    public void SmokeOff()
    {
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
        // revive = true;
        reviveLogic();
    }

    public void DestroyAI()
    {
        if (Networking.IsOwner(gameObject) || Networking.LocalPlayer == null)
            Health = 0;
    }

    void OnParticleCollision(GameObject other)
    {
        bool damage = false;
        if (TrackerObject != null && TrackerObject.UIScript != null && !TrackerObject.UIScript.AIDamageLocalOnly)
        {
            damage = true;
        }

        if (damage || Networking.IsOwner(localPlayer, gameObject))
        {
            if (localPlayer == null)
            {
                hitDamage();
            }
            else
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hitDamage");
            }
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

    public void OwnershipSetter()
    {
        if (TrackerObject != null)
        {
            if (TrackerObject.UIScript != null && TrackerObject.UIScript.isSinglePlayer && TrackerObject.UIScript.InstanceOwner != null)
            {
                if (Networking.GetOwner(gameObject) != TrackerObject.UIScript.InstanceOwner.Owner)
                {
                    Networking.SetOwner(TrackerObject.UIScript.InstanceOwner.Owner, gameObject);
                    foreach (AITurretScript x in TurretScripts)
                    {
                        Networking.SetOwner(TrackerObject.UIScript.InstanceOwner.Owner, x.gameObject);
                    }
                }
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
                debugTargets = new MissileTrackerAndResponse[0];
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
                        MissileTrackerAndResponse[] temp = new MissileTrackerAndResponse[debugTargets.Length + 1];
                        int[] tempTargetIndices = new int[debugTargets.Length + 1];
                        debugTargets.CopyTo(temp, 0);
                        targetIndices.CopyTo(tempTargetIndices, 0);
                        temp[temp.Length - 1] = currentSelection;
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
            else
            {
                AIClassTransform.Translate(new Vector3(0, 0, currentmoveSpeed));//Something for the future.
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

                if (Networking.IsOwner(gameObject) || (!Networking.IsOwner(gameObject) && !shouldOnlyMoveInMaster))
                {
                    if (shouldAttack && !dontchaseTarget && debugTargets.Length > 0)
                    { //Attack Logic
                        shouldFollowLeft = false;
                        shouldFollowRight = false;
                        Vector3 finalVectors;
                        Vector3 V;
                        Vector3 targetPos;
                        // Transform targetTransform;
                        if (debugTargets[0].EngineController != null)
                        {
                            V = debugTargets[0].EngineController.CurrentVel;
                            targetPos = debugTargets[0].EngineController.VehicleMainObj.transform.position;
                        }
                        else if (debugTargets[0].AI != null && debugTargets[0].AI.AIClass != null && debugTargets[0].AI.AIRigidBody != null)
                        {
                            V = debugTargets[0].AI.AIRigidBody.velocity;
                            targetPos = debugTargets[0].AI.AIClass.transform.position;
                        }
                        else if (debugTargets[0].AITurret != null && debugTargets[0].AITurret.mainAIObject != null && debugTargets[0].AITurret.mainAIObject.AIRigidBody != null)
                        {
                            V = debugTargets[0].AITurret.mainAIObject.AIRigidBody.velocity;
                            targetPos = debugTargets[0].AITurret.mainAIObject.AIRigidBody.transform.position;
                        }
                        else if (debugTargets[0].AITurret != null && debugTargets[0].AITurret.mainAIObject != null && debugTargets[0].AITurret.mainAIObject == null)
                        {
                            V = Vector3.zero;
                            targetPos = debugTargets[0].AITurret.mainAIObject.transform.position;
                        }
                        else if (debugTargets[0].AITurret != null && debugTargets[0].AITurret.mainAIObject == null)
                        {
                            V = Vector3.zero;
                            targetPos = debugTargets[0].AITurret.transform.position;
                        }
                        else
                        {
                            V = Vector3.zero;
                            targetPos = debugTargets[0].gameObject.transform.position;
                        }


                        if ((AIClassTransform.position.y - (GroundOffset != null ? GroundOffset.position.y : 0f) < distanceTooCloseGround) || Vector3.Distance(AIClassTransform.position, targetPos) < distanceTooClose)
                        {
                            if (!tooClose)
                            {
                                if (EscapePointTransforms != null && EscapePointTransforms.Length > 0) tooCloseRandomizer = Random.Range(0, EscapePointTransforms.Length);
                                if (EscapePointTransformVectors != null && EscapePointTransformVectors.Length > 0) tooCloseRandomizer = Random.Range(0, EscapePointTransformVectors.Length);
                            }
                            tooClose = true;
                        }
                        else if (tooClose && ((AIClassTransform.position.y - (GroundOffset != null ? GroundOffset.position.y : 0f) > distanceExit) || Vector3.Distance(AIClassTransform.position, targetPos) > distanceExit))
                        {
                            tooClose = false;
                        }

                        Transform xxx = AIClass.gameObject.transform;
                        Vector3 escapePoint = Vector3.zero;

                        if (tooClose)
                        {

                            switch (tooCloseRandomizer)
                            {
                                case 0:
                                    escapePoint = xxx.up * 1000f;
                                    break;
                                case 1:
                                    escapePoint = xxx.up * 1000f;
                                    break;
                                case 2:
                                    escapePoint = xxx.forward * 1000f;
                                    break;
                                case 3:
                                    escapePoint = xxx.forward * -1000f;
                                    break;
                                case 4:
                                    escapePoint = xxx.right * 1000f;
                                    break;
                                case 5:
                                    escapePoint = xxx.right * -1000f;
                                    break;
                            }
                        }


                        finalVectors = !tooClose ?
                        FirstOrderIntercept(
                            AIClass.gameObject.transform.position,
                            AIRigidBody != null ? AIRigidBody.velocity : Vector3.zero,
                            AIRigidBody != null ? AIRigidBody.velocity.magnitude : currentmoveSpeed,
                            targetPos, V)
                            :
                        FirstOrderIntercept(
                            AIClass.gameObject.transform.position,
                            AIRigidBody != null ? AIRigidBody.velocity : Vector3.zero,
                            AIRigidBody != null ? AIRigidBody.velocity.magnitude : currentmoveSpeed,
                            // new Vector3(AIClass.gameObject.transform.position.x, 10000f, AIClass.gameObject.transform.position.z),
                            escapePoint,
                            AIRigidBody != null ? AIRigidBody.velocity : Vector3.zero);

                        // if(DebugObjectMovement!=null){
                        //     DebugObjectMovement.position = finalVectors;
                        // }

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

                        finalMovementSpeed = followLogic(FollowObject);
                        currentmoveSpeed = finalMovementSpeed;

                        if (cleanupFollowObjectWhenDead && (FollowObject.AI != null && FollowObject.AI.Health < 1) || (FollowObject.EngineController != null && (FollowObject.EngineController.dead || FollowObject.EngineController.Health < 0)))
                        {
                            FollowObject = null;
                        }
                    }
                }
                // Chase Target Logic


                //Finally, Move the object

                if (shouldOnlyMoveInMaster && Networking.IsOwner(gameObject))
                { // Finally
                    if (AIRigidBody != null)
                    {
                        AiConstantForce.relativeForce = new Vector3(0, 0, currentmoveSpeed);
                    }
                    else
                    {
                        AIClassTransform.Translate(new Vector3(0, 0, currentmoveSpeed), Space.Self);//Something for the future.
                    }
                }
                else if (!shouldOnlyMoveInMaster)
                {
                    if (AIRigidBody != null)
                    {
                        AiConstantForce.relativeForce = new Vector3(0, 0, currentmoveSpeed);
                    }
                    else
                    {
                        AIClassTransform.Translate(new Vector3(0, 0, currentmoveSpeed), Space.Self);//Something for the future.
                    }
                }

                //Update Sync Values

                if (Networking.IsOwner(gameObject) && (shouldPing || useSmoothingSync))
                {
                    if (uselocalPosition)
                    {
                        posSync = GameObjectTransform.localPosition;
                    }
                    else
                    {
                        posSync = GameObjectTransform.position;
                    }

                    if (shouldPing)
                    {
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
                    if (useSmoothingSync)
                    {
                        posSync = uselocalPosition ? GameObjectTransform.localPosition : GameObjectTransform.position;
                        rotationSync = GameObjectTransform.localRotation;
                        veloSync = AIRigidBody != null ? AIRigidBody.velocity : Vector3.zero;
                    }
                }
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

    public float followLogic(MissileTrackerAndResponse Tracker)
    {
        Rigidbody ObjectVelocity = null;
        bool translateMode = false;
        float targetspeed = 0f;
        if (Tracker != null)
        {
            if (Tracker.AI != null && Tracker.AI.AIRigidBody != null)
            {
                ObjectVelocity = Tracker.AI.AIRigidBody;
            }
            else if (Tracker.EngineController != null)
            {
                ObjectVelocity = Tracker.EngineController.VehicleRigidbody;
            }
            else if (Tracker.AI != null && Tracker.AI.AIRigidBody == null && Tracker.AI.useTranslate && useTranslate)
            {
                targetspeed = Tracker.AI.currentmoveSpeed;
                translateMode = true;
            }
        }
        float finalMovespeed = currentmoveSpeed;
        Transform Following = null;
        if (shouldFollowLeft)
        {
            Following = Tracker.FollowL;
        }
        if (shouldFollowRight)
        {
            Following = Tracker.FollowR;
        }

        if (Following == null)
        {
            Following = Tracker.Tailer != null ? Tracker.Tailer : Tracker.gameObject.transform;
        }

        float dist = Vector3.Distance(AIClassTransform.position, Following.position);

        //Normal Follow Logic, With Rigidbody
        if (ObjectVelocity != null && !translateMode && !useTranslate)
        {
            CurrentVelocity = AIRigidBody.velocity.magnitude;
            TargetVelocity = ObjectVelocity.velocity.magnitude;

            if (dist < formDistanceSlowdown && dist > targetFloatDistance)
            {
                float temp = 0f;
                if (CurrentVelocity > TargetVelocity)
                {
                    temp = -Mathf.Lerp(AiConstantForce.relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f;
                }
                else
                {
                    temp = Mathf.Lerp(AiConstantForce.relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f; ;
                }

                finalMovespeed = Mathf.Clamp(finalMovespeed + temp, 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = true;
                debug_OUTDISTANCESLOWDOWN = false;
            }
            else if (dist > formDistanceSlowdown)
            {
                finalMovespeed = Mathf.Clamp(Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist), 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = true;
            }
            else if (dist < targetFloatDistance && (dist < formDistanceSlowdown))
            {

                float temp = Mathf.Lerp(AiConstantForce.relativeForce.z, ObjectVelocity.transform.GetComponent<ConstantForce>().relativeForce.z, dist) * Time.deltaTime * 0.1f;
                finalMovespeed = finalMovespeed - temp;

                debug_WITHINTHRESHORD = true;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = false;
            }
        }

        //Normal AI with Rigidbody, Following a NonRigidbody
        else if (ObjectVelocity == null && translateMode && !useTranslate)
        {
            CurrentVelocity = AIRigidBody.velocity.magnitude;
            TargetVelocity = targetspeed;

            if (dist < formDistanceSlowdown && dist > targetFloatDistance)
            {
                float temp = 0f;
                if (CurrentVelocity > TargetVelocity)
                {
                    temp = -Mathf.Lerp(AiConstantForce.relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f;
                }
                else
                {
                    temp = Mathf.Lerp(AiConstantForce.relativeForce.z, maxMovementSpeed, CurrentVelocity - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f; ;
                }

                finalMovespeed = Mathf.Clamp(finalMovespeed + temp, 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = true;
                debug_OUTDISTANCESLOWDOWN = false;
            }
            else if (dist > formDistanceSlowdown)
            {
                finalMovespeed = Mathf.Clamp(Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist), 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = true;
            }
            else if (dist < targetFloatDistance && (dist < formDistanceSlowdown))
            {

                float temp = Mathf.Lerp(AiConstantForce.relativeForce.z, TargetVelocity, dist) * Time.deltaTime * 0.1f;
                finalMovespeed = finalMovespeed - temp;

                debug_WITHINTHRESHORD = true;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = false;
            }
        }

        //AI With no Rigidbody, Following a Rigidbody
        else if (ObjectVelocity != null && useTranslate)
        {
            TargetVelocity = ObjectVelocity.velocity.magnitude;
            if (dist < formDistanceSlowdown && dist > targetFloatDistance)
            {
                float temp = 0f;
                if (currentmoveSpeed > targetspeed)
                {
                    temp = -Mathf.Lerp(currentmoveSpeed, maxMovementSpeed, currentmoveSpeed - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f;
                }
                else
                {
                    temp = Mathf.Lerp(currentmoveSpeed, maxMovementSpeed, currentmoveSpeed - TargetVelocity) * (Time.deltaTime * formMultiplier / dist) * 0.1f; ;
                }

                finalMovespeed = Mathf.Clamp(finalMovespeed + temp, 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = true;
                debug_OUTDISTANCESLOWDOWN = false;
            }
            else if (dist > formDistanceSlowdown)
            {
                finalMovespeed = Mathf.Clamp(Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist), 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = true;
            }
            else if (dist < targetFloatDistance && (dist < formDistanceSlowdown))
            {

                float temp = Mathf.Lerp(currentmoveSpeed, TargetVelocity, dist) * Time.deltaTime * 0.1f;
                finalMovespeed = finalMovespeed - temp;

                debug_WITHINTHRESHORD = true;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = false;
            }
        }


        //AI with no Rigidbody, Following a nonRigidbody, 
        else if (ObjectVelocity == null && translateMode && useTranslate)
        {
            if (dist < formDistanceSlowdown && dist > targetFloatDistance)
            {
                float temp = 0f;
                if (currentmoveSpeed > targetspeed)
                {
                    temp = -Mathf.Lerp(currentmoveSpeed, maxMovementSpeed, currentmoveSpeed - targetspeed) * (Time.deltaTime * formMultiplier / dist) * 0.1f;
                }
                else
                {
                    temp = Mathf.Lerp(currentmoveSpeed, maxMovementSpeed, currentmoveSpeed - targetspeed) * (Time.deltaTime * formMultiplier / dist) * 0.1f; ;
                }

                finalMovespeed = Mathf.Clamp(finalMovespeed + temp, 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = true;
                debug_OUTDISTANCESLOWDOWN = false;
            }
            else if (dist > formDistanceSlowdown)
            {
                finalMovespeed = Mathf.Clamp(Mathf.Lerp(finalMovespeed, maxMovementSpeed, Time.deltaTime * formMultiplier / dist), 0, maxMovementSpeed);
                debug_WITHINTHRESHORD = false;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = true;
            }
            else if (dist < targetFloatDistance && (dist < formDistanceSlowdown))
            {

                float temp = Mathf.Lerp(currentmoveSpeed, targetspeed, dist) * Time.deltaTime * 0.1f;
                finalMovespeed = finalMovespeed - temp;

                debug_WITHINTHRESHORD = true;
                debug_INDISTANCESLOWDOWN = false;
                debug_OUTDISTANCESLOWDOWN = false;
            }
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

    public void SetDamagable()
    {
        damageable = true;
    }

    void Update()
    {
        // if(TrackerObject!=null && TrackerObject.UIScript!=null && TrackerObject.UIScript.isSinglePlayer && Networking.GetOwner(gameObject)!=localPlayer){
        // OwnershipSetter();
        // }
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


            if (Networking.GetOwner(gameObject) != Networking.LocalPlayer)
            {
                if (useSmoothingSync)
                {
                    if (uselocalPosition)
                    {
                        AIClassTransform.localPosition = posSync;
                    }
                    else
                    {
                        AIClassTransform.position = posSync;
                    }
                    AIClassTransform.localRotation = rotationSync;
                }
                if (AIRigidBody != null) AIRigidBody.velocity = veloSync;
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
                        if (AIRigidBody != null) AIRigidBody.useGravity = true;
                    }
                }
                if (Health < (fullHealth / 2))
                {
                    if (onHalfHealth != null && !triggerHalfHealthRan)
                    {
                        if (Networking.IsOwner(gameObject))
                        {
                            // onHalfHealth.run = true;
                            UIScript.AddToQueueScript(onHalfHealth);
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
                                            // onDeadTurrets.run = true;
                                            UIScript.AddToQueueScript(onDeadTurrets);
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
                                            // onHalfTurrets.run = true;
                                            UIScript.AddToQueueScript(onHalfTurrets);
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
            reviveLogic();
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
                // onDestroy.run = true;
                UIScript.AddToQueueScript(onDestroy);
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

    public void reviveLogic()
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
            if (AIRigidBody != null)
            {
                AIRigidBody.mass = startmass;
                AIRigidBody.useGravity = false;
            }
        }
        revive = false;
        dead = false;
        deadplay = false;
        triggerDeadTurretsRan = false;
        triggerDeadTriggerRan = false;
        triggerHalfTurretRan = false;
        triggerHalfHealthRan = false;
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

        if (targetVelocity != Vector3.zero)
        {
            float t = FirstOrderInterceptTime(
            shotSpeed,
            targetRelativePosition,
            targetRelativeVelocity
        );
            return targetPosition + t * (targetRelativeVelocity);
        }
        else
        {
            return targetPosition;
        }
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