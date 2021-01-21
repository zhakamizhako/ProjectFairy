
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class NPCInteractionAndBehaviour : UdonSharpBehaviour
{
    public Text TextObject;
    public Text CharacterNameTextObject;
    public GameObject Panel;
    public string CharacterName;
    public string[] TestNPC;
    public float timeBeforeHide = 15;
    public float timerCountdown = 0;
    public bool isShowing = false;
    void Start()
    {
        if(TextObject){
            TextObject.text = "";
            CharacterNameTextObject.text="";
        }
        if(Panel!=null){
            Panel.SetActive(false);
        }
    }
    void Update(){
        //If Left alone for a while, clean up panel
        if(isShowing){
            if(timerCountdown < timeBeforeHide){
                timerCountdown = timerCountdown + Time.deltaTime;
            }
            if(timerCountdown > timeBeforeHide){
                Panel.SetActive(false);
                isShowing = false;
            }
        }
    }
    public void Interact() {
        Panel.SetActive(true);
        CharacterNameTextObject.text = CharacterName;
        if(TestNPC.Length > 0){
            int rInt = Random.Range (0, TestNPC.Length);
            TextObject.text = TestNPC[rInt];
        }
        isShowing = true;
        timerCountdown = 0;
    }

}
