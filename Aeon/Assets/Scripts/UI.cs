﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {

    public PlayerManager playerManager;
    public Text enemyNumText;
    public Text enemyNumTextPrompt;
    private int enemyNum = 7;
    public Text allyNumText;
    public Text allyNumTextPrompt;
    private int allyNum = 0;
    public Text playerNumText;
    public Text playerNumTextPrompt;
    private int playerNum = 1;
    public bool gameStarted = false;
    
    private float timeSinceLastNumChange = 0;
    private float inputNumChangeDelay = 1.0f;
    private float inputNumChangeDelayReset;

    private float numSwitchDelay = 0;

    private enum eSelectedNum
    {
        enemy,
        ally,
        player,
    }

    private eSelectedNum selectedNum = eSelectedNum.enemy;

    // Use this for initialization
    void Start () {
        enemyNumTextPrompt.color = new Color(1, 0, 0, 1);
        enemyNumText.color = new Color(1, 0, 0, 1);

        inputNumChangeDelayReset = inputNumChangeDelay;
    }

    void SwitchText(float upDownIn)
    {
        int upDown = 0;
        if (upDownIn < -0.1)
        {
            upDown = -1;
        }
        else if (upDownIn > 0.1)
        {
            upDown = 1;
        }
        selectedNum += upDown;
        if (selectedNum < eSelectedNum.enemy)
        {
            selectedNum = eSelectedNum.enemy;
        }
        else if (selectedNum > eSelectedNum.player)
        {
            selectedNum = eSelectedNum.player;
        }


        switch (selectedNum)
        {
            case eSelectedNum.ally:
            {
                allyNumTextPrompt.color = new Color(1, 0, 0, 1);
                allyNumText.color = new Color(1, 0, 0, 1);

                enemyNumTextPrompt.color = new Color(1, 1, 1, 1);
                enemyNumText.color = new Color(1, 1, 1, 1);

                playerNumTextPrompt.color = new Color(1, 1, 1, 1);
                playerNumText.color = new Color(1, 1, 1, 1);
            }
            break;
            case eSelectedNum.enemy:
            {
                enemyNumTextPrompt.color = new Color(1, 0, 0, 1);
                enemyNumText.color = new Color(1, 0, 0, 1);

                allyNumTextPrompt.color = new Color(1, 1, 1, 1);
                allyNumText.color = new Color(1, 1, 1, 1);

                playerNumTextPrompt.color = new Color(1, 1, 1, 1);
                playerNumText.color = new Color(1, 1, 1, 1);
            }
            break;
            case eSelectedNum.player:
            {
                playerNumTextPrompt.color = new Color(1, 0, 0, 1);
                playerNumText.color = new Color(1, 0, 0, 1);

                allyNumTextPrompt.color = new Color(1, 1, 1, 1);
                allyNumText.color = new Color(1, 1, 1, 1);

                enemyNumTextPrompt.color = new Color(1, 1, 1, 1);
                enemyNumText.color = new Color(1, 1, 1, 1);
            }
            break;
        }
    }

    void IncreaseDecreaseNum(float num, bool pass)
    {
        timeSinceLastNumChange += Time.deltaTime;
        if (timeSinceLastNumChange < inputNumChangeDelay && !pass)
        {
            return;
        }
        timeSinceLastNumChange = 0;

        int increaseDecrease = 0;
        if (num > 0)
        {
            increaseDecrease = 1;
        }
        else if (num < 0)
        {
            increaseDecrease = -1;
        }

        switch (selectedNum)
        {
            case eSelectedNum.ally:
            {
                allyNum += increaseDecrease;
                if (allyNum < 0)
                {
                    allyNum = 0;
                }
                else if (allyNum > 50)
                {
                    allyNum = 50;
                }
                allyNumText.text = allyNum.ToString();
            }
            break;
            case eSelectedNum.enemy:
            {
                enemyNum += increaseDecrease;
                if (enemyNum < 0)
                {
                    enemyNum = 0;
                }
                else if (enemyNum > 50)
                {
                    enemyNum = 50;
                }
                enemyNumText.text = enemyNum.ToString();
            }
            break;
            case eSelectedNum.player:
            {
                playerNum += increaseDecrease;
                if (playerNum < 1)
                {
                    playerNum = 1;
                }
                else if (playerNum > 4)
                {
                    playerNum = 4;
                }
                playerNumText.text = playerNum.ToString();
            }
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            return;
        }

        float increaseDecreaseInput = Input.GetAxis("LeftJoystickX_P0");
        if (increaseDecreaseInput != 0)
        {
            IncreaseDecreaseNum(Input.GetAxis("LeftJoystickX_P0"), inputNumChangeDelay == inputNumChangeDelayReset);
            if (inputNumChangeDelay > 0)
            {
                inputNumChangeDelay -= 0.015f;
            }
        }
        else if (inputNumChangeDelay != inputNumChangeDelayReset)
        {
            timeSinceLastNumChange = 0;
            inputNumChangeDelay = inputNumChangeDelayReset;
        }

        //numSwitchDelay
        float numSwitchInput = Input.GetAxis("LeftJoystickY_P0");
        if (numSwitchInput != 0)
        {
            if (numSwitchDelay > 0)
            {
                numSwitchDelay -= Time.deltaTime;
            }
            if (numSwitchDelay <= 0)
            {
                SwitchText(numSwitchInput);
                numSwitchDelay = 0.1f;
            }
        }

        if (Input.GetButton("A_P0"))
        {
            gameStarted = true;
            enemyNumText.enabled = false;
            enemyNumTextPrompt.enabled = false;

            allyNumText.enabled = false;
            allyNumTextPrompt.enabled = false;

            playerNumText.enabled = false;
            playerNumTextPrompt.enabled = false;

            playerManager.CreateWorld(enemyNum, allyNum, playerNum);
        }
    }
}