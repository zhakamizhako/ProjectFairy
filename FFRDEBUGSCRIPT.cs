
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
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        Debug.Log("Debug Script init");
    }
    void Update(){
        if(TextOutput!=null && TPSScript!=null &&TPSScript.TabShown ){
            // if(localPlayer!=null){
                // pos = localPlayer.GetPosition();
                if(localPlayer!=null){
                    pos = localPlayer.GetPosition();
                }
                string text = "X:" + pos.x + "\nY:" + pos.y + "\nZ:" + pos.z + "\n" +
                "--ProjectFairyTest Debug Menu--\n" + 
                "--V0.1--\n" + 
                "--Press TAB to hide--\n" + 
                "--AIOBJECTS--\n";
                if(AIObjects.Length >0){
                    string txtAIs = "";
                    foreach(AIObject g in AIObjects){
                        txtAIs = txtAIs + "\n" +
                        g.gameObject.name + "\n" +
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
                        g.gameObject.name + "\n" +
                        "turretType:" +(g.turretType) + "\n" ;
                        if(g.Target!=null){ txtTurrets = txtTurrets + "target:" + g.Target;}
                        else { txtTurrets = txtTurrets + "target:null";}
                        if(localPlayer!=null){ txtTurrets= txtTurrets + "Owner:" + ((Networking.GetOwner(g.gameObject)).displayName); }
                        else{ txtTurrets = txtTurrets + "Owner:" + "local"; }
                        // "target:" +(g.Target!=null ? g.Target :null) + "\n" +
                        txtTurrets = txtTurrets + "\ntargetIndex::" + (g.currentTargetIndex) + "\n" +
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
                        txtCAC = txtCAC + "\n" +
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
    }
}
