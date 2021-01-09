using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
public class TriggerScript : UdonSharpBehaviour
{

    [Tooltip("Should Script run")]
    [Header("Run Options")]
    public bool run = false;
    public bool ran = false;
    public bool stopped = false;
    public bool stopOnEnd = false;
    // [UdonSynced (UdonSyncMode.None)] public bool ranSync = false;
    public float timer = 0f;
    public bool[] isRunning;
    public TriggerScript AfterRun;
    public PlayerUIScript UIScript;
    public bool playRegardless = false;

    [Tooltip("Should script run globally")]
    public bool runInSync = false;
    public bool disabled = false;
    [Header("TrackerObjects")]
    public MissileTrackerAndResponse[] TrackerObjects;
    public int[] TrackerObjectToShowAt; //[Index Number] - MissileTrackerResponse number, Number :: Dialog Line number.
    public int[] TrackerObjectToTargetableAt; //[Index Number] - MissileTrackerResponse number, Number  :: Dialog Line number.
    // public MissileTargets TargetUpdates;
    // public MissileTrackerAndResponse TrackerObject;
    public bool toHide = false; // Hide Tracker Objects
    public bool toShow = true; // Show Trackker Objects
    [Header("Dialog Lines Options")]
    public bool isJustRandom = false;
    public int currentX = 0;
    [TextArea] public string[] DialogLines;
    public float[] TimePerDialog;

    public AudioSource[] DialogSounds;
    // public Text textObject; //This is where all the texts are drawn. Maybe stick it with a player's Head.
    // public Text textObjectVR; //Automatically disabled if on VR
    public float delayBetweenDialogues = 1f;

    [Header("AI Options")]

    public bool enableAis = false;
    public bool disableAis = false;
    public bool setAIToFire = false;
    public int[] toggleAIOn; // Index - AI number, Value - Dialogue number.
    public AIObject[] aiToToggle;

    [Header("Event Options")]
    public EventManager eventManager;
    public bool isEvent;
    public bool isCloseEvent;
    public string eventId;
    private bool isCalledEvent = false;
    public bool clearBetweenDialogues = true;

    [Header("UdonBehaviour Call Options")]
    public bool callUBEvent = false;
    public UdonBehaviour UB;
    public bool runUBEventGlobally = false;
    public int runUBEventon = 0;
    public string customEvent;

    [Header("Music Options")]
    public AudioSource Music;
    public bool PlayMusic = false;
    public bool StopMusic = false;
    public int PlayMusicOn = 0;
    [TextArea] public string MusicDetails; //Or Chapter.

    void Start()
    {
        isRunning = new bool[DialogLines.Length];
        for (int x = 0; x < isRunning.Length; x++)
        {
            isRunning[x] = false;
        }
        Assert(UIScript != null, "Start: UISCRIPT MUST NOT BE null");
    }

    public void runSync()
    {
        // if (!Networking.IsOwner (gameObject)){
        run = true;
        ran = true;
        UIScript.ReceiveTrigger(this);
        // }
    }

    public void UBevent()
    {
        if (UB != null)
        {
            UB.SendCustomEvent(customEvent);
        }
    }

    public void runEvent()
    {
        run = true;
    }

