using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HitDetector : UdonSharpBehaviour {
    public EngineController EngineControl;
    public MissileTrackerAndResponse Tracker;
    public LandingSystemController LSC;
    public GameObject OwnGun;
    // private float damage = 0f;
    private void Start () {
        Assert (EngineControl != null, "Start: EngineControl != null");
    }
    void OnParticleCollision (GameObject other) {
        if (other == null || EngineControl.dead) return; //avatars can't shoot you, and you can't get hurt when you're dead
        if (other != OwnGun) {
            if (Networking.IsOwner (EngineControl.gameObject) || (other.GetComponent<GunParticle> () != null && other.GetComponent<GunParticle> ().fromShot != null)) {
                if (EngineControl.localPlayer == null) {
                    PlaneHit ();
                } else {
                    SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlaneHit");
                }
            }
        }

    }
    public void PlaneHit()
    {
        EngineControl.PlaneHit();
    }
    public void Respawn()//called by the explode animation on last frame
    {
        if (EngineControl.InEditor)//editor
        {
            EngineControl.Respawn_event();
        }
        else if (EngineControl.IsOwner)
        {
            EngineControl.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Respawn_event");//owner broadcasts because it's more reliable than everyone doing it individually
        }
    }
    public void MoveToSpawn()//called 3 seconds before respawn, to prevent a glitch where the plane will appear where it died for a second for non-owners
    {
        if (EngineControl.IsOwner)
        {
            if (EngineControl.InEditor)
            {
                EngineControl.VehicleTransform.SetPositionAndRotation(EngineControl.Spawnposition, Quaternion.Euler(EngineControl.Spawnrotation));
            }
            else
            { //this should respawn it in VRC, doesn't work in editor
                EngineControl.VehicleMainObj.transform.position = new Vector3(EngineControl.VehicleMainObj.transform.position.x, -10000, EngineControl.VehicleMainObj.transform.position.z);
            }
        }
    }

    public void Respawn_event () //called by Respawn()
    {
        //re-enable plane model and effects
        EngineControl.EffectsControl.DoEffects = 6f; //wake up if was asleep
        EngineControl.EffectsControl.PlaneAnimator.SetTrigger ("instantgeardown");
        if (EngineControl.InEditor) {
            EngineControl.VehicleMainObj.transform.rotation = Quaternion.Euler (EngineControl.Spawnrotation);
            EngineControl.VehicleMainObj.transform.position = EngineControl.Spawnposition;
            EngineControl.Health = EngineControl.FullHealth;
            EngineControl.EffectsControl.GearUp = false;
            EngineControl.EffectsControl.Flaps = true;
        } else if (EngineControl.IsOwner) {
            EngineControl.Health = EngineControl.FullHealth;
            //this should respawn it in VRC, doesn't work in editor
            // EngineControl.VehicleMainObj.transform.position = new Vector3 (EngineControl.VehicleMainObj.transform.position.x, -10000, EngineControl.VehicleMainObj.transform.position.z);
            EngineControl.VehicleMainObj.transform.rotation = Quaternion.Euler (EngineControl.Spawnrotation);
                EngineControl.VehicleMainObj.transform.position = EngineControl.Spawnposition;
            EngineControl.EffectsControl.GearUp = false;
            EngineControl.EffectsControl.Flaps = true;
            EngineControl.VehicleMainObj.GetComponent<Rigidbody> ().velocity = Vector3.zero;
            if (EngineControl.hbcontroller != null) {
                EngineControl.hbcontroller.Respawn ();
            }
        }
    }
    public void NotDead()//called by 'respawn' animation twice because calling on the last frame of animation is unreliable for some reason
    {
        if (EngineControl.InEditor)
        {
            EngineControl.Health = EngineControl.FullHealth;
        }
        else if (EngineControl.IsOwner)
        {
            EngineControl.Health = EngineControl.FullHealth;
        }
        EngineControl.dead = false;
    }
    public void NotDead_event () //called by NotDead()
    {
        if (EngineControl.InEditor) {
            EngineControl.Health = EngineControl.FullHealth;
        } else if (EngineControl.IsOwner) {
            EngineControl.Health = EngineControl.FullHealth;
        }
        EngineControl.dead = false; //because respawning gives us an immense number of Gs because we move so far in one frame, we stop being 'dead' 5 seconds after we respawn. Can't explode when 'dead' is set. 
    }

    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogWarning("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}
