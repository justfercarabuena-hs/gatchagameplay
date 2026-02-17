using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int expReward = 3;
    public delegate void MonsterKilled(int exp);
    public static event MonsterKilled OnMonsterKilled;
    public int maxHealth;
    public int currentHealth;
    private EnemyMovement enemyMovement;
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
      //  enemyMovement = GetComponent<EnemyMovement>();
      //  anim = GetComponent<Animator>();
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if(currentHealth <= 0)
        {
            OnMonsterKilled(expReward);
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
        // anim.SetBool("isDead", true);
        // enemyMovement.enabled = false;
        // GetComponent<Collider2D>().enabled = false;
        // this.enabled = false;
    }
}
