
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RadarObject : UdonSharpBehaviour
{
    public MissileTrackerAndResponse Tracking;
    public MissileScript TrackingMissile;
    public RadarRender ParentRender;
    private Renderer matmat;
    public float distance = 0f;
    public bool isAssigned = false;
    public bool toRemove = false;
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
        else if(!toRemove)
        {
            if (!toRemove)
            {
                var vv = (ParentRender.ReferenceArea.position);
                Vector3 xx = Vector3.zero;
                if (Utilities.IsValid( Tracking)) { xx = Tracking.transform.position; }
                else if (Utilities.IsValid(TrackingMissile)) { xx = TrackingMissile.transform.position; }
                else
                {
                    Remove();
                }
                // var xx = Tracking != null ? Tracking.transform.position : TrackingMissile.transform.position;
                Vector3 pos = vv + xx;
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
                    toRemove=true;
                    Remove();
                }
                if(Tracking!=null && Tracking.AI!=null && Tracking.AI.dead){
                    Remove();
                }
                if(Tracking!=null &&!Tracking.isRendered){
                    Remove();
                }


                if (Tracking != null)
                {

                    if (Tracking.isEnemy)
                    {
                        matmat.material.SetColor("_Color", Color.magenta);
                    }
                    if (Tracking.isObjective)
                    {
                        matmat.material.SetColor("_Color", Color.green);
                    }
                    if (Tracking.isAlly)
                    {
                        matmat.material.SetColor("_Color", Color.cyan);
                    }
                }

                if (TrackingMissile != null)
                {
                    matmat.material.SetColor("_Color", Color.red);
                }
            }

        }

    }
}
