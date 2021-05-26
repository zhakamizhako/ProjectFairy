using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MissileScript : UdonSharpBehaviour
{

    // public GameObject MissileOwner;
    public GameObject Target;
    private GameObject Dafuq;
    public GameObject MissileObject;
    // public Rigidbody RigidMissile;
    public GameObject MissileClass;
    public GameObject ExplosionEffects;
    public AudioSource[] ExplosionSounds;
    public GameObject LaunchedFrom;
    public bool isExploded = false;
    public float destroyTimer = 0;
    private float MissileTimer = 0;
    private float colliderOn = 0.5f;
    private float missileColliderTimer = 0;
    public float missileSpeed = 1000;
    public int explosionAmount = 20;
    public float giveupDistance = 3f;
    public float closeDistance = 3f;
    public string type = "missile"; //If missile, or bomb.
    public int missileType = 1; // 0 = rockets, 1 
    public bool usesOld = false;
    public float RotSpeed = 15;
    public float maxAngle = 45f;
    // private bool follow = false;
    private bool isSet = false;
    private bool explodeSound = false;
    private bool calledOff = false;
    private bool missed = false;
    private bool close = false;
    private bool missedCalled = false;
    public float explosionParticleShortTime = 0.1f;
    public ParticleSystem ExplosionShortSystem;
    public ParticleSystem Smoke;
    public float explosionShortTimer = 0f;
    public bool explosionshorted = false;
    public bool explosionshortstop = false;
    public float explodeAt = 50f;
    public bool hasHitIndicator = false;
    public bool indicatorCalled = false;
    private Rigidbody MissileRigidBody;
    private ConstantForce MissileConstantForce;
    public float timerLimit = 10; //missile life in case of no targets and went through clips or sky
    // [UdonSynced (UdonSyncMode.None)] public Vector3 targetDirection;
    public GameObject TargetTest;
    public Vector3 TargetScale;
    private WeaponSelector LaunchedWP;
    private MissileTrackerAndResponse targetObjectTracker;
    private Vector3 targetPosLastFrame;
    private float missileDist = 0;
    private Transform Targeting;
    private Quaternion guidedRotation;
    private Transform missileTransform;
    public AITurretScript turretScript;
    void Start()
    {
        if (type == "missile")
        {
            colliderOn = 0.5f;
        }
        else if (type == "bomb")
        {
            colliderOn = 1f;
        }
        else if (type == "flak")
        {
            if (turretScript != null)
            {
                timerLimit = turretScript.TimerLimitFlak;
            }
            colliderOn = 0.3f;
        }
        MissileRigidBody = MissileClass.GetComponent<Rigidbody>();
        MissileConstantForce = MissileClass.GetComponent<ConstantForce>();
        LaunchedWP = LaunchedFrom != null ? LaunchedFrom.GetComponent<WeaponSelector>() : null;
        missileTransform = MissileClass.transform;

        // Debug.Log ("MissileScript Initialized");
        if (Target != null && type!="flak")
        {
            // Debug.Log ("I Have Target!");
            if (Target.GetComponent<MissileTargeterParent>() != null)
            {
                var b = Target.GetComponent<MissileTargeterParent>();
                if (b.Target != null && b.noTarget == false)
                {
                    targetObjectTracker = b.Target;
                    if (targetObjectTracker.Tailer != null)
                    {
                        Targeting = targetObjectTracker.Tailer;
                    }
                    else
                    {
                        Targeting = targetObjectTracker.transform;
                    }
                    // Debug.Log ("Assigned");
                }
            }
            else if (Target.GetComponent<AITurretScript>() != null && type!="flak")
            {
                var b = Target.GetComponent<AITurretScript>();
                if (b.Target != null)
                {
                    targetObjectTracker = b.Target;
                    if (targetObjectTracker.Tailer != null)
                    {
                        Targeting = targetObjectTracker.Tailer;
                    }
                    else
                    {
                        Targeting = targetObjectTracker.transform;
                    }
                    // Debug.Log ("AssignedAI");
                }
            }
        }
        else
        {
            // Debug.Log ("Target Must not be null????");
        }
    }

    //Missile on Impact
    public void OnTriggerEnter(Collider col)
    {
        if (!isExploded)
        {
            ExplodeMissile();
        }
    }

    public void OnTriggerStay(Collider col)
    {
        if (!isExploded)
        {
            ExplodeMissile();
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (!isExploded)
        {
            ExplodeMissile();
        }
    }

    public void OnCollisionEnter(Collision col)
    {
        if (!isExploded)
        {
            ExplodeMissile();
        }
    }

    //The entire explosion shenanigans
    void ExplodeMissile()
    {
        MissileObject.SetActive(false);
        ExplosionEffects.SetActive(true);
        isExploded = true;
        destroyTimer = 0;
        if (Smoke != null)
        {
            Smoke.Stop();
        }
        MissileConstantForce.relativeForce = Vector3.zero; //Stop constant forward
        MissileRigidBody.velocity = Vector3.zero; // In bisaya terms, "PARA SURE GYUD NA DILI MULIHOK."
        //Freeze Missile Object
        MissileRigidBody.constraints = RigidbodyConstraints.FreezePositionZ;
        MissileRigidBody.constraints = RigidbodyConstraints.FreezePositionX;
        MissileRigidBody.constraints = RigidbodyConstraints.FreezePositionY;
        MissileRigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
        MissileRigidBody.constraints = RigidbodyConstraints.FreezeRotationX;
        MissileRigidBody.constraints = RigidbodyConstraints.FreezeRotationY;
    }

    void Update()
    {
        // Debug.Log("I AM FIRED");
        if (isExploded)
        {
            if (ExplosionShortSystem != null)
            {
                if (!explosionshorted)
                {
                    // ExplosionEffects.Emit (explosionAmount);
                    ExplosionShortSystem.gameObject.SetActive(true);
                    explosionshorted = true;
                }
                if (!explosionshortstop && explosionShortTimer < explosionParticleShortTime)
                {
                    explosionShortTimer = explosionShortTimer + Time.deltaTime;
                }
                else if (!explosionshortstop && explosionShortTimer > explosionParticleShortTime)
                {
                    ExplosionShortSystem.Stop();
                    explosionshortstop = true;
                }
            }
        }
        if (missileColliderTimer < colliderOn)
        { //Checker for initial missile fire to avoid collision
            missileColliderTimer += Time.deltaTime;
        }
        else if (missileColliderTimer > colliderOn && isSet == false)
        {
            Collider mCollider = MissileClass.GetComponent<Collider>();
            mCollider.enabled = true;
            // follow = true;
            isSet = true;
        }
        if (MissileTimer < timerLimit)
        { //Missile cleanu ptool
            MissileTimer += Time.deltaTime;
        }
        if (!isExploded)
        {
            if (type == "missile" || type=="flak")
                MissileConstantForce.relativeForce = new Vector3(0, 0, missileSpeed);

            if (type == "bomb")
                MissileConstantForce.force = new Vector3(0, -missileSpeed, 0);
            //Tail the target. First checks if whether the target has a Tailer which is where the missile should chase after.
            //If not because you're lazy to assign one, then it tracks the targeter's location.
            if (targetObjectTracker != null)
            {
                Transform missile = MissileClass.transform;
                if (targetObjectTracker != null)
                {
                    if (MissileTimer > .3f)
                    {
                        Vector3 relPos = Targeting.position - missileTransform.position;
                        float angleToTarget = Mathf.Abs(Vector3.Angle(missileTransform.forward.normalized, relPos.normalized));
                        missileDist = Vector3.Distance(missile.position, targetObjectTracker.gameObject.transform.position);

                        if (missileDist < closeDistance && !close)
                        {
                            close = true;
                        }
                        if (missileDist > giveupDistance && close)
                        {
                            missed = true;
                        }
                        if (missileDist < explodeAt)
                        {
                            ExplodeMissile();
                        }
                        if (angleToTarget > maxAngle)
                        {
                            missed = true;
                        }

                        if (!missed)
                        {
                            targetObjectTracker.isChasing = true;
                            if (!indicatorCalled)
                            {
                                indicatorCalled = true;
                                targetObjectTracker.receiveTracker(this);
                            }
                            if (Targeting != null)
                            { //heatseeker
                                if (usesOld)
                                {
                                    missile.LookAt(Targeting);
                                }
                                else if (missileType == 1)
                                {
                                    var ObjectToTargetVector = Targeting.gameObject.transform.position - missile.position;
                                    var AIForward = missile.forward;
                                    var targetDirection = ObjectToTargetVector.normalized;
                                    var rotationAxis = Vector3.Cross(AIForward, targetDirection);
                                    var deltaAngle = Vector3.Angle(AIForward, targetDirection);

                                    missile.Rotate(rotationAxis, Mathf.Min(RotSpeed * Time.deltaTime, deltaAngle), Space.World);

                                }
                                else if (missileType == 2)
                                { //predictive
                                    Vector3 finalVectors;
                                    Vector3 V;
                                    if (targetObjectTracker.EngineController != null)
                                    {
                                        V = targetObjectTracker.EngineController.CurrentVel;
                                    }
                                    else if (targetObjectTracker.AI != null && targetObjectTracker.AI.AIClass != null && targetObjectTracker.AI.AIRigidBody != null)
                                    {
                                        if(Networking.GetOwner(targetObjectTracker.AI.gameObject) == Networking.LocalPlayer){
                                            V = targetObjectTracker.AI.AIRigidBody.velocity;
                                        }else{
                                            V = targetObjectTracker.AI.veloSync!=Vector3.zero ?targetObjectTracker.AI.veloSync : targetObjectTracker.AI.AIRigidBody.velocity;
                                        }
                                    }
                                    else
                                    {
                                        V = Vector3.zero;
                                    }
                                    finalVectors = FirstOrderIntercept(missile.gameObject.transform.position, MissileRigidBody.velocity, MissileRigidBody.velocity.magnitude, Targeting.gameObject.transform.position, V);
                                    if (TargetTest != null)
                                    {
                                        var dist = Vector3.Distance(finalVectors, LaunchedFrom.transform.position);
                                        TargetTest.transform.position = finalVectors;
                                        TargetTest.transform.localScale = TargetScale * dist;
                                    }
                                    // missile.LookAt (finalVectors);

                                    var ObjectToTargetVector = finalVectors - missile.position;
                                    var AIForward = missile.forward;
                                    var targetDirection = ObjectToTargetVector.normalized;
                                    var rotationAxis = Vector3.Cross(AIForward, targetDirection);
                                    var deltaAngle = Vector3.Angle(AIForward, targetDirection);

                                    missile.Rotate(rotationAxis, Mathf.Min(RotSpeed * Time.deltaTime, deltaAngle), Space.World);
                                }
                                else if (missileType == 3)
                                {
                                    // relPos = Targeting.position - missileTransform.position;
                                    // float angleToTarget = Mathf.Abs(Vector3.Angle(missileTransform.forward.normalized, relPos.normalized));
                                    Vector3 targetVelocity = Targeting.position - targetPosLastFrame;
                                    targetVelocity /= Time.deltaTime;

                                    float predictedSpeed = Mathf.Min(MissileRigidBody.velocity.magnitude * MissileTimer);
                                    float timeToImpact = missileDist / Mathf.Max(predictedSpeed, 1.0f);

                                    // Create lead position based on target velocity and time to impact.                
                                    Vector3 leadPos = Targeting.position + targetVelocity * timeToImpact;
                                    Vector3 leadVec = leadPos - missileTransform.position;

                                    //print(leadVec.magnitude.ToString());

                                    //=====================================================

                                    // It's very easy for the lead position to be outside of the seeker head. To prevent
                                    // this, only allow the target direction to be 90% of the seeker head's limit.
                                    relPos = Vector3.RotateTowards(relPos.normalized, leadVec.normalized, maxAngle * Mathf.Deg2Rad * 0.9f, 0.0f);
                                    guidedRotation = Quaternion.LookRotation(relPos, missileTransform.up);

                                    //Debug.DrawRay(target.position, targetVelocity * timeToImpact, Color.red);
                                    //Debug.DrawRay(target.position, targetVelocity * timeToHit, Color.red);
                                    //Debug.DrawRay(transform.position, leadVec, Color.red);

                                    targetPosLastFrame = Targeting.position;

                                    missile.rotation = Quaternion.RotateTowards(missile.rotation, guidedRotation, RotSpeed * Time.deltaTime);
                                    //reference https://github.com/brihernandez/AceArcadeMissiles/blob/master/Assets/AceArcadeMissiles/Scripts/AAMissile.cs
                                }
                            }
                        }
                        if (missed)
                        {
                            if (!missedCalled)
                            {
                                if (hasHitIndicator && LaunchedWP != null)
                                {
                                    LaunchedWP.miss = true;
                                }
                                targetObjectTracker.isChasing = false;
                                missedCalled = true;
                                if (targetObjectTracker != null && !calledOff)
                                {
                                    targetObjectTracker.removeTracker(this);
                                    calledOff = true;
                                } //This will disable the missile alert in your cockpit.

                            }
                        }

                    }
                }
                else
                {
                    missile.LookAt(targetObjectTracker.gameObject.transform);
                }
            }
        }
        // Sets the timer before the missile Object deletes itself for cleanup.

        if (isExploded)
        {
            if (!explodeSound)
            {
                if (hasHitIndicator && !missed && LaunchedWP != null && Dafuq != null)
                {
                    LaunchedWP.hit = true;
                }
                explodeSound = true;
                if (ExplosionSounds != null && ExplosionSounds.Length > 0)
                {
                    int rInt = Random.Range(0, ExplosionSounds.Length);
                    ExplosionSounds[rInt].Play();
                }
            }
            if (destroyTimer < 5f)
            {
                destroyTimer = destroyTimer + Time.deltaTime;

                if (targetObjectTracker != null && !calledOff)
                {
                    targetObjectTracker.isChasing = false;
                    targetObjectTracker.removeTracker(this);
                    calledOff = true;
                } //This will disable the missile alert in your cockpit.



            }
            if (destroyTimer > 5f)
            {
                DestroyImmediate(gameObject);
            }
        }

        if (!isExploded && MissileTimer > timerLimit)
        { //Missile cleanup tool
            ExplodeMissile();
            MissileTimer = 0;
        }
    }

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