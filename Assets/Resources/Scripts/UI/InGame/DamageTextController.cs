using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class DamageTextController : MonoBehaviour
{
    // Set Font Size
    private float minFontSize;
    private float sizeChangeSpeed;

    // Set LifeTime
    private float moveSpeed = 0.15f;
    private float alphaSpeed = 1.5f;
    public float destroyTime = 0.4f;

    // Set Timer
    private float time;

    // Set Damage Text
    public string damage;

    Color alpha;
    TextMeshPro txt;

    private void Awake()
    {
        if (gameObject.name == "CriticalDmgTxt")
        {
            minFontSize = 2f;
            sizeChangeSpeed = 2f;
        }
        else
        {
            minFontSize = 1f;
            sizeChangeSpeed = 1.5f;
        }
    }

    private void Start()
    {
        time = 0;

        txt = GetComponent<TextMeshPro>();
        txt.text = damage;
        alpha = txt.color;

        Destroy(gameObject, destroyTime);
    }

    private void Update()
    {
        // Move the Damage Text Upward
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0));

        if (time < 0.2f)
        {
            txt.fontSize += Time.deltaTime * sizeChangeSpeed;
        }
        else
        {
            // If the font size hasn't reached the minimum size, keep decreasing
            if (txt.fontSize > minFontSize)
            {
                txt.fontSize -= Time.deltaTime * sizeChangeSpeed;
            }
        }

        time += Time.deltaTime * sizeChangeSpeed;

        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed);
        txt.color = alpha;
    }

    internal void SetText(int damage)
    {
        throw new NotImplementedException();
    }
}