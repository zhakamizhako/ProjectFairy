
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class RadarObject : UdonSharpBehaviour
{
    public MissileTrackerAndResponse Tracking;
    public MissileScript TrackingMissile;
    public TarpsArea TrackingTarps;
    public RadarRender ParentRender;
    private MeshRenderer Rend;
    private Renderer matmat;
    public float distance = 0f;
    public bool isAssigned = false;
    public bool toRemove = false;
    public Text TextObject;
    public Image IconRenderer;
    private Transform transformReference;
    public float r = 5;
    public float g = 3;
    public float b = 0;
    private float WaitForCallBackTimeout = 4f;
    private float WaitForCallBackTimer_current = 0f;

    public void Remove()
    {
        toRemove = true;
        Debug.Log("To remove called");
        ParentRender.Remove(this);
    }

    public void toDestroy()
    {

        ParentRender.CallBackReceived();
        Destroy(gameObject);
    }

    void Start()
    {
        matmat = gameObject.GetComponent<Renderer>();
        Rend = gameObject.GetComponent<MeshRenderer>();
        // if(ParentRender==null){
        //     Remove();
        // }

        // if(TrackingMissile==null || Tracking==null){
        //     Remove();
        // }
    }

    void Update()
    {
        if (toRemove)
        {
            // if(ParentRender.waitForCallback){
                if(WaitForCallBackTimer_current > WaitForCallBackTimeout){
                    Destroy(gameObject);
                }else{
                    WaitForCallBackTimer_current = WaitForCallBackTimer_current + Time.deltaTime;
                }
            // }
            return;
        }
        if (!isAssigned)
        {
            TrackingMissile = ParentRender.ToAssignMissile;
            Tracking = ParentRender.ToAssignTracker;
            TrackingTarps = ParentRender.ToAssignTarpsArea;

            if (Tracking != null)
            {
                // if (!ParentRender.renderIcons && IconRenderer != null)
                // {
                //     IconRenderer.gameObject.SetActive(false);
                // }

                if (Tracking.AI != null)
                {
                    transformReference = Tracking.AI.AIRigidBody.transform;
                }
                else if (Tracking.EngineController != null)
                {
                    transformReference = Tracking.EngineController.VehicleMainObj.transform;
                }
                else { transformReference = Tracking.MainObject.transform; }

                if (ParentRender.renderIcons && Tracking.RadarIcon != null && Tracking.RadarIcon.material != null && IconRenderer != null)
                {
                    IconRenderer.gameObject.SetActive(true);
                    IconRenderer.material = Tracking.RadarIcon.material;
                    Rend.enabled = false;

                    if (Tracking.isEnemy)
                    {
                        IconRenderer.color = Color.magenta;
                    }
                    else if (Tracking.isObjective)
                    {
                        IconRenderer.color = Color.green;
                    }
                    else if (Tracking.isAlly)
                    {
                        IconRenderer.color = Color.cyan;
                    }
                    else
                    {
                        IconRenderer.color = Color.white;
                    }
                }
            }



            if (Tracking != null || TrackingMissile != null || TrackingTarps != null)
            {
                isAssigned = true;
                ParentRender.Assigned(this);
            }
            // else
            // {
            //     Remove();
            // }

        }
        else if (!toRemove)
        {
            //Object Movement Logic. positioning Data.
            var vv = ParentRender.ReferenceArea.position;
            Vector3 xx = Vector3.zero;
            if (Utilities.IsValid(Tracking)) { xx = Tracking.transform.position; }
            else if (Utilities.IsValid(TrackingMissile)) { xx = TrackingMissile.transform.position; }
            else if (Utilities.IsValid(TrackingTarps)) { xx = TrackingTarps.transform.position; }
            else
            {
                Remove();
            }
            // var xx = Tracking != null ? Tracking.transform.position : TrackingMissile.transform.position;
            // Vector3 pos = ParentRender.ReferenceArea.TransformDirection(xx);
            Vector3 pos = xx - vv;
            //Test

            if (!ParentRender.use3d)
            {
                pos = new Vector3(pos.x, 0, pos.z) * ParentRender.scale;



                // float angleToTarget = Mathf.Atan2(xx.x, xx.z) * Mathf.Rad2Deg;
                // float anglePlayer = ParentRender.ReferenceArea.eulerAngles.y;
                // float angleRadarDegrees = angleToTarget - anglePlayer - 90;
                // float normalisedDistanceToTarget = pos.magnitude;
                // float angleRadians = angleRadarDegrees * Mathf.Deg2Rad;
                // float blipX = normalisedDistanceToTarget * Mathf.Cos(angleRadians);
                // float blipY = normalisedDistanceToTarget * Mathf.Sin(angleRadians);

                // Vector2 xxPos = new Vector2(xx.x, xx.y);
                // Vector2 vvPos = new Vector2(vv.x, vv.y);

                // Vector2 direction = xxPos - vvPos.normalized;

                // float angleV2 = Vector2.Angle(direction, vvPos);

                // //  float angle = Vector3.Angle(ParentRender.ReferenceArea.position, ParentRender.ReferenceArea.position - xx);
                //  float distance = Vector2.Distance(xxPos, vvPos);
                // //  Vector3 x = angle * ParentRender.ReferenceArea.forward * distance;
                // Vector2 blipObject = new Vector2(Mathf.Cos(angleV2), Mathf.Sin(angleV2));

                //  pos = new Vector3(blipObject.x + distance, 0, blipObject.y + distance) * ParentRender.scale;
                 
                // pos = new Vector3(blipX, 0, blipY) * ParentRender.scale;
            }
            else
            {
                pos = pos * ParentRender.scale;
            }
            gameObject.transform.localPosition = pos;

            //End Object Movement logic

            distance = Vector3.Distance(vv, xx);
            if (distance > ParentRender.range)
            {
                Remove();
            }
            if (TrackingMissile != null && TrackingMissile.isExploded)
            {
                toRemove = true;
                Remove();
            }
            if (Tracking != null && Tracking.AI != null && Tracking.AI.dead)
            {
                Remove();
            }
            if (Tracking != null && !Tracking.isRendered)
            {
                Remove();
            }

            if (TrackingTarps != null && !TrackingTarps.belongsTo.isEnabed)
            {
                Remove();
            }
            if (TrackingTarps != null && ParentRender.wp != null && ParentRender.wp.tarps != null && !ParentRender.wp.tarps.isSelected && isAssigned)
            {
                Remove();
            }


            if (Tracking != null)
            {
                if (ParentRender.ForceTransform)
                {
                    gameObject.transform.localRotation = transformReference.localRotation;
                }
                if (TextObject != null)
                {
                    string builder = Tracking.MainObject.name;
                    // string agl = "\nAGL:";
                    // string health = "HP:";
                    if (Tracking.EngineController != null && Tracking.EngineController.Occupied) builder += "\n" + Tracking.EngineController.PilotName;

                    if (Tracking.EngineController != null) builder += "\nAGL:" + (Tracking.EngineController.VehicleMainObj.transform.position.y + Tracking.EngineController.SeaLevel * 3.28084f);
                    if (Tracking.AI != null) builder += "\nAGL:" + Tracking.AI.AIRigidBody.transform.position.y * 3.28084f;

                    if (Tracking.EngineController != null) builder += "\nHP:" + Tracking.EngineController.Health;
                    if (Tracking.AI != null) builder += "\nHP:" + Tracking.AI.Health;
                    if (Tracking.AITurret != null) builder += "\nHP:" + Tracking.AITurret.Health;

                    if (Tracking.EngineController != null) builder += "\nVel:" + Tracking.EngineController.VehicleRigidbody.velocity.magnitude * 1.9438445f;
                    if (Tracking.AI != null) builder += "\nVel:" + (Tracking.AI.AIRigidBody ? Tracking.AI.AIRigidBody.velocity.magnitude * 1.9438445f : 0f);

                    TextObject.text = builder;

                    if (ParentRender.UseOnBoardText && Tracking.isSelected)
                    {
                        if (ParentRender.useOnlyOne && ParentRender.OnBoardText != null)
                        {
                            ParentRender.OnBoardText.text = builder;
                        }
                    }
                }

                if (Tracking.isSelected)
                {
                    matmat.material.SetColor("_Color", Color.yellow);
                }
                else
                {
                    if (Tracking.isEnemy)
                    {
                        matmat.material.SetColor("_Color", Color.magenta);
                    }
                    else if (Tracking.isObjective)
                    {
                        matmat.material.SetColor("_Color", Color.green);
                    }
                    else if (Tracking.isAlly)
                    {
                        matmat.material.SetColor("_Color", Color.cyan);
                    }
                    else
                    {
                        matmat.material.SetColor("_Color", Color.white);
                    }
                }

            }

            if (TrackingMissile != null)
            {
                matmat.material.SetColor("_Color", Color.red);
                if (TextObject != null)
                {
                    TextObject.gameObject.SetActive(false);
                }

            }

            if (TrackingTarps != null)
            {
                // matmat.material.SetColor("_Color", new Color(255, 102, 0));
                matmat.material.SetColor("_Color", new Color(r, g, b));
                if (TextObject != null)
                {
                    TextObject.text = "Scan Area";
                }
            }


        }

    }
}
