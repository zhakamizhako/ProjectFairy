
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CullGroup : UdonSharpBehaviour

{
    public float RenderDistance = 7000f;
    public GameObject Object;
    private VRCPlayerApi localPlayer;
    public bool reverseCull = false;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    void LateUpdate(){
        if(localPlayer!=null){
            Vector3 pos = localPlayer.GetPosition();
            var distance = Vector3.Distance(pos, gameObject.transform.position);
            if(distance > RenderDistance){
                // if(Object.activeSelf)
               Object.SetActive(!reverseCull ? false : true);
            }else if(distance < RenderDistance){
                // if(!Object.activeSelf)
                Object.SetActive(!reverseCull ? true : false);
            }
        }
    }
}
