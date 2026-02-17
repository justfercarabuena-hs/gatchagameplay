using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator anim;
    public float cooldown = 2;
    public float timer;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public StatsUI statsUI;
    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }
    public void Attack()
    {
        if(timer <= 0)
        {
            anim.SetBool("isAttacking", true);

            timer = cooldown;
        }
      
    }

    public void DealDamage()
    {
            
            statsUI.Updatedamge();
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, StatsManager.Instance.weaponRange, enemyLayers);

            if (hitEnemies.Length > 0)
            {
                hitEnemies[0].GetComponent<EnemyHealth>().ChangeHealth(-StatsManager.Instance.damage);
                hitEnemies[0].GetComponent<EnemyKnockback>().Knockback(transform, StatsManager.Instance.knockbackForce, StatsManager.Instance.knockbackTime, StatsManager.Instance.stunTime);
            }
            StatsManager.Instance.damage += 1;
    }
    public void FinishAttack()
    {
        anim.SetBool("isAttacking", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(attackPoint.position, StatsManager.Instance.weaponRange);
    }
}
