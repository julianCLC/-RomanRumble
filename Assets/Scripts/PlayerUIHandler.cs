using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] TMP_Text score;
    [SerializeField] Slider slider;
    [SerializeField] Image sliderFill;

    public void InitializeUI(ulong playerID){
        playerName.text = "Player " + playerID.ToString();
        score.text = "Score: 0";
        slider.value = 1;

        Color color = GameManager.Instance.GetColour(playerID);
        playerName.color = color;

        sliderFill.color = color;
    }

    public void UpdateScore(int newScore){
        score.text = "Score: " + newScore;
    }

    public void UpdateHealth(float newHealth){
        // needs to be passed in as health/maxhealth
        slider.value = newHealth;
    }
}
