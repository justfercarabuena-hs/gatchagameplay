using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;
    public CanvasGroup statsCanvas;
    private bool statsOpen = false;

    private void Start()
    {
        UpdateAllSStats();
    }
    private void Update()
    {
        if (Input.GetButtonDown("ToggleStats"))
        {
            ToggleStats();
        }
    }

    public void Updatedamge()
    {
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Damage: " + StatsManager.Instance.damage.ToString();
    }

    public void UpdateSpeed()
    {
        statsSlots[1].GetComponentInChildren<TMP_Text>().text = "Speed: " + StatsManager.Instance.speed.ToString();
    }


    public void UpdateAllSStats()
    {
        Updatedamge();
        UpdateSpeed();
    }
    //For mobile
    public void ToggleStats()
    {
        if (statsOpen)
        {
            Time.timeScale = 1;
            UpdateAllSStats();
            statsCanvas.alpha = 0;
            statsOpen = false;
        }
        else
        {
            Time.timeScale = 0;
            UpdateAllSStats();
            statsCanvas.alpha = 1;
            statsOpen = true;
        }
    }

}
