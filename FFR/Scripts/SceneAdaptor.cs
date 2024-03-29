﻿
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SceneAdaptor : UdonSharpBehaviour
{
    public string ChapterTitle;
    [TextArea] public string description;
    public EngineController TargetEngineController;
    public PilotSeat targetPilotSeat;
    public Transform targetTransformAircraftPos;
    // public Vector3 OWMLCoordinates;
    public Vector3 TargetOffsetPosition; // Target offset for it to treat as 0,0,0.
    public Vector3 TargetVelocity;
    public Vector3 TargetAngularVelocity;
    public float EngineOutputTarget;
    public TriggerScript[] runScripts;
    public TriggerScript[] resetScripts;
    public PassengerSeat[] targetPassSeats;
    public bool asPilot;
    // public VRCPlayerApi Pilot;
    // public VRCPlayerApi[] Passenger;
    public Animator TargetAnimator;
    public string TargetAnimatorString;
    public Image SceneImage;

    public AIObject[] aiScriptsToReset;
    public TarpsTarget[] tarps;
    public PlayerUIScript UIScript;

    public void startScene()
    {
        if (!TargetEngineController.Occupied)
        {
            //reset scripts
            foreach (TriggerScript x in runScripts)
            {
            //    x.resetScript();
            UIScript.RestartScript(x);

            }
            foreach (TriggerScript x in resetScripts)
            {
                UIScript.RestartScript(x);
            //    x.resetScript();
            }
            //respawn ai
            foreach (AIObject x in aiScriptsToReset)
            {
                if (x.inited)
                {
                    x.revive = true;
                    if (x.AIClassTransform != null)
                    {
                        x.AIClassTransform.localPosition = x.startArea;
                        x.AIClassTransform.rotation = x.startAreaRotation;
                    }
                    if (!x.gameObject.activeSelf)
                    {
                        x.gameObject.SetActive(true);
                    }
                }
            }

            if (TargetAnimator != null)
            {
                TargetAnimator.SetBool(TargetAnimatorString, true);
            }
            // TargetEngineController.EngineOutput = EngineOutputTarget;

            if (asPilot)
            {
                // TargetEngineController.Respawn_event();
                TargetEngineController.VehicleRigidbody.velocity = Vector3.zero;
                TargetEngineController.VehicleRigidbody.angularVelocity = Vector3.zero;
                TargetEngineController.Health = TargetEngineController.FullHealth;
            
                if(TargetEngineController.hbcontroller!=null)TargetEngineController.hbcontroller.Respawn();
                
                targetPilotSeat.Interact();
                targetPilotSeat.EngineStart();
                TargetEngineController.VehicleMainObj.transform.position = TargetOffsetPosition;
                // TargetEngineController.VehicleMainObj.transform.rotation = Quaternion.Euler(Vector3.zero);
                TargetEngineController.VehicleMainObj.transform.rotation = targetTransformAircraftPos.rotation;
                targetPilotSeat.OWML.Map.position = -targetTransformAircraftPos.position;
                targetPilotSeat.OWML.EnableScript();
                
                TargetEngineController.VehicleRigidbody.velocity =TargetVelocity;
                TargetEngineController.VehicleRigidbody.angularVelocity =TargetAngularVelocity;
                TargetEngineController.PlayerThrottle = EngineOutputTarget;
                TargetEngineController.SetGearUp();
                TargetEngineController.SetFlapsOff();
                // TargetEngineController.EffectsControl.Flaps = false;
                // TargetEngineController.EffectsControl.GearUp = true;
            }

            foreach (TarpsTarget x in tarps)
            {
                x.isEnabed = true;
            }

            foreach (TriggerScript x in runScripts)
            {
                // x.run = true;
                UIScript.AddToQueueScript(x);
            }
        }

    }
}
