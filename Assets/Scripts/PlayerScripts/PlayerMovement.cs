using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

  
    public int facingDirection = 1; // 1 for right, -1 for left
    public Rigidbody2D rb;

    public Animator animator;
    private bool isKnockedbacked = false;

    public PlayerCombat playerCombat;
    public MobileJoystick joystick; 
    private void Update()
    {
        if(Input.GetButtonDown("Slash"))
        {
            playerCombat.Attack();
        }
    }
    // FixedUpdate is called once per physics frame
    void FixedUpdate()
{
    if (isKnockedbacked) return;

    float horizontal;
    float vertical;

    // 🎮 MOBILE JOYSTICK INPUT
    if (joystick != null)
    {
        horizontal = joystick.Horizontal;
        vertical = joystick.Vertical;
    }
    // 🖥️ KEYBOARD INPUT (PC testing)
    else
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    if (horizontal > 0 && transform.localScale.x < 0 ||
        horizontal < 0 && transform.localScale.x > 0)
    {
        Flip();
    }

    animator.SetFloat("Horizontal", Mathf.Abs(horizontal));
    animator.SetFloat("Vertical", Mathf.Abs(vertical));

    rb.linearVelocity = new Vector2(
        horizontal * StatsManager.Instance.speed,
        vertical * StatsManager.Instance.speed
    );
}


    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);    
    }
    public void Knockback(Transform enemy, float knockbackForce, float stunTime)
    {
        isKnockedbacked = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.linearVelocity = direction * knockbackForce;
        StartCoroutine(KnockbackCounter(stunTime));
    }
    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.linearVelocity = Vector2.zero;
        isKnockedbacked = false;
    }
}
