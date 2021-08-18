
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class RadarObject : UdonSharpBehaviour
{
    public MissileTrackerAndResponse Tracking;
    public MissileScript TrackingMissile;
    public RadarRender ParentRender;
    private MeshRenderer Rend;
    private Renderer matmat;
    public float distance = 0f;
    public bool isAssigned = false;
    public bool toRemove = false;
    public Text TextObject;
    public Image IconRenderer;

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
            return;
        }
        if (!isAssigned)
        {
            TrackingMissile = ParentRender.ToAssignMissile;
            Tracking = ParentRender.ToAssignTracker;

            if (Tracking != null)
            {
                // if (!ParentRender.renderIcons && IconRenderer != null)
                // {
                //     IconRenderer.gameObject.SetActive(false);
                // }

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



            if (Tracking != null || TrackingMissile != null)
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
            var vv = (ParentRender.ReferenceArea.position) - (ParentRender.ReferenceArea.position);
            Vector3 xx = Vector3.zero;
            if (Utilities.IsValid(Tracking)) { xx = Tracking.transform.position; }
            else if (Utilities.IsValid(TrackingMissile)) { xx = TrackingMissile.transform.position; }
            else
            {
                Remove();
            }
            // var xx = Tracking != null ? Tracking.transform.position : TrackingMissile.transform.position;
            // Vector3 pos = ParentRender.ReferenceArea.TransformDirection(xx);
            Vector3 pos = xx + vv;
            if (!ParentRender.use3d)
            {
                pos = new Vector3(pos.x, 0, pos.z) * ParentRender.scale;
            }
            else
            {
                pos = pos * ParentRender.scale;
            }
            gameObject.transform.localPosition = pos;

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


            if (Tracking != null)
            {
                if (ParentRender.ForceTransform)
                {
                    gameObject.transform.localRotation = Tracking.MainObject.transform.localRotation;
                }
                if (TextObject != null)
                {
                    string builder = Tracking.MainObject.name;
                    // string agl = "\nAGL:";
                    // string health = "HP:";
                    if (Tracking.EngineController != null) builder += "\n" + Tracking.EngineController.PilotName;

                    if (Tracking.EngineController != null) builder += "\nAGL:" + (Tracking.EngineController.VehicleMainObj.transform.position.y + Tracking.EngineController.SeaLevel * 3.28084f);
                    if (Tracking.AI != null) builder += "\nAGL:" + Tracking.AI.AIRigidBody.transform.position.y * 3.28084f;

                    if (Tracking.EngineController != null) builder += "\nHP:" + Tracking.EngineController.Health;
                    if (Tracking.AI != null) builder += "\nHP:" + Tracking.AI.Health;
                    if (Tracking.AITurret != null) builder += "\nHP:" + Tracking.AITurret.Health;

                    if (Tracking.EngineController != null) builder += "\nVel:" + Tracking.EngineController.VehicleRigidbody.velocity.magnitude * 1.9438445f;
                    if (Tracking.AI != null) builder += "\nVel:" + (Tracking.AI.AIRigidBody ? Tracking.AI.AIRigidBody.velocity.magnitude * 1.9438445f : 0f);

                    TextObject.text = builder;
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
            }


        }

    }
}
