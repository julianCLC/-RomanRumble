using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] TMP_Text score;
    [SerializeField] Slider slider;
    [SerializeField] Image sliderFill;
    public ulong _playerUIId {get; private set;}

    public void InitializeUI(ulong playerID, string _name = ""){
        _playerUIId = playerID;
        if(_name == ""){
            SetName("Player " + playerID.ToString());
        }
        else{
            SetName(_name);
        }
        
        UpdateScore(0);
        UpdateHealth(1);

        // Change colour based on id
        Color color = GameManager.Instance.GetColour(playerID%4);
        playerName.color = color;
        sliderFill.color = color;
    }

    public void SetName(string _name){
        playerName.text = _name;
    }

    public void UpdateScore(int newScore){
        score.text = "Score: " + newScore;
    }

    public void UpdateHealth(float newHealth){
        // needs to be passed in as health/maxhealth
        slider.value = newHealth;
    }
}
