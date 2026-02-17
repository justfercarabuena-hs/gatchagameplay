using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;
    public float attackRange = 2.0f;
    public float attackCooldown = 2.0f;
    public float attackCooldownTimer = 0.0f;
    public float playerDetectionRange = 5.0f;
    public Transform detectionPoint;
    public LayerMask playerLayer;
    private int facingDirection = -1;

    public Rigidbody2D rb;
    private EnemyState enemyStateState;

     private Transform player;
    private Animator animm;
    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        animm = GetComponent<Animator>();
        ChanegeState(EnemyState.Idle);
    }


    // Update is called once per frame
    void Update()
    {
        if(enemyStateState != EnemyState.Knockback)
        {
        CheckForPlayer();
        if(attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }
        if(enemyStateState == EnemyState.Chasing)
        {
            Chase();
        } else if(enemyStateState == EnemyState.Attacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
        }
    }

    void Chase()
    {
        if(Vector2.Distance(transform.position, player.transform.position) <= attackRange && attackCooldownTimer <= 0)
        {
            attackCooldownTimer = attackCooldown;
            ChanegeState(EnemyState.Attacking);
            
        }
        if(player.position.x > transform.position.x && facingDirection == -1 ||
            player.position.x < transform.position.x && facingDirection == 1)
            {
            Flip();
            }
                 Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * speed;
        
    }
    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);    
    }
    private void CheckForPlayer()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectionRange, playerLayer);
        if(hitPlayers.Length > 0)
        {
            player = hitPlayers[0].transform;
            //if the playeris iin attack range and the cd is ready
            if(Vector2.Distance(transform.position, player.transform.position) < attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attackCooldown;
                ChanegeState(EnemyState.Attacking);
                
            }
            else if(Vector2.Distance(transform.position, player.position) > attackRange && enemyStateState != EnemyState.Attacking)
            {
            ChanegeState(EnemyState.Chasing);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            ChanegeState(EnemyState.Idle);
        }
         
    }     
        
    public void ChanegeState(EnemyState newState)
    {
        //Exit the animation of the current state
        if(enemyStateState == EnemyState.Idle)
            animm.SetBool("isIdle", false);
        else if(enemyStateState == EnemyState.Chasing)
            animm.SetBool("isChasing", false);
        else if(enemyStateState == EnemyState.Attacking)
            animm.SetBool("isAttacking", false);
        
        //Update the state
        enemyStateState = newState;
        //Update the animation based on the new state
        if(enemyStateState == EnemyState.Idle)
            animm.SetBool("isIdle", true);
        else if(enemyStateState == EnemyState.Chasing)
            animm.SetBool("isChasing", true);
        else if(enemyStateState == EnemyState.Attacking)
            animm.SetBool("isAttacking", true);


      //  animm.SetInteger("State", (int)newState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionPoint.position, playerDetectionRange);
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Knockback
}