using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Vector2 acceleration;
    float speed = 20.1f;
    Rigidbody2D myBody;
    GameObject self;
    Vector2 forward;
    int playerId;
    Camera trackingCamera;
    Vector3 offset = new Vector3(0, 0, -10);
    UnityEngine.KeyCode forwardKey = KeyCode.W;
    UnityEngine.KeyCode leftKey = KeyCode.A;
    UnityEngine.KeyCode rightKey = KeyCode.D;
    public GameObject playerManagerObject;
    public PlayerManager playerManager;
    PlayerManager.Player player;

    // Use this for initialization
    void Start () {
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();

        playerManagerObject = GameObject.Find("PlayerManagerMain");
        //get playerId from the PlayerManager
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        playerId = playerManager.AssignPlayer();
        player = playerManager.players[playerId];
        forwardKey = player.forward;
        leftKey = player.left;
        rightKey = player.right;

        //getting the camera
        trackingCamera = GameObject.Find("camera" + playerId).GetComponent<Camera>();
    }

    void Awake()
    {

    }
	
	// Update is called once per frame
	void Update () {

        trackingCamera.transform.position = transform.position + offset;
        float deltaTime = Time.deltaTime;
        bool pressed = false;
        forward = new Vector2(transform.up.x, transform.up.y);

        if (Input.GetKeyDown(forwardKey) || Input.GetKey(forwardKey))
        {
            acceleration += deltaTime * speed * forward;
            pressed = true;
        }

        if (Input.GetKeyDown(rightKey) || Input.GetKey(rightKey))
        {
            transform.Rotate(Vector3.forward * -3);
        }

        if (Input.GetKeyDown(leftKey) || Input.GetKey(leftKey))
        {
            transform.Rotate(Vector3.forward * 3);
        }


        if (!pressed)
        {
            acceleration *= 0.9f;
            //set the angular drag to lower while the ship isn't thrusting
            myBody.angularDrag = 0.1f;
        }
        if (pressed)
        {
            //set the angular drag to higher while the ship is moving
            myBody.angularDrag = 3.0f;
            myBody.velocity += acceleration * deltaTime;
        }
    }
}
