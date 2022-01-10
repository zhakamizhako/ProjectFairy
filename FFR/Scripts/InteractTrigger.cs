
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractTrigger : UdonSharpBehaviour
{
    public UdonBehaviour Class;
    public string Event;

    void Interact(){
        if(Class !=null && Event!=null){
            Class.SendCustomEvent(Event);
        }
    }
}
