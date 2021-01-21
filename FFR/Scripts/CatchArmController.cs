using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon;

public class CatchArmController : UdonSharpBehaviour
{
    public Animator AnimationControl;
    [UdonSynced(UdonSyncMode.None)] public bool isReady; // Catch Arm is Deployed
    [UdonSynced(UdonSyncMode.None)] public bool isCaught;
    [UdonSynced(UdonSyncMode.None)] public bool isPulling;
    [UdonSynced(UdonSyncMode.None)] public bool isLaunched;
    [UdonSynced(UdonSyncMode.None)] public bool onCooldown;
    [UdonSynced(UdonSyncMode.None)] public int selectedIndex = -1; //-1 means no activity.
    public LandingSystemControllerList LSC_list;
    public Transform Detector;
    public LayerMask DetectionLayer;
    public Transform SnagArea;
    public float detectionRange = 1000f;
    public float detectionRadius = 30f;
    public float snagRange = 3f;
    public Transform[] DebugHits;
    public LandingSystemController Current_LSC;
    public MissileTrackerAndResponse TrackerObject;
    public TriggerScript onCatchArmReady;
    public TriggerScript onCatch;
    public TriggerScript onLaunch;
    public TriggerScript onAbortApproach;
    public AudioSource onCatchSound;
    public AudioSource onLaunchSound;
    public AudioSource onReadySound;
    public float LaunchTime = 1f;
    private float LaunchTimer = 0f;
    public float launchCooldown = 3f;
    private float cooldownTimer = 0f;
    public float checkEvery = 5f;
    private float checkEveryTimer = 0f;
    public float isPullingTime = 0.5f;
    private float isPullingTimer = 0f;
    public float holdTime = 8f;
    [HideInInspector] public float holdTimer = 0f;
    private bool isPlayedSnag = false;
    private bool isPlayedLaunch = false;
    private bool isPlayedReady = false;
    public float LerpMultiplier = 1f;
    public float CatapultStrength = 300f;
    public bool isCatapult;
    public bool isCatchArm;
    VRCPlayerApi localPlayer;
    ConstraintSource constraintSource;
    //TODO:
    //Add a lerp time from flight to the catcher via isPulling

    void Start()
    {
        Assert(AnimationControl != null, "Start: AnimationControl != null");
        Assert(Detector != null, "Start: Detector != null");
        Assert(LSC_list != null, "Start: LSC LIST MUST NOT BE null");
        TrackerObject.isRendered = false;
        localPlayer = Networking.LocalPlayer;
    }

    public void snagCall()
    {
        if (Networking.IsOwner(gameObject))
        {
            isCaught = true;
            isPulling = true;
        }
    }

