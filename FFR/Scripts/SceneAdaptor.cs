
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

    public void startScene()
    {
        if (!TargetEngineController.Occupied)
        {
            //reset scripts
            foreach (TriggerScript x in runScripts)
            {
               x.resetScript();

            }
            foreach (TriggerScript x in resetScripts)
            {
               x.resetScript();
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
            TargetEngineController.EngineOutput = EngineOutputTarget;

            if (asPilot)
            {
                // TargetEngineController.Respawn_event();
                TargetEngineController.Health = TargetEngineController.FullHealth;
                TargetEngineController.hbcontroller.Respawn();
                TargetEngineController.VehicleRigidbody.velocity =TargetVelocity;
                TargetEngineController.VehicleRigidbody.angularVelocity =TargetAngularVelocity;
                targetPilotSeat.Interact();
                targetPilotSeat.EngineStart();
                TargetEngineController.VehicleMainObj.transform.position = TargetOffsetPosition;
                TargetEngineController.VehicleMainObj.transform.rotation = Quaternion.Euler(Vector3.zero);
                TargetEngineController.VehicleMainObj.transform.rotation = targetTransformAircraftPos.rotation;
                targetPilotSeat.OWML.Map.position = -targetTransformAircraftPos.position;
                targetPilotSeat.OWML.EnableScript();
            }

            foreach (TarpsTarget x in tarps)
            {
                x.isEnabed = true;
            }

            foreach (TriggerScript x in runScripts)
            {
                x.run = true;
            }
        }

    }
}
