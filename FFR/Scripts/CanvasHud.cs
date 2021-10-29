﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class CanvasHud : UdonSharpBehaviour
{
    public Text AltimeterText;
    public EngineController EngineControl;
    private EffectsController EffectsControl;
    ///comment this when not using HBController.
    public HitboxControllerAndEffects hbcontroller;
    public Animator[] CanvasHUDAnimator;
    public float LiftDivisor = 1500f;
    //Comment this line when not using thirdperson camera.
    public ThirdPersonPlayerCamera third;
    public GameObject thirdHUD;
    // public FHudController fhud;
    // public LayerMask UILayer;
    public GameObject cockpitcam;
    private bool ThirdEnabled = false;
    public bool isVR = false;
    public float SpeedDivisor1 = 360;
    public float SpeedDivisor2 = 360;
    public float SpeedDivisor3 = 360;
    public float AltDivisor1 = 1000;
    public float AltDivisor2 = 10000;
    public float AltDivisor3 = 10000;
    public bool doLogarithmicSpeed1 = false;
    public bool doLogarithmicSpeed2 = false;
    public float LogarithmicVal1Speed = 1000;
    public float LogarithmicVal2Speed = 1000;
    public float pullupDistance = 200f;
    public Transform parentTransform;
    public PlayerUIScript UIScript;
    private VRCPlayerApi localPlayer;
    public bool checkPullup = false;
    public float checkPullupEvery = 1f;
    private float checkpulluptimer = 0f;
    public LayerMask pullupdetector;
    public bool isFollowVR = true;
    public Vector3 offsets = new Vector3(.5f, 0, .5f);
    private int TempAltimeterVal = 0;
    private Vector3 startSize;
    private int index = 0;
    private int maxLength = 0;
    public GameObject[] DisableOnThird;
    public GameObject[] EnableOnThird;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        startSize = gameObject.transform.localScale;
        EffectsControl = EngineControl ? EngineControl.EffectsControl : null;
        maxLength = CanvasHUDAnimator.Length;
    }

    void LateUpdate()
    {
        if (isFollowVR && isVR && UIScript != null)
        {
            if (parentTransform != null)
            {
                float offset = UIScript.vrDistance;
                float size = UIScript.vrSize;
                if (localPlayer != null)
                {
                    var PrePos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                    parentTransform.position = PrePos;
                    parentTransform.position += parentTransform.forward * offset;
                    parentTransform.rotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                    gameObject.transform.localScale = startSize * size;
                }
                else
                {
                    parentTransform.position = parentTransform.forward * offset;
                    //  parentTransform.rotation = new Vector3(0,0,0);
                }
            }
        }
    }
    void Update()
    {
        if (EngineControl != null)
        {
            //REQUIRES HBCONTROLLER
            //Comment the entire  if statement if not using thirdperson camera.
            if (!isVR && thirdHUD != null)
            {
                if (third.enabledCam == true)
                {
                    if (!ThirdEnabled)
                    {
                        cockpitcam.SetActive(true);
                        thirdHUD.SetActive(true);
                        ThirdEnabled = true;
                        foreach (GameObject x in EnableOnThird)
                        {
                            x.SetActive(true);
                        }
                        foreach (GameObject x in DisableOnThird)
                        {
                            x.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (ThirdEnabled)
                    {
                        thirdHUD.SetActive(false);
                        cockpitcam.SetActive(false);
                        ThirdEnabled = false;
                        foreach (GameObject x in EnableOnThird)
                        {
                            x.SetActive(false);
                        }
                        foreach (GameObject x in DisableOnThird)
                        {
                            x.SetActive(true);
                        }
                    }
                }
            }
            if (AltimeterText != null)
            {
                //comment below when not using OWML.
                float value = EngineControl.VehicleMainObj.transform.position.y +
                        (EngineControl.OWML != null && EngineControl.OWML.ScriptEnabled ?
                                 (EngineControl.OWML.AnchorCoordsPosition.y - EngineControl.OWML.Map.position.y) + EngineControl.SeaLevel * 3.28084f
                                 : (EngineControl.VehicleMainObj.transform.position.y + EngineControl.SeaLevel * 3.28084f));

                //Uncomment this line when not using OWML.
                // float value = EngineControl.VehicleMainObj.transform.position.y + EngineControl.SeaLevel * 3.28084f;

                int testAltimeterVal = Mathf.RoundToInt(value);
                if (testAltimeterVal != TempAltimeterVal)
                {
                    TempAltimeterVal = testAltimeterVal;
                    AltimeterText.text = string.Format("{0}ft", testAltimeterVal);
                }
                // AltimeterText.text = ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f).ToString ("F0") + "ft";

            }
            if (CanvasHUDAnimator != null)
            {
                doEngines();

                //comment below when not using OWML.
                float value = EngineControl.VehicleMainObj.transform.position.y +
                       (EngineControl.OWML != null && EngineControl.OWML.ScriptEnabled ?
                                (EngineControl.OWML.AnchorCoordsPosition.y - EngineControl.OWML.Map.position.y) + EngineControl.SeaLevel * 3.28084f
                                : (EngineControl.VehicleMainObj.transform.position.y + EngineControl.SeaLevel * 3.28084f));

                //Uncomment this line when not using OWML.
                // float value = EngineControl.VehicleMainObj.transform.position.y + EngineControl.SeaLevel * 3.28084f;

                CanvasHUDAnimator[index].SetFloat("gauge_altneedle", (value / AltDivisor1));
                CanvasHUDAnimator[index].SetFloat("gauge_altw", (value / AltDivisor2));
                CanvasHUDAnimator[index].SetFloat("gauge_alt3", (value / AltDivisor3));
                CanvasHUDAnimator[index].SetFloat("gauge_liftneedle", (EngineControl.CurrentVel.y * 60 * 3.28084f) / LiftDivisor + .5f); // Note: Lift on every 60 seconds / Total value  ?. 

                //Level
                var body = EngineControl.VehicleMainObj.transform.rotation.eulerAngles;
                CanvasHUDAnimator[index].SetFloat("gauge_level", (body.z / 360) + offsets.z);
                CanvasHUDAnimator[index].SetFloat("gauge_pitch", (body.x / 180) + offsets.x);

                //heading
                float angle = -EngineControl.VehicleMainObj.transform.rotation.eulerAngles.y;
                CanvasHUDAnimator[index].SetFloat("gauge_heading", (angle / 360) + offsets.y);

                if (checkPullup) { doPullup(); }

            }
            //HitboxController Specifics. Comment starts here
            if (hbcontroller != null)
            {
                CanvasHUDAnimator[index].SetFloat("health_wingL", (hbcontroller.initHealthLWing - hbcontroller.HealthLWing) / hbcontroller.initHealthLWing * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_wingR", (hbcontroller.initHealthRWing - hbcontroller.HealthRWing) / hbcontroller.initHealthRWing * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_rudderL", (hbcontroller.initHealthLRudder - hbcontroller.HealthLRudder) / hbcontroller.initHealthLRudder * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_rudderR", (hbcontroller.initHealthRRudder - hbcontroller.HealthRRudder) / hbcontroller.initHealthRRudder * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_aileronL", (hbcontroller.initHealthLAileron - hbcontroller.HealthLAileron) / hbcontroller.initHealthLAileron * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_aileronR", (hbcontroller.initHealthRAileron - hbcontroller.HealthRAileron) / hbcontroller.initHealthRAileron * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_engineL", (hbcontroller.initHealthLEngine - hbcontroller.HealthLEngine) / hbcontroller.initHealthLEngine * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_engineR", (hbcontroller.initHealthREngine - hbcontroller.HealthREngine) / hbcontroller.initHealthREngine * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_flapsL", (hbcontroller.initHealthLFlap - hbcontroller.HealthLFlap) / hbcontroller.initHealthLFlap * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_flapsR", (hbcontroller.initHealthRFlap - hbcontroller.HealthRFlap) / hbcontroller.initHealthRFlap * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_elevatorL", (hbcontroller.initHealthLElevator - hbcontroller.HealthLElevator) / hbcontroller.initHealthLElevator * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_elevatorR", (hbcontroller.initHealthRElevator - hbcontroller.HealthRElevator) / hbcontroller.initHealthRElevator * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("health_body", (hbcontroller.initHealthBody - hbcontroller.HealthBody) / hbcontroller.initHealthBody * 100f * 0.01f);
                CanvasHUDAnimator[index].SetFloat("gauge_health", (hbcontroller.EngineControl.Health - hbcontroller.EngineControl.FullHealth) / hbcontroller.EngineControl.FullHealth * 100f * 0.01f);
            }
            //HitboxController Specifics. Comment ends here. 
        }

        if (index + 1 < maxLength)
        {
            index = index + 1;
        }
        else
        {
            index = 0;
        }
    }

    public void doPullup()
    {
        if (checkpulluptimer < checkPullupEvery)
        {
            checkpulluptimer = checkpulluptimer + Time.deltaTime;
            return;
        }
        else { checkpulluptimer = 0f; }

        if (EngineControl.EffectsControl.GearUp)
        {
            var hit = Physics.Raycast(EngineControl.GroundDetector.position, -(EngineControl.GroundDetector.up), pullupDistance, pullupdetector, QueryTriggerInteraction.UseGlobal);
            if (hit) CanvasHUDAnimator[index].SetBool("pull_up", true);
            else CanvasHUDAnimator[index].SetBool("pull_up", false);
        }
        else { CanvasHUDAnimator[index].SetBool("pull_up", false); }
    }

    public void doEngines()
    {
        //Comment only if statement and brackets if not using HBController. 
        if (!hbcontroller.isLEngineDead)
        {
            CanvasHUDAnimator[index].SetFloat("gauge_RPML", EngineControl.EngineOutput);
            CanvasHUDAnimator[index].SetFloat("gauge_speedL", doLogarithmicSpeed1 ? Mathf.Log((((EngineControl.CurrentVel.magnitude) * 1.9438445f) / SpeedDivisor1), LogarithmicVal1Speed) : (((EngineControl.CurrentVel.magnitude) * 1.9438445f) / SpeedDivisor1));
            CanvasHUDAnimator[index].SetBool("gauge_ABL", EffectsControl.AfterburnerOn);
        }
        //Comment if not using hbcontroller
        else
        {
            CanvasHUDAnimator[index].SetFloat("gauge_RPML", 0);
            CanvasHUDAnimator[index].SetFloat("gauge_speedL", 0);
            CanvasHUDAnimator[index].SetBool("gauge_ABL", false);
        }
        //Comment only if statement and brackets if not using HBController. 
        if (!hbcontroller.isREngineDead)
        {
            CanvasHUDAnimator[index].SetFloat("gauge_RPMR", EngineControl.EngineOutput);
            CanvasHUDAnimator[index].SetBool("gauge_ABR", EffectsControl.AfterburnerOn);
            CanvasHUDAnimator[index].SetFloat("gauge_speedR", doLogarithmicSpeed2 ? Mathf.Log((((EngineControl.CurrentVel.magnitude) * 1.9438445f) / SpeedDivisor2), LogarithmicVal2Speed) : (((EngineControl.CurrentVel.magnitude) * 1.9438445f) / SpeedDivisor2));
        }
        //Comment if not using hbcontroller
        else
        {
            CanvasHUDAnimator[index].SetFloat("gauge_RPMR", 0);
            CanvasHUDAnimator[index].SetFloat("gauge_speedR", 0);
            CanvasHUDAnimator[index].SetBool("gauge_ABR", false);
        }
        CanvasHUDAnimator[index].SetFloat("gauge_speed3", ((EngineControl.CurrentVel.magnitude) * 1.9438445f) / SpeedDivisor3);
    }
}