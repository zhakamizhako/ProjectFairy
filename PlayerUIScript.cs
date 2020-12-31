using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerUIScript : UdonSharpBehaviour {
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
    public Text textObject; //This is where all the texts are drawn. Maybe stick it with a player's Head.
    public Text textObjectVR; //Automatically disabled if on VR
    public Text ChapterTitleText;
    public Text ChapterTitleTextVr;
    public Transform parentHolderTexts;
    private Vector3 startsizeTextVR;
    private Vector3 startsizeTextDesktop;
    private float musicFade=2;
    public float musiFadeTimer =0;
    private bool isPlayingMusic = false;
    private bool isSwitchingMusic = false;
    private bool isStoppingMusic = false;
    private bool showTitle = false;

    public AudioSource CurrentPlayingMusic;
    public AudioSource QueueAudio;
    public Animator MusicAni;
    public Animator MusicAniVR;
    public Slider MusicVolume;
    public Text MusicVolumeText;

    public Slider IconSlider;
    public Text IconSliderText;
    public float IconSize = 0f;
    public MissileTrackerAndResponse sampleThing;

    void Start () {
        Debug.Log ("UI Script started");
        localPlayer = Networking.LocalPlayer;
        if (VRSizeSlider != null) {
            startVRSize = VRSizeSlider.value;
        }
        if (VRSizeSlider2 != null) {
            startVRSize2 = VRSizeSlider2.value;
        }
        if (VRDistanceSlider != null) {
            startVRDistance = VRDistanceSlider.value;
        }
        if (VRDistanceSlider2 != null) {
            startVRDistance2 = VRDistanceSlider2.value;
        }
        if (textObject != null) {
            startsizeTextDesktop = textObject.transform.localScale;
        }
        if (textObjectVR != null) {
            startsizeTextVR = textObjectVR.transform.localScale;
        }
        if (ChapterTitleText!=null){
            ChapterTitleText.text = "";
        }
        if (ChapterTitleTextVr=null){
            ChapterTitleTextVr.text = "";
        }
        if(IconSliderText!=null){
            IconSliderText.text = "";
        }
        Assert (textObject != null, "Start: TextObject MUST NOT BE null");
        Assert (textObjectVR != null, "Start: textobjectvr MUST NOT BE null");
        Assert (parentHolderTexts != null, "Start: parentholder MUST NOT BE null");
    }

    public void ResetDefaults () {
        VRSizeSlider.value = startVRSize;
        VRSizeSlider2.value = startVRSize2;
        VRDistanceSlider.value = startVRDistance;
        VRDistanceSlider2.value = startVRDistance2;
    }

    void Update () {
        if (VRDistanceSlider != null && vrDistanceText != null) {
            vrDistance = VRDistanceSlider.value;
            vrDistanceText.text = vrDistance.ToString ("F3");
        }
        if (VRDistanceSlider2 != null && vrDistanceText2 != null) {
            vrDistance2 = VRDistanceSlider2.value;
            vrDistanceText2.text = vrDistance2.ToString ("F3");
        }
        if (VRSizeSlider != null && vrSizeText != null) {
            vrSize = VRSizeSlider.value;
            vrSizeText.text = vrSize.ToString ("F3");
        }
        if (VRSizeSlider2 != null && vrSizeText2 != null) {
            vrSize2 = VRSizeSlider2.value;
            vrSizeText2.text = vrSize2.ToString ("F3");
        }
        if(IconSlider!=null && IconSliderText!=null){
            IconSize = IconSlider.value;
            IconSliderText.text = IconSize.ToString("F3");
        }
        if (PreviewToggle != null && PreviewHUD != null) {
            preview = PreviewToggle.isOn;
            PreviewHUD.SetActive (preview);
            if (PreviewHUD2 != null) {
                PreviewHUD2.SetActive (preview);
            }
            if(sampleThing!=null){
                sampleThing.ShowTargets = preview;
            }
            if (preview) {
                textObjectVR.text = "<<MESSAGE>>\nPreview Message";
                textObject.text = "<<MESSAGE>>\nPreview Message";
            } else {
                if (TriggerScripts != null && TriggerScripts.Length > 0) {
                    //do nothing
                } else {
                    //if none, cleanup.
                    textObjectVR.text = "";
                    textObject.text = "";
                }
            }
        }
        if (localPlayer != null) {
            if (localPlayer.IsUserInVR ()) {
                textObject.gameObject.SetActive (false);
                textObjectVR.gameObject.SetActive (true);
            } else {
                textObject.gameObject.SetActive (true);
                textObjectVR.gameObject.SetActive (false);
            }
        }
        if (parentHolderTexts != null && textObject != null && textObjectVR != null && textObject != null) {
            parentHolderTexts.position = localPlayer!=null ? localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero;
            parentHolderTexts.rotation = localPlayer!=null ? localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.Head).rotation : Quaternion.Euler(Vector3.zero);
            parentHolderTexts.transform.position = parentHolderTexts.position;
            parentHolderTexts.transform.position += textObjectVR.transform.forward * vrDistance;
            parentHolderTexts.transform.rotation = localPlayer!=null ? localPlayer.GetTrackingData (VRCPlayerApi.TrackingDataType.Head).rotation :Quaternion.Euler(Vector3.zero);
            parentHolderTexts.transform.localScale = startsizeTextVR * vrSize;
            // textObject.transform.position = parentHolderTexts.position;
            // textObject.transform.position += textObject.transform.forward * vrDistance;
            // textObject.transform.localScale = startsizeTextDesktop * vrSize;
        }
        if(CurrentPlayingMusic!=null && MusicVolume!=null && MusicVolumeText!=null){
            if(isSwitchingMusic){
                if(musiFadeTimer < musicFade){
                    musiFadeTimer = musiFadeTimer + Time.deltaTime;
                    CurrentPlayingMusic.volume = CurrentPlayingMusic.volume - musiFadeTimer;
                }else{
                    musiFadeTimer = 0;
                    CurrentPlayingMusic.Stop();
                    CurrentPlayingMusic = QueueAudio;
                    isSwitchingMusic = false;
                    showTitle = true;
                    CurrentPlayingMusic.Play();
                    CurrentPlayingMusic.volume = MusicVolume.value;
                }
            }
            if(isStoppingMusic){

            }
            if(showTitle){

            }
            // CurrentPlayingMusic.volume = MusicVolume.value;
            // MusicVolumeText.text = MusicVolume.value.ToString();
        }
        

    }

    public void ReceiveMusic(AudioSource Audio, string Title){
        if(Audio !=null){
            QueueAudio = Audio;
            isSwitchingMusic = true;
        }
    }

    public void StopMusic(){

    }

    public void ReceiveTrigger (TriggerScript x) {
        TriggerScript[] temp = new TriggerScript[TriggerScripts.Length + 1];
        TriggerScripts.CopyTo (temp, 0);
        temp[temp.Length - 1] = x;
        TriggerScripts = temp;
    }

    public void RemoveTrigger (TriggerScript x) {
        TriggerScript[] temp = new TriggerScript[TriggerScripts.Length - 1];
        int b = 0;
        for (int y = 0; y < TriggerScripts.Length; y++) {
            if (TriggerScripts[y] != x) {
                temp[b] = TriggerScripts[y];
                b = b + 1;
            }
        }
        TriggerScripts = temp;
    }

    void LateUpdate () {
        if (timerStarted && timer < TimeToFade) {
            UIAnimator.SetFloat ("fadeTime", (timer - TimeToFade) * TimeToFade / 100f);
            timer = timer + Time.deltaTime;
        } else {
            timerStarted = false;
            timer = 0;
        }
    }
    public override void OnPlayerJoined (VRCPlayerApi player) {
        for (int i = 0; i < allPlayers.Length; i++) {
            if (allPlayers[i] == null) {
                allPlayers[i] = player;
                break;
            }
        }
        Logs.text = Logs.text + " \n" + player.displayName + " has Joined.";
        timerStarted = true;
        timer = 0;
    }
    public override void OnPlayerLeft (VRCPlayerApi player) {
        for (int i = 0; i < allPlayers.Length; i++) {
            if (allPlayers[i] == player) {
                allPlayers[i] = null;
                break;
            }
        }
        Logs.text = Logs.text + " \n" + player.displayName + " has Left.";
        timerStarted = true;
        timer = 0;
    }

    private void Assert (bool condition, string message) {
        if (!condition) {
            Debug.LogError ("Assertion failed : '" + GetType () + " : " + message + "'", this);
        }
    }
}