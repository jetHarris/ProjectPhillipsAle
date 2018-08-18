using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Vector2 acceleration;
    float speed = 20.1f;
    Rigidbody2D myBody;
    GameObject self;
    Vector2 forward;

	// Use this for initialization
	void Start () {
        acceleration = new Vector2();
        //self = this.GetComponentInParent<GameObject>();
        myBody = gameObject.GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {

        float deltaTime = Time.deltaTime;
        bool pressed = false;
        forward = new Vector2(transform.up.x, transform.up.y);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W))
        {
            acceleration += deltaTime * speed * forward;
            pressed = true;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.forward * -3);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * 3);
        }


        if (!pressed)
        {
            acceleration *= 0.9f;
        }
        if (pressed)
        {
            myBody.velocity += acceleration * deltaTime;
        }
    }
}
