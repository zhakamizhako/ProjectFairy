
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TarpsTarget : UdonSharpBehaviour
{
    public bool doneScan = false;
    public float timerScan = 0;
    public float timeToScan = 5;
    public MissileTrackerAndResponse TrackerObject;
    public TriggerScript AfterScan;
    public TriggerScript ScanFail;
    public TriggerScript onEnterScan;
    public Animator tarpsTargetAni;
    public string AniString;
    public bool aniArgument;
    public bool HideAfterScan = true;
    public bool ReturnToZeroIfFail = true;
    public float range = 1000f;
    public bool isShown = false;
    public bool isActive = false;
}
