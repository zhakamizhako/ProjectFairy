using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class IndicatorScript : UdonSharpBehaviour {
    public MissileScript misScript;
    public Text indicator;
    public MissileTrackerAndResponse ParentClass;
    public float correctionWorld = 10000f;
    public GameObject CorrectionPlacement;
    public GameObject DebugArrow;
    public bool destroy = false;
    void Start () {
        misScript = ParentClass.temporary;
        Debug.Log ("READY");
        // gameObject.transform.position = CorrectionPlacement.transform.position;
        // gameObject.transform.localScale = CorrectionPlacement.transform.localScale;
    }
    public void toDestroy()
     {
         destroy = true;
         // Will be called just prior to destruction of the gameobject to which this script is attached
         
     }

    void Update () {
        if(destroy){
            DestroyImmediate(gameObject);
        }
        if (!destroy && ParentClass != null && misScript != null && indicator != null) {
            if (ParentClass.EngineController != null && ParentClass.EngineController.Piloting || ParentClass.EngineController.Passenger) {
                gameObject.transform.LookAt (misScript.gameObject.transform.position, ParentClass.gameObject.transform.up);
                gameObject.transform.localRotation = Quaternion.Euler (0,0, -gameObject.transform.localRotation.eulerAngles.y);
                if (DebugArrow != null) {
                    DebugArrow.transform.LookAt (misScript.gameObject.transform.position, ParentClass.gameObject.transform.up);
                }
                float dist = Vector3.Distance (misScript.gameObject.transform.position, ParentClass.gameObject.transform.position);
                indicator.text = dist.ToString ("F0");
            }
        }
    }
}