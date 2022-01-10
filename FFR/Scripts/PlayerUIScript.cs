using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerUIScript : UdonSharpBehaviour
{
    public Text Logs;
    public Animator UIAnimator;
    public Animator MusicUIAnimator;
    public Animator MusicUIVRAnimator;
    public float TimeToFade = 4f;
    public float timer = 0f;
    public bool timerStarted = false;
    VRCPlayerApi[] allPlayers = new VRCPlayerApi[80];
    public float vrDistance = 0;
    public float vrDistance2 = 0;
    public float vrSize = 0;
    public float vrSize2 = 0;
    public float verticalDistanceHud;
    public float verticalDistanceUi;
    public Slider VRDistanceSlider;
    public Slider VRDistanceSlider2;
    public Slider VRSizeSlider;
    public Slider VRSizeSlider2;
    public Slider VerticalPositionSlider_UI;
    public Slider VerticalPositionSlider_HUD;
    public Text VerticalPositionText_UI;
    public Text VerticalPositionText_HUD;
    public Text vrDistanceText;
    public Text vrDistanceText2;
    public Text vrSizeText;
    public Text vrSizeText2;
    public Toggle PreviewToggle;
    public bool preview;
    public GameObject PreviewHUD;
    public GameObject PreviewHUD2;
    public TriggerScript[][] TriggerScripts;
    public VRCPlayerApi localPlayer;
    private float startVRDistance;
    private float startVRDistance2;
    private float startVRSize;
    private float startVRSize2;
    private float startIconSize;
    private float startVerticalDistanceHUD;
    private float startVerticalDistanceUI;
    public Text[] textObject; //This is where all the texts are drawn. Maybe stick it with a player's Head.
    public Text[] textObjectVR; //Automatically disabled if on VR
    public Renderer IconRenderer;
    public Renderer UIPhotoRenderer;
    public Transform parentHolderTexts;
    private Vector3 startsizeTextVR;
    private Vector3 startsizeTextDesktop;
    private float musicFade = 2;
    public float musiFadeTimer = 0;
    private bool isPlayingMusic = false;
    private bool isSwitchingMusic = false;
    private bool isStoppingMusic = false;
    private bool showTitle = false;
    public AudioSource CurrentPlayingMusic;
    public AudioSource QueueAudio;
    public AudioSource IntroMusic;
    public bool isPlayingIntro = false;
    public Animator MusicAni;
    public Animator MusicAniVR;
    public Slider MusicVolume;
    public float MusicVolumeValue = 1f;
    public Text MusicVolumeText;
    private string tempMusicText;
    public Slider IconSlider;
    public Text IconSliderText;
    public float IconSize = 0f;
    private bool triggerEmpty = false;
    public bool isEnglishOrJapanese = false; //False = english, japanese = 1;
    public bool isEnglishOrJapaneseVoice = false; //0 = en, 1 = jp
    public Toggle LanguageEn;
    public Toggle LanguageJP;
    // public float triggerScriptTimeout = 15f;
    public EngineController PlayerAircraft;
    public OpenWorldMovementLogic OWMLPlayer;
    public MissileTrackerAndResponse sampleThing;
    [Header("Skybox Settings")]
    public Material Skybox;
    public float baseAtmos = 1f;
    public float AtmosphereDarkStart = 30000;
    public float AtmosphereDarkMax = 50000;
    [Header("Cloud Settings")]
    public Material CloudMat;
    public float baseHeight = 10000f;
    private float currentHeight = 0f;
    public bool disabletexts = false;
    public bool inited = false;
    public int framesBeforeCheck = 10;
    private int framePass = 0;

    public Toggle OWMLIntervalCheckbox;
    public Toggle OWMLDistanceCheckbox;
    public Toggle OWMLConstantCheckbox;

    public Slider OWMLIntervalSlider;
    public Slider OWMLDistanceSlider;

    public Text OWMLDistanceText;
    public Text OWMLIntervalText;



    public bool OWMLMoveByChunks = false; //<- False == Move world always, true == move only by chunks
    public float ChunkDistance = 5000f;
    public bool OWMLMoveByTime = false;
    public float MoveByTimeSeconds = 10f;

    public bool isSinglePlayer = false;
    public bool isMultiplayer = false;
    public InstanceownerChecker InstanceOwner;

    public bool AIDamageLocalOnly = true; //change this for multiplayer.

    public TriggerScript[] TriggerList;
    public bool sleepTriggers = false; //Sync can be an issue.

    public Transform MapObject;

    void Start()
    {
        // Debug.Log("UI Script started");
        localPlayer = Networking.LocalPlayer;
        if (VRSizeSlider != null)
        {
            startVRSize = VRSizeSlider.value;
        }
        if (VRSizeSlider2 != null)
        {
            startVRSize2 = VRSizeSlider2.value;
        }
        if (VRDistanceSlider != null)
        {
            startVRDistance = VRDistanceSlider.value;
        }
        if (VRDistanceSlider2 != null)
        {
            startVRDistance2 = VRDistanceSlider2.value;
        }
        if (VerticalPositionSlider_HUD != null)
        {
            startVerticalDistanceHUD = VerticalPositionSlider_HUD.value;
        }
        if (VerticalPositionSlider_UI != null)
        {
            startVerticalDistanceUI = VerticalPositionSlider_UI.value;
        }

        //---------------------
        if (textObject != null)
        {
            startsizeTextDesktop = textObject[0].transform.localScale;
        }
        if (textObjectVR != null)
        {
            startsizeTextVR = textObjectVR[0].transform.localScale;
        }
        //---------------------

        if (IconSliderText != null)
        {
            IconSliderText.text = "";
        }
        if (IconSlider != null)
        {
            startIconSize = IconSize;
        }
        if (MusicVolume != null)
        {
            MusicVolumeValue = MusicVolume.value;
            MusicVolumeText.text = MusicVolumeValue + "";
        }
        if (Skybox != null)
        {
            // baseAtmos = Skybox.GetFloat("_AtmosphereThickness");
            Skybox.SetFloat("_AtmosphereThickness", baseAtmos);
        }
        if (CloudMat != null)
        {
            CloudMat.SetFloat("_FromHeight", baseHeight);
        }
        if (UIPhotoRenderer != null)
        {
            IconRenderer = UIPhotoRenderer.GetComponent<Renderer>();
        }
        if (OWMLDistanceCheckbox != null)
        {
            OWMLDistanceCheckbox.isOn = OWMLMoveByChunks;
        }
        if (OWMLConstantCheckbox != null)
        {
            OWMLConstantCheckbox.isOn = (!OWMLMoveByChunks && !OWMLMoveByTime);
        }
        if (OWMLIntervalCheckbox != null)
        {
            OWMLIntervalCheckbox.isOn = OWMLMoveByTime;
        }
        if (OWMLIntervalSlider != null && OWMLIntervalText != null && MoveByTimeSeconds != OWMLIntervalSlider.value)
        {
            MoveByTimeSeconds = OWMLIntervalSlider.value;
            OWMLIntervalText.text = MoveByTimeSeconds.ToString("F0");
        }
        if (OWMLDistanceSlider != null && OWMLDistanceText != null && ChunkDistance != OWMLDistanceSlider.value)
        {
            ChunkDistance = OWMLDistanceSlider.value;
            OWMLDistanceText.text = ChunkDistance.ToString("F0");
        }

        //Set TextObjects
        Assert(textObject != null, "Start: TextObject MUST NOT BE null");
        Assert(textObjectVR != null, "Start: textobjectvr MUST NOT BE null");
        Assert(parentHolderTexts != null, "Start: parentholder MUST NOT BE null");

        scanTriggers();
    }

    public void scanTriggers(){
        // GameObject[] triggers;
        // triggers = GameObject.FindGameObjectsWithTag("TriggerScript");
        // TriggerList = new TriggerScript[triggers.Length];
        // for(int x =0; x< TriggerList.Length;x++){
        //     TriggerList[x] = triggers[x].GetComponent<TriggerScript>();
        //     TriggerList[x].uid = x;
        // }
        // // foreach(GameObject x in triggers){
        // //     TriggerScript xx = x.GetComponent<TriggerScript>();
        // // }
    }

    public void ResetDefaults()
    {
        VRSizeSlider.value = startVRSize;
        VRSizeSlider2.value = startVRSize2;
        VRDistanceSlider.value = startVRDistance;
        VRDistanceSlider2.value = startVRDistance2;
        verticalDistanceHud = startVerticalDistanceHUD;
        verticalDistanceUi = startVerticalDistanceUI;
        IconSize = startIconSize;
    }

    public void setLanguageJP(bool arg)
    {
        if (arg == null)
        {
            isEnglishOrJapanese = !isEnglishOrJapanese;
        }
        else
        {
            isEnglishOrJapanese = arg;
        }
    }

    public void setAudioEn(bool arg)
    {

        isEnglishOrJapaneseVoice = !isEnglishOrJapaneseVoice;
    }

    public void SetOWMLTimeInterval(float x)
    {

    }
    public void SetOWMLDistanceValue(float x)
    {
        ChunkDistance = x;
    }
    public void SetOWMLInterval()
    {
        Debug.Log("A");
        OWMLMoveByChunks = false;
        OWMLMoveByTime = true;

        if (OWMLConstantCheckbox != null) OWMLConstantCheckbox.isOn = false;
        if (OWMLDistanceCheckbox != null) OWMLDistanceCheckbox.isOn = false;
        if (OWMLIntervalCheckbox != null) OWMLIntervalCheckbox.isOn = true;
    }
    public void SetOWMLDistance()
    {
        Debug.Log("B");
        OWMLMoveByChunks = true;
        OWMLMoveByTime = false;

        if (OWMLConstantCheckbox != null) OWMLConstantCheckbox.isOn = false;
        if (OWMLDistanceCheckbox != null) OWMLDistanceCheckbox.isOn = true;
        if (OWMLIntervalCheckbox != null) OWMLIntervalCheckbox.isOn = false;
    }
    public void SetOWMLConstant()
    {
        Debug.Log("C");
        OWMLMoveByChunks = false;
        OWMLMoveByTime = false;

        if (OWMLConstantCheckbox != null) OWMLConstantCheckbox.isOn = true;
        if (OWMLDistanceCheckbox != null) OWMLDistanceCheckbox.isOn = false;
        if (OWMLIntervalCheckbox != null) OWMLIntervalCheckbox.isOn = false;
    }

    public void OnPlayerRespawn(VRCPlayerApi player)
    {
        if(player == localPlayer)
        {
            MapObject.position = Vector3.zero;
            localPlayer.SetVelocity(Vector3.zero);
        }
    }

    void Update()
    {
        if (!inited)
        {
            if (framePass < framesBeforeCheck)
            {
                framePass = framePass + 1;
            }
            else
            {
                if (localPlayer != null)
                {
                    if (localPlayer.IsUserInVR())
                    {
                        foreach (Text x in textObject)
                        {
                            x.gameObject.SetActive(false);
                        }
                        foreach (Text x in textObjectVR)
                        {
                            x.gameObject.SetActive(true);
                        }
                        // Debug.Log("USER IN VR");
                    }
                    else
                    {
                        foreach (Text x in textObject)
                        {
                            x.gameObject.SetActive(true);
                        }
                        foreach (Text x in textObjectVR)
                        {
                            x.gameObject.SetActive(false);
                        }
                        // Debug.Log("USER NOT IN VR");
                    }
                }
                inited = true;
            }
            Debug.Log(framePass);
        }

        // if (!inited)
        // {

        // inited = true;
        // }
        if (VRDistanceSlider != null && vrDistanceText != null && vrDistance != VRDistanceSlider.value)
        {
            vrDistance = VRDistanceSlider.value;
            vrDistanceText.text = vrDistance.ToString("F3");
        }

        if (VerticalPositionSlider_HUD != null && VerticalPositionText_HUD != null && verticalDistanceHud != VerticalPositionSlider_HUD.value)
        {
            verticalDistanceHud = VerticalPositionSlider_HUD.value;
            VerticalPositionText_HUD.text = verticalDistanceHud.ToString("F3");
        }
        if (VerticalPositionSlider_UI != null && VerticalPositionText_UI != null && verticalDistanceUi != VerticalPositionSlider_UI.value)
        {
            verticalDistanceUi = VerticalPositionSlider_UI.value;
            VerticalPositionText_UI.text = verticalDistanceUi.ToString("F3");
        }

        if (VRDistanceSlider2 != null && vrDistanceText2 != null && vrDistance2 != VRDistanceSlider2.value)
        {
            vrDistance2 = VRDistanceSlider2.value;
            vrDistanceText2.text = vrDistance2.ToString("F3");
        }
        if (VRSizeSlider != null && vrSizeText != null && vrSize != VRSizeSlider.value)
        {
            vrSize = VRSizeSlider.value;
            vrSizeText.text = vrSize.ToString("F3");
        }
        if (VRSizeSlider2 != null && vrSizeText2 != null && vrSize2 != VRSizeSlider2.value)
        {
            vrSize2 = VRSizeSlider2.value;
            vrSizeText2.text = vrSize2.ToString("F3");
        }
        if (IconSlider != null && IconSliderText != null && IconSize != IconSlider.value)
        {
            IconSize = IconSlider.value;
            IconSliderText.text = IconSize.ToString("F3");
        }
        if (OWMLIntervalSlider != null && OWMLIntervalText != null && MoveByTimeSeconds != OWMLIntervalSlider.value)
        {
            MoveByTimeSeconds = OWMLIntervalSlider.value;
            OWMLIntervalText.text = MoveByTimeSeconds.ToString("F0");
        }
        if (OWMLDistanceSlider != null && OWMLDistanceText != null && ChunkDistance != OWMLDistanceSlider.value)
        {
            ChunkDistance = OWMLDistanceSlider.value;
            OWMLDistanceText.text = ChunkDistance.ToString("F0");
        }

        if (MusicVolume != null && MusicVolumeText != null && MusicVolumeValue != MusicVolume.value)
        {
            MusicVolumeValue = MusicVolume.value;
            MusicVolumeText.text = MusicVolumeValue.ToString("F3");
            if (CurrentPlayingMusic != null)
            {
                if (IntroMusic != null) IntroMusic.volume = MusicVolumeValue;
                CurrentPlayingMusic.volume = MusicVolumeValue;
            }
        }
        if (LanguageEn != null && LanguageJP != null)
        {
            if (LanguageEn.isOn)
            {
                isEnglishOrJapanese = false;
                LanguageJP.isOn = false;
            }
            if (LanguageJP.isOn)
            {
                isEnglishOrJapanese = true;
                LanguageEn.isOn = false;
            }
            // isEnglishOrJapanese
        }
        if (PreviewToggle != null && PreviewHUD != null)
        {
            preview = PreviewToggle.isOn;
            if (sampleThing != null)
            {
                sampleThing.ShowTargets = preview;
            }

            if (preview)
            {
                textObjectVR[0].text = "<<MESSAGE>>\nPreview Message";
                textObject[0].text = "<<MESSAGE>>\nPreview Message";
                PreviewHUD.SetActive(true);
                if (PreviewHUD2 != null)
                {
                    PreviewHUD2.SetActive(true);
                }
            }
            else
            {
                if (TriggerScripts != null && TriggerScripts.Length > 0 && !triggerEmpty)
                {
                    //do nothing
                }
                else if (!triggerEmpty)
                {
                    //if none, cleanup.
                    textObjectVR[0].text = "";
                    textObject[0].text = "";
                    triggerEmpty = true;
                }
                if (PreviewHUD.activeSelf)
                {
                    PreviewHUD.SetActive(false);
                }
                if (PreviewHUD2 != null && PreviewHUD2.activeSelf)
                {
                    PreviewHUD2.SetActive(false);
                }
            }
        }

        if (Skybox != null && PlayerAircraft != null)
        {
            if (PlayerAircraft.OWML != null && PlayerAircraft.OWML.ScriptEnabled)
            {
                float offsetHeight = (PlayerAircraft.OWML.AnchorCoordsPosition.y - PlayerAircraft.OWML.Map.position.y) + PlayerAircraft.SeaLevel * 3.28084f;
                Vector3 Offsets = new Vector3(
                    PlayerAircraft.OWML.AnchorCoordsPosition.x - PlayerAircraft.OWML.Map.position.x,
                    PlayerAircraft.OWML.AnchorCoordsPosition.y - PlayerAircraft.OWML.Map.position.y,
                    PlayerAircraft.OWML.AnchorCoordsPosition.z - PlayerAircraft.OWML.Map.position.z
                    );
                if (offsetHeight > AtmosphereDarkStart)
                {
                    Skybox.SetFloat("_AtmosphereThickness", baseAtmos - ((offsetHeight - AtmosphereDarkStart) / AtmosphereDarkMax));
                }
                else
                {
                    Skybox.SetFloat("_AtmosphereThickness", 1);
                }

                if (CloudMat != null)
                {
                    CloudMat.SetVector("_OffsetXYZ", new Vector4(Offsets.x, baseHeight - Offsets.y, Offsets.z, 0));
                }
            }
            if (PlayerAircraft.OWML == null)
            {
                Skybox.SetFloat("_AtmosphereThickness", 1);
                if (CloudMat != null)
                {
                    CloudMat.SetFloat("_FromHeight", baseHeight);
                }
            }
        }
    }

    public void AddToQueueScript(TriggerScript scriptx)
    {
        scriptx.gameObject.SetActive(true);
        scriptx.run = true;
    }

    public void RestartScript(TriggerScript scriptx)
    {
        scriptx.stopped = false;
        scriptx.ranAfterRun = false;
        scriptx.enabledGameObject = false;
        scriptx.currentX = 0;
        scriptx.runSceneAdaptor = false;
    }
    public void ReceiveMusic(AudioSource Audio, AudioSource Intro, string Title)
    {
        if (Audio != null)
        {
            if (IntroMusic != null)
            {
                IntroMusic.Stop();
            }
            IntroMusic = Intro;
            QueueAudio = Audio;
            isSwitchingMusic = true;
            isPlayingIntro = false;
            tempMusicText = Title;
        }
    }

    public void StopMusic()
    {
        if (CurrentPlayingMusic != null)
        {
            CurrentPlayingMusic.Stop();
        }
    }

    public void ForceCleanupUI()
    {
        TriggerScript[] temp = new TriggerScript[0];
        if (textObject != null) foreach (Text x in textObject) { x.text = ""; }
        if (textObjectVR != null) foreach (Text x in textObjectVR) { x.text = ""; }
    }

    public void ReceiveTrigger(TriggerScript x, int id)
    {
        Debug.LogError("ReceiveTriggerCalled");
        if (x.stopped)
        {
            return;
        }

        for (int y = 0; y < TriggerScripts[id].Length; y++)
        {
            if (TriggerScripts[id][y] == x)
            {
                return;
            }
        }

        TriggerScript[] temp = new TriggerScript[TriggerScripts[id].Length + 1];
        TriggerScripts[id].CopyTo(temp, 0);
        temp[temp.Length - 1] = x;
        TriggerScripts[id] = temp;
    }

    public void RemoveTrigger(TriggerScript x, int id)
    {
        Debug.LogError("RemoveTriggerCalled");
        TriggerScript[] temp = new TriggerScript[TriggerScripts[id].Length - 1];
        int b = 0;
        for (int y = 0; y < TriggerScripts[id].Length; y++)
        {
            if (TriggerScripts[id][y] != x)
            {
                temp[b] = TriggerScripts[id][y];
                b = b + 1;
            }
        }
        TriggerScripts[id] = temp;
    }

    void checkEmpty()
    {

    }

    void LateUpdate()
    {
        if (parentHolderTexts != null && textObject != null && textObjectVR != null && textObject != null && TriggerScripts.Length > 0)
        {
            triggerEmpty = false;
            parentHolderTexts.position = localPlayer != null ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero;
            parentHolderTexts.rotation = localPlayer != null ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation : Quaternion.Euler(Vector3.zero);
            Vector3 pos = Vector3.zero;
            pos = parentHolderTexts.position;
            pos += textObjectVR[0].transform.forward * vrDistance;
            pos += textObjectVR[0].transform.up * verticalDistanceUi;
            parentHolderTexts.transform.position = pos;
            parentHolderTexts.transform.rotation = localPlayer != null ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation : Quaternion.Euler(Vector3.zero);
            parentHolderTexts.transform.localScale = startsizeTextVR * vrSize;
            // foreach (Text x in textObject)
            // {
            //     x.transform.position = parentHolderTexts.position;
            //     x.transform.position += x.transform.forward * vrDistance;
            //     x.transform.localScale = startsizeTextDesktop * vrSize;
            // }

        }
        else if (TriggerScripts == null || TriggerScripts.Length < 1)
        { //disable to cleanup gc?
            // if (textObject.gameObject.activeSelf) textObject[0].gameObject.SetActive(false);
            // if (textObjectVR.gameObject.activeSelf) textObjectVR[0].gameObject.SetActive(false);

            triggerEmpty = true;
        }
        if (timerStarted && timer < TimeToFade)
        {
            UIAnimator.SetFloat("fadeTime", (timer - TimeToFade) * TimeToFade / 100f);
            timer = timer + Time.deltaTime;
        }
        else if (timerStarted && timer > TimeToFade)
        {
            timerStarted = false;
            timer = 0;
        }

        if (CurrentPlayingMusic == null && QueueAudio && isSwitchingMusic)
        {
            CurrentPlayingMusic = QueueAudio;
        }

        if (CurrentPlayingMusic != null && MusicVolume != null && MusicVolumeText != null)
        {
            if (!isSwitchingMusic)
            {
                if (IntroMusic != null && isPlayingIntro)
                {
                    if (!IntroMusic.isPlaying)
                    {
                        isPlayingIntro = false;
                        CurrentPlayingMusic.Play();
                    }
                }
            }
            if (isSwitchingMusic)
            {
                if (musiFadeTimer < musicFade)
                {
                    musiFadeTimer = musiFadeTimer + Time.deltaTime;
                    CurrentPlayingMusic.volume = CurrentPlayingMusic.volume - musiFadeTimer;
                }
                else
                {
                    musiFadeTimer = 0;
                    if (IntroMusic != null) IntroMusic.Stop();
                    CurrentPlayingMusic.Stop();
                    CurrentPlayingMusic = QueueAudio;
                    isSwitchingMusic = false;
                    showTitle = true;
                    if (IntroMusic != null)
                    {
                        IntroMusic.Play();
                        isPlayingIntro = true;
                    }
                    else
                    {
                        CurrentPlayingMusic.Play();
                    }
                    if (IntroMusic != null)
                    {
                        IntroMusic.volume = MusicVolume.value;
                    }
                    CurrentPlayingMusic.volume = MusicVolume.value;
                    QueueAudio = null;
                }
            }
            if (isStoppingMusic)
            {
                if (musiFadeTimer < musicFade)
                {
                    musiFadeTimer = musiFadeTimer + Time.deltaTime;
                    CurrentPlayingMusic.volume = CurrentPlayingMusic.volume - musiFadeTimer;
                }
                else
                {
                    CurrentPlayingMusic.Stop();
                    isStoppingMusic = false;
                }
            }
            if (showTitle)
            {

            }
            // CurrentPlayingMusic.volume = MusicVolume.value;
            // MusicVolumeText.text = MusicVolume.value.ToString();
        }
    }
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i] == null)
            {
                allPlayers[i] = player;
                break;
            }
        }
        Logs.text = Logs.text + " \n" + player.displayName + " has Joined.";
        timerStarted = true;
        timer = 0;
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if (allPlayers[i] == player)
            {
                allPlayers[i] = null;
                break;
            }
        }
        Logs.text = Logs.text + " \n" + player.displayName + " has Left.";
        timerStarted = true;
        timer = 0;
    }

    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}