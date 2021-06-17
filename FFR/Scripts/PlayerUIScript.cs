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
    public Slider VRDistanceSlider;
    public Slider VRDistanceSlider2;
    public Slider VRSizeSlider;
    public Slider VRSizeSlider2;
    public Text vrDistanceText;
    public Text vrDistanceText2;
    public Text vrSizeText;
    public Text vrSizeText2;
    public Toggle PreviewToggle;
    public bool preview;
    public GameObject PreviewHUD;
    public GameObject PreviewHUD2;
    public TriggerScript[] TriggerScripts;
    public VRCPlayerApi localPlayer;
    private float startVRDistance;
    private float startVRDistance2;
    private float startVRSize;
    private float startVRSize2;
    private float startIconSize;
    public Text textObject; //This is where all the texts are drawn. Maybe stick it with a player's Head.
    public Text textObjectVR; //Automatically disabled if on VR
    public Text ChapterTitleText;
    public Text ChapterTitleTextVr;
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
    public Toggle LanguageEn;
    public Toggle LanguageJP;
    // public float triggerScriptTimeout = 15f;
    public EngineController PlayerAircraft;
    public MissileTrackerAndResponse sampleThing;

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
        if (textObject != null)
        {
            startsizeTextDesktop = textObject.transform.localScale;
        }
        if (textObjectVR != null)
        {
            startsizeTextVR = textObjectVR.transform.localScale;
        }
        if (ChapterTitleText != null)
        {
            ChapterTitleText.text = "";
        }
        if (ChapterTitleTextVr = null)
        {
            ChapterTitleTextVr.text = "";
        }
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
        Assert(textObject != null, "Start: TextObject MUST NOT BE null");
        Assert(textObjectVR != null, "Start: textobjectvr MUST NOT BE null");
        Assert(parentHolderTexts != null, "Start: parentholder MUST NOT BE null");
    }

    public void ResetDefaults()
    {
        VRSizeSlider.value = startVRSize;
        VRSizeSlider2.value = startVRSize2;
        VRDistanceSlider.value = startVRDistance;
        VRDistanceSlider2.value = startVRDistance2;
        IconSize = startIconSize;
    }

    void Update()
    {
        if (VRDistanceSlider != null && vrDistanceText != null && vrDistance != VRDistanceSlider.value)
        {
            vrDistance = VRDistanceSlider.value;
            vrDistanceText.text = vrDistance.ToString("F3");
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
        if(LanguageEn!=null && LanguageJP!=null){
            if(LanguageEn.isOn){
                isEnglishOrJapanese = false;
                LanguageJP.isOn = false;
            }
            if(LanguageJP.isOn){
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
                textObjectVR.text = "<<MESSAGE>>\nPreview Message";
                textObject.text = "<<MESSAGE>>\nPreview Message";
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
                    textObjectVR.text = "";
                    textObject.text = "";
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
    }



    public void ReceiveMusic(AudioSource Audio, AudioSource Intro, string Title)
    {
        if (Audio != null)
        {
            IntroMusic = Intro;
            QueueAudio = Audio;
            isSwitchingMusic = true;
            isPlayingIntro = false;
            tempMusicText = Title;
        }

        Debug.Log("eh?");
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
        if (textObject != null) textObject.text = "";
        if (textObjectVR != null) textObjectVR.text = "";
    }

    public void ReceiveTrigger(TriggerScript x)
    {
        if(x.stopped){
            return;
        }

        for(int y = 0; y < TriggerScripts.Length; y++){
            if(TriggerScripts[y]==x){
                return;
            }
        }

        TriggerScript[] temp = new TriggerScript[TriggerScripts.Length + 1];
        TriggerScripts.CopyTo(temp, 0);
        temp[temp.Length - 1] = x;
        TriggerScripts = temp;
    }

    public void RemoveTrigger(TriggerScript x)
    {
        TriggerScript[] temp = new TriggerScript[TriggerScripts.Length - 1];
        int b = 0;
        for (int y = 0; y < TriggerScripts.Length; y++)
        {
            if (TriggerScripts[y] != x)
            {
                temp[b] = TriggerScripts[y];
                b = b + 1;
            }
        }
        TriggerScripts = temp;
    }

    void LateUpdate()
    {
        if (parentHolderTexts != null && textObject != null && textObjectVR != null && textObject != null && TriggerScripts.Length > 0)
        {
            triggerEmpty = false;
            if (localPlayer != null)
            {
                if (localPlayer.IsUserInVR())
                {
                    if (textObject.gameObject.activeSelf)
                        textObject.gameObject.SetActive(false);
                    if (!textObjectVR.gameObject.activeSelf)
                        textObjectVR.gameObject.SetActive(true);
                }
                else
                {
                    if (!textObject.gameObject.activeSelf)
                        textObject.gameObject.SetActive(true);
                    if (textObjectVR.gameObject.activeSelf)
                        textObjectVR.gameObject.SetActive(false);
                }
            }
            parentHolderTexts.position = localPlayer != null ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero;
            parentHolderTexts.rotation = localPlayer != null ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation : Quaternion.Euler(Vector3.zero);
            parentHolderTexts.transform.position = parentHolderTexts.position;
            parentHolderTexts.transform.position += textObjectVR.transform.forward * vrDistance;
            parentHolderTexts.transform.rotation = localPlayer != null ? localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation : Quaternion.Euler(Vector3.zero);
            parentHolderTexts.transform.localScale = startsizeTextVR * vrSize;
            // textObject.transform.position = parentHolderTexts.position;
            // textObject.transform.position += textObject.transform.forward * vrDistance;
            // textObject.transform.localScale = startsizeTextDesktop * vrSize;
        }
        else if (TriggerScripts == null || TriggerScripts.Length < 1)
        { //disable to cleanup gc?
            if (textObject.gameObject.activeSelf) textObject.gameObject.SetActive(false);
            if (textObjectVR.gameObject.activeSelf) textObjectVR.gameObject.SetActive(false);
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