    void Update()
    {
        if (Detector != null)
        {
            // Debug.Log ("OWNER:" + Networking.IsOwner (gameObject));
            if (!isReady && !onCooldown && (localPlayer == null || Networking.IsOwner(gameObject)))
            {
                if (checkEveryTimer > checkEvery)
                {
                    checkEveryTimer = 0;
                    Debug.DrawLine(Detector.position, Detector.position + Detector.forward * detectionRange);
                    RaycastHit[] hit = Physics.SphereCastAll(Detector.position, detectionRadius, Detector.forward, detectionRange, DetectionLayer, QueryTriggerInteraction.Collide); //in case of shit happens like multiple rayhitted objects
                    DebugHits = new Transform[hit.Length];
                    for (int x = 0; x < hit.Length; x++)
                    {
                        // Debug.Log("HITS");
                        DebugHits[x] = hit[x].transform;
                    }
                    if (hit.Length > 0)
                    {
                        for (int x = 0; x < hit.Length; x++)
                        { // skim through the list of detected targets
                            GameObject currentHitObject = hit[x].transform.gameObject;
                            float hitDistance = hit[x].distance;
                            LandingSystemController bb = null;
                            if (currentHitObject.GetComponent<HitDetector>() != null && currentHitObject.GetComponent<HitDetector>().LSC != null)
                            {
                                bb = currentHitObject.GetComponent<HitDetector>().LSC;
                                for (int XX = 0; XX < LSC_list.LSC_LIST.Length; XX++)
                                {
                                    if (LSC_list.LSC_LIST[XX] == bb)
                                    {
                                        selectedIndex = XX;
                                        break;
                                    }
                                }

                                if (bb.EngineControl != null && !bb.isSnagged)
                                {
                                    if (!bb.EngineControl.EffectsControl.GearUp)
                                    {
                                        AnimationControl.SetBool("arm_deployed", true);
                                        isReady = true;
                                        TrackerObject.isRendered = true;
                                        // if(isCatchArm && bb.fhud!=null && bb.fhud.CatchArmReady!=null && !bb.fhud.CatchArmReady.activeSelf){
                                        //     bb.fhud.CatchArmReady.SetActive(true);
                                        // }
                                        Current_LSC = bb;
                                        // if(localPlayer!=null && bb.EngineControl.Piloting){
                                        //     Networking.SetOwner(bb.EngineControl.localPlayer, gameObject);
                                        // }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if(Current_LSC!=null && Current_LSC.fhud!=null && Current_LSC.fhud.CatchArmReady!=null && Current_LSC.fhud.CatchArmReady.activeSelf){
                            Current_LSC.fhud.CatchArmReady.SetActive(false);
                        }
                        Current_LSC = null;
                        selectedIndex = -1;
                        // Debug.Log ("SET NULL");
                    }
                }
                else
                {
                    checkEveryTimer = checkEveryTimer + Time.deltaTime;
                }
            }
            //Constant updater
            if (!Networking.IsOwner(gameObject) && selectedIndex != -1)
            {
                Current_LSC = LSC_list.LSC_LIST[selectedIndex]; // force update.
            }
            else
            {
                AnimationControl.SetBool("arm_deployed", false);
            }

            //fhud indicators
            // if(Current_LSC!=null && Current_LSC.fhud!=null){
            //     var fhud = Current_LSC.fhud;
            //     if(isReady && isCatapult && fhud.CatapultReady!=null && !fhud.CatapultReady.activeSelf){
            //         fhud.CatapultReady = 
            //     }
            // }

            if (selectedIndex != -1)
            {
                if (localPlayer != null && Current_LSC != null && Current_LSC.EngineControl != null && Current_LSC.EngineControl.Occupied == false && Networking.IsOwner(gameObject))
                { // in the event that a person has left the plane while snagged.
                    isReady = false;
                    isCaught = false;
                    isLaunched = false;
                    isPulling = false;
                    Current_LSC.isSnagged = false;
                    Current_LSC.EngineControl.CatapultStatus = 0;
                    Current_LSC.EngineControl.VehicleRigidbody.isKinematic = false;
                    // for (int Z = 0; Z < Current_LSC.constraint.sourceCount; Z++) {
                    //     Current_LSC.constraint.RemoveSource (Z);
                    // }
                    // // Current_LSC.constraint.RemoveSource (0);
                    // Current_LSC.constraint.constraintActive = false;
                    Current_LSC.CAC = null;
                    Current_LSC = null;
                    AnimationControl.SetBool("arm_deployed", false);
                    selectedIndex = -1;
                    Debug.Log("Release Snag; Unoccupied.");
                }
                if (isReady)
                {
                    AnimationControl.SetBool("arm_deployed", true);
                    if (isCatchArm && Current_LSC.fhud != null && Current_LSC.fhud.CatchArmReady != null && !Current_LSC.fhud.CatchArmReady.activeSelf)
                    {
                        Current_LSC.fhud.CatchArmReady.SetActive(true);
                    }
                    if (!isPlayedReady && onReadySound != null)
                        onReadySound.Play();

                    checkEveryTimer = 0;
                    if (!isCaught)
                    {
                        if (Current_LSC != null)
                        {
                            float distance = Vector3.Distance(Current_LSC.gameObject.transform.position, SnagArea.position);
                            Debug.DrawLine(SnagArea.position, Current_LSC.gameObject.transform.position);
                            if (distance > detectionRange || (Current_LSC != null && Current_LSC.isSnagged))
                            {
                                if (localPlayer == null || Networking.IsOwner(gameObject))
                                    isReady = false;
                                AnimationControl.SetBool("arm_deployed", false);
                                TrackerObject.isRendered = false;
                            }
                            if (distance <= snagRange)
                            {
                                AnimationControl.SetBool("arm_caught", true);
                                Current_LSC.CAC = this;
                                 if (isCatchArm && Current_LSC.fhud != null && Current_LSC.fhud.CatchArmReady != null && !Current_LSC.fhud.CatchArmReady.activeSelf)
                                {
                                    Current_LSC.fhud.CatchArmReady.SetActive(false);
                                }
                                if (isCatchArm && Current_LSC.fhud != null && Current_LSC.fhud.CatchArmLocked != null && !Current_LSC.fhud.CatchArmLocked.activeSelf)
                                {
                                    Current_LSC.fhud.CatchArmLocked.SetActive(true);
                                }
                                if (isCatapult && Current_LSC.fhud != null && Current_LSC.fhud.CatapultReady != null && !Current_LSC.fhud.CatapultReady.activeSelf)
                                {
                                    Current_LSC.fhud.CatapultReady.SetActive(true);
                                }
                                if (Current_LSC.EngineControl.localPlayer == null || Networking.IsOwner(Current_LSC.EngineControl.VehicleMainObj))
                                {
                                    Networking.SetOwner(Current_LSC.EngineControl.localPlayer, gameObject);
                                    Current_LSC.EngineControl.VehicleRigidbody.isKinematic = true;
                                    Current_LSC.isSnagged = true;
                                    if (localPlayer == null) { snagCall(); }
                                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "snagCall");
                                    Current_LSC.EngineControl.dead = true;
                                    Current_LSC.EngineControl.CatapultStatus = 1;
                                    // Debug.Log ("Caught");
                                }
                                // Current_LSC.EngineControl.CatapultStatus = 1;
                            }
                            if(Current_LSC.EngineControl.EffectsControl.GearUp){
                                AnimationControl.SetBool("arm_deployed", false);
                                isReady = false;
                                TrackerObject.isRendered = false;
                                Current_LSC.CAC = null;
                                Current_LSC = null;
                            }
                        }
                        else
                        {
                            if (localPlayer == null || Networking.IsOwner(gameObject))
                            {
                                selectedIndex = -1;
                                isReady = false;
                            }
                            AnimationControl.SetBool("arm_deployed", false);
                            TrackerObject.isRendered = false;
                            Current_LSC = null;
                        }
                    }
                    if (isCaught)
                    {
                        // Debug.Log ("Caught Logic Running");
                        AnimationControl.SetBool("arm_caught", true);
                        if (Current_LSC != null)
                        { // checker for current LSC
                            TrackerObject.isRendered = false;

                            if (isPulling)
                            {
                                if (Current_LSC.EngineControl.localPlayer == null || Networking.IsOwner(Current_LSC.EngineControl.VehicleMainObj))
                                {
                                    Vector3 lerpPos = Vector3.Lerp(Current_LSC.EngineControl.VehicleMainObj.transform.position, SnagArea.position, LerpMultiplier * isPullingTimer);
                                    Quaternion lerpRot = Quaternion.Lerp(Current_LSC.EngineControl.VehicleMainObj.transform.rotation, SnagArea.rotation, LerpMultiplier * isPullingTimer);
                                    Current_LSC.EngineControl.VehicleMainObj.transform.position = lerpPos;
                                    Current_LSC.EngineControl.VehicleMainObj.transform.rotation = lerpRot;
                                }
                                isPullingTimer = isPullingTimer + Time.deltaTime;
                                if (isPullingTimer > isPullingTime)
                                {
                                    isPullingTimer = 0;
                                    isPulling = false;

                                    if (Current_LSC.EngineControl.Piloting)
                                        Current_LSC.EngineControl.dead = false;

                                    // constraintSource.sourceTransform = SnagArea;
                                    // constraintSource.weight = 1;
                                    // Current_LSC.constraint.AddSource (constraintSource);
                                    // Current_LSC.constraint.constraintActive = true;
                                }
                            }
                            else
                            {
                                // if (Current_LSC.source.sourceTransform == null) {

                                // }
                                // if (Current_LSC.EngineControl.localPlayer == null || Networking.IsOwner (Current_LSC.EngineControl.VehicleMainObj)) {
                                //Broadcast to everyone.
                                Current_LSC.EngineControl.VehicleMainObj.transform.position = SnagArea.position;
                                Current_LSC.EngineControl.VehicleMainObj.transform.rotation = SnagArea.rotation;
                                // }
                            }

                            if (holdTimer < holdTime)
                                holdTimer = holdTime + Time.deltaTime;
                        }
                        else
                        {
                            if (localPlayer == null || Networking.IsOwner(gameObject))
                            {
                                isCaught = false;
                            }
                        }
                    }
                    if (isLaunched)
                    {
                        AnimationControl.SetBool("launch", true);
                        LaunchTimer = LaunchTimer + Time.deltaTime;
                        if (isCatchArm && Current_LSC.fhud != null && Current_LSC.fhud.CatchArmReady != null && Current_LSC.fhud.CatchArmReady.activeSelf)
                        {
                            Current_LSC.fhud.CatchArmReady.SetActive(false);
                        }
                        if (isCatchArm && Current_LSC.fhud != null && Current_LSC.fhud.CatchArmLocked != null && Current_LSC.fhud.CatchArmLocked.activeSelf)
                        {
                            Current_LSC.fhud.CatchArmLocked.SetActive(false);
                        }
                        if (isCatapult && Current_LSC.fhud != null && Current_LSC.fhud.CatapultReady != null && Current_LSC.fhud.CatapultReady.activeSelf)
                        {
                            Current_LSC.fhud.CatapultReady.SetActive(false);
                        }
                        if (LaunchTimer > LaunchTime)
                        {
                            if (Current_LSC.EngineControl.localPlayer == null || Networking.IsOwner(Current_LSC.EngineControl.VehicleMainObj))
                            {
                                Current_LSC.EngineControl.CatapultStatus = 0;
                            }
                            // for (int Z = 0; Z < Current_LSC.constraint.sourceCount; Z++) {
                            //     Current_LSC.constraint.RemoveSource (Z);
                            // }
                            // Current_LSC.constraint.RemoveSource (0);
                            // Current_LSC.constraint.constraintActive = false;
                            AnimationControl.SetBool("arm_deployed", false);
                            AnimationControl.SetBool("arm_caught", false);
                            AnimationControl.SetBool("launch", false);
                            LaunchTimer = 0;
                            if (localPlayer == null || Networking.IsOwner(gameObject))
                            {
                                isLaunched = false;
                                onCooldown = true;
                                Current_LSC.CAC = null;
                                selectedIndex = -1;
                                isCaught = false;
                                isReady = false;
                                isPulling = false;
                            }
                            Current_LSC.isSnagged = false;
                            Current_LSC = null;
                            holdTimer = 0;
                        }
                    }
                }

            }
            if (onCooldown)
            {
                AnimationControl.SetBool("arm_deployed", false);
                AnimationControl.SetBool("arm_caught", false);
                AnimationControl.SetBool("launch", false);
                cooldownTimer = cooldownTimer + Time.deltaTime;
                if (cooldownTimer > launchCooldown)
                {
                    cooldownTimer = 0;
                    if (localPlayer == null || Networking.IsOwner(gameObject))
                        onCooldown = false;
                }
            }
        }
    }

    public void Launch()
    {
        isLaunched = true;
        if (Current_LSC != null && Current_LSC.EngineControl.Piloting)
        {
            Current_LSC.EngineControl.VehicleRigidbody.isKinematic = false;
            Current_LSC.EngineControl.CatapultLaunchStrength = CatapultStrength;
            Current_LSC.EngineControl.CatapultStatus = 2;
        }
    }

    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}