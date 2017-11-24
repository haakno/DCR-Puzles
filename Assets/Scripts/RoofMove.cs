using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoofMove : MonoBehaviour
{

    public float turnspeed;
    public float openSpeed;
    public CogWheelTurn cogWheel;

    private bool open = false;

    // Use this for initialization
    void Start()
    {
  //      open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.z < 1.4 && !open)
        {
            Move(openSpeed);
        }
        else if (transform.localScale.z > 0.2 && open)
        {
            Move(-openSpeed);
        }
        else
        {
            cogWheel.setTurnspeed(0);
        }
    }

    void Move(float speed)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z + speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - speed / 2 * Time.deltaTime);
    }

    public void setOpen(bool open)
    {
        this.open = open;
        cogWheel.setTurnspeed(turnspeed);
    }

}
