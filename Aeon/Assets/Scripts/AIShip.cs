﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIShip : Ship
{
    //Joce notes
    //todo
    //dont just activate strafe on firing
    //have variable or increase of pursue distance
    private PlayerManager playerManager;
    private Ship target;
    private Ship aggressor;

    private float slowAngularAngle = 4;
    private float overTurnAngle = 2;
    private float thrustAngle = 20;
    private float shootAngle = 5;
    private float followDistance = 5;

    private float fleeDistance = 8;
    private float fleeOffsetTime = 0;
    private float fleeOffsetMaxTime = 0;
    private Vector2 fleeOffset;

    private Vector2 targetLocation;

    public List<AIShip> followingShips;
    private enum eState
    {
        Hunting,
        Fleeing,
        StrafingToAttack,
        StrafingToFlee,
        Travelling,
        Following
    }

    private eState state;
    // Use this for initialization
    void Start () {
        firingTimerReset = firingTimer;
        acceleration = new Vector2();
        myBody = gameObject.GetComponent<Rigidbody2D>();
        GameObject playerManagerObject = GameObject.Find("PlayerManagerMain");
        playerManager = playerManagerObject.GetComponent<PlayerManager>();
        playerManager.ships.Add(this);
        state = eState.Travelling;
        fleeOffset = new Vector2();
        targetLocation = transform.position;
        new Vector2(transform.up.x, transform.up.y);
        shipId = playerManager.AssignShip();
        followingShips = new List<AIShip>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (!isAlive)
        {
            return;
        }

        forward.x = transform.up.x;
        forward.y = transform.up.y;
        float deltaTime = Time.deltaTime;

        switch (state)
        {
            case eState.Travelling:
            {
                // find a target if we dont have one
                if (target == null || !target.isAlive)
                {
                    //todo better way to find enemies, likely using distance
                    int shipNum = playerManager.ships.Count;
                    int checkNum = Random.Range(0, shipNum - 1);
                    for (int i = 0; i < shipNum; i++)
                    {
                        Ship temp = playerManager.ships[checkNum];
                        if (temp.shipId != shipId && temp.isAlive && temp.teamId != teamId)
                        {
                            target = temp;
                            state = eState.Hunting;
                            break;
                        }
                        checkNum++;
                        if (checkNum >= shipNum)
                        {
                            checkNum = 0;
                        }
                    }
                }
                else
                {
                    state = eState.Hunting;
                }
            }
            break;

            case eState.Following:
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
                    }
                }

            }
            break;

            case eState.Hunting:
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
                        FireLaser();

                        //chance check to see if this turns into a strafing run
                        if (thrustingTime > 0 && Random.Range(0, 50) > 45) //if its moving at a good pace
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
                            state = eState.StrafingToAttack;
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
                    }

                    //check for any other ship that is following the same target
                    //if (followingShips.Count == 0)
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
                                    temp.state == eState.Hunting &&
                                    temp.target.shipId == this.target.shipId
                                    )
                                {
                                    target = temp;
                                    FindFollowingPosition();
                                    state = eState.Following;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    state = eState.Travelling;
                }
            }
            break;

            case eState.Fleeing:
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
                        state = eState.Travelling;
                    }


                }
                else
                {
                    //todo
                    //search for any nearby enemy units, if there is one, assign a new aggressor
                    //if not break out of fleeing
                    state = eState.Travelling;
                }
            }
            break;

            case eState.StrafingToFlee:
            {
                TurnTowards(deltaTime, targetLocation);
                MoveForward(deltaTime, 3.3f, true);

                //test distance from target, if it is small enough, then go into fleeing
                fleeOffsetTime += deltaTime;
                float distance = Vector2.Distance(targetLocation, transform.position);
                if (distance < 0.2f || fleeOffsetTime > fleeOffsetMaxTime)
                {
                    state = eState.Fleeing;
                }
            }
            break;

            case eState.StrafingToAttack:
            {
                TurnTowards(deltaTime, targetLocation);
                MoveForward(deltaTime, 3.3f, true);

                //test distance from target, if it is small enough, then go into fleeing
                fleeOffsetTime += deltaTime;
                float distance = Vector2.Distance(targetLocation, transform.position);
                if (distance < 0.2f)
                {
                    state = eState.Hunting;
                }
            }
            break;
        };

        //update any followers
        int followerCount = followingShips.Count;
        bool first = true;
        for (int i = 0; i < followerCount; i++)
        {
            if (followingShips[i] == null || followingShips[i].state != eState.Following)
            {
                followingShips.RemoveAt(i);
                break;
            }

            if (followerCount > 1)
            {
                followingShips[i].UpdateFollowingTargetLocation(followerCount, first);
                first = false;
            }
            else
            {
                followingShips[i].UpdateFollowingTargetLocation();
            }
            
        }
    }

    public void UpdateFollowingTargetLocation(int count = 1, bool first = true)
    {
        Vector2 normalizedTargetForward = target.forward.normalized;
        Vector2 targPos = new Vector2(target.transform.position.x, target.transform.position.y);
        if (count > 1)
        {
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
        else
        {
            targetLocation = targPos + 
                normalizedTargetForward * 0.3f;
        }
    }

    public void TotalFollowerCount(ref int followerCount)
    {
        if (followingShips.Count > 0)
        {
            followerCount++;
            followingShips[0].TotalFollowerCount(ref followerCount);
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
        newLaser.velocity = new Vector3(forward.x, forward.y, 0) * 5;
        newLaser.velocity.x += myBody.velocity.x;
        newLaser.velocity.y += myBody.velocity.y;
        newLaser.ship = this;
        newLaser.damage = 10;
        firingTimer = firingTimerReset;
        if (followingShips.Count > 1 && state != eState.Following)
        {
            for (int i = 0; i < followingShips.Count; i++)
            {
                followingShips[i].FireLaser();
            }
        }
    }

    public void AssignNewFollowTarget(ref int levels, int targetLevel, AIShip newTarg)
    {
        if(levels == targetLevel)
        {
            AIShip otherShip = (AIShip)target;
            otherShip.followingShips.Remove(this);
            target = newTarg;
            newTarg.followingShips.Add(this);
        }
        else
        {
            levels++;
            followingShips[0].AssignNewFollowTarget(ref levels, targetLevel, newTarg);
        }
    }

    public void FindFollowingPosition(AIShip parent = null)
    {
        AIShip otherShip = (AIShip)target;
        int followerCount = otherShip.followingShips.Count;
        if (followerCount < 2)
        {
            otherShip.followingShips.Add(this);
            //when you add in this way there has to be some sort of check to split the followers
            int totalFollowing = 0;
            TotalFollowerCount(ref totalFollowing);
            if (totalFollowing > 1)
            {
                int levelCount = 0;
                AssignNewFollowTarget(ref levelCount, totalFollowing / 2, otherShip);
            }
        }
        else
        {
            int leftSideCount = 0;
            otherShip.followingShips[0].TotalFollowerCount(ref leftSideCount);

            int rightSideCount = 0;
            otherShip.followingShips[1].TotalFollowerCount(ref rightSideCount);

            if (leftSideCount < rightSideCount)
            {
                otherShip.followingShips[0].AssignShipFollow(this);
            }
            else
            {
                otherShip.followingShips[1].AssignShipFollow(this);
            }
        }
    }

    public void MoveForward(float deltaTime, float angularDrag, bool thrusting, float modifier = 1)
    {
        if (thrusting)
        {
            acceleration = deltaTime * speed * Mathf.Min(1, thrustingTime / speedUpTime) * modifier * forward;
            thrustingTime += deltaTime;
        }
        else
        {
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

    public override void TakeDamage(float damageAmount, Ship attacker)
    {
        base.TakeDamage(damageAmount, attacker);
        if (isAlive)
        {
            //todo
            //do a chance check and maybe check fearfulness (doesn't exist yet)
            //check current health as well
            if (Random.Range(0, 50) > 22)
            {
                aggressor = attacker;
                switch (state)
                {
                    case eState.Hunting:
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
                        state = eState.StrafingToFlee;
                    }
                    break;
                    default:
                    {
                        state = eState.Fleeing;
                        RecalulateFleeOffset();
                    }
                    break;
                };
            }
        }
        else
        {
            playerManager.ships.Remove(this);
        }
    }

    public void RecalulateFleeOffset()
    {
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
