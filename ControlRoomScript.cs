
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ControlRoomScript : UdonSharpBehaviour
{
    public GameObject CameraStuff;
    public GameObject RadarStuff;
    public GameObject ThePictureToShow;
    void Start()
    {
        
    }

    void Update(){
        if(CameraStuff.activeSelf || RadarStuff.activeSelf){
            ThePictureToShow.SetActive(true);
        }else{
            ThePictureToShow.SetActive(false);
        }
    }
}
