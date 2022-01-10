using UdonSharp;
using UnityEngine;
// ReSharper disable once RedundantUsingDirective
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
    public TriggerScript[] AfterRuns;
    public PlayerUIScript UIScript;
    public bool playRegardless = false;

    [Tooltip("Should script run globally")]
    public bool runInSync = false;
    public bool disabled = false;
    [Header("TrackerObjects")]
    public GameObject[] GameObjectsToManipulate;
    public bool EnableOrDisable = false;
    public MissileTrackerAndResponse[] TrackerObjects;
    public int[] TrackerObjectToShowAt; //[Index Number] - MissileTrackerResponse number, Number :: Dialog Line number.
    public int[] TrackerObjectToTargetableAt; //[Index Number] - MissileTrackerResponse number, Number  :: Dialog Line number.
    // public MissileTargets TargetUpdates;
    // public MissileTrackerAndResponse TrackerObject;
    public bool toHide = false; // Hide Tracker Objects
    public bool toShow = true; // Show Trackker Objects
    [Header("Dialog Lines Options")]
    public int textObjectId = 0;
    public bool isJustRandom = false;
    public int currentX = 0;
    [TextArea] public string[] DialogLines;
    [TextArea] public string[] DialogLinesJP;
    public float[] TimePerDialog;
    public AudioSource[] DialogSounds;
    public AudioSource[] DialogSoundsJP;
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
    public AudioSource IntroMusic;
    public bool PlayMusic = false;
    public bool StopMusic = false;
    public int PlayMusicOn = 0;
    [TextArea] public string MusicDetails; //Or Chapter.
    private bool updateString = false;
    [System.NonSerializedAttribute] public bool ranAfterRun = false;
    [System.NonSerializedAttribute] public bool enabledGameObject = false;
    public bool canIgnoreDialogue = false;
    public float timeBeforeIgnore = 3f;
    public float timerForIgnore = 0f;

    public Animator aniTrigger;
    public string AnimatorString;
    public bool AnimatorArgument;
    public int RunAnimatorOn = 0;
    public bool isSameAsEn = true;
    public bool isSameAsEnSound = true; // just default for safety shit

    public SceneAdaptor sceneAdaptorToRun; // Scene Adaptor to run after the dialogues run.
    public bool runSceneAdaptor = false;
    [System.NonSerializedAttribute] public bool ranSceneAdaptor = false;
    public float sleepIn = 5;
    public float sleepTimer = 0f;

    public int uid;

    void Start()
    {
        isRunning = DialogLines != null ? new bool[DialogLines.Length] : new bool[0];
        for (int x = 0; x < isRunning.Length; x++)
        {
            isRunning[x] = false;
        }
        Assert(UIScript != null, "Start: UISCRIPT MUST NOT BE null");

    }

    public void callRunSync()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runSync");
    }

    public void resetScript()
    {
        stopped = false;
        ranAfterRun = false;
        enabledGameObject = false;
        currentX = 0;
        runSceneAdaptor = false;
    }

    public void runSync()
    {
        if (!ran)
        {
            // if (!Networking.IsOwner (gameObject)){
            run = true;
            ran = true;
            UIScript.ReceiveTrigger(this, textObjectId);
            // }
        }
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
        if(UIScript.sleepTriggers && !ran){
            sleepTimer = sleepTimer+Time.deltaTime;
            if(sleepTimer > sleepIn){
                sleepTimer = 0f;
                gameObject.SetActive(false);
            }
        }
        if (run && runInSync == false && ran == false && !stopped)
        { // If global event..
            ran = true;
            if (!playRegardless)
                UIScript.ReceiveTrigger(this, textObjectId);
            // runScript ();
        }
        else if (run && runInSync && ran == false && !stopped)
        { // If LocalPlayer event only...
            // ranSync = true;
            // ran = true;
            if (Networking.LocalPlayer == null) { UIScript.ReceiveTrigger(this, textObjectId); }
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runSync");
            ran = true;
        }
        if (stopped && !stopOnEnd)
        { // do not 'stop' after trigged.
            stopped = false;
        }

        if (canIgnoreDialogue && ran && !stopped && !playRegardless && UIScript != null && UIScript.TriggerScripts[textObjectId] != null && UIScript.TriggerScripts[textObjectId][0] != this)
        {
            if (timerForIgnore < timeBeforeIgnore)
            {
                timerForIgnore = timerForIgnore + Time.deltaTime;
            }
            if (timerForIgnore > timeBeforeIgnore)
            {
                run = false;
                ran = false;
                stopped = true;
                UIScript.RemoveTrigger(this, textObjectId);
                timerForIgnore = 0f;
            }
        }

        if (!(ran & !stopped) || ((playRegardless || UIScript.TriggerScripts.Length <= 0 ||
                                   UIScript.TriggerScripts[textObjectId] == null ||
                                   UIScript.TriggerScripts[textObjectId].Length <= 0 ||
                                   UIScript.TriggerScripts[textObjectId][0] != this) && !playRegardless)) return;
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
            //dialogues
            if (isSameAsEn || (!isSameAsEn && !UIScript.isEnglishOrJapanese))//en
            {
                if (UIScript.textObject[textObjectId] != null && !updateString)
                {
                    UIScript.textObject[textObjectId].text = DialogLines[currentX];
                }
                if (UIScript.textObjectVR[textObjectId] != null && !updateString)
                {
                    UIScript.textObjectVR[textObjectId].text = DialogLines[currentX];
                }

            }
            if (!isSameAsEn && UIScript.isEnglishOrJapanese)//jp
            {
                if (UIScript.textObject[textObjectId] != null && !updateString)
                {
                    UIScript.textObject[textObjectId].text = DialogLinesJP[currentX];
                }
                if (UIScript.textObjectVR[textObjectId] != null && !updateString)
                {
                    UIScript.textObjectVR[textObjectId].text = DialogLinesJP[currentX];
                }
            }

            //Sound
            if (isSameAsEnSound || (!isSameAsEnSound && !UIScript.isEnglishOrJapaneseVoice))
            {
                if (DialogSounds[currentX] != null)
                {
                    DialogSounds[currentX].Play();
                }
            }
            if (!isSameAsEnSound && UIScript.isEnglishOrJapaneseVoice)
            {
                if (DialogSoundsJP[currentX] != null)
                {
                    DialogSoundsJP[currentX].Play();
                }
            }

            updateString = true;
            if (aniTrigger != null && AnimatorString != null && RunAnimatorOn == currentX)
            {
                aniTrigger.SetBool(AnimatorString, AnimatorArgument);
            }
            if (PlayMusic && !StopMusic && PlayMusicOn == currentX)
            {
                UIScript.ReceiveMusic(Music, IntroMusic, MusicDetails);
            }

            if (!PlayMusic && StopMusic && PlayMusicOn == currentX)
            {
                UIScript.StopMusic();
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
        }

        if (!enabledGameObject && GameObjectsToManipulate != null && GameObjectsToManipulate.Length > 0)
        {
            foreach (GameObject x in GameObjectsToManipulate)
            {
                x.SetActive(EnableOrDisable);
            }
            enabledGameObject = true;
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
                    UIScript.RemoveTrigger(this, textObjectId);
                    // ranSync = false;
                    // if (clearBetweenDialogues)
                    // {
                    UIScript.textObject[textObjectId].text = "";
                    UIScript.textObjectVR[textObjectId].text = "";
                    // }
                    for (int x = 0; x < isRunning.Length; x++)
                    {
                        isRunning[x] = false;
                    }
                    currentX = 0;
                    updateString = false;
                    timerForIgnore = 0f;
                    if (AfterRun != null && !ranAfterRun)
                    {

                        // AfterRun.run = true;
                        UIScript.AddToQueueScript(AfterRun);
                        ranAfterRun = true;
                    }
                    if (AfterRuns != null)
                    {
                        foreach (TriggerScript x in AfterRuns)
                        {
                            UIScript.AddToQueueScript(x);
                            // x.run = true;
                        }
                        ranAfterRun = true;
                    }
                    if (runSceneAdaptor && sceneAdaptorToRun != null)
                    {
                        sceneAdaptorToRun.startScene();
                    }
                }
                else
                {
                    currentX = currentX + 1;
                    updateString = false;
                }
            }
            if (clearBetweenDialogues)
            {
                if (UIScript.textObject[textObjectId] != null)
                {
                    UIScript.textObject[textObjectId].text = "";
                }
                if (UIScript.textObjectVR[textObjectId] != null)
                {
                    UIScript.textObjectVR[textObjectId].text = "";
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

        timer = timer + Time.deltaTime;
        // }

    }

    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}