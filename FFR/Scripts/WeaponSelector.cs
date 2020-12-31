using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WeaponSelector : UdonSharpBehaviour {
    public MissilePlaneSystem[] MissilePlaneSystems;
    public GameObject[][] selectedWeapohHUDs;
    [UdonSynced (UdonSyncMode.None)] public int selectedSystem = 0;
    public bool hasFlares = false;
    public ParticleSystem[] flareObject;
    public GameObject Tailer;
    public int flaresCount = 3;
    public int flaresNow = 3;
    public float flareCooldown = 15;
    public float flareCountdown = 0;
    public float returnTracker = 3;
    private int prevselected = 0;
    public Text FlareText;
    public Text SwitchWeaponText;
    public EngineController EngineController;
    public AudioSource FlareSound;
    public AudioSource AfterFlareSource;
    public AudioSource NoFlareSound;
    public AudioClip afterFlareSound;
    public ParentConstraint TailerConstraint;
    public AudioSource SwitchWeapon;
    public GameObject MissIndicator;
    [HideInInspector] public bool miss = false;
    private float MissIndicatorTimer = 0f;
    public float MissIndicatorTime = 1f;

    public GameObject HitIndicator;
    [HideInInspector] public bool hit = false;
    private float HitIndicatorTimer = 0f;
    public float HitIndicatorTime = 1f;
    public float afterFlareTime = 1f;
    public float afterFlareTimer = 0f;
    private bool afterFlare = false;
    private bool called = false;
    void Start () {
        if (FlareText != null) {
            FlareText.text = "FLARES: " + flaresNow;
        }
        if (SwitchWeaponText != null) {
            SwitchWeaponText.text = MissilePlaneSystems[selectedSystem].gameObject.name;
        }
        for (int x = 0; x < selectedWeapohHUDs[selectedSystem].Length; x++) {
            selectedWeapohHUDs[selectedSystem][x].SetActive (true);
        }
        if (HitIndicator != null) {
            HitIndicator.SetActive (false);
        }
        if (MissIndicator != null) {
            MissIndicator.SetActive (false);
        }
        for(int x =0; x<MissilePlaneSystems.Length;x++){
            MissilePlaneSystems[x].gameObject.SetActive(false);
        }
        if(MissilePlaneSystems.Length>0 && MissilePlaneSystems[selectedSystem]!=null){
            MissilePlaneSystems[selectedSystem].selectedWeapon = true;
        }
    }

    public void callChangeTarget () {
        MissilePlaneSystems[selectedSystem].newChangeTargetCommand ();
    }

    void Update () {
        if (flareObject.Length > 0) {
            if (flaresNow < flaresCount) {
                if (flareCountdown > returnTracker) {
                    Tailer.GetComponent<BoxCollider> ().enabled = false;
                    TailerConstraint.constraintActive = true;
                    foreach (ParticleSystem fo in flareObject) {
                        ParticleSystem.EmissionModule v = fo.emission;
                        v.enabled = false;
                    }
                }
                if (flareCountdown < flareCooldown) {
                    flareCountdown = flareCountdown + Time.deltaTime;
                } else {
                    flaresNow = flaresNow + 1;
                    // isFiredFlare = false;
                    flareCountdown = 0;
                    if (FlareText != null)
                        FlareText.text = "FLARES: " + flaresNow;
                }
            }
        }
        if (EngineController.Piloting || EngineController.Passenger) {

            if (miss && MissIndicatorTimer < MissIndicatorTime) {
                if (MissIndicator != null) {
                    MissIndicator.SetActive (true);
                }
                MissIndicatorTimer = MissIndicatorTimer + Time.deltaTime;
            } else {
                if (MissIndicator != null && MissIndicator.activeSelf) {
                    MissIndicator.SetActive (false);
                }
                MissIndicatorTimer = 0f;
                miss = false;
            }
            callEnable();
            // if(!MissilePlaneSystems[selectedSystem].gameObject.activeSelf){
            //     MissilePlaneSystems[selectedSystem].gameObject.SetActive(true);
            // }

            // if(afterFlare && afterFlareTimer < afterFlareTime){
            //     afterFlareTimer = afterFlareTimer + Time.deltaTime;
            // }else if(afterFlare && afterFlareTimer > afterFlareTime){
            //     if(AfterFlareSource!=null && afterFlareSound!=null){
            //         AfterFlareSource.PlayOneShot(afterFlareSound);
            //     }
            //     afterFlareTimer = 0;
            //     afterFlare = false;
            // }

            if (hit && HitIndicatorTimer < HitIndicatorTime) {
                if (HitIndicator != null) {
                    HitIndicator.SetActive (true);
                }
                HitIndicatorTimer = HitIndicatorTimer + Time.deltaTime;
            } else {
                if (HitIndicator != null && HitIndicator.activeSelf) {
                    HitIndicator.SetActive (false);
                }
                HitIndicatorTimer = 0f;
                hit = false;
            }

        }
        if (EngineController.localPlayer == null || EngineController.Piloting) {
            if (Input.GetKeyDown (KeyCode.X)) {
                callFlares ();
            }
            if (Input.GetKeyDown (KeyCode.U)) {
                SwitchWeaponSystem ();
            }

        }
        if (EngineController.localPlayer != null && !EngineController.Piloting) {
            if(!EngineController.Occupied){
                callDisable();
            //     MissilePlaneSystems[selectedSystem].gameObject.SetActive(false);
            }
            if(EngineController.Occupied){
                callEnable();
            //     MissilePlaneSystems[selectedSystem].gameObject.SetActive(true);
            }
            if (prevselected != selectedSystem) {
                Debug.Log("Changing Weapon");
                MissilePlaneSystems[prevselected].showTargets = false;
                MissilePlaneSystems[prevselected].selectedWeapon = false;
                MissilePlaneSystems[selectedSystem].selectedWeapon = true;
                // MissilePlaneSystems[selectedSystem].gameObject.SetActive (true);
                Debug.Log(MissilePlaneSystems[selectedSystem].gameObject.name);
                MissilePlaneSystems[selectedSystem].showTargets = true;
                if(MissilePlaneSystems[prevselected].isGun){
                    MissilePlaneSystems[prevselected].gunAnimator.SetBool("firing", false);
                }
                MissilePlaneSystems[prevselected].gameObject.SetActive (false);
                for (int x = 0; x < selectedWeapohHUDs[prevselected].Length; x++) {
                    selectedWeapohHUDs[prevselected][x].SetActive (false);
                }
                for (int x = 0; x < selectedWeapohHUDs[selectedSystem].Length; x++) {
                    selectedWeapohHUDs[selectedSystem][x].SetActive (true);
                }
                prevselected = selectedSystem;
                Debug.Log("Weapon Changed");
            }
        }
    }

    public void callEnable(){
        foreach(MissilePlaneSystem x in MissilePlaneSystems){
            if(!x.gameObject.activeSelf)
            x.gameObject.SetActive(true);
        }
    }

    public void callDisable(){
         foreach(MissilePlaneSystem x in MissilePlaneSystems){
            if(x.gameObject.activeSelf)
            x.gameObject.SetActive(false);
        }
    }
    public void SwitchWeaponSystem () {
        if (Networking.IsOwner (gameObject)) {
            if (SwitchWeapon != null)
                SwitchWeapon.Play ();
            var prevSelected = selectedSystem;
            MissilePlaneSystems[selectedSystem].showTargets = false;
            MissilePlaneSystems[selectedSystem].timerLocking = 0;
            MissilePlaneSystems[selectedSystem].isLocking = false;
            MissilePlaneSystems[selectedSystem].isLocked = false;
            MissilePlaneSystems[selectedSystem].selectedWeapon = false;
            MissilePlaneSystems[selectedSystem].misTarget.noTarget = true;
            if(MissilePlaneSystems[selectedSystem].isGun){
                MissilePlaneSystems[selectedSystem].gunAnimator.SetBool("firing", false);
            }
            MissilePlaneSystems[selectedSystem].gameObject.SetActive (false);
            for (int x = 0; x < selectedWeapohHUDs[selectedSystem].Length; x++) {
                selectedWeapohHUDs[selectedSystem][x].SetActive (false);
            }
            if (selectedSystem + 1 < MissilePlaneSystems.Length) {
                selectedSystem += 1;
            } else {
                selectedSystem = 0;
            }
            MissilePlaneSystems[selectedSystem].gameObject.SetActive (true);
            Debug.Log("Setting Ownership");
            if(EngineController.localPlayer!=null && !Networking.IsOwner(MissilePlaneSystems[selectedSystem].gameObject)){
                Networking.SetOwner(EngineController.localPlayer, MissilePlaneSystems[selectedSystem].gameObject);
                Debug.Log("Ownership set");
            }
            
            MissilePlaneSystems[selectedSystem].timerLocking = 0;
            MissilePlaneSystems[selectedSystem].showTargets = true;
            MissilePlaneSystems[selectedSystem].selectedWeapon = true;
            MissilePlaneSystems[selectedSystem].selectedTargetIndex = MissilePlaneSystems[prevSelected].selectedTargetIndex;
            if (SwitchWeaponText != null) {
                SwitchWeaponText.text = MissilePlaneSystems[selectedSystem].gameObject.name;
            }
            for (int x = 0; x < selectedWeapohHUDs[selectedSystem].Length; x++) {
                selectedWeapohHUDs[selectedSystem][x].SetActive (true);
            }

        }
    }

    public void callFlares () {
        if (EngineController.localPlayer == null)
            flareSync ();
        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "flareSync");
    }
    public void flareSync () {
        if (hasFlares && flareObject.Length > 0) {
            if (flaresNow > 0) {
                if (Tailer != null) {
                    flareCountdown = 0;
                    Tailer.GetComponent<BoxCollider> ().enabled = true;
                    TailerConstraint.constraintActive = false;
                    Tailer.GetComponent<Rigidbody> ().AddRelativeForce (Vector3.forward * -20);
                    // isFiredFlare = true;
                    flaresNow = flaresNow - 1;
                    // afterFlare = true;
                    // afterFlareTimer = 0f;
                    if (FlareSound != null)
                        FlareSound.Play ();

                    if (flaresNow == 0) {
                        if (FlareText != null)
                            FlareText.text = "FLARES: EMPTY";
                    } else {
                        if (FlareText != null)
                            FlareText.text = "FLARES: " + flaresNow;
                    }
                    foreach (ParticleSystem fo in flareObject) {
                        // if (!isHeavyPlane) {
                        ParticleSystem.EmissionModule v = fo.emission;
                        v.enabled = true;
                        // }
                    }
                    if(AfterFlareSource!=null && afterFlareSound!=null && (EngineController.Piloting||EngineController.Passenger)){
                        AfterFlareSource.PlayOneShot(afterFlareSound);
                    }
                } else {
                    Debug.Log ("Put the Flare inside a Tailer, and Tailer must exist in the outside, and must be constrained with the parent Gameobject.");
                }
            }else{
                if(NoFlareSound!=null){
                    NoFlareSound.Play();
                }
            }
        } else if (hasFlares && flareObject == null) {
            Debug.Log ("Hey wtf. Put the damn flare objects or else it wont work!");
        }
    }

}