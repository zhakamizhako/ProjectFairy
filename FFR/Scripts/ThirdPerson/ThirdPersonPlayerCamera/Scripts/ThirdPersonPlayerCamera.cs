using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[AddComponentMenu("")]
public class ThirdPersonPlayerCamera : UdonSharpBehaviour
{

    private VRCPlayerApi _playerLocal;
    private GameObject _visionTracker;
    public Camera ThirdCam;
    private Camera _uiCamera;
    private GameObject _follower;
    private AudioSource _audioPlayer;
    private Camera _camera;
    private bool _isEditor;
    public bool _isActive;
    private bool _isStatic;
    private bool _isVr;
    private bool _zoom;
    private int _mode = 1;

    [Tooltip("Follow points for the camera")]
    public Transform[] cameraTarget;
    public AudioClip[] soundEffects;
    public GameObject[] menuItems;

    [Tooltip("Which layers to collide with")]
    public LayerMask wallCollisions;
    [Tooltip("How fast the camera follows the player")]
    public float followSpeed = 1.5f;
    [Tooltip("How fast the camera rotates to follow the player")]
    public float rotationSpeed = 1.5f;
    [Tooltip("Whether or not the camera lerps or moves instantly")]
    public bool movementSmoothing = false;
    public GameObject PostProcessingGlobal;
    public bool isPlayerCamera = true;
    public Transform FollowObject;
    // public GameObject VRDialog;
    // public GameObject DesktopDialog;
    public bool enabledCam = false;
    public bool TabShown = false;

    void Start()
    {
        _playerLocal = Networking.LocalPlayer;
        if (_playerLocal == null)
        {
            _isEditor = true;
        }
        else
        {
            if (_playerLocal.IsUserInVR()) _isVr = true;

            // if (_isVr) {
            //     if (DesktopDialog != null) {
            //         DesktopDialog.SetActive (false);
            //     }
            // }
            // if (!_isVr) {
            //     if (VRDialog != null) {
            //         VRDialog.SetActive (false);
            //     }
            // }
        }

        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name == "Third_Camera") ThirdCam = (Camera)child.GetComponent<Camera>();
            if (child.name == "Vision_Tracker")
            {
                _audioPlayer = (AudioSource)child.GetComponent<AudioSource>();
                if (!_isVr) _uiCamera = (Camera)child.GetComponent<Camera>();
                _visionTracker = child.gameObject;
            }
            if (child.name == "Follower") _follower = child.gameObject;
        }
        _camera = ThirdCam.GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (!_isEditor)
        {
            if (isPlayerCamera)
            {
                _visionTracker.transform.position = _playerLocal.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                _visionTracker.transform.rotation = _playerLocal.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            }
            else
            {
                _visionTracker.transform.position = FollowObject.position;
                _visionTracker.transform.rotation = FollowObject.rotation;
            }

        }
        if (isPlayerCamera)
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                _isActive = !_isActive;
                ThirdCam.enabled = _isActive;
                if (!_isVr) _uiCamera.enabled = _isActive;
                _audioPlayer.PlayOneShot(soundEffects[0]);
                _follower.transform.position = cameraTarget[_mode].position;
                _follower.transform.rotation = cameraTarget[_mode].rotation;
                ThirdCam.transform.position = cameraTarget[_mode].position;
                ThirdCam.transform.rotation = cameraTarget[_mode].rotation;
                enabledCam = ThirdCam.enabled;
                // PostProcessingGlobal.SetActive(!enabledCam);
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                menuItems[0].SetActive(!menuItems[0].activeInHierarchy);
                TabShown = menuItems[0].activeSelf;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
        }
        if (_isActive) CamManager();

    }

    private void CamManager()
    {
        if (isPlayerCamera)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0)) //Behind Player
            {
                if (_isStatic) return;
                if (_mode == 0) return;
                _mode = 0;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) //Right Shoulder
            {
                if (_isStatic) return;
                if (_mode == 1) return;
                _mode = 1;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) //Left Shoulder
            {
                if (_isStatic) return;
                if (_mode == 2) return;
                _mode = 2;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) //In-front of Player
            {
                if (_isStatic) return;
                if (_mode == 3) return;
                _mode = 3;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) //In-front of Player
            {
                movementSmoothing = !movementSmoothing;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) //Free Camera
            {
                _isStatic = !_isStatic;
                _audioPlayer.PlayOneShot(soundEffects[2]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6)) //Zoom
            {
                _zoom = !_zoom;
                if (_zoom)
                {
                    _audioPlayer.PlayOneShot(soundEffects[4]);
                }
                else
                {
                    _audioPlayer.PlayOneShot(soundEffects[3]);
                }
            }
            if (_zoom)
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, 30, 0.25f);
            }
            else
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, 60, 0.25f);
            }
        }

        if (!_isStatic)
        {
            _follower.transform.position = cameraTarget[_mode].position;
            _follower.transform.rotation = cameraTarget[_mode].rotation;

            var wallHit = new RaycastHit();
            if (Physics.Linecast(_visionTracker.transform.position, _follower.transform.position, out wallHit, wallCollisions))
            {
                _follower.transform.position = wallHit.point;
            }
            else
            {
                _follower.transform.position = cameraTarget[_mode].transform.position;
            }
            if (movementSmoothing)
            {
                var thirdTran = ThirdCam.transform;
                ThirdCam.transform.position = Vector3.Lerp(thirdTran.position, _follower.transform.position + (_follower.transform.forward / 10), followSpeed * 0.1f);
                ThirdCam.transform.rotation = Quaternion.Lerp(thirdTran.rotation, _follower.transform.rotation, rotationSpeed * 0.1f);
            }
            else
            {
                ThirdCam.transform.position = _follower.transform.position;
                ThirdCam.transform.rotation = _follower.transform.rotation;
            }
        }

    }
}