    void Update()
    {
        if (run && runInSync == false && ran == false && !stopped)
        { // If global event..
            ran = true;
            UIScript.ReceiveTrigger(this);
            // runScript ();
        }
        else if (run && runInSync && ran == false && !stopped)
        { // If LocalPlayer event only...
            // ranSync = true;
            // ran = true;
            if (Networking.LocalPlayer == null) { UIScript.ReceiveTrigger(this); }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runSync");
            ran = true;
        }
        if (stopped && !stopOnEnd)
        { // do not 'stop' after trigged.
            stopped = false;
        }

        if (ran & !stopped && ((!playRegardless && UIScript.TriggerScripts.Length > 0 && UIScript.TriggerScripts[0] == this) || playRegardless))
        {
            // if ((ran) && !stopped) {
            if (isEvent && eventManager != null && !isCalledEvent)
            {
                eventManager.activeCall(this);
                isCalledEvent = true;
            }
            if (isCloseEvent && !isCalledEvent)
            {
                isCalledEvent = true;
                eventManager.closeEventCall(eventId);
            }
            if (!isRunning[currentX])
            {
                isRunning[currentX] = true;
                if (UIScript.textObject != null)
                {
                    UIScript.textObject.text = DialogLines[currentX];
                }
                if (UIScript.textObjectVR != null)
                {
                    UIScript.textObjectVR.text = DialogLines[currentX];
                }
                if (DialogSounds[currentX] != null)
                {
                    DialogSounds[currentX].Play();
                }
            }

            if (PlayMusic && !StopMusic && PlayMusicOn == currentX)
            {
                UIScript.ReceiveMusic(Music, MusicDetails);
            }

            if (!PlayMusic && StopMusic && PlayMusicOn == currentX)
            {
                UIScript.StopMusic();
            }



            // if (enableAI || disableAI) {
            //     if (aiToToggle != null) {
            //         if (currentX == toggleAIOn) {
            //             if (enableAI) { aiToToggle.disabled = false; }
            //             if (disableAI) { aiToToggle.disabled = true; }
            //         }
            //     }
            // }
            if (!isJustRandom)
            {

            }

            if (timer > TimePerDialog[currentX])
            {
                if (timer > TimePerDialog[currentX] + delayBetweenDialogues)
                {
                    timer = 0f;
                    if (currentX + 1 == DialogLines.Length)
                    {
                        run = false;
                        ran = false;
                        stopped = true;
                        UIScript.RemoveTrigger(this);
                        // ranSync = false;
                        if (clearBetweenDialogues)
                        {
                            UIScript.textObject.text = "";
                            UIScript.textObjectVR.text = "";
                        }
                        for (int x = 0; x < isRunning.Length; x++)
                        {
                            isRunning[x] = false;
                        }
                        currentX = 0;
                        if (AfterRun != null)
                        {
                            AfterRun.run = true;
                        }
                    }
                    else
                    {
                        currentX = currentX + 1;
                    }
                }
                if (clearBetweenDialogues)
                {
                    if (UIScript.textObject != null)
                    {
                        UIScript.textObject.text = "";
                    }
                    if (UIScript.textObjectVR != null)
                    {
                        UIScript.textObjectVR.text = "";
                    }
                }
            }
            if (TrackerObjects != null && TrackerObjects.Length > 0)
                for (int bx = 0; bx < TrackerObjects.Length; bx++)
                {

                    if (TrackerObjectToShowAt != null && TrackerObjectToShowAt.Length > 0)
                    {
                        for (int g = 0; g < TrackerObjectToShowAt.Length; g++)
                        {
                            if (TrackerObjectToShowAt[g] != -1 && TrackerObjectToShowAt[g] == currentX)
                            {
                                if (toShow) { TrackerObjects[g].isRendered = true; }
                                if (toHide)
                                {
                                    TrackerObjects[g].isRendered = false;
                                    TrackerObjects[g].isTargetable = false;
                                }
                            }
                        }

                    }
                    if (TrackerObjectToTargetableAt != null && TrackerObjectToTargetableAt.Length > 0)
                    {
                        for (int g = 0; g < TrackerObjectToTargetableAt.Length; g++)
                        {
                            if (TrackerObjectToTargetableAt[g] != -1 && TrackerObjectToTargetableAt[g] == currentX)
                            {
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
            if (aiToToggle != null && aiToToggle.Length > 0)
                for (int bx = 0; bx < aiToToggle.Length; bx++)
                {
                    if (toggleAIOn.Length > 0)
                    {
                        for (int g = 0; g < toggleAIOn.Length; g++)
                        {
                            if (toggleAIOn[g] != -1 && toggleAIOn[g] == currentX)
                            {
                                if (enableAis) { aiToToggle[g].disabled = false; }
                                if (disableAis) { aiToToggle[g].disabled = true; }
                            }
                        }
                    }
                }

            if (callUBEvent && UB != null && customEvent != null && runUBEventon == currentX)
            {
                if (runUBEventGlobally)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "UBevent");
                }
                else
                {
                    UBevent();
                }
            }

            timer = timer + Time.deltaTime;
            // }
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