using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private EnemyMovement enemyMovement;
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyMovement = GetComponent<EnemyMovement>();
    }
    public void Knockback(Transform playerTransform, float knockbackForce, float knockbackTime, float stunTime)
    {
        enemyMovement.ChanegeState(EnemyState.Knockback);
        StartCoroutine(StunTimer(knockbackTime,stunTime));
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.linearVelocity = direction * knockbackForce;
        
    }

    private IEnumerator StunTimer(float knockbackTime,float stunTime)
    {
        yield return new WaitForSeconds(knockbackTime);
        enemyMovement.ChanegeState(EnemyState.Idle);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(stunTime);
        enemyMovement.ChanegeState(EnemyState.Idle);
    }
}