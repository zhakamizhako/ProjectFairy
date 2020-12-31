using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MissileScript : UdonSharpBehaviour {

    // public GameObject MissileOwner;
    public GameObject Target;
    private GameObject Dafuq;
    public GameObject MissileObject;
    // public Rigidbody RigidMissile;
    public GameObject MissileClass;
    public GameObject ExplosionEffects;
    public AudioSource[] ExplosionSounds;
    public GameObject LaunchedFrom;
    private bool isExploded = false;
    private float destroyTimer = 0;
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
    public float timerLimit = 10; //missile life in case of no targets and went through clips or sky
    // [UdonSynced (UdonSyncMode.None)] public Vector3 targetDirection;
    public GameObject TargetTest;
    public Vector3 TargetScale;

    void Start () {
        if (type == "missile") {
            colliderOn = 0.5f;
        } else if (type == "bomb") {
            colliderOn = 1f;
        }
        Debug.Log ("MissileScript Initialized");
        if (Target != null) {
            // Debug.Log ("I Have Target!");
            if (Target.GetComponent<MissileTargeterParent> () != null) {
                var b = Target.GetComponent<MissileTargeterParent> ();
                if (b.Target != null && b.noTarget == false) {
                    Dafuq = b.Target;
                }
            } else if (Target.GetComponent<AITurretScript> () != null) {
                var b = Target.GetComponent<AITurretScript> ();
                if (b.Target != null) {
                    Dafuq = b.Target;
                }
            }
        } else {
            // Debug.Log ("Target Must not be null????");
        }
    }

    //Missile on Impact
    public void OnTriggerEnter (Collider col) {
        if (!isExploded) {
            ExplodeMissile ();
        }
    }

    public void OnTriggerStay (Collider col) {
        if (!isExploded) {
            ExplodeMissile ();
        }
    }

    public void OnTriggerExit (Collider col) {
        if (!isExploded) {
            ExplodeMissile ();
        }
    }

    public void OnCollisionEnter (Collision col) {
        if (!isExploded) {
            ExplodeMissile ();
        }
    }

    //The entire explosion shenanigans
    void ExplodeMissile () {
        MissileObject.SetActive (false);
        ExplosionEffects.SetActive(true);
        isExploded = true;
        destroyTimer = 0;
        if (Smoke != null) {
            Smoke.Stop ();
        }
        MissileClass.GetComponent<ConstantForce> ().relativeForce = Vector3.zero; //Stop constant forward
        MissileClass.GetComponent<Rigidbody> ().velocity = Vector3.zero; // In bisaya terms, "PARA SURE GYUD NA DILI MULIHOK."
        //Freeze Missile Object
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionZ;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionX;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionY;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationZ;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationX;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationY;
    }

    void Update () {
        // Debug.Log("I AM FIRED");
        if (isExploded) {
            if (ExplosionShortSystem != null) {
                if (!explosionshorted) {
                    // ExplosionEffects.Emit (explosionAmount);
                    ExplosionShortSystem.gameObject.SetActive (true);
                    explosionshorted = true;
                }
                if (!explosionshortstop && explosionShortTimer < explosionParticleShortTime) {
                    explosionShortTimer = explosionShortTimer + Time.deltaTime;
                } else if (!explosionshortstop && explosionShortTimer > explosionParticleShortTime) {
                    ExplosionShortSystem.Stop ();
                    explosionshortstop = true;
                }
            }
        }
        if (missileColliderTimer < colliderOn) { //Checker for initial missile fire to avoid collision
            missileColliderTimer += Time.deltaTime;
        } else if (missileColliderTimer > colliderOn && isSet == false) {
            Collider mCollider = MissileClass.GetComponent<Collider> ();
            mCollider.enabled = true;
            // follow = true;
            isSet = true;
        }
        if (MissileTimer < timerLimit) { //Missile cleanu ptool
            MissileTimer += Time.deltaTime;
        }
        if (!isExploded) {
            if (type == "missile")
                MissileClass.GetComponent<ConstantForce> ().relativeForce = new Vector3 (0, 0, missileSpeed);

            if (type == "bomb")
                MissileClass.GetComponent<ConstantForce> ().force = new Vector3 (0, -missileSpeed, 0);
            //Tail the target. First checks if whether the target has a Tailer which is where the missile should chase after.
            //If not because you're lazy to assign one, then it tracks the targeter's location.
            if (Dafuq != null) {
                var targetObject = Dafuq.GetComponent<MissileTrackerAndResponse> ();
                Transform missile = MissileClass.transform;
                if (targetObject != null) {
                    if (MissileTimer >.3f) {

                        if (Vector3.Distance (missile.position, targetObject.gameObject.transform.position) < closeDistance && !close) {
                            close = true;
                        }
                        if (Vector3.Distance (missile.position, targetObject.gameObject.transform.position) > giveupDistance && close) {
                            missed = true;
                        }
                        if (Vector3.Distance (missile.position, targetObject.gameObject.transform.position) < explodeAt) {
                            ExplodeMissile ();
                        }

                        if (!missed) {
                            targetObject.isChasing = true;
                            if(!indicatorCalled){
                                indicatorCalled=true;
                                targetObject.receiveTracker(this);
                            }
                            if (targetObject.Tailer != null) { //heatseeker
                                if (usesOld) {
                                    missile.LookAt (targetObject.Tailer);
                                } else if (missileType == 1) {
                                    var ObjectToTargetVector = targetObject.Tailer.gameObject.transform.position - missile.position;
                                    var AIForward = missile.forward;
                                    var targetDirection = ObjectToTargetVector.normalized;
                                    var rotationAxis = Vector3.Cross (AIForward, targetDirection);
                                    var deltaAngle = Vector3.Angle (AIForward, targetDirection);

                                    missile.Rotate (rotationAxis, Mathf.Min (RotSpeed * Time.deltaTime, deltaAngle), Space.World);

                                } else if (missileType == 2) { //predictive
                                    Vector3 finalVectors;
                                    Vector3 V;
                                    if (targetObject.EngineController != null) {
                                        V = targetObject.EngineController.CurrentVel;
                                    } else if (targetObject.AI != null && targetObject.AI.AIClass != null && targetObject.AI.AIRigidBody != null) {
                                        V = targetObject.AI.AIRigidBody.velocity;
                                    } else {
                                        V = Vector3.zero;
                                    }
                                    finalVectors = FirstOrderIntercept (missile.gameObject.transform.position, missile.gameObject.GetComponent<Rigidbody> ().velocity, missile.gameObject.GetComponent<Rigidbody> ().velocity.magnitude, targetObject.Tailer.gameObject.transform.position, V);
                                    // Vector3 D = targetObject.gameObject.transform.position - missile.position;
                                    // float A = (V.sqrMagnitude - missile.gameObject.GetComponent<Rigidbody> ().velocity.magnitude) * missile.gameObject.GetComponent<Rigidbody> ().velocity.magnitude;
                                    // float B = 2 * Vector3.Dot (D, V);
                                    // float C = D.sqrMagnitude;
                                    // if (A >= 0) {
                                    //     Debug.LogError ("No solution exists");
                                    //     finalVectors = targetObject.gameObject.transform.position;
                                    // } else {
                                    //     float rt = Mathf.Sqrt (B * B - 4 * A * C);
                                    //     float dt1 = (-B + rt) / (2 * A);
                                    //     float dt2 = (-B - rt) / (2 * A);
                                    //     float dt = (dt1 < 0 ? dt2 : dt1);
                                    //     finalVectors = targetObject.gameObject.transform.position;
                                    // }
                                    if (TargetTest != null) {
                                        var dist = Vector3.Distance (finalVectors, LaunchedFrom.transform.position);
                                        TargetTest.transform.position = finalVectors;
                                        TargetTest.transform.localScale = TargetScale * dist;
                                    }
                                    // missile.LookAt (finalVectors);

                                    var ObjectToTargetVector = finalVectors - missile.position;
                                    var AIForward = missile.forward;
                                    var targetDirection = ObjectToTargetVector.normalized;
                                    var rotationAxis = Vector3.Cross (AIForward, targetDirection);
                                    var deltaAngle = Vector3.Angle (AIForward, targetDirection);

                                    missile.Rotate (rotationAxis, Mathf.Min (RotSpeed * Time.deltaTime, deltaAngle), Space.World);
                                }
                            } else {
                                if (usesOld) {
                                    missile.LookAt (Dafuq.transform);
                                } else if (missileType == 1) {
                                    var ObjectToTargetVector = Dafuq.transform.position - missile.position;
                                    var AIForward = missile.forward;
                                    var targetDirection = ObjectToTargetVector.normalized;
                                    var rotationAxis = Vector3.Cross (AIForward, targetDirection);
                                    var deltaAngle = Vector3.Angle (AIForward, targetDirection);

                                    missile.Rotate (rotationAxis, Mathf.Min (RotSpeed * Time.deltaTime, deltaAngle), Space.World);
                                } else if (missileType == 2) {
                                    Vector3 finalVectors;
                                    Vector3 V;
                                    V = Vector3.zero;
                                    finalVectors = FirstOrderIntercept (missile.gameObject.transform.position, missile.gameObject.GetComponent<Rigidbody> ().velocity, missile.gameObject.GetComponent<Rigidbody> ().velocity.magnitude, Dafuq.transform.position, V);
                                }
                            }
                        }
                        if (missed) {
                            if (!missedCalled) {
                                if(hasHitIndicator && LaunchedFrom.GetComponent<WeaponSelector>()!=null){
                                    LaunchedFrom.GetComponent<WeaponSelector>().miss = true;
                                }
                                targetObject.isChasing = false;
                                missedCalled = true;
                            }
                        }

                    }
                } else {
                    missile.LookAt (Dafuq.GetComponent<Transform> ());
                }
            }
        }
        // Sets the timer before the missile Object deletes itself for cleanup.

        if (isExploded) {
            if (!explodeSound) {
                if(hasHitIndicator && !missed && LaunchedFrom.GetComponent<WeaponSelector>()!=null && Dafuq!=null){
                    LaunchedFrom.GetComponent<WeaponSelector>().hit = true;
                }
                explodeSound = true;
                if (ExplosionSounds != null && ExplosionSounds.Length > 0) {
                    int rInt = Random.Range (0, ExplosionSounds.Length);
                    ExplosionSounds[rInt].Play ();
                }
            }
            if (destroyTimer < 5f) {
                destroyTimer += Time.deltaTime;
                if (Dafuq != null) {
                    if (Dafuq.GetComponent<MissileTrackerAndResponse> () != null && !calledOff) {
                        Dafuq.GetComponent<MissileTrackerAndResponse> ().isChasing = false;
                        Dafuq.GetComponent<MissileTrackerAndResponse> ().removeTracker(this);
                        calledOff = true;
                    } //This will disable the missile alert in your cockpit.

                }

            }
            if (destroyTimer > 5f) {
                DestroyImmediate (MissileClass);
            }
        }

        if (MissileTimer > timerLimit) { //Missile cleanup tool
            ExplodeMissile ();
            MissileTimer = 0;
        }
    }

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