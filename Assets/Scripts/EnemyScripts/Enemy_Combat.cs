using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    public int enemyDamage = 1;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float knockbackForce = 5f;
    public float stunTime;
    public LayerMask playerLayers;
   
    public void Attack()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        if(hitPlayers.Length > 0)
        {
            var player = hitPlayers[0].gameObject;
            var playerHealth = player.GetComponent<PlayerHealth>();
            var playerMovement = player.GetComponent<PlayerMovement>();

            playerHealth.ChangeHealth(-enemyDamage);

            // Only apply knockback if player is still active/alive
            if (player.activeInHierarchy && StatsManager.Instance.currentHealth > 0 && playerMovement != null && playerMovement.isActiveAndEnabled)
            {
                playerMovement.Knockback(transform, knockbackForce, stunTime);
            }
        }   
    }
}
