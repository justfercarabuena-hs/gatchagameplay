using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    [Header("Combat Stats")]
    public int damage;
    public float weaponRange;
    public float knockbackForce;
    public float knockbackTime;
    public float stunTime;

    [Header("Movement Stats")]
    public int speed;

    [Header("Health Stats")]
    public int maxHealth = 20;
    public int currentHealth;



    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Initialize current health if not set via Inspector
            if (currentHealth <= 0)
            {
                currentHealth = maxHealth;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
