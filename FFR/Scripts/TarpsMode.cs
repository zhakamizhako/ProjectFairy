
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TarpsMode : UdonSharpBehaviour
{
    public Transform Detector;
    public Animator UITarpsAni;
    public Animator TarpsAni;
    public TarpsTarget CurrentTarpsTarget;
    public TarpsArea[] tarpAreas;
    public WeaponSelector weaponSelector;
    public TarpsList tarpList;
    public LayerMask TarpsLayer;
    public float detectorRange = 1000f;
    public float detectorRadius = 100f;
    public int selectedTarpsIndex = -1;
    [UdonSynced(UdonSyncMode.None)] public bool isSelected = false;
    [UdonSynced(UdonSyncMode.None)] public bool isScanning = false;
    public float pingEvery = 1f;
    private float pingTimer = 0f;
    [UdonSynced(UdonSyncMode.Smooth)] public float ScanTimer = 0f;
    [UdonSynced(UdonSyncMode.Smooth)] public float isBreakingTimer = 0f;
    [UdonSynced(UdonSyncMode.Smooth)] public float ScanFloat = 0f;
    [UdonSynced(UdonSyncMode.Smooth)] public float distanceFloat = 0f;
    [UdonSynced(UdonSyncMode.Smooth)] public float breakingFloat = 0f;
    public bool isNear = false;
    public bool isFar = false;
    [UdonSynced(UdonSyncMode.None)] public bool isBreaking = false;
    private bool isBreakingRan = false;
    private bool buttonDown = false;
    public bool isTarpsAvailable = true;
    private int IncrementorCheckerInt = 0;
    private bool doneIncrement = false;
    public float lastDistanceCheck = 0f;
    private float currentDistanceCheck = 0f;
    private int TarpAreasLength = 0;
    public bool changeAfterDone = true;
    private RaycastHit[] hits;
    private float timeset = 0;
    private float timesetmax = 2;
    private bool sleeping = false;

    public PlayerUIScript UIScript;

    void Start()
    {
        if (UIScript == null)
        {
            Debug.LogError("[MISSING UI SCRIPT] WARNING! MISSING UI SCRIPT!");
        }
    }

    void Update()
    {
        if (!weaponSelector.EngineController.Piloting && !isSelected && isTarpsAvailable)
        {
            if (!sleeping)
            {
                if (CurrentTarpsTarget != null)
                {
                    CurrentTarpsTarget = null;
                    isScanning = false;
                    isBreaking = false;
                    tarpAreas = null;
                    TarpAreasLength = 0;

                }
                if (TarpsAni != null)
                {
                    TarpsAni.SetBool("tarps", false);
                }
                UITarpsAni.SetBool("ReadyScan", false);

                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "stopScan");
                sleeping = true;


                return;
            }

        }
        if (weaponSelector.EngineController.Piloting && !isSelected)
        {
            if (timeset > timesetmax)
            {
                CurrentTarpsTarget = null;
                tarpAreas = new TarpsArea[0];
                isScanning = false;
                isFar = false;
                isNear = false;
                isBreaking = false;
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "stopScan");
            }
            else
            {
                timeset = timeset + Time.deltaTime;
            }
        }

        // if(doneIncrement && pingTimer < pingEvery){
        //     pingTimer = pingTimer+Time.deltaTime;
        // }else{
        //     pingTimer = 0;
        //     ping();
        // }
        // if (TarpsAni != null)
        // {
        //     TarpsAni.SetBool("tarps", isSelected);
        // }

        if (weaponSelector.EngineController.Piloting)
        {

            if (isScanning && !isSelected)
            {
                Fail();
                if (tarpAreas.Length > 0)
                {
                    HandleExit(tarpAreas[0]);
                }
            }

            IncrementorChecker();
            TarpsAni.SetBool("tarps", isSelected);
            UITarpsAni.SetBool("TarpsMode", isSelected);
            UITarpsAni.SetBool("isScanning", isScanning);
            if (weaponSelector.EngineController.Piloting && isSelected)
            {
                inputChecker();
                sleeping = false;
            }
            if (isScanning)
            {
                UITarpsAni.SetBool("ReadyScan", false);
                AnimateUI();
                Timer();
                // DistanceChecker();
            }
            if (!isScanning)
            {
                if (CurrentTarpsTarget == null && tarpAreas.Length > 0)
                {
                    CurrentTarpsTarget = tarpAreas[0].belongsTo;
                }
                if (CurrentTarpsTarget != null && CurrentTarpsTarget.isEnabed)
                {
                    UITarpsAni.SetBool("ReadyScan", true);
                }
                if (tarpAreas.Length == 0)
                {
                    // if (CurrentTarpsTarget == null)
                    // {
                    CurrentTarpsTarget = null;
                    UITarpsAni.SetBool("ReadyScan", false);
                    // }
                }
            }
        }

        if (weaponSelector.EngineController.Passenger)
        {
            TarpsAni.SetBool("tarps", isSelected);
            UITarpsAni.SetBool("TarpsMode", isSelected);
            UITarpsAni.SetBool("isScanning", isScanning);
            UITarpsAni.SetFloat("ScanValue", ScanFloat);
            UITarpsAni.SetBool("isBreaking", isBreaking);
            UITarpsAni.SetFloat("distance", distanceFloat);
            UITarpsAni.SetFloat("BreakingValue", breakingFloat);
        }

    }

    public void ping()
    {
        hits = Physics.SphereCastAll(Detector.position, detectorRadius, Detector.forward, detectorRange, TarpsLayer, QueryTriggerInteraction.Collide);
    }

    public void scanSuccessCall()
    {
        UITarpsAni.SetBool("ScanSuccess", true);
    }

    public void stopScan()
    {
        UITarpsAni.SetBool("isScanning", false);
        UITarpsAni.SetBool("ReadyScan", false);
        UITarpsAni.SetBool("isBreaking", false);
    }

    public void Timer()
    {
        UITarpsAni.SetBool("isBreaking", isBreaking);
        if (!isBreaking)
        {
            isBreakingTimer = 0f;
            isBreaking = false;
            if (ScanTimer < CurrentTarpsTarget.timeToScan)
            {
                // ScanTimer = ScanTimer + Time.deltaTime ;
                if (isFar) ScanTimer = ScanTimer + (Time.deltaTime / 2);
                if (isNear) ScanTimer = ScanTimer + Time.deltaTime;
            }
            else
            {
                if (CurrentTarpsTarget.AfterScan != null && CurrentTarpsTarget.AfterScan.Length > 0)
                {
                    int x = CurrentTarpsTarget.AfterScan.Length == 0 ? 0 : Random.Range(0, CurrentTarpsTarget.AfterScan.Length - 1);
                    // CurrentTarpsTarget.AfterScan[x].run = true;
                    UIScript.AddToQueueScript(CurrentTarpsTarget.AfterScan[x]);
                }
                if (CurrentTarpsTarget.HideAfterScan)
                {
                    CurrentTarpsTarget.isEnabed = false;
                }

                isScanning = false;
                ScanTimer = 0;

                UITarpsAni.SetBool("ScanSuccess", true);
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "scanSuccessCall");

                if (weaponSelector.EngineController.Piloting && changeAfterDone)
                {
                    // CurrentTarpsTarget = null;
                    weaponSelector.SwitchWeaponSystem();
                    // HandleExit(tarpAreas[0]);
                    // tarpAreas = null;
                }

            }
        }
        else
        {
            isBreakingTimer = isBreakingTimer + Time.deltaTime;

            UITarpsAni.SetFloat("BreakingValue", isBreakingTimer / CurrentTarpsTarget.timeToBreakSignal);
            breakingFloat = isBreakingTimer / CurrentTarpsTarget.timeToBreakSignal;

            if (!isBreakingRan && CurrentTarpsTarget.onBreakingScan != null && CurrentTarpsTarget.onBreakingScan.Length > 0)
            {
                int x = CurrentTarpsTarget.onBreakingScan.Length == 0 ? 0 : Random.Range(0, CurrentTarpsTarget.onBreakingScan.Length - 1);
                // CurrentTarpsTarget.onBreakingScan[x].run = true;
                UIScript.AddToQueueScript(CurrentTarpsTarget.onBreakingScan[x]);
                isBreakingRan = true;
            }

            if (isBreakingTimer > CurrentTarpsTarget.timeToBreakSignal)
            {
                Fail();
            }
        }

    }

    public void AnimateUI()
    {
        UITarpsAni.SetFloat("ScanValue", ScanTimer / CurrentTarpsTarget.timeToScan);
        if (Networking.GetOwner(gameObject) == weaponSelector.EngineController.localPlayer)
        {
            ScanFloat = ScanTimer / CurrentTarpsTarget.timeToScan;
        }
    }

    public void failScan()
    {
        UITarpsAni.SetBool("Fail", true);
        UITarpsAni.SetBool("isBreaking", false);
        UITarpsAni.SetBool("isScanning", false);
        UITarpsAni.SetFloat("BreakingValue", 0);
        UITarpsAni.SetBool("ReadyScan", false);
    }

    public void Fail()
    {
        UITarpsAni.SetBool("Fail", true);
        UITarpsAni.SetFloat("ScanValue", 0);
        UITarpsAni.SetBool("isBreaking", false);
        UITarpsAni.SetBool("isScanning", false);
        UITarpsAni.SetFloat("BreakingValue", 0);
        UITarpsAni.SetBool("ReadyScan", false);

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "failScan");

        isScanning = false;
        isFar = false;
        isNear = false;
        isBreaking = false;

        if (!weaponSelector.EngineController.Passenger)
        {
            if (CurrentTarpsTarget.ScanFail != null && CurrentTarpsTarget.ScanFail.Length > 0)
            {
                int x = CurrentTarpsTarget.ScanFail.Length == 0 ? 0 : Random.Range(0, CurrentTarpsTarget.ScanFail.Length - 1);
                // CurrentTarpsTarget.ScanFail[x].run = true;
                UIScript.AddToQueueScript(CurrentTarpsTarget.ScanFail[x]);
            }
        }
        if (CurrentTarpsTarget != null && CurrentTarpsTarget.ReturnToZeroIfFail)
        {
            ScanTimer = 0;
        }

    }

    public void IncrementorChecker()
    {
        if (isScanning)
        {
            if (tarpAreas == null || tarpAreas.Length == 0)
            {
                isBreaking = true;
            }
            else
            {
                isBreaking = false;
            }
        }

        Transform pos = Detector != null ? Detector : gameObject.transform;
        if (IncrementorCheckerInt >= tarpAreas.Length)
        {
            IncrementorCheckerInt = 0;
            return;
        }

        float dist = Vector3.Distance(pos.position, tarpAreas[IncrementorCheckerInt].transform.position);

        if (IncrementorCheckerInt == 0 && tarpAreas[IncrementorCheckerInt] != null)
        {
            currentDistanceCheck = dist;
        }
        else if (IncrementorCheckerInt > 0)
        {
            if (dist < currentDistanceCheck)
            {
                currentDistanceCheck = dist;
            }

        }

        if (IncrementorCheckerInt + 1 < tarpAreas.Length)
        {
            IncrementorCheckerInt = IncrementorCheckerInt + 1;

        }
        else
        {
            IncrementorCheckerInt = 0;
            doneIncrement = true;

            lastDistanceCheck = currentDistanceCheck;

            if (isScanning)
            {
                UITarpsAni.SetFloat("distance", lastDistanceCheck / CurrentTarpsTarget.breakRange);
                distanceFloat = lastDistanceCheck / CurrentTarpsTarget.breakRange;
                if (lastDistanceCheck > CurrentTarpsTarget.nearRange)
                {
                    isFar = true;
                    isNear = false;
                    isBreaking = false;
                    isBreakingRan = false;
                }
                if (lastDistanceCheck < CurrentTarpsTarget.nearRange)
                {
                    isNear = true;
                    isFar = false;
                    isBreaking = false;
                    isBreakingRan = false;
                }
                if (lastDistanceCheck > CurrentTarpsTarget.breakRange)
                {
                    isFar = false;
                    isNear = false;
                    isBreaking = true;
                }
            }
        }
    }
    void inputChecker()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > .5f) && (buttonDown == false)))
        {
            if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > .5f) { buttonDown = true; }
            fireAreaCheck();

        }
        if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") < .5f && buttonDown == true)
        {
            buttonDown = false;
        }
    }

    public void HandleExit(TarpsArea t)
    {
        if (t == null)
        {
            return;
        }
        if (!(tarpAreas.Length > 0))
        {
            return;
        }
        if (TarpAreasLength > 0)
        {
            TarpsArea[] temp = new TarpsArea[tarpAreas.Length - 1];
            int b = 0;
            bool crossed = false;

            bool found = false;
            for (int x = 0; x < tarpAreas.Length; x++)
            {
                if (tarpAreas[x] != null && tarpAreas[x] == t)
                {
                    found = true;
                }
            }
            if (found)
            {
                for (int y = 0; y < tarpAreas.Length; y++)
                {
                    if (tarpAreas[y] != t && tarpAreas[y] != null && y < tarpAreas.Length && Utilities.IsValid(tarpAreas[y]))
                    {
                        temp[b] = tarpAreas[y];
                        b = b + 1;

                    }
                    else
                    {
                        crossed = true;
                    }
                }
                if (crossed)
                {
                    tarpAreas = temp;
                    TarpAreasLength = tarpAreas.Length;
                }
            }
        }
    }

    public void HandleEnter(TarpsArea t)
    {
        if (!isSelected)
        {
            return;
        }
        if (!t.belongsTo.isEnabed)
        {
            return;
        }
        if (t.belongsTo == CurrentTarpsTarget || CurrentTarpsTarget == null)
        {
            TarpsArea[] temp;
            temp = new TarpsArea[tarpAreas.Length + 1];
            tarpAreas.CopyTo(temp, 0);
            temp[tarpAreas.Length] = t;
            tarpAreas = temp;

            TarpAreasLength = tarpAreas.Length;
        }
    }
    void fireAreaCheck()
    {
        if (tarpAreas.Length > 0 && !isScanning)
        {
            CurrentTarpsTarget = tarpAreas[0].belongsTo;
            ScanTimer = 0;
            isScanning = true;

            if (CurrentTarpsTarget.onEnterScan != null && CurrentTarpsTarget.onEnterScan.Length > 0)
            {
                int x = CurrentTarpsTarget.onEnterScan.Length == 0 ? 0 : Random.Range(0, CurrentTarpsTarget.onEnterScan.Length - 1);
                // CurrentTarpsTarget.onEnterScan[x].run = true;
                UIScript.AddToQueueScript(CurrentTarpsTarget.onEnterScan[x]);
            }
        }
    }
}
