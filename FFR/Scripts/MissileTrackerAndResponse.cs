using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class MissileTrackerAndResponse : UdonSharpBehaviour
{
    [Header("Tracker Properties")]
    public Transform Tailer; // Attach the Tailer to the flareObject, mind you.
    public Transform Tailer2; // for the AA Guns to track. I dont think we need to drop this.
    public GameObject MainObject;
    public AITurretScript AITurret;
    public AIObject AI;
    public EngineController EngineController;
    public TarpsMode tarps;

    [Header("Tracker Settings")]
    public bool isTargetable = true;
    public bool isRendered = true;
    public bool isGloballyRendered = true;
    public bool isGloballyTargetable = true;
    public bool isWaypoint = false;
    public bool isWaypointEnabled = true;
    public bool isWaypointEnabledSync = true;
    [UdonSynced(UdonSyncMode.None)] public bool isTracking = false;
    public bool isChasing = false;
    private bool soundStarted = false;
    private bool soundCautionStarted = false;
    [Header("Waypoint Settings")]
    public TriggerScript onEnter; // For waypoints
    public Transform WaypointDetector;
    public float WaypointDetectorRange = 100;
    // public LayerMask layermask = 23;
    public bool VehicleOnlyWaypoint = true;
    public bool hideAfterWaypointContact = true;
    public bool hideInSyncContact = true;
    private bool globalCheck = false;
    private bool hasRendered = false;
    private bool hasTargeted = false;
    private bool hasWaypointEnabled = false;
    public bool isEnemy = false;
    public bool isAlly = false;
    public bool isUnknown = false;
    public bool isObjective = false;
    private bool startIsAlly;
    private bool startIsEnemy;
    private bool startIsUnknown;
    private bool startIsObjective;
    [Header("Display Settings")]
    public GameObject TargetIconRender;
    public Text TrackerText;
    // public Text PlayerText;
    // public Text DistanceText;
    // public Text HealthText;
    public GameObject textIsAlly;
    public GameObject textIsEnemy;
    public GameObject textIsUknown;
    public GameObject textIsObjective;
    [Header("Indicator Warnings")]
    public GameObject hudAlert;
    public AudioSource CautionSound;
    public AudioSource MissileAlert;
    public GameObject CautionObject;
    public MissileScript[] TrackList;
    public GameObject[] Indicators;
    public GameObject IndicatorPrefabVR;
    public GameObject IndicatorPrefab;
    public Transform Placement;
    public Transform PlacementDesktop;
    public MissileScript temporary;
    private GameObject runtime;
    public bool testVR = false;
    public bool ShowTargets = false;
    public Transform debug;
    private Vector3 ScaleTarget = new Vector3(0.01f, 0.01f, 0.01f);
    public bool isSelected = false;
    public Transform FollowL;
    public Transform FollowR;
    // public Transform[] DebugHits;
    public bool culled = false;
    public PlayerUIScript UIScript;
    // public bool isGroundTarget = false;
    // public bool isAirTarget = false;
    public bool ShowDistance = true;
    // public bool showX = false;
    public bool ShowText = true;
    public GameObject XIcon;
    private Renderer IconRenderer;
    public bool HideIfFar = true;
    public float farDistance = 12000f;
    public bool showneverthelessDistance = false;
    public float currentDistance = 0f;
    public float tempAngleCheck = 0f;
    public float cullAngle = 75f;
    private float tempHealth = 0f;
    private bool healthUpdate = false;
    private string words = "";
    // public PathwaySystem pathway;
    // public bool isPathway = false;
    // public bool showInRadar = true;

    public RadarRender[] RadarRenders;

    [Header("Objective Markers only")]
    public ObjectiveIndicator objectiveIndicatorPrefab;
    public bool showObjectiveTracker = false;
    //ui
    public GameObject[] OBJIndicators;
    public bool isRenderedMarker = false;
    public Image RadarIcon;
    private VRCPlayerApi localPlayer;
    private float dist = 0;

    public bool RunOnDestroy = false;
    public bool RunOnLock = false;
    public bool RunOnMissile = false;

    private bool ranDestroy = false;
    private bool ranLocks = false;
    private bool ranMissile = false;

    public TriggerScript[] RunDestroys;
    public TriggerScript[] RunLocks;
    public TriggerScript[] RunMissiles;
    public bool AddIgnore = false;

    void Start()
    {
        hasRendered = isRendered;
        hasTargeted = isTargetable;
        hasWaypointEnabled = isWaypointEnabled;
        startIsObjective = isObjective;
        startIsAlly = isAlly;
        startIsAlly = isEnemy;
        startIsUnknown = isUnknown;
        if (IndicatorPrefab != null)
        {
            IndicatorPrefab.SetActive(false);
        }
        if (IndicatorPrefabVR != null)
        {
            IndicatorPrefabVR.SetActive(false);
        }
        ShowTargets = false;
        TargetIconRender.SetActive(false);

        if (TargetIconRender != null)
        {
            IconRenderer = TargetIconRender.GetComponent<Renderer>();
        }

        localPlayer = Networking.LocalPlayer;

        if (isWaypoint && UIScript == null)
        {
            Debug.LogError("Waypoint but no UI Script present! Waypoint will not work!");
        }
        if (UIScript == null || IconRenderer==null)
        {
            Debug.LogError("[ZHAK_Trackers] UI Script Missing And/Or Icon is missing!");
        }
    }
    public void receiveTracker(MissileScript misScript)
    {
        if (EngineController != null && (EngineController.Piloting || EngineController.Passenger))
        {
            MissileScript[] temp = new MissileScript[TrackList.Length + 1];
            TrackList.CopyTo(temp, 0);

            temp[temp.Length - 1] = misScript;
            TrackList = temp;

            GameObject[] temp2 = new GameObject[Indicators.Length + 1];
            Indicators.CopyTo(temp2, 0);

            if (Placement != null && ((EngineController.localPlayer == null && testVR) || EngineController.localPlayer != null && EngineController.localPlayer.IsUserInVR()) && (IndicatorPrefabVR != null && Indicators != null))
            {
                runtime = VRCInstantiate(IndicatorPrefabVR);
                runtime.transform.parent = Placement;
                runtime.transform.position = IndicatorPrefabVR.transform.position;
                runtime.transform.localScale = IndicatorPrefabVR.transform.localScale;
                // Debug.Log("VR---!");
            }
            else
            if (PlacementDesktop != null && ((EngineController.localPlayer == null && !testVR) || EngineController.localPlayer != null && !EngineController.localPlayer.IsUserInVR()) && (IndicatorPrefab != null && Indicators != null))
            {

                runtime = VRCInstantiate(IndicatorPrefab);
                runtime.transform.parent = PlacementDesktop;
                runtime.transform.position = IndicatorPrefab.transform.position;
                runtime.transform.localScale = IndicatorPrefab.transform.localScale;
            }
            runtime.SetActive(true);
            temp2[temp2.Length - 1] = runtime;
            Indicators = temp2;
            runtime = null;
            temporary = misScript;
        }
    }

    public void AddObjectiveMarker(ObjectiveIndicator obj)
    {
        Debug.Log("Attempting to add");
        if (UIScript == null || UIScript.PlayerAircraft == null)
        {
            Debug.LogError("MissingUISCript or PlayerAircraft");
            return;
        }

        var runtime2 = VRCInstantiate(obj.gameObject);

        GameObject[] temp = new GameObject[OBJIndicators.Length + 1];
        OBJIndicators.CopyTo(temp, 0);

        temp[temp.Length - 1] = runtime2;
        OBJIndicators = temp;
        // GameObject[] temp2 = new GameObject[OBJIndicators.Length + 1];
        // OBJIndicators.CopyTo(temp2, 0);

        runtime2.SetActive(true);
        // temp2[temp2.Length - 1] = runtime2;
        // OBJIndicators = temp2;
        Debug.Log("AAdded");
        // runtime = null;

    }

    public void RemoveObjectiveTracker(ObjectiveIndicator obj)
    {
        if (UIScript == null || UIScript.PlayerAircraft == null)
        {
            return;
        }

        bool found = false;
        for (int x = 0; x < OBJIndicators.Length; x++)
        {
            if (OBJIndicators[x] != null && OBJIndicators[x] == obj.gameObject)
            {
                found = true;
            }
        }
        if (found)
        {
            GameObject[] temp = new GameObject[OBJIndicators.Length - 1];
            // Debug.Log("B");
            int y = 0;
            for (int x = 0; x < OBJIndicators.Length; x++)
            {
                // Debug.Log("SCAN");
                if (OBJIndicators[x] != obj.gameObject)
                {
                    temp[y] = OBJIndicators[x];
                    y = y + 1;
                    // Debug.Log("INCREMENT");
                }
                else
                {
                    OBJIndicators[x].GetComponent<ObjectiveIndicator>().toDestroy();
                    // Destroy(Indicators[x]);
                    Debug.Log("DESTROY");
                }
            }
            OBJIndicators = temp;
        }
        else
        {
            Debug.Log("Not found");
        }
    }


    public void cleanupOBJMarkers()
    {
        Debug.Log("CLEANUP");
        for (int x = 0; x < OBJIndicators.Length; x++)
        {
            if (OBJIndicators[x] != null)
            {
                DestroyImmediate(OBJIndicators[x]);
            }
        }
        OBJIndicators = new GameObject[0];
        Debug.Log("CLEANDUP OBJIndicators");
    }

    void OnBecameVisible()
    {
        culled = false;
    }

    void OnBecameInvisible()
    {
        culled = true;
    }

    public void cleanup()
    {
        Debug.Log("CLEANUP");
        for (int x = 0; x < Indicators.Length; x++)
        {
            if (Indicators[x] != null)
            {
                DestroyImmediate(Indicators[x]);
            }
        }
        TrackList = new MissileScript[0];
        Indicators = new GameObject[0];
        Debug.Log("CLEANDUP");
    }
    public void removeTracker(MissileScript misScript)
    {
        if (EngineController != null && (EngineController.Piloting || EngineController.Passenger))
        {
            Debug.Log("A");
            bool found = false;
            if (TrackList.Length == Indicators.Length)
            { // If list matches
                for (int x = 0; x < TrackList.Length; x++)
                {
                    if (TrackList[x] != null && TrackList[x] == misScript && Indicators[x] != null)
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    MissileScript[] temp = new MissileScript[TrackList.Length - 1];
                    GameObject[] temp2 = new GameObject[Indicators.Length - 1];
                    // Debug.Log("B");
                    int y = 0;
                    for (int x = 0; x < TrackList.Length; x++)
                    {
                        // Debug.Log("SCAN");
                        if (TrackList[x] != misScript)
                        {
                            temp[y] = TrackList[x];
                            if (Indicators[x] != null)
                            {
                                temp2[y] = Indicators[x];
                                // Debug.Log("ASSIGN");
                            }
                            y = y + 1;
                            // Debug.Log("INCREMENT");
                        }
                        else
                        {
                            Indicators[x].GetComponent<IndicatorScript>().toDestroy();
                            // Destroy(Indicators[x]);
                            Debug.Log("DESTROY");
                        }
                    }
                    // Debug.Log("C");
                    TrackList = temp;
                    Indicators = temp2;
                    // Debug.Log("DONE");
                }
            }
            else
            { //Force cleanup the damn list in order to avoid crashing. 
                cleanup();
            }
        }
    }

    public void hideSync()
    {
        isRendered = false;
        isTargetable = false;
    }

    public void showSync()
    {
        isRendered = true;
    }

    public void targetSync()
    {
        isTargetable = true;
    }
    public void targetHideSync()
    {
        isTargetable = false;
    }

    public void waypointEnabled()
    {
        isWaypointEnabled = true;
    }

    public void waypointDisabled()
    {
        isWaypointEnabled = false;
    }

    void LateUpdate()
    {
        if (isWaypoint && isWaypointEnabled)
        {
            dist = Vector3.Distance(localPlayer.GetPosition(), WaypointDetector.position);
            if (dist < WaypointDetectorRange && UIScript != null && ((VehicleOnlyWaypoint && UIScript.PlayerAircraft != null) || !VehicleOnlyWaypoint))
            {
                if (hideAfterWaypointContact)
                {
                    isRendered = false;
                    isWaypointEnabled = false;
                }
                if (hideInSyncContact)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hideSync");
                }
                if (onEnter != null)
                {
                    Debug.Log("Enter");
                    // onEnter.run = true;
                    UIScript.AddToQueueScript(onEnter);
                }
            }
        }

        if (isRendered && ShowText)
        {
            if (textIsAlly != null)
            {
                if (isAlly && textIsAlly.activeSelf != true)
                {
                    textIsAlly.SetActive(true);
                }
                else if (!isAlly && textIsAlly.activeSelf == true)
                {
                    textIsAlly.SetActive(false);
                }
            }
            if (textIsEnemy != null)
            {
                if (isEnemy && textIsEnemy.activeSelf != true)
                {
                    textIsEnemy.SetActive(true);
                }
                else if (!isEnemy && textIsEnemy.activeSelf == true)
                {
                    textIsEnemy.SetActive(false);
                }
            }
            if (textIsUknown != null)
            {
                if (isUnknown && textIsUknown.activeSelf != true)
                {
                    textIsUknown.SetActive(true);
                }
                else if (!isUnknown && textIsUknown.activeSelf == true)
                {
                    textIsUknown.SetActive(false);
                }
            }
            if (textIsObjective != null)
            {
                if (isObjective && textIsObjective.activeSelf != true)
                {
                    textIsObjective.SetActive(true);
                }
                else if (!isObjective && textIsObjective.activeSelf == true)
                {
                    textIsObjective.SetActive(false);
                }
            }
        }
    }
    void Update()
    {
        if (HideIfFar)
        {
            currentDistance = Vector3.Distance(Networking.LocalPlayer != null ? Networking.LocalPlayer.GetPosition() : Vector3.zero, gameObject.transform.position);
            var distance = Vector3.Distance(Networking.LocalPlayer != null ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero, gameObject.transform.position);
            var ObjectToTargetVector = gameObject.transform.position - (Networking.LocalPlayer != null ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero);
            var AIForward = Networking.LocalPlayer != null ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward : Vector3.zero;
            var targetDirection = ObjectToTargetVector.normalized;
            tempAngleCheck = Vector3.Angle(targetDirection, AIForward);
            if ((currentDistance > farDistance && !showneverthelessDistance) || tempAngleCheck > cullAngle)
            {
                culled = true;
            }
            else
            {
                culled = false;
            }
        }

        if (ShowTargets)
        {
            if (isRendered && !culled)
            {
                Vector3 lookatPos = Vector3.zero;
                if (!TargetIconRender.activeSelf) { TargetIconRender.SetActive(true); }
                if (Networking.LocalPlayer != null)
                {
                    lookatPos = Networking.LocalPlayer.GetPosition();
                }
                else if (debug != null)
                {
                    lookatPos = debug.position;
                }
                else
                {
                    lookatPos = Vector3.zero;
                }
                TargetIconRender.transform.LookAt(lookatPos);
                var dist = Vector3.Distance(lookatPos, gameObject.transform.position);
                TargetIconRender.transform.localScale = UIScript != null ? (new Vector3(UIScript.IconSize, UIScript.IconSize, UIScript.IconSize) * dist) : ScaleTarget * dist;

                if (TrackerText != null)
                {
                    var words = "";
                    if (MainObject != null)
                    { //Target Name
                        words = words + "" + MainObject.name;
                    }
                    if (EngineController != null)
                    { // For Player Based Targets
                        if (EngineController.Occupied && EngineController.PilotName != null)
                        { //Pilot Name
                            words = words + "\n" + EngineController.PilotName;
                        }
                        if (EngineController.Health > 0 && !EngineController.dead)
                        { //Health
                            words = words + "\nHP:" + EngineController.Health;
                            if (IconRenderer.material.color != Color.white && !isSelected)
                            {
                                IconRenderer.material.SetColor("_Color", Color.white);
                            }
                            else if (isSelected)
                            {
                                if (IconRenderer.material.color != Color.yellow)
                                    IconRenderer.material.SetColor("_Color", Color.yellow);
                            }
                            else if (!isSelected)
                            {
                                IconRenderer.material.SetColor("_Color", Color.white);
                            }
                        }
                        else
                        {
                            words = words + "\nDestroyed";
                            if (IconRenderer.material.color != Color.red)
                                IconRenderer.material.SetColor("_Color", Color.red);
                        }

                    }
                    if (AITurret != null)
                    { // For Turret Based Targets

                        if (AITurret.Health > 0 && AITurret.damageable)
                        { //Health
                            words = words + "\nHP:" + AITurret.Health;
                            if (IconRenderer.material.color != Color.white && !isSelected)
                            {
                                IconRenderer.material.SetColor("_Color", Color.white);
                            }
                            else if (isSelected)
                            {
                                if (IconRenderer.material.color != Color.yellow)
                                    IconRenderer.material.SetColor("_Color", Color.yellow);
                            }
                            else if (!isSelected)
                            {
                                IconRenderer.material.SetColor("_Color", Color.white);
                            }
                        }
                        else
                        {
                            words = words + "\nDestroyed";
                            if (IconRenderer.material.color != Color.red)
                                IconRenderer.material.SetColor("_Color", Color.red);
                        }

                    }
                    if (AI != null)
                    { // For AI based targets
                        if (AI.Health > 0 && AI.damageable)
                        { //Health
                            words = words + "\nHP:" + AI.Health;
                            if (IconRenderer.material.color != Color.white && !isSelected)
                            {
                                IconRenderer.material.SetColor("_Color", Color.white);
                            }
                            else if (isSelected)
                            {
                                if (IconRenderer.material.color != Color.yellow)
                                    IconRenderer.material.SetColor("_Color", Color.yellow);
                            }
                            else if (!isSelected)
                            {
                                IconRenderer.material.SetColor("_Color", Color.white);
                            }
                        }
                        else if (AI.Health <= 0)
                        {
                            words = words + "\nDestroyed";
                            if (IconRenderer.material.color != Color.red)
                                IconRenderer.material.SetColor("_Color", Color.red);
                        }
                    }
                    if (ShowDistance || isSelected)
                    {
                        float distance = 0f;
                        if (EngineController != null && EngineController.localPlayer != null)
                        {
                            distance = Vector3.Distance(EngineController.localPlayer.GetPosition(), TargetIconRender.transform.position);
                        }
                        else
                        {
                            distance = Vector3.Distance(lookatPos, TargetIconRender.transform.position);
                        }

                        words = words + "\n" + Mathf.Round(distance);
                    }
                    if (TrackerText.text != words) TrackerText.text = words;
                }
            }
            else
            {
                if (TargetIconRender.activeSelf)
                    TargetIconRender.SetActive(false);
            }

            if (isRendered)
            {
                if (objectiveIndicatorPrefab != null && showObjectiveTracker && isObjective)
                {
                    if (!isRenderedMarker && culled)
                    {
                        AddObjectiveMarker(objectiveIndicatorPrefab);
                        isRenderedMarker = true;
                    }
                    if (isRenderedMarker && !culled)
                    {
                        RemoveObjectiveTracker(objectiveIndicatorPrefab);
                        isRenderedMarker = false;
                    }
                }
            }

            if (!isRendered && OBJIndicators != null && OBJIndicators.Length > 0)
            {
                cleanupOBJMarkers();
                isRenderedMarker = false;
            }
        }
        else
        {
            if (TargetIconRender.activeSelf)
                TargetIconRender.SetActive(false);
        }
        // if (isChasing == true)
        // {
        //     // Debug.Log("Someone's chasing me.");
        // }
        if (isWaypoint && isWaypointEnabledSync)
        {
            if (isWaypointEnabled && isWaypointEnabled != hasWaypointEnabled)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "waypointEnabled");
                // Debug.Log("called once waypoint");
            }
            else if (!isWaypointEnabled && isWaypointEnabled != hasWaypointEnabled)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "waypointDisabled");
                // Debug.Log("called once disabled");
            }
            hasWaypointEnabled = isWaypointEnabled;
        }
        if (isGloballyRendered)
        {
            if (isRendered && isRendered != hasRendered)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "showSync");
                // Debug.Log("called once");
            }
            else if (!isRendered && isRendered != hasRendered)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hideSync");
                // Debug.Log("called once hidden");
            }
            hasRendered = isRendered;
        }
        if (isGloballyTargetable)
        {
            if (isTargetable && isTargetable != hasTargeted)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "targetSync");
                // Debug.Log("called once target");
            }
            else if (!isTargetable && isTargetable != hasTargeted)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "targetHideSync");
                // Debug.Log("called once target hidden");
                // globalCheck = true;
            }
            hasTargeted = isTargetable;
        }

        if (isChasing == true && soundStarted == false)
        { // Yeah, a constant checker i know. 
            if (MissileAlert != null)
                MissileAlert.Play();
            soundStarted = true;
            if (hudAlert != null)
                hudAlert.SetActive(true);
            // Debug.Log("Alert sound started playing");

            if (RunOnMissile && RunMissiles != null && RunMissiles.Length > 0 && !ranMissile &&  Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                ranMissile = true;
                int xRand = Random.Range(AddIgnore ? -1 : 0, RunMissiles.Length);
                if (xRand != -1)
                {
                    // RunMissiles[xRand].run = true;
                    UIScript.AddToQueueScript(RunMissiles[xRand]);
                }
            }
        }
        else if (soundStarted == true && isChasing == false)
        {
            if (hudAlert != null)
                hudAlert.SetActive(false);
            if (MissileAlert != null)
                MissileAlert.Stop();
            soundStarted = false;
            ranMissile = false;
            // Debug.Log("Alert sound Stopped");
        }

        if (isTracking)
        {
            if (CautionSound != null && soundCautionStarted == false)
            {
                CautionSound.Play();
                if (CautionObject != null)
                    CautionObject.SetActive(true);
                soundCautionStarted = true;
                if (RunOnLock && RunLocks != null && RunLocks.Length > 0 && !ranLocks && Networking.IsOwner(Networking.LocalPlayer, gameObject))
                {
                    ranLocks = true;
                    int xRand = Random.Range(AddIgnore ? -1 : 0, RunLocks.Length);
                    if (xRand != -1)
                    {
                        // RunLocks[xRand].run = true;
                        UIScript.AddToQueueScript(RunLocks[xRand]);
                    }
                }

            }
        }
        else
        {
            if (CautionSound != null && soundCautionStarted == true)
            {
                CautionSound.Stop();
                if (CautionObject != null)
                    CautionObject.SetActive(false);
                soundCautionStarted = false;
                ranLocks = false;
            }
        }

        // On Destroy
        if (RunOnDestroy && RunDestroys != null && RunDestroys.Length > 0)
        {
            if (!ranDestroy)
            {
                bool run = false;
                if (EngineController != null)
                {
                    if (EngineController.dead) run = true;
                }

                if (AI != null)
                {
                    if(AI.dead) run = true;
                }

                if (AITurret != null)
                {
                    if(AITurret.dead) run = true;
                }

                if (run && Networking.IsOwner(Networking.LocalPlayer, gameObject))
                {
                    int randx = Random.Range(AddIgnore ? -1 : 0, RunDestroys.Length);
                    if (randx != -1)
                    {
                        // RunDestroys[randx].run = true;
                        UIScript.AddToQueueScript(RunDestroys[randx]);
                    }
                    ranDestroy = true;
                }
            }else{
                if((EngineController!=null && !EngineController.dead) || (AI!=null && !AI.dead) || (AITurret!=null && !AITurret.dead)){
                    ranDestroy = false;
                }
            }
        }
        //cleanups
        if (EngineController != null)
        {
            if (EngineController.dead || (!EngineController.Piloting && !EngineController.Passenger))
            {
                if (CautionSound != null && soundCautionStarted == true)
                {
                    CautionSound.Stop();
                    if (CautionObject != null)
                        CautionObject.SetActive(false);
                    soundCautionStarted = false;
                }
                if (MissileAlert != null)
                {
                    MissileAlert.Stop();
                    soundStarted = false;
                    if (hudAlert != null)
                        hudAlert.SetActive(false);
                }
            }
        }

    }
}