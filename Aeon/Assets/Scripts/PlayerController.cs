using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Ship {
    GameObject self;
    int playerId;
    Camera trackingCamera;
    Vector3 offset = new Vector3(0, 0, -10);
    private GameObject playerManagerObject;
    private PlayerManager playerManager;
    PlayerManager.Player player;
    private bool lastDirectionForward = true;
    private float deathTime = 0;
    private float deathTimeMax = 3;

    //todo
    //death

    // Use this for initialization
    void Start () {
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        health = 60;
        speed = 5500;

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
            SpriteRenderer art = gameObject.GetComponentInChildren<SpriteRenderer>();
            if (art != null)
            {
                art.color = new Color(1, 1, 1, 1);
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
        trackingCamera.transform.position = transform.position + offset;
        

        bool pressed = false;
        bool thrusting = false;
        
        forward.x = transform.up.x;
        forward.y = transform.up.y;

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
            Vector2 normalizedForward = forward.normalized;
            Vector3 offset = fireRight ? new Vector3(-normalizedForward.y, normalizedForward.x, 0).normalized * 0.65f :
                new Vector3(normalizedForward.y, -normalizedForward.x, 0).normalized * 0.65f;
            fireRight = !fireRight;

            Laser newLaser = (Laser)Instantiate(laserPrefab,
                transform.position + (new Vector3(normalizedForward.x, normalizedForward.y, 0) * -0.02f) + offset,
                transform.rotation);

            newLaser.velocity = new Vector3(forward.x, forward.y, 0) * 5;
            newLaser.velocity.x += myBody.velocity.x;
            newLaser.velocity.y += myBody.velocity.y;
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
                SpriteRenderer art = gameObject.GetComponentInChildren<SpriteRenderer>();
                if (art != null)
                {
                    art.color = new Color(1, 1, 1, 1);
                }
                Destroy(temp.gameObject);
                deathTime = 0;
                playerManager.ships.RemoveAt(i);
                break;
            }
        }
    }
}
