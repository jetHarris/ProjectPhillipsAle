using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : Ship {
    GameObject self;
    int playerId;
    Camera trackingCamera;
    GameObject trackingBackground;
    Vector3 cameraOffset = new Vector3(0, 0, -10);
    Vector3 backgroundOffset = new Vector3(10, 10, 1);
    private GameObject playerManagerObject;
    private PlayerManager playerManager;
    PlayerManager.Player player;
    private bool lastDirectionForward = true;
    bool thrusting = false;
    bool stabilizing = true;
    protected SpriteRenderer selecterArt;

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

        //getting the background
        trackingBackground = GameObject.Find("backgroundSprite" + playerId);

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
            return;
        }
        CustomUpdate();

        trackingCamera.transform.position = (transform.position + cameraOffset);

        trackingBackground.transform.position = (transform.position + backgroundOffset);

        bool pressed = false;
        
        forward.x = transform.up.x;
        forward.y = transform.up.y;

        //find nearby ships and make them follow you
        if (Input.GetButton("A_P" + playerId) && followingShips.Count < 2)
        {
            MakeShipsFollow();
        }

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
            FireLaser();
        }

        //debug to reset the scene
        if (Input.GetButton("X_P" + playerId))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
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

    public void FireLaser()
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
            //newLaser.velocity.x += myBody.velocity.x * (theDot / maxSpeed);
            //newLaser.velocity.y += myBody.velocity.y * (theDot / maxSpeed);
        }
        newLaser.ship = this;
        newLaser.damage = 10;
        firingTimer = firingTimerReset;

        for (int i = 0; i < followingShips.Count; i++)
        {
            followingShips[i].FireLaser();
        }
    }

    private void MakeShipsFollow()
    {
        List<Ship> nearestShips = new List<Ship>();
        int shipNum = playerManager.ships.Count;
        for (int i = 0; i < shipNum; i++)
        {
            if (playerManager.ships[i].AIControlled)
            {
                AIShip temp = (AIShip)playerManager.ships[i];
                if (temp.isAlive &&
                    temp.teamId == teamId)
                {
                    if (Vector2.Distance(temp.transform.position, transform.position) < 3)
                    {
                        nearestShips.Add(temp);
                        if (nearestShips.Count >= 2)
                        {
                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < nearestShips.Count; i++)
        {
            AIShip temp = (AIShip)nearestShips[i];
            temp.state = AIShip.eState.Following;
            temp.target = this;
            followingShips.Add(temp);
            if (temp.followingShips.Count > 0)
            {
                for (int j = 0; j < temp.followingShips.Count; j++)
                {
                    AIShip subShip = (AIShip)temp.followingShips[j];
                    subShip.target = null;
                    subShip.state = AIShip.eState.Travelling;
                }
            }
        }
    }

    public override void TakeDamage(float damageAmount, Ship attacker)
    {
        base.TakeDamage(damageAmount, attacker);
    }
}
