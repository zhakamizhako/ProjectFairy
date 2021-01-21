
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDKBase;
using VRC.Udon;

public class WeaponSystem : UdonSharpBehaviour
{
    public string[] WeaponType;
    public GameObject[] MissileFabs;
    public string[] types; // Valid Types - 'gun', 'aam3', 'aam5', 'agm', 'laser', 'bomb', 'multiaam'
    [UdonSynced(UdonSyncMode.None)] public int SelectedWeapon;
    public MissileTrackerAndResponse MyTracker;
    public GameObject TargetChangeHud;
    public GameObject NoTargetHUD;
    public GameObject LockSightHUD;
    public Transform[][] SpawnAreas; //First Dimension - WeaponType; Second Dimension -- Spawn Area
    public int[] Ammo;
    private float[][] CooldownMissiles;
    private bool[][] WeaponFired;
    public float[] CooldownTimes;//Weapon type
    public GameObject[][][] CooldownMissileHUD;//First dimension - weapon type, 2nd dimension - Missile Slot/Ammo Slot, 3rd dimension - GameObjects, whether VR or Desktop
    public ParentConstraint[][] WeaponConstraints;//First Dimension - Weapon type, 2nd dimension - GameObjects for that specific ammo slot.
    public bool[] isConstraintType; //
    public AudioSource NextTarget;
    public AudioSource NoTarget;
    public MissileTargets misTargetScript;
    private RaycastHit[] Objects;
    public Transform SpawnParent; //For Use on Openworld spawn
    public float[] timeToLock;
    private float[] cooldownTimes;
    public float[] minimumRange;
    public float[] range;
    public float[] radius;
    public bool isLocking = false;
    public bool isLocked = false;
    public LayerMask layermask; //Targeting layer
    public Transform TargetDetector;
    public Transform ChangeTargetDetector; //In case that it needs to be jsut at the front
    public GameObject CurrentHitObject;
    private float frameLast = 0;
    private bool updateCrsor = false;
    public float updateIntervals = 0.08f;
    public AudioSource[] locking;
    public AudioSource[] locked;
    [System.NonSerializedAttribute][HideInInspector][UdonSynced (UdonSyncMode.None)] public int launchArea = 0;
    public Vector3 ScaleTarget = new Vector3(0.6f, 0.6f, 0.1f);
    [UdonSynced (UdonSyncMode.None)] public int selectedTargetIndex = -1;
    private bool buttonDown = false;
    private GameObject MissileRuntime;
    public AudioSource[] WeaponFire;
    public Animator[] WeaponAnimators;
    public float[] gunSpeed;
    public float[] overheatTime;
    public float[] overheatTimer;
    public float[] overheatCool;
    public bool[] isOverheating;
    public GameObject[] GunIcons;

    [Header ("Internal Settings")]
    public float UpdateTendency = 0.1f;
    public float UpdateTimer = 0f;
    private int currentIndex = 0;

    void Start()
    {
        if(LockSightHUD!=null){
            LockSightHUD.SetActive(false);
        }
        foreach(GameObject x in GunIcons){
            if(x!=null) x.SetActive(false);
        }

        // if(WeaponType.Length > 0){
        //     for(int x=0;x<WeaponType.Length;x++){
        //         switch(WeaponType[x]){
        //             case "aam": 
                    
        //         }
        //     }
        // }
    }
}
