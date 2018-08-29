using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dan_PlayerController : MonoBehaviour {

    public Transform mPlayerTransform;
    public Rigidbody2D mPlayerRigidBody;
    public float mPlayerSpeed;

	// Use this for initialization
	void Start () {
        mPlayerTransform = gameObject.gameObject.GetComponent<Transform>();
        mPlayerRigidBody = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        float movementHorizontal = Input.GetAxis("Horizontal");
        float movementVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(movementHorizontal, movementVertical);

        mPlayerRigidBody.velocity = movement * mPlayerSpeed;

        mPlayerTransform.Rotate(Vector3.forward * movementHorizontal);

        Debug.Log("Boob");
    }
}
