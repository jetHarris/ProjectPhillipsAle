using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {
    public Laser laserPrefab;
    protected float firingTimer = 0.2f;
    protected float firingTimerReset;
    protected float maxSpeed = 5;
    protected float speedUpTime = 0.5f;
    protected float thrustingTime = 0;
    public float health = 30;
    public float shieldHealth = 40;
    public float shieldCount = 0;
    public Vector2 forward;
    protected float speed = 5000f;
    protected float turnSpeed = 1;
    protected Vector2 acceleration;
    public Rigidbody2D myBody;
    public int teamId = 0;
    public int shipId;
    public bool AIControlled = true;
    protected bool fireRight = true;
    protected float shieldTimer = 0;
    protected float shieldTimerReset = 1;
    protected SpriteRenderer shipArt;
    protected SpriteRenderer thrusterArt;
    protected SpriteRenderer shieldArt;
    protected SpriteRenderer teamColourArt;
    protected float alertTimer = 0;
    protected bool goingRed = true;
    public List<AIShip> followingShips;
    public bool isAlive
    {
        get { return health > 0; }
    }

    // Use this for initialization
    void Start () {
        firingTimerReset = firingTimer;
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        followingShips = new List<AIShip>();
    }

    protected void LateStart()
    {
        var Sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
        shipArt = Sprites[0];
        thrusterArt = Sprites[1];
        thrusterArt.enabled = false;
        shieldArt = Sprites[2];
        shieldArt.color = new Color(1, 1, 1, 0);
        teamColourArt = Sprites[3];
        teamColourArt.color = PlayerManager.teamColours[teamId];
    }
	
	// Update is called once per frame
	protected void CustomUpdate () {
        float deltaTime = Time.deltaTime;
		if (shieldHealth <= 0)
        {
            shieldCount += deltaTime;
            if (shieldCount > 4)
            {
                shieldCount = 0;
                shieldHealth = 40;
                shipArt.color = new Color(1, 1, 1, 1);
            }
            else
            {
                if (goingRed)
                {
                    alertTimer += deltaTime;
                    if (alertTimer >= 1)
                    {
                        goingRed = false;
                    }
                }
                else
                {
                    alertTimer -= deltaTime;
                    if (alertTimer <= 0)
                    {
                        goingRed = true;
                    }
                }

                shipArt.color = new Color(1, 0.5f + (0.5f * alertTimer), 0.5f + (0.5f * alertTimer), 1);
            }
        }

        if (shieldTimer > 0)
        {
            shieldTimer -= deltaTime;
            shieldArt.color = new Color(1, 1, 1, shieldTimer / shieldTimerReset);
        }

        //update any followers
        int followerCount = followingShips.Count;
        bool first = true;
        for (int i = 0; i < followerCount; i++)
        {
            if (followingShips[i] == null || !followingShips[i].isAlive ||followingShips[i].state != AIShip.eState.Following)
            {
                followingShips.RemoveAt(i);
                break;
            }

            followingShips[i].UpdateFollowingTargetLocation(first);
            first = false;
        }
    }

    public virtual void TakeDamage(float damageAmount, Ship attacker)
    {
        if (isAlive)
        {
            if (shieldHealth > 0)
            {
                shieldHealth -= damageAmount;
                shieldTimer = shieldTimerReset;
            }
            else
            {
                health -= damageAmount;
            }

            if (!isAlive)
            {
                if (shipArt != null)
                {
                    thrusterArt.enabled = false;
                    shipArt.color = new Color(94 / 255f, 79 / 255f, 79 / 255f, 150 / 255f);
                    shieldArt.color = new Color(1, 1, 1, 0);
                }
                myBody.drag = 1;
            }
        }
    }
}
