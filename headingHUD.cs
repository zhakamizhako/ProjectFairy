using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class headingHUD : UdonSharpBehaviour {
    public AircraftController EngineController;
    public EffectsController EController;
    // public HudController HUDController;
    public GameObject PlaneBody;
    public Transform HeadingTool;
    public Transform Altimeter;
    public Transform Speedometer;
    public Transform GunHud;
    public GameObject FlapsUI;
    public GameObject GearUpUI;
    public GameObject GearDownUI;
    public GameObject OverGUI;
    public GameObject DangerUI;
    public GameObject ChangeTargetUI;
    public GameObject NoTargetUI;
    public GameObject PullupUI;
    public AudioSource AB;
    public GameObject ABUI;
    public AudioSource Pullupaudio;
    public Text AltimeterText;
    public Text SpeedometerText;
    public Text GsText;
    public Text SpeedMachText;
    public Text HealthText;
    public Text CopilotHealthText;
    public GameObject MaveFlipMode;
    [UdonSynced (UdonSyncMode.None)] private float speedFloatSync;
    [UdonSynced (UdonSyncMode.None)] private string GSpeedSync;
    [UdonSynced (UdonSyncMode.None)] private string HealthTextSync;
    
    public float divisibleValue = 5;
    public float timeout = 1.5f;
    public float timer = 0;
    private bool isPlayedAB = false;
    private bool isPlayedPullup = false;

    // public float SeaLevel = -200000;
    private Vector3 altimeterLerp = new Vector3 (0, 0, 0);
    private Vector3 speedometerLerp = new Vector3 (0, 0, 0);
    private Vector3 headingTurn = new Vector3 (0, 0, 0);
    private Vector3 gunhudLerp = new Vector3 (0, 0, 0);

    void Start () {
        // EngineController.localPlayer = Networking.LocalPlayer;
    }

    void Update () {
        if (EngineController != null && EngineController.VehicleRigidbody != null) {
            if (Speedometer != null) {
                if (EngineController.VehicleRigidbody) {
                    speedometerLerp = Vector3.Lerp (speedometerLerp, new Vector3 (-((EngineController.speedFloatSync * 1.9438445f) / 5.5f), 0, 0), 4f * Time.deltaTime);
                    Speedometer.localRotation = Quaternion.Euler (speedometerLerp);
                }
            }
            if (MaveFlipMode != null) {
                if (EngineController != null) {
                    if (EngineController.Flaps) {
                        MaveFlipMode.SetActive (false);
                    } else {
                        MaveFlipMode.SetActive (true);
                    }
                }
            }
            if (Altimeter != null) {
                altimeterLerp = Vector3.Lerp (altimeterLerp, new Vector3 (-(EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f) / divisibleValue, 0, 0), 4.5f * Time.deltaTime);
                Altimeter.localRotation = Quaternion.Euler (altimeterLerp);
            }
            if (FlapsUI != null) { if (EngineController.Flaps) { FlapsUI.SetActive (true); } else { FlapsUI.SetActive (false); } }
            if (HealthText != null) { HealthText.text = "HP: " + EngineController.Health.ToString ("F0"); }
            if (CopilotHealthText != null) { CopilotHealthText.text = "HP: " + EngineController.Health.ToString ("F0"); }
            if (AltimeterText != null) {
                AltimeterText.text = ((EngineController.CenterOfMass.position.y + -EngineController.SeaLevel) * 3.28084f).ToString ("F0") + "ft";
            }
            if (SpeedometerText != null) {
                if (EngineController.VehicleRigidbody != null) {
                    SpeedometerText.text = (((EngineController.speedFloatSync) * 1.9438445f).ToString ("F0")) + "kt";
                }
            }
            if (SpeedMachText != null) {
                SpeedMachText.text = ((EngineController.speedFloatSync) / 343f).ToString ("F2");
            }
            if (GsText != null) {
                GsText.text = EngineController.Gs.ToString ("F2");
            }
            if (PullupUI != null) {
                if (EngineController.GearUp) {
                    if ((EngineController.CenterOfMass.position.y + -EngineController.SeaLevel) * 3.28084f < 500f) {
                        PullupUI.SetActive (true);
                        if (Pullupaudio != null && isPlayedPullup == false) {
                            Pullupaudio.Play ();
                            isPlayedPullup = true;
                        }
                    } else {
                        PullupUI.SetActive (false);
                        if (Pullupaudio != null && isPlayedPullup == true) {
                            Pullupaudio.Stop ();
                            isPlayedPullup = false;
                        }
                    }

                } else {
                    PullupUI.SetActive (false);
                    if (Pullupaudio != null && isPlayedPullup == true) {
                        Pullupaudio.Stop ();
                        isPlayedPullup = false;
                    }
                }
            }
            if (GearDownUI != null) { if (!EngineController.GearUp) { GearDownUI.SetActive (true); } else { GearDownUI.SetActive (false); } }
            if (DangerUI != null) {
                if (EngineController.Health < (EngineController.FullHealth * .25)) {
                    DangerUI.SetActive (true);
                } else {
                    DangerUI.SetActive (false);
                }
            }
            if (AB != null) {
                if (EngineController.Afterburner) {
                    if (isPlayedAB == false) {
                        Debug.Log ("Played");
                        AB.Play ();
                        isPlayedAB = true;
                        if (ABUI != null)
                            ABUI.SetActive (true);
                    }
                } else {
                    isPlayedAB = false;
                    if (ABUI != null)
                        ABUI.SetActive (false);
                }
            }
            if (OverGUI != null && EController != null) {
                if (EngineController.Gs > ((EController.MaxGs) * (.75f))) {
                    OverGUI.SetActive (true);
                } else {
                    OverGUI.SetActive (false);
                }
            }
            if (GunHud != null) {
                Rigidbody MainBodyPlane = PlaneBody.GetComponent<Rigidbody> ();
                var localVelocity = PlaneBody.transform.InverseTransformDirection (MainBodyPlane.velocity);
                // Debug.Log("X:"+localVelocity.x +"::: Y:"+localVelocity.y+"::: Z:"+localVelocity.z);
                gunhudLerp = Vector3.Lerp (gunhudLerp, new Vector3 (-localVelocity.x / 2.5f, 0, localVelocity.y / 2.5f), 4.5f * Time.deltaTime);
                GunHud.localPosition = gunhudLerp;
            }

            if (PlaneBody != null && HeadingTool != null) {
                Rigidbody MainBodyPlane = PlaneBody.GetComponent<Rigidbody> ();
                float angle = (Mathf.Atan2 (MainBodyPlane.velocity.x, MainBodyPlane.velocity.z) * Mathf.Rad2Deg);
                angle = (angle + 360f) % 360f;
                headingTurn = Vector3.Lerp (headingTurn, new Vector3 (0, 0, -angle), 4.5f * Time.deltaTime);
                HeadingTool.localRotation = Quaternion.Euler (headingTurn);
            }
        }

    }
}