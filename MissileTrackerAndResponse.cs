using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class MissileTrackerAndResponse : UdonSharpBehaviour {
        public Transform Tailer; // Attach the Tailer to the flareObject, mind you.
        public Transform Tailer2; // for the AA Guns to track. I dont think we need to drop this.
        public SoundController Soundman;
        public GameObject hudAlert;
        public GameObject TargetIconRender;
        public GameObject MainObject;
        public AITurretScript AITurret;
        public AIObject AI;
        public AudioSource CautionSound;
        public GameObject CautionObject;
        public AircraftController EngineController;
        public Text TrackerText;
        public bool isTargetable = true;
        public bool isRendered = true;
        // [UdonSynced (UdonSyncMode.None)] public bool isGloballyRendered = true;
        public bool isWaypoint = false;
        public TriggerScript onEnter; // For waypoints
        [UdonSynced (UdonSyncMode.None)] public bool isTracking = false;
        [UdonSynced (UdonSyncMode.None)] public bool isChasing = false;
        private bool soundStarted = false;
        private bool soundCautionStarted = false;
        public Transform WaypointDetector;
        public float WaypointDetectorRange = 100;
        public float WaypointDetectorRadius = 100;
        public LayerMask layermask = 23;
        public bool hideAfterWaypointContact = true;
        public bool hideInSyncContact = true;
        void Start () {
        }

        void hideSync(){
            isRendered = false;
        }

        void Update () {
            if (isChasing == true) {
                // Debug.Log("Someone's chasing me.");
            }
            if (isWaypoint) {
                // if(isGloballyRendered){

                // }
                RaycastHit[] hit = Physics.SphereCastAll (WaypointDetector.position, WaypointDetectorRadius, WaypointDetector.forward, WaypointDetectorRange, layermask, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects
                if (hit.Length > 0) {
                    for (int x = 0; x < hit.Length; x++) { // skim through the list of detected targets
                        GameObject currentHitObject = hit[x].transform.gameObject;
                        float hitDistance = hit[x].distance;
                        Transform[] children = currentHitObject.GetComponentsInChildren<Transform> ();
                        MissileTrackerAndResponse bb = null;
                        if (currentHitObject.GetComponent<HitDetector> () != null) {
                            bb = currentHitObject.GetComponent<HitDetector> ().Tracker;
                        } else if (currentHitObject.GetComponent<MissileTrackerAndResponse> () != null) {
                            bb = currentHitObject.GetComponent<MissileTrackerAndResponse> ();
                        }

                        if (bb.EngineController != null) {
                            if (bb.EngineController.localPlayer != null && bb.EngineController.localPlayer.IsOwner (bb.EngineController.VehicleMainObj)) {
                                if (hideAfterWaypointContact) {
                                    isRendered = false;
                                }
                            } else if (bb.EngineController.localPlayer == null) { //Editormode
                                if (hideAfterWaypointContact) {
                                    isRendered = false;
                                }
                                if(hideInSyncContact){
                                    SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "hideSync");
                                }
                            }

                            if (onEnter != null) {
                                onEnter.run = true;
                            }
                        }
                    }
                }
            }
                if (Soundman != null && isChasing == true && soundStarted == false) { // Yeah, a constant checker i know. 
                    if (Soundman.MissileAlert != null)
                        Soundman.MissileAlert.Play ();
                    soundStarted = true;
                    if (hudAlert != null)
                        hudAlert.SetActive (true);
                    Debug.Log ("Alert sound started playing");
                } else if (Soundman != null && soundStarted == true && isChasing == false) {
                    if (hudAlert != null)
                        hudAlert.SetActive (false);
                    Soundman.MissileAlert.Stop ();
                    soundStarted = false;
                    Debug.Log ("Alert sound Stopped");
                }

                if (isTracking) {
                    if (CautionSound != null && soundCautionStarted == false) {
                        CautionSound.Play ();
                        if(CautionObject!=null)
                        CautionObject.SetActive (true);
                        soundCautionStarted = true;

                    }
                } else {
                    if (CautionSound != null && soundCautionStarted == true) {
                        CautionSound.Stop ();
                        if(CautionObject!=null)
                        CautionObject.SetActive (false);
                        soundCautionStarted = false;

                    }
                }

                if(EngineController!=null){
                    if(EngineController.dead){
                        if(CautionSound!=null && soundCautionStarted==true){
                            CautionSound.Stop();
                            if(CautionObject!=null)
                            CautionObject.SetActive(false);
                            soundCautionStarted = false;
                        }
                        if(Soundman!=null){
                            Soundman.MissileAlert.Stop();
                            soundStarted = false;
                            if(hudAlert!=null)
                            hudAlert.SetActive(false);
                        }
                    }
                }

            }
        }