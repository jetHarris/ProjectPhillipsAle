using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Vector2 acceleration;
    float speed = 5000f;
    float turnSpeed = 1;
    Rigidbody2D myBody;
    GameObject self;
    Vector2 forward;
    int playerId;
    Camera trackingCamera;
    Vector3 offset = new Vector3(0, 0, -10);
    private GameObject playerManagerObject;
    private PlayerManager playerManager;
    PlayerManager.Player player;
    public Laser laserPrefab;
    private float firingTimer = 0.2f;
    private float firingTimerReset;
    private float maxSpeed = 5;
    private float speedUpTime = 0.5f;
    private float thrustingTime = 0;
    private bool lastDirectionForward = true;
    public float health = 30;

    public bool isAlive
    {
        get { return health > 0; }
    }

    //todo
    //health
    //death
    //not being able to do both triggers at once

    // Use this for initialization
    void Start () {
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();

        playerManagerObject = GameObject.Find("PlayerManagerMain");
        //get playerId from the PlayerManager
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        playerId = playerManager.AssignPlayer();
        player = playerManager.players[playerId];

        //getting the camera
        trackingCamera = GameObject.Find("camera" + playerId).GetComponent<Camera>();

        firingTimerReset = firingTimer;
    }

    void Awake()
    {

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (!isAlive)
        {
            return;
        }
        trackingCamera.transform.position = transform.position + offset;
        float deltaTime = Time.deltaTime;

        bool pressed = false;
        bool thrusting = false;
        forward = new Vector2(transform.up.x, transform.up.y);

        float propulsion = Input.GetAxis("RightTrigger_P" + playerId);
        if (propulsion > 0)
        {
            acceleration = deltaTime * speed * propulsion * Mathf.Min(1, thrustingTime/speedUpTime) * forward;
            pressed = true;
            thrusting = true;
            if (!lastDirectionForward)
            {
                thrustingTime = 0;
                lastDirectionForward = true;
            }
        }

        float backwardPropulsion = Input.GetAxis("LeftTrigger_P" + playerId);
        if (backwardPropulsion > 0 && propulsion == 0)
        {
            acceleration = deltaTime * (speed/3) * backwardPropulsion * Mathf.Min(1, thrustingTime / speedUpTime) * (forward *-1);
            pressed = true;
            if (lastDirectionForward)
            {
                thrustingTime = 0;
                lastDirectionForward = false;
            }
        }

        if (firingTimer > 0)
        {
            firingTimer -= Time.deltaTime;
            if (firingTimer < 0)
            {
                firingTimer = 0;
            }
        }

        if (Input.GetButton("B_P" + playerId) && firingTimer == 0)
        {
            Laser newLaser = (Laser)Instantiate(laserPrefab,
                transform.position + (new Vector3(forward.x, forward.y, 0) * 1),
                transform.rotation);
            newLaser.velocity = new Vector3(forward.x, forward.y, 0) * 5;
            newLaser.velocity.x += myBody.velocity.x;
            newLaser.velocity.y += myBody.velocity.y;
            newLaser.ship = this;
            newLaser.damage = 10;
            firingTimer = firingTimerReset;
        }

        float turningAxis = Input.GetAxis("LeftJoystickX_P" + playerId);
        if (turningAxis != 0)
        {
            myBody.AddTorque(-turningAxis * turnSpeed);
            pressed = true;
        }

        if (!thrusting)
        {
            //set the angular drag to lower while the ship isn't thrusting
            myBody.angularDrag = 1.5f;
            thrustingTime = 0;
        }
        if (pressed)
        {
            //set the angular drag to higher while the ship is moving
            if (thrusting)
            {
                thrustingTime += deltaTime;
                myBody.angularDrag = 3.3f;
                float theDot = Vector2.Dot(myBody.velocity, forward);
                if (theDot < 0)
                {
                    myBody.velocity += acceleration * deltaTime;
                }

                myBody.velocity += acceleration * deltaTime;

                if (myBody.velocity.magnitude > maxSpeed)
                {
                    myBody.velocity = myBody.velocity.normalized * maxSpeed;
                }
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if (!isAlive)
        {
            SpriteRenderer art = gameObject.GetComponentInChildren<SpriteRenderer>();
            if (art != null)
            {
                art.color = new Color(94, 79, 79);
            }
        }
    }
}
