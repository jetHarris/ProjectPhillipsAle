using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour {
    public Laser laserPrefab;
    public Laser chargedLaserPrefab;
    protected float firingTimer = 0.2f;
    protected float firingTimerReset;
    protected float maxSpeed = 5;
    protected float speedUpTime = 0.5f;
    protected float thrustingTime = 0;
    public float health = 30;
    public float shieldHealth = 40;
    public float shieldHealthStart;
    public Vector2 forward;
    protected float speed = 5000f;
    protected float turnSpeed = 1;
    protected Vector2 acceleration;
    public Rigidbody2D myBody;
    public int teamId = 0;
    public int shipId;
    public bool AIControlled = true;
    protected bool fireRight = true;
    protected float shieldFlashTimer = 0;
    protected float shieldTimerReset = 1;
    protected float timeSinceLastHit = 0;
    protected SpriteRenderer shipArt;
    protected SpriteRenderer thrusterArt;
    protected Animator thrusterAnim;
    protected SpriteRenderer shieldArt;
    protected Animator shieldAnim;
    protected SpriteRenderer teamColourArt;
    protected SpriteRenderer miniMapArt;
    protected float alertTimer = 0;
    protected const float ALERT_TIME_MAX = 1;
    public List<AIShip> followingShips;
    public Explosion explosionPrefab;
    public Explosion smallExplosionPrefab;
    public Explosion shieldHitPrefab;
    public Explosion junk;
    public bool isAlive
    {
        get { return health > 0; }
    }

    [System.Flags]
    public enum eShipState
    {
        None = 0,
        Boosting = 1,
        Thrusting = 2,
        ChargingSecondary = 4,
        Stabilizing = 8,
        Repelling = 16,
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
        //thrusterArt.enabled = false;
        thrusterAnim = thrusterArt.GetComponent<Animator>();
        shieldArt = Sprites[2];
        shieldAnim = shieldArt.GetComponent<Animator>();
        shieldArt.color = new Color(1, 1, 1, 0);
        teamColourArt = Sprites[3];
        teamColourArt.color = PlayerManager.teamColours[teamId];
        //minimap display
        miniMapArt = Sprites[4];
        miniMapArt.color = PlayerManager.teamColours[teamId];

        shieldHealthStart = shieldHealth;
    }
	
	// Update is called once per frame
	protected void CustomUpdate () {
        float deltaTime = Time.deltaTime;
		if (shieldHealth <= 0 && alertTimer > 0)
        {
            alertTimer -= deltaTime;
            shipArt.color = new Color(1, (1 - (alertTimer / ALERT_TIME_MAX)), (1 - (alertTimer / ALERT_TIME_MAX)), 1);
        }

        if (shieldHealth < shieldHealthStart)
        {
            if (timeSinceLastHit < 50)
            {
                timeSinceLastHit += deltaTime;
            }

            if (timeSinceLastHit > 2)
            {
                shieldHealth += 1;
                shipArt.color = new Color(1, 1, 1, 1);
            }
        }

        if (shieldFlashTimer > 0)
        {
            shieldFlashTimer -= deltaTime;
            shieldArt.color = new Color(1, 1, 1, shieldFlashTimer / shieldTimerReset);
        }

        //update any followers
        int followerCount = followingShips.Count;
        bool first = true;
        for (int i = 0; i < followerCount; i++)
        {
            if (followingShips[i] == null || !followingShips[i].isAlive ||followingShips[i].stateAI != AIShip.eAIState.Following)
            {
                followingShips.RemoveAt(i);
                break;
            }

            followingShips[i].UpdateFollowingTargetLocation(first);
            first = false;
        }
    }

    public virtual void TakeDamage(float damageAmount, Ship attacker, Vector2 hitlocation)
    {
        if (isAlive)
        {
            timeSinceLastHit = 0;
            if (shieldHealth > 0)
            {
                shieldHealth -= damageAmount;
                shieldFlashTimer = shieldTimerReset;
                //damage bleed through on by default currently
                if (shieldHealth <= 0)
                {
                    shieldAnim.SetTrigger("broken");
                    health += shieldHealth;
                }
                Instantiate(shieldHitPrefab, hitlocation, transform.rotation);
            }
            else
            {
                health -= damageAmount;
                alertTimer = ALERT_TIME_MAX;
                Instantiate(smallExplosionPrefab, hitlocation, transform.rotation);
            }

            if (!isAlive)
            {
                if (shipArt != null)
                {
                    shipArt.color = new Color(94 / 255f, 79 / 255f, 79 / 255f, 150 / 255f);
                    shieldArt.color = new Color(1, 1, 1, 0);
                    miniMapArt.enabled = false;
                }
                if (AIControlled)
                {
                    myBody.drag = 1; //remove this when state logic is in this class
                }
                Die();
            }

        }
    }

    public virtual void Die()
    {

    }
}
