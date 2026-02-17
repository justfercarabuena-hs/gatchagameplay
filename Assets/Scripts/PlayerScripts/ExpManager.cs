using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ExpManager : MonoBehaviour
{
   public int level;
    public int currentExp;
    public int expToNextLevel = 10;
    public float expGrowthMultiplier = 1.2f;
    public Slider expSlider;
    public TMP_Text currentlevelText;

    private void Start()
    {
        UpdateUI();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            GainExp(2);
        }
    }
    public void GainExp(int amount)
    {
        currentExp += amount;

        if(currentExp >= expToNextLevel)
        {
            LevelUp();
        }
            UpdateUI();
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expGrowthMultiplier);
    }

    private void UpdateUI()
    {
        expSlider.maxValue = expToNextLevel;
        expSlider.value = currentExp;
        currentlevelText.text = "Level: " + level.ToString();
    }

    private void OnEnable()
    {
        EnemyHealth.OnMonsterKilled += GainExp;
    }
    private void OnDisable()
    {
        EnemyHealth.OnMonsterKilled -= GainExp;
    }
}
