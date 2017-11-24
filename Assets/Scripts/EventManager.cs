using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * class need a way to destroy the Event game object. 
 */

public class EventManager : MonoBehaviour {

    // Events that together with included determine if this event is enabled.
    public List<EventManager> conditions;
    public List<EventManager> milestones;

    // Events this event affects the state of, need to affect in order given.
    public List<EventManager> exclusions;
    public List<EventManager> inclusions;
    public List<EventManager> responses;

    public GameObject bars;
    public GameObject roof;
    public GameObject lightbulb;

    public Renderer buttonRend;
    public Material buttonPressed;

    public bool executed = false;  //should this event have a green button not yellow?
    public bool included = false;  //should this event be visible?
    public bool pending = false;   //should this events light be on?

    private bool event_eneabled; //can this event be executed? and works the bars.
    private bool dead = false; //To see if an event is dead meaning no need to check for enabledness, for use later.

    private Light event_light;

    private bool setUp = false; //so nothing can happen until after the setup;

    private BarMove barMove;
    private RoofMove roofMove;

    public void Initialize()
    {
        event_light = lightbulb.GetComponent<Light>();
        barMove = bars.GetComponent<BarMove>();
        roofMove = roof.GetComponent<RoofMove>();

        roofMove.setOpen(false);
        LightSwitch(false);
    }

    public GameObject Event { get; set; }

    public string ID { get; set; }

    // the setup is finished from GameController, check if dead, or enabled before get possible to use. 
    public void SetUp()
    {
        IsDead();
        IsEnabled();
        setUp = true;
    }

    private void Update()
    {
        IsEnabled();
    }

    private void OnMouseDown()
    {
        if (!setUp) return;
        if (IsEnabled())
        {
            Executed();
            GameController.Retest = true;
        }
    }

    /*
     * Returns the state of an event. order is:
     * included = GetState(executed, pending)
     */
    public bool GetState(out bool executed, out bool pending)
    {
        executed = this.executed;
        pending = this.pending;
        return included;
    }

    public bool IsEnabled()
    {
        bool eventen = event_eneabled;
        if (!included || dead)
        {  

            event_eneabled = false;
            if(eventen != event_eneabled) barMove.setOpen(event_eneabled);
            return event_eneabled;
        }
        if (conditions != null) 
        {
            foreach (EventManager e in conditions)
            {
                bool ex;
                bool re;
                bool inc = e.GetState(out ex, out re);
                if (!ex && inc)
                {
                    event_eneabled = false;
                    if (eventen != event_eneabled) barMove.setOpen(event_eneabled);
                    return event_eneabled;
                }
            }
        }
        if (milestones != null) 
        {
            foreach (EventManager e in milestones)
            {
                bool ex;
                bool re;
                bool inc = e.GetState(out ex, out re);
                if (re && inc)
                {
                    event_eneabled = false;
                    if (eventen != event_eneabled) barMove.setOpen(event_eneabled);
                    return event_eneabled;
                }
            }
        }
        event_eneabled = true;
        if (eventen != event_eneabled) barMove.setOpen(event_eneabled);
        return event_eneabled;
    }

    // the event is executed and has to affect how the others work. 
    public void Executed()
    {
        executed = true;
        SetExecuted();
        pending = false;
        LightSwitch(pending);
        foreach (EventManager e in exclusions)
        {
            e.Exclude();
        }
        foreach(EventManager e in inclusions)
        {
            e.Include();
        }
        foreach(EventManager e in responses)
        {
            e.MakePending();
        }
    }

    // exclude the event
    public void Exclude()
    {
        included = false;
        roofMove.setOpen(included);
    }

    // include the event
    public void Include()
    {
        included = true;
        roofMove.setOpen(included);
    }

    // changes state to pending.
    public void MakePending()
    {
        pending = true;
        LightSwitch(pending);
    }



    // checks to see if this is a dead event, (this may be changed later for better algorithms and possible just a call to set it from gamecontroller or similar. 
    public void IsDead()
    {
        dead = false;
        foreach(EventManager e in conditions)
        {
            if(this == e)
            {
                dead = true;
            }
        }
        //following is debug info to controll if later algorithms work on finding if dead, may have to be moved to item with all info. 
 //       if (dead) Debug.Log("event: " + ID + " was set to dead");
    }

    public void LightSwitch(bool active)
    {
        event_light.enabled = active;
    }

    //call when event is executed
    public void SetExecuted()
    {
        buttonRend.sharedMaterial = buttonPressed;
    }

    public void AddCondition(EventManager e){
        conditions.Add(e);
    }

    public void AddExclusion(EventManager e){
        exclusions.Add(e);
    }

    public void AddInclusion(EventManager e)
    {
        inclusions.Add(e);
    }

    public void AddResponse(EventManager e)
    {
        responses.Add(e);
    }

    public void AddMilestone(EventManager e)
    {
        milestones.Add(e);
    }
}
