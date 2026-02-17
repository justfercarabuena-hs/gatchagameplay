using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerHealth : MonoBehaviour
{

    public TMP_Text healthText;
    public Animator healthTextAnim;
    private void Start()
    {
        healthText.text = "HP: " + StatsManager.Instance.currentHealth.ToString() + "/" + StatsManager.Instance.maxHealth.ToString();
        
    }
    public void ChangeHealth(int amount)
    {
        StatsManager.Instance.currentHealth += amount;
        healthTextAnim.Play("TextUpdate");
        // Clamp health between 0 and max
        StatsManager.Instance.currentHealth = Mathf.Clamp(StatsManager.Instance.currentHealth, 0, StatsManager.Instance.maxHealth);
        healthText.text = "HP: " + StatsManager.Instance.currentHealth.ToString() + "/" + StatsManager.Instance.maxHealth.ToString();
        if (StatsManager.Instance.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Add death logic here
        gameObject.SetActive(false);
        Debug.Log("Player has died.");
    }
}