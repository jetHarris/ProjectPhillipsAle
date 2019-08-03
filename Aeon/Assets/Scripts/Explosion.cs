﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    protected SpriteRenderer explosionArt;
    public float opacity = 1.0f;
    // Use this for initialization
    void Start () {
        var Sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
        explosionArt = Sprites[0];
        if (PlayerManager.explosionSoundCooldown <= 0)
        {
            AudioSource sound = GetComponent<AudioSource>();
            if (sound != null)
            {
                sound.Play();
                PlayerManager.explosionSoundCooldown = 0.2f;
            }

        }
    }
	
	// Update is called once per frame
	void Update () {
        opacity -= 0.009f;
        explosionArt.color = new Color(1, 1, 1, opacity);
        if (opacity <= 0)
        {
            Destroy(gameObject);
        }
	}
}
