using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class FHudController : UdonSharpBehaviour
{
    public EngineController EngineController;
    public EffectsController EController;
    public GameObject PlaneBody;
    public Transform HeadingTool;
    public Transform Altimeter;
    public Transform Speedometer;
    public Transform GunHud;
    public Transform HorizonTool;
    public Transform LevelTool;
    public GameObject FlapsUI;
    public GameObject GearUpUI;
    public GameObject GearDownUI;
    public GameObject OverGUI;
    public GameObject DangerUI;
    public GameObject ChangeTargetUI;
    public GameObject NoTargetUI;
    public GameObject PullupUI;
    public GameObject LimiterUI;
    public GameObject AirBrakeUI;
    public AudioSource AB;
    public AudioSource MissileAlertSound;
    public GameObject ABUI;
    public AudioSource Pullupaudio;
    public Text AltimeterText;
    public Text SpeedometerText;
    public Text GsText;
    public Text SpeedMachText;
    public Text HealthText;
    public Text CopilotHealthText;
    public Text AngleOfAttack;
    public GameObject MaveFlipMode;
    public float divisibleValue = 5;
    private bool isPlayedAB = false;
    private bool isPlayedPullup = false;

    // public float SeaLevel = -200000;
    private Vector3 altimeterLerp = new Vector3(0, 0, 0);
    private Vector3 speedometerLerp = new Vector3(0, 0, 0);
    private Vector3 headingTurn = new Vector3(0, 0, 0);
    private Vector3 gunhudLerp = new Vector3(0, 0, 0);
    public float distance_from_head = 1.333f;
    public Transform parentTransform;
    public bool isVR;
    public bool onTesting = false;
    public PlayerUIScript UIScript;
    private VRCPlayerApi localPlayer;
    private Vector3 startSize;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        startSize = gameObject.transform.localScale;
        // if (EngineController!=null && !EngineController.InEditor) {
        //     gameObject.SetActive (false);
        // }
        // EngineController.localPlayer = Networking.LocalPlayer;
    }

    void Update()
    {
        if (localPlayer != null && (onTesting || EngineController != null) && UIScript != null)
        {
            if (parentTransform != null)
            {
                float offset = UIScript.vrDistance2;
                float size = UIScript.vrSize2;
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
        if (EngineController != null && EngineController.VehicleRigidbody != null && (EngineController.Passenger || EngineController.Piloting))
        {
            if (Speedometer != null)
            {
                if (EngineController.VehicleRigidbody)
                {
                    speedometerLerp = Vector3.Lerp(speedometerLerp, new Vector3(-((EngineController.CurrentVel.magnitude * 1.9438445f) / 5.5f), 0, 0), 4f * Time.deltaTime);
                    Speedometer.localRotation = Quaternion.Euler(speedometerLerp);
                }
            }
            if (MaveFlipMode != null)
            {
                if (EngineController != null)
                {
                    if (EngineController.EffectsControl.Flaps)
                    {
                        if (MaveFlipMode.activeSelf)
                            MaveFlipMode.SetActive(false);
                    }
                    else
                    {
                        if (!MaveFlipMode.activeSelf)
                            MaveFlipMode.SetActive(true);
                    }
                }
            }
            if (AirBrakeUI != null)
            {
                if (EngineController.BrakeInput == 1)
                {
                    if (!AirBrakeUI.activeSelf)
                        AirBrakeUI.SetActive(true);
                }
                else
                {
                    if (AirBrakeUI.activeSelf)
                        AirBrakeUI.SetActive(false);
                }
            }
            if (Altimeter != null)
            {
                altimeterLerp = Vector3.Lerp(altimeterLerp, new Vector3(-(EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f) / divisibleValue, 0, 0), 4.5f * Time.deltaTime);
                Altimeter.localRotation = Quaternion.Euler(altimeterLerp);
            }
            if (HorizonTool != null)
            {
                var hrotate = EngineController.VehicleMainObj.transform.localRotation.eulerAngles;
                // float new_z = hrotate.z;
                // hrotate.z = 0;
                hrotate.y = 0;
                hrotate.z = -hrotate.z;
                HorizonTool.localRotation = Quaternion.Euler(-hrotate);
                HorizonTool.localPosition = Vector3.zero;
            }
            if (LevelTool != null)
            {
                var hrotate = EngineController.VehicleMainObj.transform.rotation.eulerAngles;
                // float new_z = hrotate.z;
                hrotate.z = 0;
                hrotate.x = 0;
                hrotate.y = EngineController.VehicleMainObj.transform.rotation.eulerAngles.z;
                LevelTool.localRotation = Quaternion.Euler(-hrotate);
                LevelTool.localPosition = Vector3.zero;
            }
            if (FlapsUI != null)
            {
                if (EngineController.EffectsControl.Flaps)
                {
                    if (!FlapsUI.activeSelf)
                        FlapsUI.SetActive(true);
                }
                else
                {
                    if (FlapsUI.activeSelf)
                        FlapsUI.SetActive(false);
                }
            }
            if (HealthText != null) { HealthText.text = "HP: " + EngineController.Health.ToString("F0"); }
            if (CopilotHealthText != null) { CopilotHealthText.text = "HP: " + EngineController.Health.ToString("F0"); }
            if (AltimeterText != null)
            {
                AltimeterText.text = ((EngineController.CenterOfMass.position.y + -EngineController.SeaLevel) * 3.28084f).ToString("F0") + "ft";
            }
            if (SpeedometerText != null)
            {
                if (EngineController.VehicleRigidbody != null)
                {
                    SpeedometerText.text = (((EngineController.CurrentVel.magnitude) * 1.9438445f).ToString("F0")) + "kt";
                }
            }
            if (LimiterUI != null)
            {
                if (EngineController.FlightLimitsEnabled)
                {
                    if (!LimiterUI.activeSelf)
                        LimiterUI.SetActive(true);
                }
                else
                {
                    if (LimiterUI.activeSelf)
                        LimiterUI.SetActive(false);
                }
            }
            if (SpeedMachText != null)
            {
                SpeedMachText.text = ((EngineController.CurrentVel.magnitude) / 343f).ToString("F2");
            }
            if (GsText != null)
            {
                GsText.text = EngineController.Gs.ToString("F2");
            }
            if (AngleOfAttack != null)
            {
                AngleOfAttack.text = EngineController.AngleOfAttack.ToString("F0");
            }
            if (PullupUI != null)
            {
                if (EngineController.EffectsControl.GearUp)
                {
                    if ((EngineController.CenterOfMass.position.y + -EngineController.SeaLevel) * 3.28084f < 500f)
                    {
                        PullupUI.SetActive(true);
                        if (Pullupaudio != null && isPlayedPullup == false)
                        {
                            Pullupaudio.Play();
                            isPlayedPullup = true;
                        }
                    }
                    else
                    {
                        PullupUI.SetActive(false);
                        if (Pullupaudio != null && isPlayedPullup == true)
                        {
                            Pullupaudio.Stop();
                            isPlayedPullup = false;
                        }
                    }

                }
                else
                {
                    PullupUI.SetActive(false);
                    if (Pullupaudio != null && isPlayedPullup == true)
                    {
                        Pullupaudio.Stop();
                        isPlayedPullup = false;
                    }
                }
            }
            if (GearDownUI != null)
            {
                if (!EngineController.EffectsControl.GearUp)
                {
                    if (!GearDownUI.activeSelf)
                        GearDownUI.SetActive(true);
                }
                else
                {
                    if (GearDownUI.activeSelf)
                        GearDownUI.SetActive(false);
                }
            }
            if (DangerUI != null)
            {
                if (EngineController.Health < (EngineController.FullHealth * .25))
                {
                    if (!DangerUI.activeSelf)
                        DangerUI.SetActive(true);
                }
                else
                {
                    if (DangerUI.activeSelf)
                        DangerUI.SetActive(false);
                }
            }
            if (AB != null)
            {
                if (EngineController.EffectsControl.AfterburnerOn)
                {
                    if (isPlayedAB == false)
                    {
                        AB.Play();
                        isPlayedAB = true;
                        if (ABUI != null)
                            ABUI.SetActive(true);
                    }
                }
                else
                {
                    isPlayedAB = false;
                    if (ABUI != null)
                        ABUI.SetActive(false);
                }
            }
            if (OverGUI != null && EController != null)
            {
                if (EngineController.Gs > ((EngineController.MaxGs) * (.60f)))
                {
                    if (!OverGUI.activeSelf)
                        OverGUI.SetActive(true);
                }
                else
                {
                    if (OverGUI.activeSelf)
                        OverGUI.SetActive(false);
                }
            }
            if (GunHud != null)
            {

                // Vector3 tempvel = Vector3.zero;
                // if (EngineController.CurrentVel.magnitude < 2) {
                //     tempvel = -Vector3.up * 2; //straight down instead of spazzing out when moving very slow
                // } else {
                //     tempvel = EngineController.CurrentVel;
                // }
                Rigidbody MainBodyPlane = PlaneBody.GetComponent<Rigidbody>();
                var localVelocity = PlaneBody.transform.InverseTransformDirection(EngineController.CurrentVel);
                // Debug.Log("X:"+localVelocity.x +"::: Y:"+localVelocity.y+"::: Z:"+localVelocity.z);
                gunhudLerp = Vector3.Lerp(gunhudLerp, new Vector3(-localVelocity.x / 2.5f, 0, localVelocity.y / 2.5f), 4.5f * Time.deltaTime);
                GunHud.localPosition = gunhudLerp;

                // GunHud.position = transform.position + tempvel;
                // GunHud.localPosition = GunHud.localPosition.normalized * distance_from_head;
            }

            if (PlaneBody != null && HeadingTool != null)
            {
                Rigidbody MainBodyPlane = PlaneBody.GetComponent<Rigidbody>();
                float angle = (Mathf.Atan2(MainBodyPlane.velocity.x, MainBodyPlane.velocity.z) * Mathf.Rad2Deg);
                angle = (angle + 360f) % 360f;
                // headingTurn = Vector3.Lerp (headingTurn, new Vector3 (0, 0, -angle), 4.5f * Time.deltaTime);
                Vector3 headingTurn = EngineController.VehicleMainObj.transform.rotation.eulerAngles;
                headingTurn.z = EngineController.VehicleMainObj.transform.rotation.eulerAngles.y;
                headingTurn.x = 0;
                headingTurn.y = 0;
                HeadingTool.localRotation = Quaternion.Euler(headingTurn);
            }
        }
        if (EngineController != null && !EngineController.Piloting)
        {
            if (Pullupaudio != null && isPlayedPullup)
            {
                Pullupaudio.Stop();
                isPlayedPullup = false;
            }
        }

    }
}