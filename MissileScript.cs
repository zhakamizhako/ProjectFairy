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
    public ParticleSystem ExplosionEffect;
    public AudioSource[] ExplosionSounds;
    private bool isExploded = false;
    private float destroyTimer = 0;
    private float MissileTimer = 0;
    private float missileColliderOn = 0.5f;
    private float missileColliderTimer = 0;
    public float missileSpeed = 1000;
    public int explosionAmount = 20;
    private bool follow = false;
    private bool isSet = false;
    private bool explodeSound = false;
    public float timerLimit = 10; //missile life in case of no targets and went through clips or sky
    [UdonSynced (UdonSyncMode.None)] public Vector3 targetDirection;

    void Start () {
        // Debug.Log ("MissileScript Initialized");
        if (Target != null) {
            // Debug.Log ("I Have Target!");
            if (Target.GetComponent<MissileTargeterParent> () != null) {
                var b = Target.GetComponent<MissileTargeterParent> ();
                if (b.Target != null && b.noTarget == false) {
                    Dafuq = b.Target;
                }
            }else if(Target.GetComponent<AITurretScript>()!=null){
                var b = Target.GetComponent<AITurretScript>();
                if(b.Target!=null){
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
        ExplosionEffect.Emit (explosionAmount);
        isExploded = true;
        destroyTimer = 0;
        MissileClass.GetComponent<ConstantForce> ().relativeForce = new Vector3 (0, 0, 0); //Stop constant forward
        MissileClass.GetComponent<Rigidbody> ().velocity = new Vector3 (0, 0, 0); // In bisaya terms, "PARA SURE GYUD NA DILI MULIHOK."
        //Freeze Missile Object
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionZ;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionX;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionY;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationZ;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationX;
        MissileClass.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeRotationY;
    }

    void Update () {
        if (missileColliderTimer < missileColliderOn) { //Checker for initial missile fire to avoid collision
            missileColliderTimer += Time.deltaTime;
        } else if (missileColliderTimer > missileColliderOn && isSet == false) {
            Collider mCollider = MissileClass.GetComponent<Collider> ();
            mCollider.enabled = true;
            // follow = true;
            isSet = true;
        }
        if (MissileTimer < timerLimit) { //Missile cleanu ptool
            MissileTimer += Time.deltaTime;
        }
        if (!isExploded) {
            MissileClass.GetComponent<ConstantForce> ().relativeForce = new Vector3 (0, 0, missileSpeed);
            //Tail the target. First checks if whether the target has a Tailer which is where the missile should chase after.
            //If not because you're lazy to assign one, then it tracks the targeter's location.
            if (Dafuq != null) {
                var targetObject = Dafuq.GetComponent<MissileTrackerAndResponse> ();
                Transform missile = MissileClass.GetComponent<Transform> ();
                if (targetObject != null) {
                    // Debug.Log("Sending Alarm Sound");
                    targetObject.isChasing = true;
                    if (MissileTimer >.3f) {
                        if (targetObject.Tailer != null) {
                            missile.LookAt (targetObject.Tailer);
                        } else {
                            missile.LookAt (Dafuq.GetComponent<Transform> ());
                        }
                    }
                } else {
                    missile.LookAt (Dafuq.GetComponent<Transform> ());
                    // targetDirection = MissileClass.GetComponent<Transform> ().position - Dafuq.GetComponent<Transform> ().position;
                }

                // if (targetObject.Tailer != null) {
                //     targetDirection = MissileClass.GetComponent<Transform> ().position - targetObject.Tailer.position;
                // } else {
                //     targetDirection = MissileClass.GetComponent<Transform> ().position - Dafuq.GetComponent<Transform> ().position;
                // }
                // var rotatory = Quaternion.RotateTowards (missile.rotation, Quaternion.LookRotation (-targetDirection), Time.time * 5                                                                                                                     );
                // MissileClass.GetComponent<Rigidbody> ().MoveRotation (rotatory);
            }
        }
        // Sets the timer before the missile Object deletes itself for cleanup.

        if (isExploded) {
            if (!explodeSound) {
                explodeSound = true;
                if (ExplosionSounds != null && ExplosionSounds.Length > 0) {
                    int rInt = Random.Range (0, ExplosionSounds.Length);
                    ExplosionSounds[rInt].Play ();
                }
            }
            if (destroyTimer < 5f) {
                destroyTimer += Time.deltaTime;
                if (Dafuq != null) {
                    if (Dafuq.GetComponent<MissileTrackerAndResponse> () != null) {
                        Dafuq.GetComponent<MissileTrackerAndResponse> ().isChasing = false;
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
}