using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HitboxDamageDetector : UdonSharpBehaviour {
    public bool isBody = false;
    public bool isLWing = false;
    public bool isRWing = false;
    public bool isLRudder = false;
    public bool isRRudder = false;
    public bool isLAileron = false;
    public bool isRAileron = false;
    public bool isLElevator = false;
    public bool isRElevator = false;
    public bool isLEngine = false;
    public bool isREngine = false;
    public bool isLFlap = false;
    public bool isRFlap = false;
    public HitboxControllerAndEffects hbcontroller;
    public GameObject OwnGun;
    void OnParticleCollision (GameObject other) {
        Debug.Log ("Hit");
        if (hbcontroller != null && !hbcontroller.EngineControl.dead) {
            if (hbcontroller.EngineControl.localPlayer == null) {
                if (other != OwnGun)
                    callDamage ();
            } else {
                if (Networking.IsOwner (hbcontroller.EngineControl.gameObject) || (other.GetComponent<GunParticle> () != null && other.GetComponent<GunParticle> ().fromShot != null)) {
                    if (other != OwnGun)
                        SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "callDamage");
                }
            }
        }
    }

    public void callDamage () {
        if (hbcontroller != null && hbcontroller.EngineControl != null && !hbcontroller.EngineControl.dead) {
            if (isBody && hbcontroller.HealthBody > 0) {
                hbcontroller.HealthBody = hbcontroller.HealthBody - 10;
            }
            if (isLWing && hbcontroller.HealthLWing > 0) {
                hbcontroller.HealthLWing = hbcontroller.HealthLWing - 10;
            }
            if (isRWing && hbcontroller.HealthRWing > 0) {
                hbcontroller.HealthRWing = hbcontroller.HealthRWing - 10;
            }
            if (isLRudder && hbcontroller.HealthLRudder > 0) {
                hbcontroller.HealthLRudder = hbcontroller.HealthLRudder - 10;
            }
            if (isRRudder && hbcontroller.HealthRRudder > 0) {
                hbcontroller.HealthRRudder = hbcontroller.HealthRRudder - 10;
            }
            if (isLAileron && hbcontroller.HealthLAileron > 0) {
                hbcontroller.HealthLAileron = hbcontroller.HealthLAileron - 10;
            }
            if (isRAileron && hbcontroller.HealthRAileron > 0) {
                hbcontroller.HealthRAileron = hbcontroller.HealthRAileron - 10;
            }
            if (isLElevator && hbcontroller.HealthLElevator > 0) {
                hbcontroller.HealthLElevator = hbcontroller.HealthLElevator - 10;
            }
            if (isRElevator && hbcontroller.HealthRElevator > 0) {
                hbcontroller.HealthRElevator = hbcontroller.HealthRElevator - 10;
            }
            if (isLEngine && hbcontroller.HealthLEngine > 0) {
                hbcontroller.HealthLEngine = hbcontroller.HealthLEngine - 10;
            }
            if (isREngine && hbcontroller.HealthREngine > 0) {
                hbcontroller.HealthREngine = hbcontroller.HealthREngine - 10;
            }
            if (isLFlap && hbcontroller.HealthLFlap > 0) {
                hbcontroller.HealthLFlap = hbcontroller.HealthLFlap - 10;
            }
            if (isRFlap && hbcontroller.HealthRFlap > 0) {
                hbcontroller.HealthRFlap = hbcontroller.HealthRFlap - 10;
            }
            Debug.Log ("HIT!");
        }
    }
}
