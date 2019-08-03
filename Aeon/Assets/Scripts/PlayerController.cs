using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    protected SpriteRenderer selecterArt;
    protected SpriteRenderer pullArt;
    float chargingTimer = 0;
    float startingHealth = 0;
    float startingShield = 0;
    float shoutCooldown = 0.8f;
    float boostSpeedModifier = 1;
    float boostAmount = 100;
    float boostStartingAmount;
    float notUsingBoostTimer = 0;

    [System.Flags] public enum eShipState
    {
        None = 0,
        Boosting = 1,
        Thrusting = 2,
        ChargingSecondary = 3,
        Stabilizing = 4,
    }

    public eShipState state;

    public bool HasFlag(eShipState flag)
    {
        return (state & flag) != 0;
    }

    public void AddFlag(eShipState flag)
    {
        state |= flag;
    }

    public void RemoveFlag(eShipState flag)
    {
        state &= ~flag;
    }

    // Use this for initialization
    void Start () {
        state = eShipState.None;
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        health = 60;
        startingHealth = health;
        startingShield = shieldHealth;
        boostStartingAmount = boostAmount;
        speed = 2000;
        maxSpeed = 5;
        speedUpTime = 4f;
        myBody.drag = 0;

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

        var Sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
        pullArt = Sprites[5];
        pullArt.enabled = false;
    }

    void Awake()
    {

    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (isAlive)
        {
            float shoot = Input.GetAxis("RightTrigger_P" + playerId);
            if (shoot > 0 && firingTimer == 0)
            {
                FireLaser(false);
            }

            if (Input.GetButton("Y_P" + playerId))
            {
                chargingTimer += deltaTime;
            }
            else if (chargingTimer > 0)
            {
                FireLaser(true);
            }
        }

        //minimap
        if (Input.GetButtonDown("Back_P" + playerId))
        {
            UI.Instance.MinimapEnableDisable(playerId, true);
        }
        else if (Input.GetButtonUp("Back_P" + playerId))
        {
            UI.Instance.MinimapEnableDisable(playerId, false);
        }

        UI.Instance.UpdatePlayerStatus(playerId, health/startingHealth, shieldHealth/startingShield, boostAmount/boostStartingAmount);
    }

    // Update is called once per frame
    void FixedUpdate () {
        //revive cheat currently
        if (Input.GetButton("Start_P" + playerId))
        {
            health = 999;
            if (shipArt != null)
            {
                shipArt.color = new Color(1, 1, 1, 1);
                miniMapArt.enabled = true;
            }
        }

        //debug to reset the scene
        if (Input.GetButton("X_P" + playerId))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
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

        //shouting commands
        if (shoutCooldown > 0)
        {
            shoutCooldown -= deltaTime;
        }
        //find nearby ships and make them follow you
        if (Input.GetAxis("LeftRightD_P" + playerId) > 0.5 && followingShips.Count < 2 && shoutCooldown <= 0)
        {
            UI.Instance.Shout(playerId, "Follow");
            MakeShipsFollow();
            shoutCooldown = 0.8f;
        }

        //tell nearby ships to break off
        if (Input.GetAxis("LeftRightD_P" + playerId) < -0.5 && followingShips.Count > 0 && shoutCooldown <= 0)
        {
            UI.Instance.Shout(playerId, "Break Off");
            for (int i = followingShips.Count-1; i > -1; i--)
            {
                followingShips[i].target = null;
                followingShips[i].state = AIShip.eState.Travelling;
                followingShips.RemoveAt(i);
            }
            shoutCooldown = 0.8f;
        }

        //turning on and off the stabilizers
        float stabilizer = Input.GetAxis("LeftJoystickY_P" + playerId);
        if (stabilizer > 0.5 && !HasFlag(eShipState.Stabilizing))
        {
            AddFlag(eShipState.Stabilizing);
            myBody.drag = 1.4f;
        }
        else if (stabilizer <= 0.5 && HasFlag(eShipState.Stabilizing))
        {
            RemoveFlag(eShipState.Stabilizing);
            myBody.drag = 0;
        }

        float propulsion = Input.GetAxis("LeftTrigger_P" + playerId);
        if (propulsion > 0)
        {
            acceleration = deltaTime * speed * propulsion * Mathf.Max(0.5f,Mathf.Clamp(thrustingTime/speedUpTime,0,1)) * forward;
            pressed = true;
            if (!thrusting)
            {
                thrusterAnim.SetBool("IsStarting", true);
                thrusterAnim.SetBool("IsEnding", false);
            }
            thrusting = true;
        }
        else
        {
            if (thrusting)
            {
                thrusterAnim.SetBool("IsStarting", false);
                thrusterAnim.SetBool("IsEnding", true);
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

        float turningAxis = Input.GetAxis("LeftJoystickX_P" + playerId);
        if (turningAxis != 0)
        {
            myBody.AddTorque(-turningAxis * turnSpeed);
            pressed = true;
        }

        //boosting logic
        if (boostSpeedModifier > 1)
        {
            boostSpeedModifier -= 0.05f;
        }
        float lateralBoost = Input.GetAxis("RightJoystickX_P" + playerId);
        float verticalBoost = Input.GetAxis("RightJoystickY_P" + playerId);
        if ((lateralBoost != 0 || verticalBoost != 0) && boostAmount > 0)
        {
            if (!HasFlag(eShipState.Boosting))
            {
                AddFlag(eShipState.Boosting);
                pullArt.enabled = true;
            }
            notUsingBoostTimer = 0;
            
            if (boostAmount == 0)
            {
                boostAmount = -60;
            }
        }
        else if (HasFlag(eShipState.Boosting))
        {
            RemoveFlag(eShipState.Boosting);
            pullArt.enabled = false;
        }
        else if ((notUsingBoostTimer += deltaTime) > 1 && boostAmount < boostStartingAmount)
        {
            boostAmount += 2;
        }

        if (boostAmount > 0)
        {
            if (lateralBoost != 0 && Mathf.Abs(lateralBoost) > Mathf.Abs(verticalBoost))
            {
                Vector2 normalizedForward = forward.normalized;
                Vector2 boost = lateralBoost < 0 ? new Vector2(-normalizedForward.y, normalizedForward.x).normalized :
                    new Vector2(normalizedForward.y, -normalizedForward.x).normalized;
                myBody.AddForce(boost * 50);
                boostAmount -= 0.3f;
                pressed = true;
                pullArt.transform.rotation = Quaternion.Euler(myBody.transform.rotation.eulerAngles + new Vector3(0, 0, lateralBoost < 0 ? 90 : -90));
            }
            else if (verticalBoost < 0)
            {
                boostSpeedModifier = 2;
                pullArt.transform.rotation = myBody.transform.rotation;
                boostAmount -= 1;
            }
            else if (verticalBoost > 0 && turningAxis != 0)
            {
                myBody.AddTorque(-turningAxis * turnSpeed * 2);
                pullArt.transform.rotation = Quaternion.Euler(myBody.transform.rotation.eulerAngles + new Vector3(0, 0, turningAxis < 0 ? 120 : -120));
                boostAmount -= 0.3f;
            }
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
            }
        }

        if (myBody.velocity.magnitude > (maxSpeed * boostSpeedModifier))
        {
            myBody.velocity = myBody.velocity.normalized * (maxSpeed * boostSpeedModifier);
        }

        trackingCamera.orthographicSize = Mathf.Clamp((5 + 2 * (1 - (myBody.velocity.magnitude / maxSpeed))),5,7);
    }

    public void FireLaser(bool big)
    {
        Vector2 normalizedForward = forward.normalized;
        Vector3 offset = fireRight ? new Vector3(-normalizedForward.y, normalizedForward.x, 0).normalized * 0.65f :
            new Vector3(normalizedForward.y, -normalizedForward.x, 0).normalized * 0.65f;
        fireRight = !fireRight;

        if (!big)
        {
            Laser newLaser = (Laser)Instantiate(laserPrefab,
                transform.position + (new Vector3(normalizedForward.x, normalizedForward.y, 0) * -0.02f) + offset,
                transform.rotation);

            newLaser.velocity = new Vector3(forward.x, forward.y, 0) * 15;
            newLaser.ship = this;
            newLaser.damage = 10;
            newLaser.LateStart();
            for (int i = 0; i < followingShips.Count; i++)
            {
                followingShips[i].FireLaser();
            }
        }
        else if (big && chargingTimer > 0.2f)
        {
            Laser newLaser = (Laser)Instantiate(chargedLaserPrefab,
                transform.position + (new Vector3(normalizedForward.x, normalizedForward.y, 0) * 0.8f),
                transform.rotation);
            newLaser.velocity = new Vector3(forward.x, forward.y, 0) * 8;
            newLaser.ship = this;
            newLaser.damage = 100;
            newLaser.LateStart();
            chargingTimer = 0;
        }

        firingTimer = firingTimerReset;
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
                        if (nearestShips.Count >= 2 - followingShips.Count)
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
