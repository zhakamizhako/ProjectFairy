using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerScript : UdonSharpBehaviour {
    public bool run = false; // synced with everyone else
    public bool runInSync = false;
    public bool disabled = false;
    public bool toHide = false; // Hide Tracker Objects
    public bool toShow = true; // Show Trackker Objects
    [TextArea] public string[] DialogLines;
    public float[] TimePerDialog;
    public int currentX = 0;
    public MissileTrackerAndResponse[] TrackerObjects;
    public int[] TrackerObjectToShowAt; //[Index Number] - MissileTrackerResponse number, Number :: Dialog Line number.
    public int[] TrackerObjectToTargetableAt; //[Index Number] - MissileTrackerResponse number, Number  :: Dialog Line number.
    public MissileTargets TargetUpdates;
    // public MissileTrackerAndResponse TrackerObject;
    public AudioSource[] DialogSounds;
    public Text textObject; //This is where all the texts are drawn. Maybe stick it with a player's Head.
    public Text textObjectVR; //Automatically disabled if on VR
    public float delayBetweenDialogues = 1f;
    public bool ran = false;
    public bool stopped = false;
    public bool stopOnEnd = false;
    [UdonSynced (UdonSyncMode.None)] public bool ranSync = false;
    public float timer = 0f;
    public bool[] isRunning;
    public bool enableAis = false;
    public bool disableAis = false;
    public int[] toggleAIOn; // Index - AI number, Value - Dialogue number.
    public AIObject[] aiToToggle;
    public TriggerScript AfterRun;

    void Start () {
        isRunning = new bool[DialogLines.Length];
        for (int x = 0; x < isRunning.Length; x++) {
            isRunning[x] = false;
        }
    }

    // void runScript () {
    //     Debug.Log ("Script RUN");
    //     for (float timer = 0f; currentX < DialogLines.Length; timer = timer + Time.deltaTime) {

    //     }
    //     if (textObjectVR != null) {
    //         textObjectVR.text = "";
    //     }
    //     textObject.text = "";
    //     Debug.Log ("Script Done");
    // }

    void Update () {
        if (run && runInSync == false && ran == false) { // If global event..
            ran = true;
            // runScript ();
        } else if (run && runInSync && ran == false) { // If LocalPlayer event only...
            ranSync = true;
            // ran = true;
            // SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runScript");
        }
        if (stopped && !stopOnEnd) { // do not 'stop' after trigged.
            stopped = false;
        }

        if ((ran || ranSync) && !stopped) {
            if (!isRunning[currentX]) {
                isRunning[currentX] = true;
                if (textObject != null) {
                    textObject.text = DialogLines[currentX];
                }
                if (textObjectVR != null) {
                    textObjectVR.text = DialogLines[currentX];
                }
                if (DialogSounds[currentX] != null) {
                    DialogSounds[currentX].Play ();
                }
            }

            // if (enableAI || disableAI) {
            //     if (aiToToggle != null) {
            //         if (currentX == toggleAIOn) {
            //             if (enableAI) { aiToToggle.disabled = false; }
            //             if (disableAI) { aiToToggle.disabled = true; }
            //         }
            //     }
            // }

            if (timer > TimePerDialog[currentX]) {
                if (timer > TimePerDialog[currentX] + delayBetweenDialogues) {
                    timer = 0f;
                    if (currentX + 1 == DialogLines.Length) {
                        run = false;
                        ran = false;
                        ranSync = false;
                        stopped = true;
                        textObject.text = "";
                        textObjectVR.text = "";
                        for (int x = 0; x < isRunning.Length; x++) {
                            isRunning[x] = false;
                        }
                        currentX = 0;
                        if (AfterRun != null) {
                            AfterRun.run = true;
                        }
                    } else {
                        currentX = currentX + 1;
                    }
                }
                if (textObject != null) {
                    textObject.text = "";
                }
                if (textObjectVR != null) {
                    textObjectVR.text = "";
                }
            }
            for (int bx = 0; bx < TrackerObjects.Length; bx++) {

                if (TrackerObjectToShowAt.Length > 0) {
                    for (int g = 0; g < TrackerObjectToShowAt.Length; g++) {
                        if (TrackerObjectToShowAt[g] != -1 && TrackerObjectToShowAt[g] == currentX) {
                            if (toShow) { TrackerObjects[g].isRendered = true; }
                            if (toHide) {
                                TrackerObjects[g].isRendered = false;
                                TrackerObjects[g].isTargetable = false;
                            }
                        }
                    }

                }
                if (TrackerObjectToTargetableAt.Length > 0) {
                    for (int g = 0; g < TrackerObjectToTargetableAt.Length; g++) {
                        if (TrackerObjectToTargetableAt[g] != -1 && TrackerObjectToTargetableAt[g] == currentX) {
                            TrackerObjects[g].isTargetable = true;
                        }
                    }
                }

                // if (TrackerObjects[bx] != null && toHide) {
                //     if (TrackerObjectToShowAt[bx] != -1 && TrackerObjectToShowAt[bx] == currentX) {
                //         TrackerObjects[TrackerObjectToShowAt[bx]].isRendered = false;
                //         TrackerObjects[TrackerObjectToShowAt[bx]].isTargetable = false;
                //     }
                // }
            }

            for(int bx = 0; bx < aiToToggle.Length;bx++){
                if(toggleAIOn.Length > 0){
                    for(int g = 0; g < toggleAIOn.Length;g++){
                        if(toggleAIOn[g] !=-1 && toggleAIOn[g]== currentX){
                            if(enableAis){ aiToToggle[g].disabled = false; }
                            if(disableAis){ aiToToggle[g].disabled = true; }
                        }
                    }
                }
            }

            timer = timer + Time.deltaTime;
        }
    }
}