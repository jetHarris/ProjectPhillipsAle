using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIShip : Ship
{
    //use the same laser math as the player
    private PlayerManager playerManager;
    public Ship target;
    private Ship aggressor;

    private float slowAngularAngle = 4;
    private float overTurnAngle = 2;
    private float thrustAngle = 20;
    private float shootAngle = 5;
    private float followDistance = 5;
    private float fireDistance = 6;

    private float fleeDistance = 8;
    private float fleeOffsetTime = 0;
    private float fleeOffsetMaxTime = 0;
    private Vector2 fleeOffset;
    private bool thrusting = false;
    private bool animThrusting = false; //Awful, take this out as soon as more state logic has been put in

    private Vector2 targetLocation;
    public enum eAIState
    {
        Hunting,
        Fleeing,
        StrafingToAttack,
        StrafingToFlee,
        Travelling,
        Following
    }

    public eAIState stateAI;
    void Start () {
        firingTimerReset = firingTimer;
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        GameObject playerManagerObject = GameObject.Find("PlayerManagerMain");
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        playerManager.ships.Add(this);
        stateAI = eAIState.Travelling;
        fleeOffset = new Vector2();
        targetLocation = transform.position;
        new Vector2(transform.up.x, transform.up.y);
        shipId = playerManager.AssignShip();
        base.LateStart();
    }

	void FixedUpdate () {

        if (!isAlive)
        {
            return;
        }

        CustomUpdate();
        forward.x = transform.up.x;
        forward.y = transform.up.y;
        float deltaTime = Time.deltaTime;

        switch (stateAI)
        {
            case eAIState.Travelling:
            {
                // find a target if we dont have one
                if (target == null || !target.isAlive || target.teamId == teamId)
                {
                    //todo better distance calculation once sections are completed
                    int shipNum = playerManager.ships.Count;
                    int foundNum = -1;
                    float dist = 9999999;
                    for (int i = 0; i < shipNum; i++)
                    {
                        Ship temp = playerManager.ships[i];
                        if (temp.shipId != shipId &&
                            temp.isAlive &&
                            temp.teamId != teamId &&
                            Vector2.Distance(temp.transform.position,transform.position) < dist)
                        {
                            dist = Vector2.Distance(temp.transform.position, transform.position);
                            foundNum = i;
                        }
                    }

                    if (foundNum >= 0)
                    {
                        target = playerManager.ships[foundNum];
                            stateAI = eAIState.Hunting;
                    }
                }
                else
                {
                        stateAI = eAIState.Hunting;
                }
            }
            break;

            case eAIState.Following:
            {
                if (target != null)
                {
                    float distance = Vector2.Distance(targetLocation, transform.position);
                    float angle = TurnTowards(deltaTime, targetLocation);

                    if (angle < thrustAngle && distance > followDistance)
                    {
                        MoveForward(deltaTime, 3.3f, true);
                    }
                    else
                    {
                        thrustingTime = 0;
                        myBody.angularDrag = 1.5f;

                        if (distance > followDistance / 2)
                        {
                            MoveForward(deltaTime, 1.5f, false, 0.2f);
                        }
                        else if (animThrusting)
                        {
                            thrusterAnim.SetBool("IsStarting", false);
                            thrusterAnim.SetBool("IsEnding", true);
                            animThrusting = false;
                        }
                    }

                    if (target.AIControlled)
                    {
                        AIShip theOtherShip = ((AIShip)target);
                        if (!target.isAlive || theOtherShip.stateAI == eAIState.Travelling || theOtherShip.stateAI == eAIState.Fleeing)
                        {
                                stateAI = eAIState.Travelling;
                            theOtherShip.followingShips.Remove(this);
                        }
                    }
                    else
                    {
                        if (!target.isAlive)
                        {
                                stateAI = eAIState.Travelling;
                            target.followingShips.Remove(this);
                        }
                    }
                }
                else
                {
                     stateAI = eAIState.Travelling;
                }

            }
            break;

            case eAIState.Hunting:
            {
                if (target != null && target.isAlive)
                {
                    if (firingTimer > 0)
                    {
                        firingTimer -= Time.deltaTime;
                        if (firingTimer < 0)
                        {
                            firingTimer = 0;
                        }
                    }

                    float distance = Vector2.Distance(target.transform.position, transform.position);
                    float angle = TurnTowards(deltaTime, target.transform.position);

                    if (angle < shootAngle && firingTimer == 0)
                    {
                        if (distance < fireDistance)
                        {
                            FireLaser();
                        }

                        //chance check to see if this turns into a strafing run
                        if (thrustingTime > 0 && Random.Range(0, 50) > 49) //if its moving at a good pace
                        {
                            float coinToss = Random.Range(0, 2);
                            if (coinToss >= 1)
                            {
                                targetLocation = new Vector2(transform.position.x, transform.position.y) +
                                    (forward.normalized * 9) +
                                    new Vector2(forward.y, -forward.x).normalized * Random.Range(2, 4);
                            }
                            else
                            {
                                targetLocation = new Vector2(transform.position.x, transform.position.y) +
                                    (forward.normalized * 9) +
                                    new Vector2(-forward.y, forward.x).normalized * Random.Range(2, 4);
                            }
                            stateAI = eAIState.StrafingToAttack;
                        }
                    }

                    if (angle < thrustAngle && distance > followDistance)
                    {
                        MoveForward(deltaTime, 3.3f, true);
                    }
                    else
                    {
                        thrustingTime = 0;
                        myBody.angularDrag = 1.5f;

                        if (distance > followDistance / 2)
                        {
                            MoveForward(deltaTime, 1.5f, false, 0.2f);
                        }
                        else if (animThrusting)
                        {
                            thrusterAnim.SetBool("IsStarting", false);
                            thrusterAnim.SetBool("IsEnding", true);
                            animThrusting = false;
                        }
                    }

                    //check for any other ship that is following the same target
                    if (followingShips.Count < 2)
                    {
                        int shipNum = playerManager.ships.Count;
                        for (int i = 0; i < shipNum; i++)
                        {
                            if (playerManager.ships[i].AIControlled)
                            {
                                AIShip temp = (AIShip)playerManager.ships[i];
                                if (temp.shipId != this.shipId &&
                                    temp.isAlive &&
                                    temp.teamId == teamId &&
                                    temp.stateAI == eAIState.Hunting &&
                                    temp.target.shipId == this.target.shipId &&
                                    temp.followingShips.Count < 2
                                    )
                                {
                                    target = temp;
                                    FindFollowingPosition();
                                    stateAI = eAIState.Following;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    stateAI = eAIState.Travelling;
                }
            }
            break;

            case eAIState.Fleeing:
            {
                if (aggressor != null)
                {
                    float distance = Vector2.Distance(aggressor.transform.position, transform.position);

                    if (distance < fleeDistance)
                    {
                        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
                        var enemyPos = aggressor.transform.position;
                        Vector2 targetDirection = pos - new Vector2(enemyPos.x, enemyPos.y);

                        fleeOffsetTime += deltaTime;
                        if (fleeOffsetTime > fleeOffsetMaxTime)
                        {
                            RecalulateFleeOffset();
                        }
                        Vector2 targetPoint = (targetDirection.normalized * fleeDistance) + pos + fleeOffset;
                        float angle = TurnTowards(deltaTime, targetPoint);
                        if (angle < thrustAngle)
                        {
                            MoveForward(deltaTime, 3.3f, true);
                        }
                        else
                        {
                            thrustingTime = 0;
                            myBody.angularDrag = 1.5f;
                            MoveForward(deltaTime, 1.5f, false, 0.2f);
                        }
                    }
                    else
                    {
                         stateAI = eAIState.Travelling;
                    }


                }
                else
                {
                        //todo
                        //search for any nearby enemy units, if there is one, assign a new aggressor
                        //if not break out of fleeing
                    stateAI = eAIState.Travelling;
                }
            }
            break;

            case eAIState.StrafingToFlee:
            {
                TurnTowards(deltaTime, targetLocation);
                MoveForward(deltaTime, 3.3f, true);

                //test distance from target, if it is small enough, then go into fleeing
                fleeOffsetTime += deltaTime;
                float distance = Vector2.Distance(targetLocation, transform.position);
                if (distance < 0.2f || fleeOffsetTime > fleeOffsetMaxTime)
                {
                    stateAI = eAIState.Fleeing;
                }
            }
            break;

            case eAIState.StrafingToAttack:
            {
                TurnTowards(deltaTime, targetLocation);
                MoveForward(deltaTime, 3.3f, true);

                //test distance from target, if it is small enough, then go into fleeing
                fleeOffsetTime += deltaTime;
                float distance = Vector2.Distance(targetLocation, transform.position);
                if (distance < 0.2f)
                {
                    stateAI = eAIState.Hunting;
                }
            }
            break;
        };
    }

    public void UpdateFollowingTargetLocation(bool first = true)
    {
        Vector2 normalizedTargetForward = target.forward.normalized;
        Vector2 targPos = new Vector2(target.transform.position.x, target.transform.position.y);
        if (first)
        {
            targetLocation = targPos +
                (normalizedTargetForward * 2) +
                (new Vector2(-normalizedTargetForward.y, normalizedTargetForward.x).normalized * 1.4f);
        }
        else
        {
            targetLocation = targPos +
                (normalizedTargetForward * 2) +
                (new Vector2(normalizedTargetForward.y, -normalizedTargetForward.x).normalized * 1.4f);
        }
    }

    public void AssignShipFollow(AIShip theShip)
    {
        if (followingShips.Count > 0)
        {
            followingShips[0].AssignShipFollow(theShip);
        }
        else
        {
            theShip.target = this;
            followingShips.Add(theShip);
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
        newLaser.ship = this;
        newLaser.damage = 10;
        newLaser.LateStart();
        firingTimer = firingTimerReset;
        for (int i = 0; i < followingShips.Count; i++)
        {
            followingShips[i].FireLaser();
        }
    }

    public void FindFollowingPosition(AIShip parent = null)
    {
        AIShip otherShip = (AIShip)target;

        otherShip.followingShips.Add(this);
        if (followingShips.Count > 0)
        {
            followingShips[0].stateAI = eAIState.Travelling;
            followingShips[0].target = null;
            followingShips.RemoveAt(0);
        }
    }

    public void MoveForward(float deltaTime, float angularDrag, bool thrustingIn, float modifier = 1)
    {
        if (!animThrusting)
        {
            thrusterAnim.SetBool("IsStarting", true);
            thrusterAnim.SetBool("IsEnding", false);
            animThrusting = true;
        }

        if (thrustingIn)
        {
            if (!thrusting)
            {
                thrusting = true;
            }
            acceleration = deltaTime * speed * Mathf.Min(1, thrustingTime / speedUpTime) * modifier * forward;
            thrustingTime += deltaTime;
        }
        else
        {
            if (thrusting)
            {
                thrusting = false;
            }
            acceleration = deltaTime * speed * modifier * forward;
        }

        myBody.angularDrag = angularDrag;

        myBody.velocity += acceleration * deltaTime;

        float modifiedMaxSpeed = modifier > 1 ? maxSpeed * modifier : maxSpeed;

        if (myBody.velocity.magnitude > modifiedMaxSpeed)
        {
            myBody.velocity = myBody.velocity.normalized * modifiedMaxSpeed;
        }
    }

    public float TurnTowards(float deltaTime, Vector2 targetPosition)
    {
        targetLocation = targetPosition;
        // where is the target?
        Vector2 targetDirection = targetPosition - new Vector2(transform.position.x, transform.position.y);
        // where are we looking?
        Vector2 lookDirection = transform.up;

        // to indicate the sign of the (otherwise positive 0 .. 180 deg) angle
        Vector3 cross = Vector3.Cross(targetDirection, lookDirection);
        // actually get the sign (either 1 or -1)
        float sign = Mathf.Sign(cross.z);

        // the angle, ranging from 0 to 180 degrees
        float angle = Vector2.Angle(targetDirection, lookDirection);
        angle = Mathf.Abs(angle);

        // apply torque in the opposite direction to decrease angle
        if (angle > overTurnAngle)
        {
            myBody.AddTorque(-sign * turnSpeed);
        }

        if (angle < slowAngularAngle)
        {
            myBody.angularVelocity = myBody.angularVelocity * 0.9f;
        }

        return angle;
    }

    public override void TakeDamage(float damageAmount, Ship attacker, Vector2 hitlocation)
    {
        base.TakeDamage(damageAmount, attacker, hitlocation);

        if (isAlive)
        {
            //todo
            //do a chance check and maybe check fearfulness (doesn't exist yet)
            //check current health as well
            if (Random.Range(0, 50) > 40)
            {
                aggressor = attacker;
                switch (stateAI)
                {
                    case eAIState.Hunting:
                    {
                        //go into a strafing flee, dont just turn directly around
                        //set the target location
                        float coinToss = Random.Range(0, 2);
                        if (coinToss >= 1)
                        {
                            targetLocation = new Vector2(transform.position.x, transform.position.y) +
                                (forward.normalized * 5) +
                                new Vector2(forward.y, -forward.x).normalized * Random.Range(2, 7);
                        }
                        else
                        {
                            targetLocation = new Vector2(transform.position.x, transform.position.y) +
                                (forward.normalized * 5) +
                                new Vector2(-forward.y, forward.x).normalized * Random.Range(2, 7);
                        }
                        fleeOffsetTime = 0;
                        fleeOffsetMaxTime = Random.Range(6, 12);
                        stateAI = eAIState.StrafingToFlee;
                    }
                    break;
                    default:
                    {
                        stateAI = eAIState.Fleeing;
                        RecalulateFleeOffset();
                    }
                    break;
                };
            }
        }
    }

    public override void Die()
    {
        base.Die();
        playerManager.ships.Remove(this);
        Instantiate(explosionPrefab, transform.position, transform.rotation);

        for (int i = 0; i < 4; ++i)
        {
            Vector3 position = transform.position;
            if (i %2 == 0)
            {
                position.x += Random.Range((float)0, (float)2);
            }
            else
            {
                position.y += Random.Range((float)0, (float)2);
            }
            var rotation = Quaternion.Euler(Random.Range((float)0, (float)360), 0, 0);
            var junk1 = Instantiate(junk, transform.position, rotation);
            Vector2 newVel = myBody.velocity;
            if (i % 2 == 0)
            {
                newVel.x += Random.Range((float)0, (float)2);
            }
            else
            {
                newVel.y += Random.Range((float)0, (float)2);
            }

            junk1.myBody.velocity = newVel;

        }

        UI.Instance.Shout("Kill");
        Destroy(gameObject);
    }

    public void RecalulateFleeOffset()
    {
        if (aggressor == null)
        {
            return;
        }
        Vector2 targetDirection = new Vector2(aggressor.transform.position.x, aggressor.transform.position.y) -
            new Vector2(transform.position.x, transform.position.y);
        float coinToss = Random.Range(0, 2);
       
        if (coinToss >= 1)
        {
            fleeOffset = new Vector2(targetDirection.y, -targetDirection.x).normalized * Random.Range(5, 15);
        }
        else
        {
            fleeOffset = new Vector2(-targetDirection.y, targetDirection.x).normalized * Random.Range(5, 15);
        }
        fleeOffsetTime = 0;
        fleeOffsetMaxTime = Random.Range(0, 5);
    }
}
