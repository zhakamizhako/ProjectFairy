
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VehicleRespawnButton : UdonSharpBehaviour
{
    public EngineController EngineControl;
    private void Start()
    {
        Assert(EngineControl != null, "Start: EngineControl != null");
    }

    public void RespawnPlane(){
        Interact();
    }
    private void Interact()
    {
        if (!EngineControl.Occupied && !EngineControl.dead)
        {
            Networking.SetOwner(EngineControl.localPlayer, EngineControl.VehicleMainObj);
            Networking.SetOwner(EngineControl.localPlayer, EngineControl.gameObject);
            Networking.SetOwner(EngineControl.localPlayer, EngineControl.EffectsControl.gameObject);
            EngineControl.VehicleMainObj.transform.position = EngineControl.Spawnposition;
            EngineControl.VehicleMainObj.transform.rotation = Quaternion.Euler(EngineControl.Spawnrotation);
            if (EngineControl.HasCanopy) { EngineControl.EffectsControl.CanopyOpen = true; }
            EngineControl.EffectsControl.GearUp = false;
            EngineControl.EffectsControl.Flaps = true;
            EngineControl.EffectsControl.HookDown = false;
            EngineControl.FlightLimitsEnabled = true;
            EngineControl.Health = EngineControl.FullHealth;
            EngineControl.Fuel = EngineControl.FullFuel;
            // EngineControl.GunAmmoInSeconds = EngineControl.FullGunAmmo;
            EngineControl.Fuel = EngineControl.FullFuel;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ButtonRespawn");
        }
    }
    public void ButtonRespawn()
    {
        EngineControl.EffectsControl.DoEffects = 6;
        EngineControl.dead = true;//this makes it invincible and unable to be respawned again for 5s
        EngineControl.EffectsControl.PlaneAnimator.SetTrigger("respawn");//this animation disables EngineControl.dead after 5s
        EngineControl.EffectsControl.PlaneAnimator.SetTrigger("instantgeardown");
        EngineControl.VehicleRigidbody.velocity = Vector3.zero;
        if(EngineControl.hbcontroller)
        EngineControl.hbcontroller.Respawn();
        if(EngineControl.OWML != null){
            EngineControl.OWML.AnchorCoordsPosition = (EngineControl.gameObject.transform.localPosition = Vector3.zero);
            EngineControl.OWML.CallForRespawn();
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