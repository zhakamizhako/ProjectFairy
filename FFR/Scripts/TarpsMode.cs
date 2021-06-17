
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
    public bool isNear = false;
    public bool isFar = false;
    private bool buttonDown = false;
    public bool isTarpsAvailable = true;
    private int IncrementorCheckerInt = 0;
    private bool doneIncrement = false;
    private float lastDistanceCheck = 0f;

    void Update()
    {
        if (!weaponSelector.EngineController.Piloting && !isSelected && isTarpsAvailable)
        {
            if (CurrentTarpsTarget != null)
            {
                CurrentTarpsTarget = null;
            }
            if(TarpsAni!=null){
                TarpsAni.SetBool("Active", false);
            }
            return;
        }

        UITarpsAni.SetBool("TarpsMode", isSelected);
        if (weaponSelector.EngineController.Piloting)
        {
            inputChecker();
        }
        if (isScanning)
        {
            AnimateUI();
            Timer();
            // IncrementorChecker();
        }
        if (!isScanning)
        {
            if(CurrentTarpsTarget==null && tarpAreas.Length > 0){
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

    public void Timer(){
        if(ScanTimer < CurrentTarpsTarget.timeToScan){
            ScanTimer = ScanTimer + Time.deltaTime;
        }else{
            if(CurrentTarpsTarget.AfterScan!=null){
                CurrentTarpsTarget.AfterScan.run = true;
            }
        }
    }

    public void AnimateUI()
    {
        UITarpsAni.SetBool("isScanning", true);
        UITarpsAni.SetFloat("ScanValue", ScanTimer / CurrentTarpsTarget.timeToScan);
    }

    public void Fail()
    {
        UITarpsAni.SetBool("Fail", true);
        UITarpsAni.SetFloat("ScanValue", 0);
    }

    public void IncrementorChecker()
    {
        Transform pos = Detector != null ? Detector : gameObject.transform;
        if (IncrementorCheckerInt >= tarpAreas.Length)
        {
            IncrementorCheckerInt = 0;
            return;
        }

        if (Vector3.Distance(pos.position, tarpAreas[IncrementorCheckerInt].transform.position) < lastDistanceCheck)
        {

        }

        if (IncrementorCheckerInt + 1 < tarpAreas.Length)
        {
            IncrementorCheckerInt = IncrementorCheckerInt + 1;

        }
        else
        {
            IncrementorCheckerInt = 0;
            doneIncrement = true;
        }
    }

    // public void toggleTarps()
    // {
    //     isSelected = !isSelected;
    // }
    // public void SelectTarps()
    // {
    //     isSelected = true;
    // }
    // public void DeselectTarps()
    // {
    //     isSelected = false;
    // }
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
        if (tarpAreas.Length > 0)
        {
            TarpsArea[] temp = new TarpsArea[tarpAreas.Length - 1];
            int b = 0;
            bool crossed = false;
            for (int y = 0; y < tarpAreas.Length; y++)
            {
                if (tarpAreas[y] != t)
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
                tarpAreas = temp;
        }
    }

    public void HandleEnter(TarpsArea t)
    {
        if (t.belongsTo == CurrentTarpsTarget || CurrentTarpsTarget == null)
        {
            TarpsArea[] temp;
            temp = new TarpsArea[tarpAreas.Length + 1];
            tarpAreas.CopyTo(temp, 0);
            temp[tarpAreas.Length] = t;
            tarpAreas = temp;
        }
    }

    // public void HandleStay(TarpsTarget t){
    //      Debug.Log("Staying in Tarps Area");
    //      Debug.Log(t);
    // }

    void fireAreaCheck()
    {
        if (tarpAreas.Length > 0 && !isScanning)
        {
            CurrentTarpsTarget = tarpAreas[0].belongsTo;
            ScanTimer = 0;
            isScanning = true;
        }
    }

    void CheckIterate()
    {
        if (tarpList != null)
        {

        }
    }
}
