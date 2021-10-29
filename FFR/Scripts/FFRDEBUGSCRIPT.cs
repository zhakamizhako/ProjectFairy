
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class FFRDEBUGSCRIPT : UdonSharpBehaviour
{
public Text TextOutput;
[System.NonSerializedAttribute][HideInInspector] public VRCPlayerApi localPlayer;
public Vector3 pos = new Vector3(0,0,0);
public AIObject[] AIObjects;
public CatchArmController[] CAC;
public AITurretScript[] AITurrets;
public ThirdPersonPlayerCamera TPSScript;
public Transform textoutputTransform;
private Vector3 initPos;
public PlayerUIScript UIScript;
public int speed = 5;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        Debug.Log("Debug Script init");
        if(TextOutput!=null){
            // textoutputTransform = TextOutput.rectTransform;
            initPos = textoutputTransform.position;
        }
    }
    void Update(){
        if(TextOutput!=null &&  TPSScript!=null && TPSScript.TabShown){
            // if(localPlayer!=null){
                // pos = localPlayer.GetPosition();
                if(localPlayer!=null){
                    pos = localPlayer.GetPosition();
                }
                string text = "X:" + pos.x + "\nY:" + pos.y + "\nZ:" + pos.z + "\n" +
                "--ProjectFairyTest Debug Menu--\n" + 
                "--V0.1--!!Close when unused. Will drop frames.!!\n" + 
                "--Press TAB to hide--\n";
                // if(UIScript!=null){
                    // string tt = "--UI Script Debug" + "\n" +
                    // "TriggerScript:" + UIScript.TriggerScripts!=null ? UIScript.TriggerScripts[0].currentX + "" : "Not active" + "\n" +
                    // "TriggerScript Length:" + UIScript.TriggerScripts!=null ? UIScript.TriggerScripts.Length + "" : "Not Active" + "\n" +
                    // "TriggerScript CurrentX: " + UIScript.TriggerScripts!=null  ? UIScript.TriggerScripts[0].currentX + "" : "Not Active";
                    // text = text + tt;
                    
                //  }
                if(AIObjects.Length >0){
                    string txtAIs = "";
                    foreach(AIObject g in AIObjects){
                        txtAIs = txtAIs + "\n" +
                        "[AI]\n" +g.gameObject.name + "\n" +
                        "Disabled:" +(g.disabled) + "\n" +
                        "mainturrets:" +(g.enableMainTurrets) + "\n" +
                        
                        "dead:" + (g.dead) + "\n" +
                        "health:" +(g.Health) + "\n" +
                        "TargetString"+ (g.TargetString) + "\n" +
                        "damagable:" +(g.damageable) + "\n";
                        if(localPlayer!=null){ txtAIs= txtAIs + "Owner:" + ((Networking.GetOwner(g.gameObject)).displayName); }
                        else{ txtAIs = txtAIs + "Owner:" + "local"; }
                        string targets ="";
                        for(int x=0;x<g.debugTargets.Length;x++){
                            targets = targets + g.debugTargets[x].name + "-" + g.targetIndices[x]+ "\n";
                        }
                        txtAIs = txtAIs + "\nTargets:\n" +targets;
                    }
                    text = text + txtAIs;
                }
                if(AITurrets.Length >0){
                     string txtTurrets = "";
                    foreach(AITurretScript g in AITurrets){
                        txtTurrets = txtTurrets + "\n" +
                        "[TURRET]\n" +
                        g.gameObject.name +" - ID: "+g.idTurret+ "\n" +
                        "turretType:" +(g.turretType) + "\n" ;
                        if(g.Target!=null){ txtTurrets = txtTurrets + "target:" + g.Target;}
                        else { txtTurrets = txtTurrets + "target:null";}
                        if(localPlayer!=null){ txtTurrets= txtTurrets + "Owner:" + ((Networking.GetOwner(g.gameObject)).displayName); }
                        else{ txtTurrets = txtTurrets + "Owner:" + "local"; }
                        // "target:" +(g.Target!=null ? g.Target :null) + "\n" +
                        txtTurrets = txtTurrets + "\ntargetIndex::" + (g.currentTargetIndex) + "\n" +
                        "AISCRIPT:" +(g.mainAIObject.gameObject.name)+ "\n" +
                        "health:" +(g.Health) + "\n" +
                        "cooldown:" +(g.fireCooldown) + "\n" +
                        "launcharea:" +(g.launchArea) + "\n" +
                        "samlaunchareas:" +(g.MissileSpawnAreas.Length) + "\n" +
                        "isfiringciws:" +(g.isFiringCiws) + "\n" +
                        "isfiringprojectile:" +(g.isFiring) + "\n" +
                        "damagable:" +(g.damageable) + "\n";
                    }
                    text = text + txtTurrets;
                }
                text = text + "--CAC's-- \n";
                if(CAC.Length>0){
                    string txtCAC = "";
                    foreach(CatchArmController c in CAC){
                        txtCAC = txtCAC + "\n" + "[CAC]\n"+
                        c.gameObject.name + "\n" +
                        "isReady:" + c.isReady + "\n" +
                        "isLaunched:" + c.isLaunched + "\n" +
                        "isPulling:" + c.isPulling + "\n" +
                        "IsLaunched:" + c.isLaunched + "\n" +
                        "SelectedIndex:" + c.selectedIndex + "\n" +
                        "onCooldown" + c.onCooldown + "\n";
                        if(c.Current_LSC!=null){ txtCAC = txtCAC + "CAC:" + c.Current_LSC + "\n";}
                        else{ txtCAC = txtCAC + "CAC:" + "null \n";}
                        if(localPlayer!=null){ txtCAC = txtCAC + "Owner:" + (Networking.GetOwner(c.gameObject).displayName) + "\n"; }
                        else{ txtCAC = txtCAC + "Owner:" + "local" + "\n"; }
                        var b = (UdonBehaviour)c.gameObject.GetComponent(typeof(UdonBehaviour));
                        if(b){  txtCAC = txtCAC + "ACTIVE" + "\n"; }
                        else{  txtCAC = txtCAC + "CRASH OR DISABLED" + "\n"; }
                    }
                    text = text + txtCAC;
                }
                TextOutput.text = text;
            // }
        }
        if(!TPSScript.TabShown){
            textoutputTransform.position =initPos;
        }else{
            if(Input.GetKey(KeyCode.KeypadMultiply)){
                textoutputTransform.position = new Vector3(textoutputTransform.position.x, textoutputTransform.position.y + speed, textoutputTransform.position.z);
            }
            if(Input.GetKey(KeyCode.Keypad9)){
                textoutputTransform.position = new Vector3(textoutputTransform.position.x, textoutputTransform.position.y - speed, textoutputTransform.position.z);
            }
            if(Input.GetKey(KeyCode.KeypadDivide)){
                textoutputTransform.position = Vector3.zero;
            }
            if(Input.GetKeyDown(KeyCode.Keypad7)){
                speed = speed - 1;
            }
            if(Input.GetKeyDown(KeyCode.Keypad8)){
                speed = speed + 1;
            }
        }
    }
}
