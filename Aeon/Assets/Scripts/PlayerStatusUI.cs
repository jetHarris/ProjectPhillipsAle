using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI : MonoBehaviour {

    public Image healthBar;
    public Image shieldBar;
    public Image ammoBar;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdatePlayerStatus(float healthRatio, float shieldRatio, float ammoRatio)
    {
        healthBar.fillAmount = healthRatio;
        shieldBar.fillAmount = shieldRatio;
        ammoBar.fillAmount = ammoRatio;
    }
}
