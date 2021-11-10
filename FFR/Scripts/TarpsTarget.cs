
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TarpsTarget : UdonSharpBehaviour
{
    public bool doneScan = false;
    public float timerScan = 0;
    public float timeToScan = 5;
    public float timeToBreakSignal = 10f;
    public MissileTrackerAndResponse TrackerObject;
    public TriggerScript[] AfterScan;
    public TriggerScript[] ScanFail;
    public TriggerScript[] onEnterScan;
    public TriggerScript[] onBreakingScan;
    public Animator tarpsTargetAni;
    public string AniString;
    public bool aniArgument;
    public bool HideAfterScan = true;
    public bool ReturnToZeroIfFail = true;
    // public float farRange = 1000f;
    public float nearRange = 500f;
    public float breakRange = 1500f;
    public GameObject[] DetectorsNear;
    public GameObject[] DetectorsFar;
    public bool isShown = false;
    public bool isActive = false;
    public bool isEnabed = true;
}
