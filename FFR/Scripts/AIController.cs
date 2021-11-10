
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AIController : UdonSharpBehaviour
{
    public AIObject[] AIObjects;

    public void AddAIObject(AIObject ai)
    {
        AIObject[] temp = new AIObject[AIObjects.Length + 1];
        AIObjects.CopyTo(AIObjects, 0);
        temp[AIObjects.Length] = ai;
        AIObjects = temp;
    }

    public void RemoveAIObject(AIObject ai)
    {
        int index = 0;
        bool found = false;

        for (int x = 0; x < AIObjects.Length; x++)
        {
            if (AIObjects[x] != null && AIObjects[x] == ai)
            {
                index = x;
                found = true;
            }
        }
        if (found)
        {
            AIObject[] temp = new AIObject[AIObjects.Length - 1];
            // Debug.Log("B"); 
            int y = 0;
            for (int x = 0; x < AIObjects.Length; x++)
            {
                if (x != index)
                {
                    temp[y] = AIObjects[x];
                    y = y + 1;
                }
            }
            AIObjects = temp;
        }
    }

    public void AssignTarget(MissileTrackerAndResponse x)
    {

    }

    public void Engage()
    {
        foreach (AIObject x in AIObjects)
        {
            x.shouldAttack = true;
            x.shouldAttackOnSight = true;
        }
    }

    public void StopEngage()
    {
        foreach (AIObject x in AIObjects)
        {
            x.shouldAttack = false;
            x.shouldAttackOnSight = false;
        }
    }

    public void SmokeOn()
    {
        foreach (AIObject x in AIObjects)
        {
            x.SmokeOn();
        }
    }

    public void SmokeOff()
    {
        foreach (AIObject x in AIObjects)
        {
            x.SmokeOff();
        }
    }


}
