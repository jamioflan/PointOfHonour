using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float health = 100f;
    public bool onFire = false;

    private float timeSinceLastFire = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        health = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastFire += Time.deltaTime;

        if (onFire && health >= 25 & timeSinceLastFire >= 5)
        {
            health = Mathf.Max(health - 2, 25);
        }
    }
}
