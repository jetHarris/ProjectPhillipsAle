using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Ship {
    GameObject self;
    int playerId;
    Camera trackingCamera;
    Vector3 cameraOffset = new Vector3(0, 0, -10);
    private GameObject playerManagerObject;
    private PlayerManager playerManager;
    PlayerManager.Player player;
    private bool lastDirectionForward = true;
    private float deathTime = 0;
    private float deathTimeMax = 3;
    bool thrusting = false;
    bool stabilizing = true;


    // Use this for initialization
    void Start () {
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        health = 60;
        speed = 5500;
        maxSpeed = 5;

        playerManagerObject = GameObject.Find("PlayerManagerMain");
        //get playerId from the PlayerManager
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        playerId = playerManager.AssignPlayer();
        player = playerManager.players[playerId];
        shipId = playerManager.AssignShip();
        AIControlled = false;

        //getting the camera
        trackingCamera = GameObject.Find("camera" + playerId).GetComponent<Camera>();

        firingTimerReset = firingTimer;
        playerManager.ships.Add(this);
        forward = new Vector2(transform.up.x, transform.up.y);

        base.LateStart();
    }

    void Awake()
    {

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        //revive cheat currently
        if (Input.GetButton("Y_P" + playerId))
        {
            health = 999;
            if (shipArt != null)
            {
                shipArt.color = new Color(1, 1, 1, 1);
            }
        }
        float deltaTime = Time.deltaTime;
        
        if (!isAlive)
        {
            deathTime += deltaTime;
            if (deathTime > deathTimeMax)
            {
                TakeOverOtherShip();
            }
            return;
        }
        CustomUpdate();

        Vector3 normalizedVel3 = myBody.velocity.normalized;
        float magnitude = myBody.velocity.magnitude;
        //float slowDown = 1;
        //the come back to the player has to be slowed down somehow
        //if (magnitude > 0)
        trackingCamera.transform.position = (transform.position + cameraOffset);// +
            //(normalizedVel3 * -((magnitude/maxSpeed)* maxCameraAway)) * slowDown;

        bool pressed = false;
        
        
        forward.x = transform.up.x;
        forward.y = transform.up.y;

        float cancelStabilizer = Input.GetAxis("LeftTrigger_P" + playerId);
        if (cancelStabilizer > 0 && stabilizing)
        {
            stabilizing = false;
            myBody.drag = 0;
        }
        else if (cancelStabilizer == 0 && !stabilizing)
        {
            stabilizing = true;
            myBody.drag = 3;
        }

        float propulsion = Input.GetAxis("RightTrigger_P" + playerId);
        if (propulsion > 0)
        {
            acceleration = deltaTime * speed * propulsion * Mathf.Min(1, thrustingTime/speedUpTime) * forward;
            pressed = true;
            if (!thrusting)
            {
                thrusterArt.enabled = true;
            }
            thrusting = true;
            if (!lastDirectionForward)
            {
                thrustingTime = 0;
                lastDirectionForward = true;
            }
        }
        else
        {
            if (thrusting)
            {
                thrusterArt.enabled = false;
            }
            thrusting = false;
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
            Vector2 normalizedForward = forward.normalized;
            Vector3 offset = fireRight ? new Vector3(-normalizedForward.y, normalizedForward.x, 0).normalized * 0.65f :
                new Vector3(normalizedForward.y, -normalizedForward.x, 0).normalized * 0.65f;
            fireRight = !fireRight;

            Laser newLaser = (Laser)Instantiate(laserPrefab,
                transform.position + (new Vector3(normalizedForward.x, normalizedForward.y, 0) * -0.02f) + offset,
                transform.rotation);

            newLaser.velocity = new Vector3(forward.x, forward.y, 0) * 15;
            float theDot = Vector2.Dot(myBody.velocity, forward);
            if (theDot > 0)
            {
                newLaser.velocity.x += myBody.velocity.x * (theDot / 4.8f);
                newLaser.velocity.y += myBody.velocity.y * (theDot / 4.8f);
            }
            newLaser.ship = this;
            newLaser.damage = 10;
            firingTimer = firingTimerReset;
        }

        //debug do damage to self
        if (Input.GetButton("X_P" + playerId))
        {
            TakeDamage(999, this);
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

    public override void TakeDamage(float damageAmount, Ship attacker)
    {
        base.TakeDamage(damageAmount, attacker);
    }

    private void TakeOverOtherShip()
    {
        //take over an AI ship somehow
        int shipNum = playerManager.ships.Count;
        for (int i = 0; i < shipNum; i++)
        {
            Ship temp = playerManager.ships[i];
            if (temp != this && temp.isAlive && temp.teamId == teamId)
            {
                myBody.velocity = temp.myBody.velocity;
                myBody.angularDrag = temp.myBody.angularDrag;
                myBody.angularVelocity = temp.myBody.angularVelocity;
                transform.position = temp.transform.position;
                transform.rotation = temp.transform.rotation;
                health = temp.health;
                if (shipArt != null)
                {
                    shipArt.color = new Color(1, 1, 1, 1);
                }
                Destroy(temp.gameObject);
                deathTime = 0;
                playerManager.ships.RemoveAt(i);
                break;
            }
        }
    }
}
