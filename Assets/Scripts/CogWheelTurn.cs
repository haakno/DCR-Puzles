using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogWheelTurn : MonoBehaviour {
    
    public Transform position;
    public float turnspeed;

    // Use this for initialization
    void Start()
    {

    } 

    public void setTurnspeed(float speed)
    {
        turnspeed = speed;
    }

	// Update is called once per frame
	void Update () {
        position.Rotate(Vector3.up * (turnspeed * Time.deltaTime));
	}
}
